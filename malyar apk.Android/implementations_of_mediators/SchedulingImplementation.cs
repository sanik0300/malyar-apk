using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;
using malyar_apk;
using malyar_apk.Shared;
using Android.Graphics;

[assembly: Dependency(typeof(malyar_apk.Droid.SchedulingImplementation))]
namespace malyar_apk.Droid
{
    internal class SchedulingImplementation : ContextDependentObject, ISchedulingMediator
    {
        public void AssignActionsOfChange(IList<TimedPictureModel> tpms)
        {
            using (var do_alyarmu = (AlarmManager)BaseContext.GetSystemService(Context.AlarmService))
            {
                for (int i = 0; i < tpms.Count; ++i)
                {
                    Intent to_receiver = new Intent(Android.App.Application.Context, typeof(AlarmReceiver)).SetAction(AndroidConstants.WP_CHANGE_ALARM)
                                                .PutExtra(AndroidConstants.FILEPATH_EXTRA_KEY, tpms[i].path_to_wallpaper)
                                                .PutExtra(Intent.ExtraIndex, i)
                                                .PutExtra(AndroidConstants.TPM_START_EXTRA_KEY, (int)tpms[i].StartTime.TotalMinutes);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) //api level 19
                    {
                        to_receiver.PutExtra(AndroidConstants.REQUEST_CODE_EXTRA_KEY, i);
                    }
                    PendingIntent pintent = FormP_IntentForReceiver(i, to_receiver);

                    long current_miliseconds = Java.Lang.JavaSystem.CurrentTimeMillis() + (long)(tpms[i].StartTime.TotalMilliseconds - DateTime.Now.TimeOfDay.TotalMilliseconds);

                    if (Build.VERSION.SdkInt >= BuildVersionCodes.M)//api level 23
                    {
                        do_alyarmu.SetExactAndAllowWhileIdle(AlarmType.Rtc, current_miliseconds, pintent);
                    }
                    else if (Build.VERSION.SdkInt >= BuildVersionCodes.Kitkat) //api level 19
                    {
                        do_alyarmu.SetExact(AlarmType.Rtc, current_miliseconds, pintent);
                    }
                    else
                    {
                        do_alyarmu.SetRepeating(AlarmType.Rtc, current_miliseconds, AlarmManager.IntervalDay, pintent);
                    }
                }
            }           
        }

        public void SetWallpaperConstant(TimedPictureModel tpm)
        {
            using (var WM = WallpaperManager.GetInstance(BaseContext))
            {
                WM.SetBitmap(BitmapFactory.DecodeFile(tpm.path_to_wallpaper));
            }
        }


        public void CleanChangesSchedule(int num_of_actions_to_remove)
        {
            using (var do_alyarmu = (AlarmManager)BaseContext.GetSystemService(Context.AlarmService))
            {
                for (int i = 0; i < num_of_actions_to_remove; ++i)
                {
                    PendingIntent p_i = FormP_IntentForReceiver(i);
                    if (p_i != null)
                    {
                        do_alyarmu.Cancel(p_i);
                        p_i.Cancel();
                    }
                }
            }               
        }

        static internal PendingIntent FormP_IntentForReceiver(int requestcode, Intent inner_intent=null)
        {
            if (inner_intent == null)
            {
                inner_intent = new Intent(Android.App.Application.Context, typeof(AlarmReceiver)).SetAction(AndroidConstants.WP_CHANGE_ALARM);
            }
            else {
                if (inner_intent.Action != AndroidConstants.WP_CHANGE_ALARM) {
                    throw new ArgumentException($"В передаваемом намерении Action должно быть только {nameof(AndroidConstants.WP_CHANGE_ALARM)}");
                }

                if (!inner_intent.Component.ClassName.EndsWith(".AlarmReceiver")) {
                    throw new ArgumentException($"Компонентом передаваемого намерения должен быть только {nameof(AlarmReceiver)}");
                }
            }        
            return PendingIntent.GetBroadcast(Android.App.Application.Context, requestcode, inner_intent, PendingIntentFlags.UpdateCurrent);
        }
    }
}