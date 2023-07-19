using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using malyar_apk.Shared;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using Xamarin.Forms;

[assembly: Dependency(typeof(malyar_apk.Droid.IO_Implementation))]
namespace malyar_apk.Droid
{
    class IO_Implementation : ContextDependentObject, IOMediator
    {
        public bool WasInitialized { get; set; }
     
        public string PathToSchedule { get { return fpath_to_schedule; } }
        
        internal static readonly string fpath_to_orig = System.IO.Path.Combine(BaseContext.GetExternalFilesDir(null).AbsolutePath, "original.png");
        internal static readonly string fpath_to_schedule = System.IO.Path.Combine(BaseContext.GetExternalFilesDir(null).AbsolutePath, "schedule");
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
                    current_wp.Dispose();
                    bmp.Dispose();
                }
            }
        }

        public event EventHandler<ValuePassedEventArgs<List<TimedPictureModel>>> ScheduleLoaded;
        public event EventHandler ScheduleSaved;
        public void OnScheduleAdded(List<TimedPictureModel> list)
        {
            ScheduleLoaded.Invoke(this, new ValuePassedEventArgs<List<TimedPictureModel>>(list));
        }
        public void BeginLoadingSchedule()
        {
            BaseContext.StartIO(fpath_to_schedule, false);
        }

        public void SaveSchedule(List<TimedPictureModel> list)
        {
            BaseContext.StartIO(fpath_to_schedule, true, list);
        }

        internal bool has_originals = true;

        public void OnScheduleSaved()
        {
            ScheduleSaved.Invoke(this, EventArgs.Empty);
        }

        public void AskForFileInPicker(TimedPictureModel who_asked = null)
        {
            Intent choose_file_perhaps = new Intent(Intent.ActionGetContent);
            choose_file_perhaps.SetType("image/*");
            choose_file_perhaps = Intent.CreateChooser(choose_file_perhaps, "ну выбирай штоли...");

            BaseContext.StartActivityForResult(choose_file_perhaps, AndroidConstants.FILEPICKER_RESULT_REQ_CODE);
            if (who_asked == null) { return;  }
            this.FilePathDelivered += who_asked.OnNewWallpaperPathGotten;
        }

        public event EventHandler<ValuePassedEventArgs<string>> FilePathDelivered;

        public void OnFilePathDelivered(string filePath)
        {
            FilePathDelivered.Invoke(this, new ValuePassedEventArgs<string>(filePath));
        }

        public event EventHandler UpdateWhichImagesExist;
        public void OnUpdateWhichFilesExist()
        {
            UpdateWhichImagesExist.Invoke(this, EventArgs.Empty);
        }
    }
}