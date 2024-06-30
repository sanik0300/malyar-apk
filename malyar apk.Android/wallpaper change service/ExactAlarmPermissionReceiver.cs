using Android.App;
using Android.Content;
using Android.OS;
using Android.Provider;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace malyar_apk.Droid.wallpaper_change_service
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    [IntentFilter(new string[] { Settings.ActionRequestScheduleExactAlarm })]
    public class ExactAlarmPermissionReceiver : BroadcastReceiver
    {
        public override void OnReceive(Context context, Intent intent)
        {
            Toast.MakeText(context, "Access to exact alarms unblocked! :)", ToastLength.Long).Show();
        }
    }
}