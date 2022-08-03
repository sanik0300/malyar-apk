using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using System.IO;
using Xamarin.Forms;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Xamarin.Forms.Platform.Android;

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

        static internal Context currentActivity;

        public void DeliverToast(string text)
        {
            Toast.MakeText(currentActivity, text, ToastLength.Short).Show();
        }

        public void OuchBadTimespan(string txt, View V)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich) {

                IVisualElementRenderer renderer = Platform.GetRenderer(V);
                if (renderer != null)
                {
                    Platform.SetRenderer(V, renderer);
                    Android.Views.View native_view = renderer.View;

                    Snackbar.Make(native_view, txt, 1200).Show();
                    
                    renderer.Dispose();
                    return;          
                }
            }
            
            Toast.MakeText(MainActivity.Current, txt, ToastLength.Short).Show();          
        }
    }
}