using System;
using System.Collections.Generic;

namespace malyar_apk.Shared
{
    public class ScheduleAddedEventArgs
    {
        public readonly List<TimedPictureModel> TPMs;
        //public readonly bool originals_present;

        public ScheduleAddedEventArgs(List<TimedPictureModel> list/*, bool origs*/)
        {
            TPMs = list; //originals_present = origs;
        }
    }
}
