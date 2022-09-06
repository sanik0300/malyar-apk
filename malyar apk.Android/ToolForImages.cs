using Android.Content;
using Android.OS;
using Android.Widget;
using System.IO;
using Xamarin.Forms;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Support.Design.Widget;
using Xamarin.Forms.Platform.Android;
using malyar_apk.Shared;
using System.Collections.Generic;
using Android.App;
using System;
using Android.Support.V4.App;
using System.Text;
using Android.Content.PM;
using malyar_apk.Shared;
using System.Threading.Tasks;

[assembly: Dependency(typeof(malyar_apk.Droid.ToolForImages))]
namespace malyar_apk.Droid
{
    class ToolForImages : IMagesMediator
    {
        static internal string path_to_schedule;

        public event EventHandler<ScheduleAddedEventArgs>  ScheduleLoaded;
        public event EventHandler ScheduleSaved;

        public void OnScheduleAdded(List<TimedPictureModel> list)
        {
            ScheduleLoaded.Invoke(this, new ScheduleAddedEventArgs(list));
        }

        public string GetPathToSchedule() { 
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

        public void DeliverToast(string text)
        {
            Toast.MakeText(MainActivity.Current, text, ToastLength.Short).Show();
        }

        public void OuchError(string description, Xamarin.Forms.View snaccbar_parent)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.IceCreamSandwich) {

                IVisualElementRenderer renderer = Platform.GetRenderer(snaccbar_parent);
                if (renderer != null)
                {
                    Platform.SetRenderer(snaccbar_parent, renderer);
                    Android.Views.View native_view = renderer.View;

                    Snackbar.Make(native_view, description, 1200).Show();

                    return;
                }
            }
            
            Toast.MakeText(MainActivity.Current, description, ToastLength.Short).Show();          
        }

        public void BeginLoadingSchedule()
        {
            MainActivity.Current.StartIO(path_to_schedule, false);
        }

        public void SaveSchedule(List<TimedPictureModel> list)
        {
            MainActivity.Current.StartIO(path_to_schedule, true, list);
        }

        private bool channelexists;
        public async void DenoteSuccesfulSave(float[] percentages)
        {
            var remote_view = new RemoteViews(Xamarin.Essentials.AppInfo.PackageName, Resource.Layout.CustomSaveNotification);        
            remote_view.SetTextViewText(Resource.Id.notif_headline, $"Сохранено обоев: {percentages.Length} шт");           
            remote_view.SetInt(Resource.Id.gradient_container, "setBackgroundResource", (Build.VERSION.SdkInt >= BuildVersionCodes.N)? Resource.Drawable.sky_gradient_24_andabove : Resource.Drawable.sky_gradient);
            remote_view.SetTextViewText(Resource.Id.time_here, DateTime.Now.ToString(Constants.TimeFormat));

            var activity_name_source = MainActivity.Current.PackageManager.GetActivityInfo(MainActivity.Current.ComponentName, PackageInfoFlags.Activities);
            remote_view.SetTextViewText(Resource.Id.appname_here, activity_name_source.NonLocalizedLabel);

            if (percentages.Length > 1)
            {
                float one_letter_width =  24/1.3f; //эх подставлять бы сюда вместо 24 размер текста как-то динамически....
                float num_of_steps = 454 / one_letter_width; //478dp of notification width - 24dp padding on both sides
                StringBuilder sb = new StringBuilder((int)num_of_steps);
                for(int i = 0; i<percentages.Length - 1; ++i)
                {
                    int num_of_steps_this_part = (int)(num_of_steps * percentages[i]);

                    for(int k = 0; k<num_of_steps_this_part-1; ++k) {
                        sb.Append(' ');
                    }
                    sb.Append('|');
                }
                remote_view.SetTextViewText(Resource.Id.gradient_container, sb.ToString());
            }

            NotificationCompat.Builder builder;
            var  manager = (NotificationManager)MainActivity.Current.GetSystemService(Context.NotificationService);
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                string CHANNELLID = "saves_here";         
                if (!channelexists)
                {
                    NotificationChannel channel = new NotificationChannel(CHANNELLID, "saves_here", NotificationImportance.High);
                    manager.CreateNotificationChannel(channel);
                    channelexists = true;
                }
                builder = new NotificationCompat.Builder(MainActivity.Current, CHANNELLID);
            }
            else {
                builder = new NotificationCompat.Builder(MainActivity.Current);
            }

            builder = builder.SetContent(remote_view).SetSmallIcon(Resource.Drawable.save_small_icon);

            manager.Notify(1, builder.Build());
            #if !DEBUG
                await Task.Delay(2000);
                manager.Cancel(1);
            #endif
            builder.Dispose();
            manager.Dispose();
        }

        public void OnScheduleSaved()
        {
            ScheduleSaved.Invoke(this, EventArgs.Empty);
        }
    }
}