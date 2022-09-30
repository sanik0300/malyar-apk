using Android.App;
using Android.Graphics;
using Android.Graphics.Drawables;
using malyar_apk.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(malyar_apk.Droid.IO_Implementation))]
namespace malyar_apk.Droid
{
    class IO_Implementation : ContextDependentObject, IOMediator
    {
        public bool WasInitialized { get; set; }
     
        public string PathToSchedule { get { return path_to_schedule; } }
        
        internal static  readonly string fpath_to_orig = ConvertFilenameToFilepath("original.png");
        public string PathToOriginalWP { get { return fpath_to_orig; } }    

        public void RememberOriginalWP()
        {
            using (MemoryStream mem = new MemoryStream())
            {
                using (WallpaperManager WM = WallpaperManager.GetInstance(BaseContext))
                {
                    Drawable current_wp = WM.Drawable;
                    Bitmap bmp = (current_wp as BitmapDrawable).Bitmap;
                    
                    if (bmp.Compress(Bitmap.CompressFormat.Png, 100, mem))
                    {
                        File.WriteAllBytes(fpath_to_orig, mem.ToArray());
                    }
                }
            }
        }

        public event EventHandler<ScheduleAddedEventArgs> ScheduleLoaded;
        public event EventHandler ScheduleSaved;
        public void OnScheduleAdded(List<TimedPictureModel> list/*, bool originals_present*/)
        {
            ScheduleLoaded.Invoke(this, new ScheduleAddedEventArgs(list/*, originals_present*/));
            //this.has_originals = originals_present;
        }
        public void BeginLoadingSchedule()
        {
            BaseContext.StartIO(path_to_schedule, false);
        }

        public void SaveSchedule(List<TimedPictureModel> list)
        {
            BaseContext.StartIO(path_to_schedule, true, list);
        }

        internal bool has_originals = true;

        public void OnScheduleSaved()
        {
            ScheduleSaved.Invoke(this, EventArgs.Empty);
        }

        internal static string ConvertFilenameToFilepath(string filename)
        {
            return System.IO.Path.Combine(BaseContext.GetExternalFilesDir(null).AbsolutePath, filename);
        }
    }
}