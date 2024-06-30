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

namespace malyar_apk.Droid
{
    [BroadcastReceiver(Enabled = true, Exported = false)]
    public class IdlenessEndReceiver : BroadcastReceiver
    {
        private readonly Intent PostponedIntent;
        public IdlenessEndReceiver() { }
        public IdlenessEndReceiver(Intent postponed)
        {
            PostponedIntent = postponed;
        }

        public override void OnReceive(Context context, Intent intent)
        {
            if (!(context as MainActivity).InForeground)
                return;
            //because let's avoid that exception "not allowed to staer service"
            context.StartService(PostponedIntent);
        }
    }
}