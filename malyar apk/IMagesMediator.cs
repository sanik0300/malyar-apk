using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;

namespace malyar_apk
{
    public interface IMagesMediator
    {
        string GetPathToSchedule();
        byte[] GetOriginalWP();

        void DeliverToast(string text);

        void OuchBadTimespan(string txt, View V);
    }
}
