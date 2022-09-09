﻿using System;
using System.Collections.Generic;
using System.Text;

namespace malyar_apk.Shared
{
    public sealed class Constants
    {
        public const int MinutesPerWallpaperByDefault = 240;
        public const string TimeFormat = @"hh\:mm";
        public const string DONT_ASK_AGAIN_KEY = "no_ask_more",
                            SAVED_WALLPAPERS_COUNT_KEY = "how_many_pics";
        public const double UiVerticalRatio = 0.8, UiHorizontalRatio = 0.9;

        public const string AUTOSAVE_SETTING = "autosave",
                            MISSING_IMG_HANDLE_SETTING = "no_img_what_do";
    }
}
