using System;
using System.Collections.Generic;
using System.Text;

namespace malyar_apk.Shared
{
    public sealed class Constants
    {
        public const int MinutesPerWallpaperByDefault = 240;
        internal const string UpdateImg = "update_img",
                              FileNotFoundResourcePath = "malyar_apk.images.file_not_found.png";

        public const string TimeFormat = @"hh\:mm";
        public const string DONT_ASK_AGAIN_KEY = "no_ask_more",
                            SAVED_WALLPAPERS_COUNT_KEY = "how_many_pics",
                            FIRST_LAUNCH_KEY = "first_time";
        public const double UiVerticalRatio = 0.8, UiHorizontalRatio = 0.9;

        public const string AUTOSAVE_SETTING = "autosave",
                            MISSING_IMG_HANDLING = "no_img_what_do";
    }
}
