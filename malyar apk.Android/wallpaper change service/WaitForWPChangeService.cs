using Android.App;
using Android.Content;
using Android.Provider;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using malyar_apk.Shared;
using System;
using System.IO;
using System.Threading;
using System.Diagnostics;

namespace malyar_apk.Droid
{
    [Service(Name = "com.sanikshomemade.malyar_apk.WaitForWPChangeService")]
    internal class WaitForWPChangeService : Service
    {
        public static bool Exists { get; private set; }
        private bool IsForegroundCurrently = false;
        public override IBinder OnBind(Intent intent) { 
            return new MyBinder(this); 
        }
        private static int alarms_count;

        [return: GeneratedEnum]
        public override StartCommandResult OnStartCommand(Intent intent, [GeneratedEnum] StartCommandFlags flags, int startId)
        {
            Exists = true;
            BroadcastWasReceived += this.DisplayCountdownByNotification;
            
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                BroadcastWasReceived += AlarmNextDayOnEvent;
            }     
            IParcelable[] tpm_parcelables = intent.GetParcelableArrayExtra(AndroidConstants.LIST_KEY);

            try {
                AssignAlarms(tpm_parcelables);
            }
            catch(Exception ex) { }

            return StartCommandResult.RedeliverIntent;
        }

        public new void StopForeground(bool remove)
        {
            base.StopForeground(remove);
            IsForegroundCurrently = false;
        }
        public new void StartForeground(int id, Notification? notification)
        {
            base.StartForeground(id, notification);
            IsForegroundCurrently = true;
        }


        private static event EventHandler BroadcastWasReceived;
        internal static void OnBroadcastWasReceived(Intent receivedIntent)
        {
            BroadcastWasReceived.Invoke(receivedIntent, EventArgs.Empty);
        }

        //set alarm with the same pending intent to time exactly in a day from now,
        //since the contained Set methods wont make a repeated alarm
        private void AlarmNextDayOnEvent(object sender, EventArgs e)
        {
            long millisNextDay = Java.Lang.JavaSystem.CurrentTimeMillis() + 86400000; //thats how many milliseconds in a day
                                                                                      //subtract seconds and millis to avoid increasing offset in time with everyday usage
            Intent intentJustUsedInReceiver = sender as Intent;
            PendingIntent pintent = PendingIntent.GetBroadcast(this,
                                                                intentJustUsedInReceiver.GetIntExtra(AndroidConstants.REQUEST_CODE_EXTRA_KEY, 0),
                                                                intentJustUsedInReceiver,
                                                                (int)Build.VERSION.SdkInt >= 31 ? PendingIntentFlags.Immutable : PendingIntentFlags.UpdateCurrent);
            AssignSingleAlarm(ref millisNextDay, (AlarmManager)this.GetSystemService(AlarmService), pintent);
        }
        private void AssignSingleAlarm(ref long millis, AlarmManager mngrInst, PendingIntent pi)
        {
            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)//api level 23
            {
                mngrInst.SetExactAndAllowWhileIdle(AlarmType.Rtc, millis, pi);
                return;
            }
            mngrInst.SetExact(AlarmType.Rtc, millis, pi);   
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
                                                .PutExtra(AndroidConstants.TPM_START_EXTRA_KEY, (int)current_tpm.StartTime.TotalMinutes);
                  
                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) //api level 19
                    {
                        to_receiver.PutExtra(AndroidConstants.REQUEST_CODE_EXTRA_KEY, i);
                    }
                    PendingIntent pintent = PendingIntent.GetBroadcast(this, i, to_receiver, (int)Build.VERSION.SdkInt >= 31? PendingIntentFlags.Immutable : PendingIntentFlags.UpdateCurrent);

                    DateTime now = DateTime.Now;     
                    long current_miliseconds = Java.Lang.JavaSystem.CurrentTimeMillis() + (long)(current_tpm.StartTime.TotalMilliseconds - now.TimeOfDay.TotalMilliseconds);
                    
                    if(current_tpm.StartTime.TotalMinutes < now.TimeOfDay.TotalMinutes) //if current wallpaper change time is earlier than current time (round to mins)
                    {
                        current_miliseconds += 86400000; //add a whole day in milliseconds
                    }

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) //api level 19
                    {
                        AssignSingleAlarm(ref current_miliseconds, do_alyarmu, pintent);
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
                    PendingIntent p_i = PendingIntent.GetBroadcast(this, i, new Intent(this, typeof(WPChangeEventReceiver)).SetAction(AndroidConstants.WP_CHANGE_ALARM), (int)Build.VERSION.SdkInt >= 31 ? PendingIntentFlags.Immutable : PendingIntentFlags.UpdateCurrent);
                    if (p_i != null)
                    {
                        do_alyarmu.Cancel(p_i);
                        p_i.Cancel();
                    }
                }
            }
        }

        private void DisplayCountdownByNotification(object sender, EventArgs eeee)
        {
            using (var manager = (NotificationManager)Android.App.Application.Context.GetSystemService(Context.NotificationService))
            {
                for (byte i = 3; i > 0; --i)
                {
                    if (!Exists) { return; } //because what if you kill the service within the 3 seconds given for this method? ;)

                    manager.Notify(2, UxImplementation.GetCountdownNotification(this, i));
                    Thread.Sleep(1000);
                }
                if(this.IsForegroundCurrently)
                {
                    manager.Notify(2, UxImplementation.GetNotificationForWaiting(this));
                }
                else {
                    manager.Cancel(2);
                }
            }
        }

        public override void OnDestroy()
        {
            ClearAlarms();
            if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat)
            {
                BroadcastWasReceived -= AlarmNextDayOnEvent;
            }
            BroadcastWasReceived -= DisplayCountdownByNotification;
            Exists = false;            
            base.OnDestroy();
        }
    }
}