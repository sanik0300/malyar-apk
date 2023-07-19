using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using malyar_apk.Shared;
using System;

namespace malyar_apk.Droid
{
    [Service]
    internal class WaitForWPChangeService : Service
    {
        public static bool Exists { get; private set; }
        public override IBinder OnBind(Intent intent) { 
            return new MyBinder(this); 
        }
        private static int alarms_count;

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Exists = true;
            IParcelable[] tpm_parcelables = intent.GetParcelableArrayExtra(AndroidConstants.LIST_KEY);

            AssignAlarms(tpm_parcelables);

            return base.OnStartCommand(intent, flags, startId);
        }

        internal void AssignAlarms(IParcelable[] sources)
        {
            alarms_count = sources.Length;

            using (var do_alyarmu = (AlarmManager)this.GetSystemService(Context.AlarmService))
            {
                for (int i = 0; i < alarms_count; ++i)
                {
                    TimedPictureModel current_tpm = (sources[i] as TPModelParcelable).source;

                    Intent to_receiver = new Intent(this, typeof(WPChangeEventReceiver)).SetAction(AndroidConstants.WP_CHANGE_ALARM)
                                                .PutExtra(AndroidConstants.FILEPATH_EXTRA_KEY, current_tpm.path_to_wallpaper)
                                                .PutExtra(Intent.ExtraIndex, i)
                                                .PutExtra(AndroidConstants.TPM_START_EXTRA_KEY, (int)current_tpm.StartTime.TotalMinutes);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) //api level 19
                    {
                        to_receiver.PutExtra(AndroidConstants.REQUEST_CODE_EXTRA_KEY, i);
                    }
                    PendingIntent pintent = PendingIntent.GetBroadcast(this, i, to_receiver, PendingIntentFlags.UpdateCurrent);

                    long current_miliseconds = Java.Lang.JavaSystem.CurrentTimeMillis() + (long)(current_tpm.StartTime.TotalMilliseconds - DateTime.Now.TimeOfDay.TotalMilliseconds);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M)//api level 23
                    {
                        do_alyarmu.SetExactAndAllowWhileIdle(AlarmType.Rtc, current_miliseconds, pintent);
                    }
                    else if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) //api level 19
                    {
                        do_alyarmu.SetExact(AlarmType.Rtc, current_miliseconds, pintent);
                    }
                    else {
                        do_alyarmu.SetRepeating(AlarmType.Rtc, current_miliseconds, AlarmManager.IntervalDay, pintent);
                    }
                }
            }
        }

        internal void ClearAlarms()
        {
            using (var do_alyarmu = (AlarmManager)this.GetSystemService(Context.AlarmService))
            {
                for (int i = 0; i < alarms_count; ++i)
                {
                    PendingIntent p_i = PendingIntent.GetBroadcast(this, i, new Intent(this, typeof(WPChangeEventReceiver)).SetAction(AndroidConstants.WP_CHANGE_ALARM), PendingIntentFlags.UpdateCurrent);
                    if (p_i != null)
                    {
                        do_alyarmu.Cancel(p_i);
                        p_i.Cancel();
                    }
                }
            }
        }

        public override void OnDestroy()
        {
            ClearAlarms();
            Exists = false;
            base.OnDestroy();
        }
    }
}