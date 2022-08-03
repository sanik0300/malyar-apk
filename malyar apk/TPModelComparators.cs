using System.Collections.Generic;

namespace malyar_apk
{

    class CompareByStartTime : IComparer<TimedPictureModel>
    {
        public int Compare(TimedPictureModel x, TimedPictureModel y)
        {
            if (x.start_time < y.start_time) { return -1; }
            if (x.start_time > y.start_time) { return 1; }
            return 0;
        }
    }
    class CompareByEndTime : IComparer<TimedPictureModel>
    {
        public int Compare(TimedPictureModel x, TimedPictureModel y)//поиск с конца нах
        {
            if (x.end_time < y.end_time) { return -1; }
            if (x.end_time > y.end_time) { return 1; }
            return 0;
        }
    }
}
