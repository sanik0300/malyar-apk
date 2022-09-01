using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;
using malyar_apk.Shared;

namespace malyar_apk
{
    public interface IMagesMediator
    {
        string GetPathToSchedule();
        byte[] GetOriginalWP();

        void DeliverToast(string text);

        void OuchError(string decription, View snaccbar_parent);


        event EventHandler<ScheduleAddedEventArgs> ScheduleLoaded;
        void OnScheduleAdded(List<TimedPictureModel> list);

        void BeginLoadingSchedule();
        void SaveSchedule(List<TimedPictureModel> list);

        event EventHandler ScheduleSaved;
        void OnScheduleSaved();
        void DenoteSuccesfulSave(float[] percentages);
    }
}
