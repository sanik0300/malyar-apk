using System;
using System.Collections.Generic;

namespace malyar_apk.Shared
{
    public class ScheduleAddedEventArgs
    {
        public readonly List<TimedPictureModel> TPMs;    

        public ScheduleAddedEventArgs(List<TimedPictureModel> list)
        {
            TPMs = list;
        }
    }
}
