using Android.Content;
using Android.OS;
using Android.Widget;
using Xamarin.Forms;
using Android.Support.Design.Widget;
using Xamarin.Forms.Platform.Android;
using Android.Support.V4.App;
using Android.App;
using System.Text;
using System;
using malyar_apk.Shared;
using Android.Content.PM;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;

[assembly: Dependency(typeof(malyar_apk.Droid.UxImplementation))]
namespace malyar_apk.Droid
{
    class UxImplementation : ContextDependentObject, IUXMediator
    {
        public async void DenoteSuccesfulSave(float[] percentages)
        {
            var remote_view = new RemoteViews(Xamarin.Essentials.AppInfo.PackageName, Resource.Layout.CustomSaveNotification);
            remote_view.SetTextViewText(Resource.Id.notif_headline, $"Сохранено обоев: {percentages.Length} шт");
            remote_view.SetInt(Resource.Id.gradient_container, "setBackgroundResource", (Build.VERSION.SdkInt >= BuildVersionCodes.N) ? Resource.Drawable.sky_gradient_24_andabove : Resource.Drawable.sky_gradient);
            remote_view.SetTextViewText(Resource.Id.time_here, DateTime.Now.ToString(Constants.TimeFormat));

            var activity_name_source = BaseContext.PackageManager.GetActivityInfo(BaseContext.ComponentName, PackageInfoFlags.Activities);
            remote_view.SetTextViewText(Resource.Id.appname_here, activity_name_source.NonLocalizedLabel);

            if (percentages.Length > 1)
            {
                float one_letter_width = 24 / 1.3f; //эх подставлять бы сюда вместо 24 размер текста как-то динамически....
                float num_of_steps = 454 / one_letter_width; //478dp of notification width - 24dp padding on both sides
                StringBuilder sb = new StringBuilder((int)num_of_steps);
                for (int i = 0; i < percentages.Length - 1; ++i)
                {
                    int num_of_steps_this_part = (int)(num_of_steps * percentages[i]);

                    for (int k = 0; k < num_of_steps_this_part - 1; ++k)
                    {
                        sb.Append(' ');
                    }
                    sb.Append('|');
                }
                remote_view.SetTextViewText(Resource.Id.gradient_container, sb.ToString());
            }

            NotificationCompat.Builder builder;
            using (var manager = (NotificationManager)BaseContext.GetSystemService(Context.NotificationService))
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    string CHANNELLID = "saves_here";

                    NotificationChannel channel = new NotificationChannel(CHANNELLID, CHANNELLID, NotificationImportance.High);
                    manager.CreateNotificationChannel(channel);

                    builder = new NotificationCompat.Builder(BaseContext, CHANNELLID);
                }
                else {
                    builder = new NotificationCompat.Builder(BaseContext);
                }

                builder = builder.SetContent(remote_view).SetSmallIcon(Resource.Drawable.save_small_icon);

                manager.Notify(1, builder.Build());

                await Task.Delay(2000);
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    manager.DeleteNotificationChannel("saves_here");
                }

                builder.Dispose();
            }
        }

        /*public bool BackgroundWorkAlreadyPrepared()
        {
            using (PackageManager pm = BaseContext.PackageManager) 
            {
               IList<ResolveInfo> infos = pm.QueryBroadcastReceivers(new Intent(Android.App.Application.Context, typeof(AlarmReceiver)).SetAction(AndroidConstants.WP_CHANGE_ALARM),
                                            PackageInfoFlags.Receivers);
                return infos.Count > 0;
            }
        }
        public void InitializeBackgroundWork()
        {
            Android.App.Application.Context.RegisterReceiver(new AlarmReceiver(), new IntentFilter(AndroidConstants.WP_CHANGE_ALARM));
        }*/

        public void DeliverToast(string text)
        {
            Toast.MakeText(BaseContext, text, ToastLength.Short).Show();
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
            
            Toast.MakeText(BaseContext, description, ToastLength.Short).Show();          
        }

        static internal Notification GetNotificationForWaiting(Context cntx)
        {
            NotificationCompat.Builder builder;
            using (var manager = (NotificationManager)cntx.GetSystemService(Context.NotificationService))
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    NotificationChannel channel = new NotificationChannel(AndroidConstants.WP_CHANGE_NOTIF_CHANNEL,
                                                                            AndroidConstants.WP_CHANGE_NOTIF_CHANNEL,
                                                                            NotificationImportance.Low);
                    manager.CreateNotificationChannel(channel);

                    builder = new NotificationCompat.Builder(cntx, AndroidConstants.WP_CHANGE_NOTIF_CHANNEL);
                }
                else {
                    builder = new NotificationCompat.Builder(cntx);
                }
            }
            Notification result = builder.SetSmallIcon(Resource.Drawable.notification_paint_roller).SetContentTitle("ждём смены обоев").Build();
            builder.Dispose();
            return result;
        }
    }
}