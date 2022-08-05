using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace malyar_apk
{
    class TimedPicturesLoader
    {
        private static List<TimedPictureModel> _schedule;
        static internal ArrayList InitExistingOnes()
        {
            IMagesMediator mediator = DependencyService.Get<IMagesMediator>();
            if(File.Exists(mediator.GetPathToSchedule()))
            {
                throw new NotImplementedException("здесь пока ремонт");
            }
            else {
                _schedule = new List<TimedPictureModel>(24/SchedulePiece.HoursPerWallpaperBydefault);
                return null; 
            }
        }

        public static event EventHandler<TPModelAddedEventArgs> IntervalInserted;
        public static event EventHandler<TPModelDeletedEventArgs> IntervalDeleted;

        private static double get_position_in_schedule_with_bin_search(TimedPictureModel TPM, IComparer<TimedPictureModel> comparer)
        {
            if (_schedule.Count == 0) {
                return 0;
            }

            int left_end = 0, right_end = _schedule.Count - 1, mid;


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
            int deletion_start = -1, deletion_range=0;
            int indx_to_clone = -1, insert_clone=-1;
            AdditionMode addmode = AdditionMode.Insert;
            
            double[] span = new double[]
            {
                get_position_in_schedule_with_bin_search(interval, new CompareByStartTime()),
                get_position_in_schedule_with_bin_search(interval, new CompareByEndTime())
            };

            newcomer_indx = (int)Math.Ceiling(span[0]);
            if(Math.Truncate(span[1]) == span[1] && _schedule.Count > 0) {
                span[1] += 1; //Чтобы это значение в любом случае показывало сколько интервалов заканчиваются раньше данного
            }

            //Part 1-----------------------------
            //Calculating where to delete
            int too_big_diff = (int)(Math.Truncate(span[1]) - Math.Ceiling(span[0]));
            if (too_big_diff >= 2)
            {
                deletion_start = newcomer_indx;
                deletion_range = too_big_diff;
                
                span[1] -= too_big_diff;
            }
            
            //Part 2-----------------------------
            if (direction == ChangeDirection.InsertNew)//Needed only if we're inserting a new interval
            {
                //Calculating where to replace or clone
                switch (span[1] - span[0])
                {
                    case 0: //ровно внутри
                        indx_to_clone = newcomer_indx - 1;
                        insert_clone = newcomer_indx + 1;
                        break;
                    case 1:
                        if (Math.Truncate(span[0]) == span[0]) {
                            addmode = AdditionMode.Replace;
                        }
                        break;
                    case 1.5:
                        addmode = AdditionMode.Replace;
                        break;
                    case 2:
                        addmode = AdditionMode.Replace;
                        break;
                }
            }

            //Part 3------------------------------
            //Reordering the _schedule
            if (deletion_start >= 0) {
                _schedule.RemoveRange(deletion_start, deletion_range);
                for (int i = 0; i < deletion_range; i++)
                {
                    IntervalDeleted.Invoke(null, new TPModelDeletedEventArgs(deletion_start));
                }
            }

            if (direction == ChangeDirection.InsertNew)//Also do only if we're inserting a new interval
            {
                if (addmode == AdditionMode.Insert) {
                    _schedule.Insert(newcomer_indx, interval);
                }
                else {
                    _schedule[newcomer_indx] = interval;
                }
                IntervalInserted.Invoke(interval, new TPModelAddedEventArgs(newcomer_indx, addmode));

                if (indx_to_clone >= 0) {
                    var another_one = _schedule[indx_to_clone].Clone() as TimedPictureModel;
                    _schedule.Insert(insert_clone, another_one);
                    IntervalInserted.Invoke(another_one, new TPModelAddedEventArgs(insert_clone, AdditionMode.Insert));
                }
            }

            //Part 4-----------------
            //Connecting intervals together
            if(newcomer_indx > 0 || direction==ChangeDirection.AffectUpwards) {
                _schedule[newcomer_indx].Join(_schedule[newcomer_indx - 1], ChangeDirection.AffectUpwards);
            }
            if( newcomer_indx < _schedule.Count - 1 && direction!=ChangeDirection.AffectUpwards) {
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
            else
            {
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
    }

    /// <summary>
    /// Used during addition of new intervals
    /// </summary>
    public enum AdditionMode : byte { Insert, Replace }
}
