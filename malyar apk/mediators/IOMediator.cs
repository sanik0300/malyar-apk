using malyar_apk.Shared;
using System;
using System.Collections.Generic;

namespace malyar_apk
{
    public interface IOMediator
    {
        string PathToSchedule { get; }
        string PathToOriginalWP { get; }

        event EventHandler<ScheduleAddedEventArgs> ScheduleLoaded;
        void OnScheduleAdded(List<TimedPictureModel> list/*, bool originals_present*/);

        void BeginLoadingSchedule();
        void SaveSchedule(List<TimedPictureModel> list);

        event EventHandler ScheduleSaved;
        void OnScheduleSaved();
        bool WasInitialized { get; set; }
        void RememberOriginalWP();
    }
}
