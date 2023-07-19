using Android.App;
using Android.Content;
using System;
using malyar_apk.Shared;
using System.IO;
using Android.Graphics;
using Xamarin.Essentials;

namespace malyar_apk.Droid
{
    [BroadcastReceiver(Enabled = true)]
    [IntentFilter(new string[] { AndroidConstants.WP_CHANGE_ALARM })]
    internal class WPChangeEventReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            if(intent.GetIntExtra(AndroidConstants.TPM_START_EXTRA_KEY, 0) < Math.Floor(DateTime.Now.TimeOfDay.TotalMinutes))
            {
                return;
            }

            string filepath_to_img = intent.GetStringExtra(AndroidConstants.FILEPATH_EXTRA_KEY);

            var WP_manager = WallpaperManager.GetInstance(context);

            if (File.Exists(filepath_to_img))
            {
                WP_manager.SetBitmap(BitmapFactory.DecodeFile(filepath_to_img));
            }
            else {
                GeneralIO.HandleMissingImage((NoSuchImgHandling)Enum.ToObject(typeof(NoSuchImgHandling), Preferences.Get(Constants.MISSING_IMG_HANDLING, 0)),
                                            intent.GetIntExtra(Intent.ExtraIndex, 0));
            }
        }
    }
}