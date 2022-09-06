using System;
using System.Collections.Generic;
using System.IO;
using malyar_apk.Shared;
using Xamarin.Forms;

namespace malyar_apk
{
    static class TimedPicturesLoader
    {
        private static List<TimedPictureModel> _schedule;

        static TimedPicturesLoader()
        {
            DependencyService.Get<IMagesMediator>().ScheduleSaved += (sender, args) => { (sender as IMagesMediator).DenoteSuccesfulSave(GeneratePercentageMap()); };
        }
        static public float[] GeneratePercentageMap()
        {
            float[] result = new float[_schedule.Count];
            for(int i =0; i< _schedule.Count; ++i)
            {
                result[i] = (float)(_schedule[i].DurationInMinutes / 1440);
            }
            return result;
        }
        static internal bool CheckOutExistingOnes()
        {
            IMagesMediator mediator = DependencyService.Get<IMagesMediator>();

            if (File.Exists(mediator.GetPathToSchedule()))
            {
                mediator.ScheduleLoaded += (s, args) => {
                    _schedule = args.TPMs != null ? args.TPMs : new List<TimedPictureModel>(24 / (Constants.MinutesPerWallpaperByDefault/60)); };
                mediator.BeginLoadingSchedule();
                return true;
            }
          
            _schedule = new List<TimedPictureModel>(24 / (Constants.MinutesPerWallpaperByDefault/60));   
            return false;
        }

        static internal void TryToSaveExistingOnes()
        {
            IMagesMediator mediator = DependencyService.Get<IMagesMediator>();
            mediator.SaveSchedule(_schedule);
        }

        public static event EventHandler<TPModelAddedEventArgs> IntervalInserted;
        public static event EventHandler<TPModelDeletedEventArgs> IntervalDeleted;

        private static double get_position_in_schedule_with_bin_search(TimedPictureModel TPM, IComparer<TimedPictureModel> comparer, int start = -1, int end = -1)
        {
            if (_schedule.Count == 0) {
                return 0;
            }

            int left_end = start < 0? 0 : start, 
                right_end = end < 0? _schedule.Count - 1 : end, 
                mid;

            do
            {
                switch (comparer.Compare(TPM, _schedule[left_end]))
                {
                    case 0:
                        return left_end;
                        break;
                    case -1:
                        return left_end + 0.5;
                        break;
                }
                switch (comparer.Compare(TPM, _schedule[right_end]))
                {
                    case 0:
                        return right_end;
                        break;
                    case 1:
                        return right_end + 0.5;
                        break;
                }
                mid = (int)Math.Ceiling((float)(left_end + right_end) / 2);
                switch (comparer.Compare(TPM, _schedule[mid]))
                {
                    case 0:
                        return mid;
                        break;
                    case -1:
                        right_end = mid;
                        break;
                    case 1:
                        left_end = mid;
                        break;
                }
            }
            while (right_end - left_end > 1);                   

            return comparer is CompareByStartTime? left_end + 0.5 : right_end+0.5;
        }

        public static void FitIntervalIn(ChangeDirection direction, TimedPictureModel interval)
        {
            int newcomer_indx;
            int deletion_start = -1, deletion_range = 0;
            int indx_to_clone = -1, insert_clone = -1;
            AdditionMode addmode = AdditionMode.Insert;

            double[] span;
            switch (direction)//Why not make it easier for the algorithm to count :)
            {
                case ChangeDirection.AffectUpwards:
                    int max = (int)get_position_in_schedule_with_bin_search(interval, new CompareByEndTime());
                    span = new double[] { get_position_in_schedule_with_bin_search(interval, new CompareByStartTime(), -1, max-1), max };
                    break;
                case ChangeDirection.AffectDownwards:
                    int min = (int)get_position_in_schedule_with_bin_search(interval, new CompareByStartTime());
                    span = new double[] { min, get_position_in_schedule_with_bin_search(interval, new CompareByEndTime(), min+1) };
                    break;
                default:
                    span = new double[] {
                                    get_position_in_schedule_with_bin_search(interval, new CompareByStartTime()),
                                    get_position_in_schedule_with_bin_search(interval, new CompareByEndTime())
                               };
                    break;
            }

            newcomer_indx = (int)Math.Ceiling(span[0]);
            if (Math.Truncate(span[1]) == span[1] && _schedule.Count > 0) {
                span[1] += 1; //Чтобы это значение в любом случае показывало сколько интервалов заканчиваются раньше данного
            }

            //Part 1-----------------------------
            //Calculating where to delete
            int too_big_diff = (int)(Math.Truncate(span[1]) - Math.Ceiling(span[0]));
            if (too_big_diff >= 2)
            {
                deletion_start = newcomer_indx;
                deletion_range = too_big_diff;

                if (direction != ChangeDirection.InsertNew)
                {
                    if (direction == ChangeDirection.AffectDownwards) { deletion_start += 1; }
                    deletion_range -= 1;
                }
                span[1] -= too_big_diff;
            }

            //Part 2---------
            //Delete the overlapped intervals
            if (deletion_start >= 0)  {
                _schedule.RemoveRange(deletion_start, deletion_range);
                for (int i = 0; i < deletion_range; ++i) {
                    IntervalDeleted.Invoke(null, new TPModelDeletedEventArgs(deletion_start));
                }
            }

            //Part 3---------------------------
            //Needed only if we're inserting a new interval
            if (direction == ChangeDirection.InsertNew)
            {            
                if (_schedule.Count >= 2 && (interval.path_to_wallpaper == _schedule[(int)span[0]].path_to_wallpaper || interval.path_to_wallpaper == _schedule[(int)span[1]].path_to_wallpaper))
                {
                    if (span[1] - span[0] >= 1)
                    {
                        if(interval.path_to_wallpaper == _schedule[(int)span[0]].path_to_wallpaper)  {
                            _schedule[(int)span[0]].EndTime = TimeSpan.FromMinutes(interval.end_time.TotalMinutes);
                        }
                        else {
                            _schedule[(int)span[1]].StartTime = TimeSpan.FromMinutes(interval.start_time.TotalMinutes);
                        }
                    }
                    else { return; }//это всё, тикаем отсюда
                }
                else {               
                    switch (span[1] - span[0])//Calculating where to replace or clone
                    {
                        case 0: //ровно внутри
                            indx_to_clone = newcomer_indx - 1;
                            insert_clone = newcomer_indx + 1;
                            break;
                        case 0.5:
                            break;//it is left as Insert
                        case 1:
                            if (Math.Truncate(span[0]) == span[0]) {
                                addmode = AdditionMode.Replace;
                            }
                            break;
                        default://1.5 or 2
                            addmode = AdditionMode.Replace;
                            break;
                    }

                    if (addmode == AdditionMode.Insert) {
                        _schedule.Insert(newcomer_indx, interval);
                    }
                    else {
                        _schedule[newcomer_indx] = interval;
                    }
                    IntervalInserted.Invoke(interval, new TPModelAddedEventArgs(newcomer_indx, addmode));

                    if (indx_to_clone >= 0)
                    {
                        var another_one = _schedule[indx_to_clone].Clone() as TimedPictureModel;
                        _schedule.Insert(insert_clone, another_one);
                        IntervalInserted.Invoke(another_one, new TPModelAddedEventArgs(insert_clone, AdditionMode.Insert));
                    }
                }
            }

            //Part 4-----------------
            //Connecting intervals together
            if (newcomer_indx > 0 || direction == ChangeDirection.AffectUpwards && newcomer_indx > 0)
            {
                _schedule[newcomer_indx].Join(_schedule[newcomer_indx - 1], ChangeDirection.AffectUpwards);
            }
            if (newcomer_indx < _schedule.Count - 1 && direction != ChangeDirection.AffectUpwards)
            {
                _schedule[newcomer_indx].Join(_schedule[newcomer_indx + 1], ChangeDirection.AffectDownwards);
            }
        }

        public static void DeleteInterval(TimedPictureModel interval)
        {
            int my_old_indx = (int)get_position_in_schedule_with_bin_search(interval, new CompareByStartTime());
            //let's get to know which neighbouring interval lasts longer before deleting the current one

            if (my_old_indx == 0)
            {
                _schedule[1].StartTime = TimeSpan.Zero;
            }            
            else if (my_old_indx == _schedule.Count - 1)
            {
                _schedule[my_old_indx - 1].EndTime = TimeSpan.FromDays(1);
            }
            else {
                if (_schedule[my_old_indx - 1].DurationInMinutes > _schedule[my_old_indx + 1].DurationInMinutes)
                {  
                     _schedule[my_old_indx - 1].Join(_schedule[my_old_indx + 1], ChangeDirection.AffectDownwards);   
                }
                else {
                    _schedule[my_old_indx + 1].Join(_schedule[my_old_indx - 1], ChangeDirection.AffectUpwards);
                }
            }
 
            _schedule.RemoveAt(my_old_indx);//i'm not complicating, this way is faster

            IntervalDeleted.Invoke(null, new TPModelDeletedEventArgs(my_old_indx));
        }

        //private static void MergeIntervals(int first_TPM_indx) { }
    }

    /// <summary>
    /// Used during addition of new intervals
    /// </summary>
    public enum AdditionMode : byte { Insert, Replace }

    public enum ChangeDirection : byte
    {
        AffectUpwards, AffectDownwards, InsertNew
    }
}
