using Android.App;
using Android.Content;
using Android.OS;
using malyar_apk.Shared;
using System.Collections.Generic;

namespace malyar_apk.Droid
{
    internal class MyBinder : Binder
    {
        private WaitForWPChangeService _service;

        public MyBinder(WaitForWPChangeService _service) { this._service = _service; }
       
        public void SendNewScheduleToService(IList<TimedPictureModel> schedule_new)
        {
            _service.ClearAlarms();
            _service.AssignAlarms(ContextDependentObject.IlistToParcelables(schedule_new));
        }

        public void SetServiceToForeground() 
        {
            _service.StartForeground(2, UxImplementation.GetNotificationForWaiting(_service));
        }
        public void SetServiceToBackground() 
        {
            _service.StopForeground(true);

            if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
            {
                using (var manager = (NotificationManager)_service.GetSystemService(Context.NotificationService))
                {
                    manager.DeleteNotificationChannel(AndroidConstants.WP_CHANGE_NOTIF_CHANNEL);
                }
            }
        }
    }

    internal class WPServiceConnection : Java.Lang.Object, IServiceConnection
    {
        public MyBinder _Binder { get; private set; }
        public bool IsConnected { get; private set; }
        public void OnServiceConnected(ComponentName name, IBinder service)
        {
            _Binder = service as MyBinder;
            IsConnected = true;
        }

        public void OnServiceDisconnected(ComponentName name)
        {
            IsConnected = false;
            _Binder = null;
        }
    }
}