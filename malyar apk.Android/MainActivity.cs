using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using System.Collections.Generic;
using Android.Content;
using Xamarin.Forms;
using malyar_apk.Shared;
using System.IO;

namespace malyar_apk.Droid
{

    [Activity(Name = "com.sanikshomemade.malyar_apk.MainActivity", LaunchMode = LaunchMode.SingleTask, Label = "malyar_apk", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private IdlenessEndReceiver idleness_receiver;
        public bool InForeground { get; private set; }
        
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ContextDependentObject.BaseContext = this;
            this.InForeground = false;

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            
            ContextDependentObject.path_to_schedule = IO_Implementation.ConvertFilenameToFilepath("schedule.json");
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
 
        public void StartIO(string filepath, bool save, IList<TimedPictureModel> models = null)
        {
            PendingIntent pi = this.CreatePendingResult(save ? AndroidConstants.TaskCode_Save : AndroidConstants.TaskCode_Load, new Intent(), 0);
            Intent intent = new Intent(this, typeof(IO_Service)).PutExtra(Intent.ExtraIntent, pi)
                                                                .PutExtra(AndroidConstants.IO_SAVE_KEY, save)
                                                                .PutExtra(AndroidConstants.FILEPATH_EXTRA_KEY, filepath);

            if (save)
            {
                var almost_parcels = new TPModelParcelable[models.Count];
                for (int i = 0; i < models.Count; ++i)
                {
                    almost_parcels[i] = new TPModelParcelable(models[i]);
                }
                intent.PutExtra(AndroidConstants.LIST_KEY, almost_parcels);
            }

            var PM = (PowerManager)GetSystemService(Context.PowerService);
            if (InForeground && (Build.VERSION.SdkInt >= BuildVersionCodes.KitkatWatch && PM.IsInteractive || Build.VERSION.SdkInt < BuildVersionCodes.KitkatWatch && PM.IsScreenOn))
            {
                this.StartService(intent);
            }
            else
            {
                idleness_receiver = new IdlenessEndReceiver(intent);
                IntentFilter filter = new IntentFilter(PowerManager.ActionDeviceIdleModeChanged);
                filter.AddAction(Intent.ActionScreenOn);
                filter.AddAction(AndroidConstants.START_IO_LOCAL_ACTION);
                RegisterReceiver(idleness_receiver, filter);
            }       
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if(idleness_receiver != null)
            {
                UnregisterReceiver(idleness_receiver);
                idleness_receiver = null;
            }
            
            var iom = DependencyService.Get<IOMediator>() as IO_Implementation; //so far i haven't found another way to get global instance of UxImplementation
            switch (requestCode)
            {
                case AndroidConstants.TaskCode_Load:
                    var parcelables = data.GetParcelableArrayExtra(AndroidConstants.RESULT_DESERIALIZED);
                    //bool origs_present = resultCode==Result.Canceled;
                    List<TimedPictureModel> future_schedule = null;
                    
                    if (parcelables != null)
                    {
                        future_schedule = new List<TimedPictureModel>(parcelables.Length);
                        foreach (IParcelable p in parcelables)
                        {
                            future_schedule.Add((p as TPModelParcelable).source);
                            //if (origs_present) { continue; }
                            //origs_present = tpm.path_to_wallpaper == Constants.just_original_keyword;
                        }
                    }
       
                    iom.OnScheduleAdded(future_schedule);
                    break;
                case AndroidConstants.TaskCode_Save:
                    iom.OnScheduleSaved();
                    break;
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
            InForeground = false;
        }

        protected override void OnResume()
        {    
            base.OnResume();
            InForeground = true;
            if (idleness_receiver == null)
                return;
            this.SendBroadcast(new Intent(AndroidConstants.START_IO_LOCAL_ACTION));
        }

        protected override void OnStop()
        {
            base.OnStop();
            InForeground = false;
        }

    }
}