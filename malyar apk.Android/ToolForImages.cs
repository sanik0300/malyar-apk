using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xamarin.Forms;
using Android.Graphics;
using Android.Graphics.Drawables;
using Java.IO;

[assembly: Dependency(typeof(malyar_apk.Droid.ToolForImages))]
namespace malyar_apk.Droid
{
    class ToolForImages : IMagesMediator
    {
        static internal string path_to_schedule;

        public string GetPathToSchedule()
        {
            return path_to_schedule;
        }

        public byte[] GetOriginalWP()
        {
            byte[] result = null;
            using (MemoryStream mem = new MemoryStream())
            {
                using (WallpaperManager WM = WallpaperManager.GetInstance(MainActivity.Current))
                {
                    Drawable current_wallpaper = WM.Drawable;
                    Bitmap bmp = (current_wallpaper as BitmapDrawable).Bitmap;

                    if (bmp.Compress(Bitmap.CompressFormat.Jpeg, 100, mem))
                    {
                        result = mem.ToArray();
                    }
                }
            }
            return result;      
        }
    }
}