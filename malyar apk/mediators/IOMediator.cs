using malyar_apk.Shared;
using System;
using System.Collections.Generic;

namespace malyar_apk
{
    public interface IOMediator
    {
        string PathToSchedule { get; }
        string PathToOriginalWP { get; }

        event EventHandler<ValuePassedEventArgs<List<TimedPictureModel>>> ScheduleLoaded;
        void OnScheduleAdded(List<TimedPictureModel> list);

        void BeginLoadingSchedule();
        void SaveSchedule(List<TimedPictureModel> list);

        event EventHandler ScheduleSaved;
        void OnScheduleSaved();
        bool WasInitialized { get; set; }
        void RememberOriginalWP();

        void AskForFileInPicker(TimedPictureModel who_asked=null);
        event EventHandler<ValuePassedEventArgs<string>> FilePathDelivered;
        void OnFilePathDelivered(string filePath);
    }
}
