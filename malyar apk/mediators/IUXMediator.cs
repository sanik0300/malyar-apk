﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Xamarin.Forms;
using malyar_apk.Shared;

namespace malyar_apk
{
    //Former ToolForImages' part, solely platform-wise methods
    public interface IUXMediator
    {
        void DeliverToast(string text);
        void OuchError(string decription, View snaccbar_parent);
        void DenoteSuccesfulSave(float[] percentages);
        void ShowWallpaperCloseUp(string pathToWP, double ratio);
    }
}
