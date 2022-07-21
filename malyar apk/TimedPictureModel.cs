using System;
using System.Collections.Generic;
using System.Text;

namespace malyar_apk
{
    public enum TimespanCompareResult : int
    {
        WITHIN = -2, EARLIER, EQUAL, LATER, MOREOVER
    }

    class TimedPictureModel : IComparable
    {
        public const string just_original = "RETRIEVE_ORIGINAL";
        public string path_to_wallpaper;
        public TimeSpan start_time=TimeSpan.MinValue, end_time=TimeSpan.MinValue;

        public bool Complete { get {
                return path_to_wallpaper != string.Empty && path_to_wallpaper != null && start_time != TimeSpan.MinValue && end_time != TimeSpan.MinValue;
                    } 
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
    }
}
