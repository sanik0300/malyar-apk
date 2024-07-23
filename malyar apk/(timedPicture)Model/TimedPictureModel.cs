using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using malyar_apk;
using Xamarin.Forms;

namespace malyar_apk.Shared { 

    [JsonConverter(typeof(ResponsiveTPMConverter))]
    public class TimedPictureModel : IComparable, ICloneable, INotifyPropertyChanged
    {
        [JsonPropertyOrder(0)]
        public string path_to_wallpaper { get; internal set; }
       
        public string GetTimeIntervalString()
        {
            TimeSpan interval = end_time - start_time;
            if(interval.Days > 0) { return "24 ч"; }
            StringBuilder sb = new StringBuilder();
            if(interval.Hours > 0)
            {
                sb.Append($"{interval.Hours} ч");
            }
            if(interval.Minutes > 0)
            {
                if(interval.Hours > 0)
                {
                    sb.Append(' ');
                }
                sb.Append($"{interval.Minutes} мин");
            }
            return sb.ToString();
        }

        internal TimeSpan start_time, end_time;
        public TimeSpan StartTime { 
            get { return start_time; } 
            internal set {
                start_time = value;
                if (PropertyChanged == null)
                    return;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(start_time)));
            }
        }

        [JsonIgnore]//а зачем записывать такие очевидные вещи, если обои друг друга сменяют
        public TimeSpan EndTime
        {
            get { return end_time; }
            internal set {
                end_time = value;
                if (PropertyChanged == null)
                    return;
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(end_time)));
            }
        }

        internal const int MinLegitMinutesDelta = 5;

        public event PropertyChangedEventHandler PropertyChanged;
        public void OnPropertyChanged(string prop_name)
        {
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(prop_name));
        }

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
        
       internal TimedPictureModel() { }

       internal static TimedPictureModel OriginalForTheWholeDay()
       {
            return new TimedPictureModel(DependencyService.Get<IOMediator>().PathToOriginalWP, TimeSpan.Zero, TimeSpan.FromDays(1));
       }

        public TimedPictureModel(string path_to_img, TimeSpan start, TimeSpan end)
        {
            this.path_to_wallpaper = path_to_img;
            this.start_time = start;
            this.end_time = end;
        }
        public TimedPictureModel(string path_to_img, TimeSpan start)
        {
            this.path_to_wallpaper = path_to_img;
            this.start_time = start;
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
            return new TimedPictureModel(this.path_to_wallpaper, this.start_time, this.end_time);
        }

        internal void Join(TimedPictureModel other, ChangeDirection direction)
        {
            switch (direction)
            {
                case ChangeDirection.AffectUpwards:
                    other.EndTime = this.start_time;
                    break;
                case ChangeDirection.AffectDownwards:
                    other.StartTime = this.end_time;
                    break;
            }
        }

        public void OnNewWallpaperPathGotten(object sender, ValuePassedEventArgs<string> e)
        {       
            path_to_wallpaper = e.value;
            PropertyChanged.Invoke(this, new PropertyChangedEventArgs(nameof(path_to_wallpaper)));
            DependencyService.Get<IOMediator>().FilePathDelivered -= this.OnNewWallpaperPathGotten;           
        }
    }
}
