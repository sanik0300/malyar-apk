using Android.App;
using Android.Content;
using Android.OS;
using Android.Util;
using malyar_apk.Shared;
using System;
using Xamarin.Essentials;
using Java.Lang;
using System.ComponentModel;
using System.IO;

namespace malyar_apk.Droid
{
    [BroadcastReceiver(Enabled = true, Exported =true)]
    [IntentFilter(new string[] { AndroidConstants.WP_CHANGE_ALARM })]
    public class AlarmReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            #if DEBUG
            Log.Debug("JEST CONTACT", "щас будем обои менять");
            #endif
            
            string filepath = intent.GetStringExtra(AndroidConstants.FILEPATH_EXTRA_KEY);
            int total_minutes = intent.GetIntExtra(AndroidConstants.TPM_START_EXTRA_KEY, 0);

            if (total_minutes < System.Math.Floor(DateTime.Now.TimeOfDay.TotalMinutes)) {
                #if DEBUG
                Log.Debug("JEST CONTACT", "а не, не будем - поздно уже");
                #endif
                return;
            }
            else if (total_minutes == 0)
            {
                string the_earliest_tpm_path;
                using (StreamReader sr = new StreamReader(new FileStream(IO_Implementation.fpath_to_schedule, FileMode.Open, FileAccess.Read, FileShare.Read)))
                {
                    for (uint i = 0; i < 2; ++i) { sr.ReadLine(); }//skip start-array and start-object token

                    the_earliest_tpm_path = sr.ReadLine().Split(" :", StringSplitOptions.RemoveEmptyEntries)[1];
                }
                if(filepath == the_earliest_tpm_path) {
                    filepath = null;
                    return;
                }
            }
            
            int indx = intent.GetIntExtra(Intent.ExtraIndex, 0);

            Intent call_service = new Intent(context, typeof(WallpaperChangeService)).PutExtra(AndroidConstants.FILEPATH_EXTRA_KEY, filepath);
            if(Preferences.Get(Constants.MISSING_IMG_HANDLING, 0) == 1)
            {
                call_service.PutExtra(Intent.ExtraIndex, indx);
            }

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += (ses, args) =>
            {
                context.StartService(call_service);
            };
            bw.RunWorkerAsync();//пАехали)))

            if(Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)//api level 19
            {
                //Собираем pending intent на следующий идентичный заход из чего есть
                Intent inner = new Intent(Application.Context, typeof(AlarmReceiver)).SetAction(AndroidConstants.WP_CHANGE_ALARM)
                                .PutExtra(AndroidConstants.FILEPATH_EXTRA_KEY, filepath)
                                .PutExtra(Intent.ExtraIndex, indx);
                PendingIntent PI = SchedulingImplementation.FormP_IntentForReceiver(intent.GetIntExtra(AndroidConstants.REQUEST_CODE_EXTRA_KEY, 0), inner);
             
                using (var AM = context.GetSystemService(Context.AlarmService) as AlarmManager)
                {
                    /*Так как алярм у нас точный по минутам, в trigger_time следующего достаточно положить просто время сейчас + 1 сутки в миллисекундном эквиваленте*/
                    AM.SetExact(AlarmType.Rtc, JavaSystem.CurrentTimeMillis() + AlarmManager.IntervalDay, PI);
                }
            }
        }
    }
}