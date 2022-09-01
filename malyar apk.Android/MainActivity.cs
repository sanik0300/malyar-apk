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
    [Activity(Name="com.sanikshomemade.malyar_apk.MainActivity", LaunchMode = LaunchMode.SingleTask, Label = "malyar_apk", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        static public MainActivity Current { get; private set; }
        private IdlenessEndReceiver idleness_receiver;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Current = this;

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            const string filename = "schedule.json";//на случай если захочу название файла менять
            ToolForImages.path_to_schedule = Path.Combine(this.GetExternalFilesDir(null).AbsolutePath, filename);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }


        public void StartIO(string filepath, bool save, IList<TimedPictureModel> models = null)
        {
            PendingIntent pi = this.CreatePendingResult(save ? AndroidConstants.TaskCode_Save : AndroidConstants.TaskCode_Load, new Intent(), 0);
            Intent intent = new Intent(this, typeof(IO_Service)).PutExtra(AndroidConstants.PARAM_PINTENT, pi)
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
            if (Build.VERSION.SdkInt >= BuildVersionCodes.KitkatWatch && PM.IsInteractive || Build.VERSION.SdkInt < BuildVersionCodes.KitkatWatch && PM.IsScreenOn)
            {
                this.StartService(intent);
            }
            else
            {
                idleness_receiver = new IdlenessEndReceiver(intent);
                IntentFilter filter = new IntentFilter(PowerManager.ActionDeviceIdleModeChanged);
                filter.AddAction(Intent.ActionScreenOn);
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
            
            ToolForImages tfi = DependencyService.Get<IMagesMediator>() as ToolForImages; //so far i haven't found another way to get global instance of toolforimages
            switch (requestCode)
            {
                case AndroidConstants.TaskCode_Load:
                    var parcelables = data.GetParcelableArrayExtra(AndroidConstants.RESULT_DESERIALIZED);
                    List<TimedPictureModel> future_schedule = null;
                    if (parcelables != null)
                    {
                        future_schedule = new List<TimedPictureModel>(parcelables.Length);
                        foreach (IParcelable p in parcelables)
                        {
                            future_schedule.Add((p as TPModelParcelable).source);
                        }
                    }
       
                    tfi.OnScheduleAdded(future_schedule);
                    break;
                case AndroidConstants.TaskCode_Save:
                    tfi.OnScheduleSaved();
                    break;
            }
        }
    }
}