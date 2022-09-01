using System;
using System.ComponentModel;
using System.Text.Json.Serialization;
using malyar_apk;

namespace malyar_apk.Shared { 

    [JsonConverter(typeof(ResponsiveTPMConverter))]
    public class TimedPictureModel : IComparable, ICloneable, INotifyPropertyChanged
    {
        internal const string just_original = "RETRIEVE_ORIGINAL";
        public string path_to_wallpaper { get; internal set; }

        internal TimeSpan start_time, end_time;
        public TimeSpan StartTime { 
            get { return start_time; } 
            set {
                start_time = value;
                if (PropertyChanged == null)
                    return;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(start_time)));
            }
        }
        public TimeSpan EndTime
        {
            get { return end_time; }
            set
            {
                end_time = value;
                if (PropertyChanged == null)
                    return;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(end_time)));
            }
        }

        internal const int MinLegitMinutesDelta = 5;

        public event PropertyChangedEventHandler PropertyChanged;

        internal double DurationInMinutes { get
            {
                return end_time.TotalMinutes - start_time.TotalMinutes;
            } 
        }

        internal static TimeSpan ClampTimespan(TimeSpan tsp)//specially for assigning time to time pickers >~<
        {
            if (tsp.TotalHours < 24)
                return tsp;
            return new TimeSpan(tsp.Hours, tsp.Minutes, 0);
        }
        
        internal TimedPictureModel() {  }

       internal static TimedPictureModel OriginalForTheWholeDay()
        {
            return new TimedPictureModel(just_original, TimeSpan.Zero, TimeSpan.FromDays(1));
        }

        public TimedPictureModel(string path_to_img, TimeSpan start, TimeSpan end)
        {
            this.path_to_wallpaper = path_to_img;
            this.start_time = start;
            this.end_time = end;
        }

        public override string ToString()
        {
            return $"{start_time.ToString(Constants.TimeFormat)} - {end_time.ToString(Constants.TimeFormat)}: {path_to_wallpaper}";
        }

        public int CompareTo(object obj)
        {
            TimedPictureModel that = obj as TimedPictureModel;
            if (that is null) { throw new TypeAccessException("ты чё пёс ты шо за обьект сюда засунул?"); }

            if(this.start_time < that.start_time && this.end_time <= that.end_time)
            {
                return -1;
            }
            if(this.start_time > that.start_time && this.end_time < that.end_time)
            {
                return -2;
            }
            if(this.start_time >= that.start_time && this.end_time > that.end_time)
            {
                return 1;
            }
            if(this.start_time < that.start_time && this.end_time > that.end_time)
            {
                return 2;
            }
            return 0;
        }    

        public object Clone()
        {            
            return new TimedPictureModel(this.path_to_wallpaper, TimeSpan.FromMinutes(this.start_time.TotalMinutes), TimeSpan.FromMinutes(this.end_time.TotalMinutes));
        }

        internal void Join(TimedPictureModel other, ChangeDirection direction)
        {
            switch (direction)
            {
                case ChangeDirection.AffectUpwards:
                    other.EndTime = TimeSpan.FromMinutes(this.start_time.TotalMinutes);
                    break;
                case ChangeDirection.AffectDownwards:
                    other.StartTime = TimeSpan.FromMinutes(this.end_time.TotalMinutes);
                    break;
            }
        }
    }
}
