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
using Android.Graphics;

[assembly: Dependency(typeof(malyar_apk.Droid.UxImplementation))]
namespace malyar_apk.Droid
{
    class UxImplementation : ContextDependentObject, IUXMediator
    {
        private static bool save_notif_channel_exists = false;
        public void DenoteSuccesfulSave(float[] percentages)
        {
            var remote_view = new RemoteViews(Xamarin.Essentials.AppInfo.PackageName, Resource.Layout.CustomSaveNotification);
            remote_view.SetTextViewText(Resource.Id.notif_headline, $"Сохранено обоев: {percentages.Length} шт");
            remote_view.SetInt(Resource.Id.gradient_container, "setBackgroundResource", (Build.VERSION.SdkInt >= BuildVersionCodes.N) ? Resource.Drawable.sky_gradient_24_andabove : Resource.Drawable.sky_gradient);
            remote_view.SetTextViewText(Resource.Id.time_here, DateTime.Now.ToString("HH:mm"));

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
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)//api level 26+
                {
                    if (!save_notif_channel_exists)
                    {
                        NotificationChannel channel = new NotificationChannel(AndroidConstants.SCHEDULE_SAVED_NOTIF_CHANNEL,
                                                                                AndroidConstants.SCHEDULE_SAVED_NOTIF_CHANNEL, NotificationImportance.High);
                        manager.CreateNotificationChannel(channel);
                        save_notif_channel_exists = true;
                    } 
                    builder = new NotificationCompat.Builder(BaseContext, AndroidConstants.SCHEDULE_SAVED_NOTIF_CHANNEL);
                }
                else { //api level 25-
                    builder = new NotificationCompat.Builder(BaseContext).SetPriority(1);//Priority High
                }
                builder = builder.SetContent(remote_view).SetSmallIcon(Resource.Drawable.save_small_icon);

                manager.Notify(1, builder.Build());
                builder.Dispose();
            }
        }

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

        static private NotificationCompat.Builder get_minimun_notif_builder(Context _cntx, string _channel_id)
        {
            NotificationCompat.Builder _builder;
            using (var manager = (NotificationManager)_cntx.GetSystemService(Context.NotificationService))
            {
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O) //api level 26+
                {
                    NotificationChannel channel = new NotificationChannel(_channel_id, _channel_id, NotificationImportance.Low);
                    manager.CreateNotificationChannel(channel);

                    _builder = new NotificationCompat.Builder(_cntx, _channel_id);
                }
                else
                { //api level 25-
                    _builder = new NotificationCompat.Builder(_cntx).SetPriority(-1); //Priority Low
                }
            }
            return _builder;
        }

        static internal Notification GetNotificationForWaiting(Context cntx)
        {
            NotificationCompat.Builder builder = get_minimun_notif_builder(cntx, AndroidConstants.WP_CHANGE_NOTIF_CHANNEL);

            Notification result = builder.SetSmallIcon(Resource.Drawable.notification_paint_roller)
                                            .SetContentTitle("ждём следующей смены обоев")
                                            .SetStyle(new NotificationCompat.BigTextStyle().BigText("Без этого уведомления не будет работать служба ожидания нужного времени, когда менять обои. Не отключайте этому приложению уведомления."))
                                            .Build();
            
            builder.Dispose();
            return result;
        }

        static internal Notification GetCountdownNotification(Context cntx, byte sec_left) 
        {
            NotificationCompat.Builder builder = get_minimun_notif_builder(cntx, AndroidConstants.WP_CHANGE_NOTIF_CHANNEL);

            Notification result = builder.SetSmallIcon(Resource.Drawable.notification_paint_roller)
                                            .SetContentTitle($"Обои сменятся через {sec_left}...")
                                            .SetLargeIcon(BitmapFactory.DecodeResource(cntx.Resources, Resource.Drawable.notification_paint_roller))
                                            .Build();
            builder.Dispose();
            return result;
        }
    }
}