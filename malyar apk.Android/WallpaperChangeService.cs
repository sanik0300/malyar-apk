using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;

using System;

using malyar_apk.Shared;
using System.IO;
using Android.Graphics;
using Xamarin.Essentials;
using Android.Support.V4.App;
using Android.Media;
using System.Threading;

namespace malyar_apk.Droid
{
    [Service(Name = "com.sanikshomemade.malyar_apk.WPChangeService")]
    public class WallpaperChangeService : Service
    {
        public override IBinder OnBind(Intent intent) { return null; }

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            NotificationCompat.Builder builder;
            var manager = (NotificationManager)this.GetSystemService(Context.NotificationService);
            string CHANNELLID = "ur_wp_change";
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            { 
                NotificationChannel channel = new NotificationChannel(CHANNELLID, CHANNELLID, NotificationImportance.High);
                manager.CreateNotificationChannel(channel);

                builder = new NotificationCompat.Builder(this, CHANNELLID);
            }
            else {
                builder = new NotificationCompat.Builder(this);
            }
            builder.SetSmallIcon(Resource.Drawable.paint_notification_small_icon).SetContentTitle("Щас поменяем тебе обои через 3...");

            this.StartForeground(1, builder.Build());

            for (byte i = 2; i>0; --i)
            {
                Thread.Sleep(1000);
                builder.SetContentTitle($"Щас поменяем тебе обои через {i}...");
                manager.Notify(1, builder.Build());
            }

            string filepath = intent.GetStringExtra(AndroidConstants.FILEPATH_EXTRA_KEY);

            var WP_manager = WallpaperManager.GetInstance(this);

            if (File.Exists(filepath))
            {
                WP_manager.SetBitmap(BitmapFactory.DecodeFile(filepath));
            }
            else
            {
                switch ((NoSuchImgHandling)Enum.ToObject(typeof(NoSuchImgHandling), Preferences.Get(Constants.MISSING_IMG_HANDLING, 0)))
                {
                    case NoSuchImgHandling.IgnoreAndWaitNext: break;

                    case NoSuchImgHandling.PutDefault:
                        WP_manager.SetBitmap(BitmapFactory.DecodeFile(IO_Implementation.fpath_to_orig));
                        break;

                    case NoSuchImgHandling.PutNext:

                        string working_pic_filepath = string.Empty;
                        uint lines_per_instance = jsonIOmanager.CountLinesOfTypeSerialization(typeof(TimedPictureModel)),
                             index_of_this = (uint)intent.GetIntExtra(Intent.ExtraIndex, 0);

                        using (StreamReader sr = new StreamReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.Read)))
                        {
                            for (uint i = 0; i < lines_per_instance * index_of_this + 1; ++i) { sr.ReadLine(); }//skip le lines of previous objects + start array token at the beginning

                            while (true)
                            {
                                sr.ReadLine();//skip the start object token
                                if (sr.EndOfStream) { break; }

                                working_pic_filepath = sr.ReadLine().Split(" :", StringSplitOptions.RemoveEmptyEntries)[1];
                                if (File.Exists(working_pic_filepath)) { break; }
                                for (uint k = 0; k < lines_per_instance - 2; ++k) { sr.ReadLine(); } //skip the rest of lines
                            }
                        }

                        WP_manager.SetBitmap(BitmapFactory.DecodeFile(File.Exists(working_pic_filepath) ? working_pic_filepath : IO_Implementation.fpath_to_orig));

                        break;
                }
            }

            WP_manager.Dispose();
            this.StopForeground(true);
            if (Build.VERSION.SdkInt >= BuildVersionCodes.O) {
                manager.DeleteNotificationChannel("ur_wp_change");
            }           
            manager.Dispose();

            return base.OnStartCommand(intent, flags, startId);
        }
    }
}