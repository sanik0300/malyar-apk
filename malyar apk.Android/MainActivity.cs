using Android.App;
using Android.Hardware.Display;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using System.Collections.Generic;
using Android.Content;
using Xamarin.Forms;
using malyar_apk.Shared;
using Android.Provider;
using Android.Database;
using System.IO;
using Android.Content.Res;
using System;
using Android.Views;

namespace malyar_apk.Droid
{
    [Activity(Name = "com.sanikshomemade.malyar_apk.MainActivity", LaunchMode = LaunchMode.SingleTask, Label = "malyar_apk", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        private Orientation currentOrientation;
        private IdlenessEndReceiver idleness_receiver;
        public bool InForeground { get; private set; }
        private bool StorageReadAllowed;

        internal WPServiceConnection WPCNN = new WPServiceConnection();

    #if __ANDROID_17__
        RotationChangeListener rotationChangeListener;
        private bool IsOrientationChanging;
    #endif

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            ContextDependentObject.InitializeWithContext(this);
            
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());
            StorageReadAllowed = DependencyService.Get<IPermitMediator>().IsPermitted(InvolvedPermissions.StorageRead);

            if (!WaitForWPChangeService.Exists) { return; }
            this.BindService(new Intent(Android.App.Application.Context, typeof(WaitForWPChangeService)), WPCNN, Bind.AutoCreate);

            IOMediator iomem = DependencyService.Get<IOMediator>();
            if (!iomem.WasInitialized || !WaitForWPChangeService.Exists) { return; }
            iomem.BeginLoadingSchedule();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Permission[] grantResults)
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
                intent.PutExtra(AndroidConstants.LIST_KEY, ContextDependentObject.IlistToParcelables(models));
            }

            var PM = (PowerManager)GetSystemService(Context.PowerService);
            if (InForeground && (Build.VERSION.SdkInt >= BuildVersionCodes.KitkatWatch && PM.IsInteractive || Build.VERSION.SdkInt < BuildVersionCodes.KitkatWatch && PM.IsScreenOn))
            {
                this.StartService(intent);
            }
            else {
                idleness_receiver = new IdlenessEndReceiver(intent);
                IntentFilter filter = new IntentFilter(PowerManager.ActionDeviceIdleModeChanged);
                filter.AddAction(Intent.ActionScreenOn);
                filter.AddAction(AndroidConstants.START_IO_LOCAL_ACTION);
                RegisterReceiver(idleness_receiver, filter);
            }
            PM.Dispose();
        }

        private string GetImagePathWithMediaCursor(string documentId)
        {
            string selection = MediaStore.Images.Media.InterfaceConsts.Id + " =? ";
            string result;

            using (ICursor cursor = ContentResolver.Query(MediaStore.Images.Media.ExternalContentUri,
                                                        null, selection,
                                                        new string[] { documentId }, null))
            {
                int columnIndex = cursor.GetColumnIndexOrThrow(MediaStore.Images.Media.InterfaceConsts.Data);
                cursor.MoveToFirst();
                result = cursor.GetString(columnIndex);

                cursor.Close();
            }

            return result;
        }


        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            base.OnActivityResult(requestCode, resultCode, data);
            if (idleness_receiver != null)
            {
                UnregisterReceiver(idleness_receiver);
                idleness_receiver = null;
            }
            
            var iom = DependencyService.Get<IOMediator>() as IO_Implementation;
            switch (requestCode)
            {
                case AndroidConstants.FILEPICKER_RESULT_REQ_CODE:

                    if (resultCode != Result.Ok) { return; }

                    if (data.Data.Host == "com.google.android.apps.docs.storage") //if taking file from google drive
                    {
                        string path_to_dir = this.GetExternalFilesDir(null).AbsolutePath + "/from_drive/";

                        if (!Directory.Exists(path_to_dir)) { Directory.CreateDirectory(path_to_dir); }

                        string path_of_saved_img = path_to_dir + data.Data.LastPathSegment;

                        if (!System.IO.File.Exists(path_of_saved_img))
                        {
                            try {
                                using (Stream uri_str = ContentResolver.OpenInputStream(data.Data))
                                {
                                    using (FileStream fs = new FileStream(path_of_saved_img, FileMode.OpenOrCreate, FileAccess.Write, FileShare.Read))
                                    {
                                        uri_str.CopyTo(fs);
                                    }
                                }
                            }
                            catch (Java.IO.FileNotFoundException)
                            {
                                ErrorDialogFragment edf = new ErrorDialogFragment(this);
                                edf.Show(this.FragmentManager, edf.Tag);

                                return;
                            }
                        }
                        iom.OnFilePathDelivered(path_of_saved_img);
                    }
                    else { //not checking host anymore because host names for gallery, explorer and so on depend on wrapper name

                        switch (data.Data.Path.Substring(1, 3))
                        {
                            case "raw": //if taking img file from gallery
                                iom.OnFilePathDelivered(data.Data.Path.Substring(5));
                                break;
                            case "ext": //from file explorer
                                iom.OnFilePathDelivered(data.Data.Path.Replace("external_files", "storage/emulated/0"));
                                break;
                            case "doc":
                                string path_to_output;

                                if (data.Data.Path.Contains("/pr"))//URI is like /document/primary:some_dir/some_file
                                {
                                    using (ICursor cursor = ContentResolver.Query(data.Data,
                                                                                    new string[1] { MediaStore.IMediaColumns.DocumentId },
                                                                                    null, null, null))
                                    {
                                        cursor.MoveToFirst();
                                        path_to_output = cursor.GetString(0).Replace("primary:", "/storage/emulated/0/");

                                        cursor.Close();
                                    }
                                }
                                else { //URI is like /document:12345678
                                    path_to_output = GetImagePathWithMediaCursor(data.Data.Path.Split(':')[1]);
                                }
                                iom.OnFilePathDelivered(path_to_output);
                                break;
                            case "pic": //image picker from android 30+, uri ends with a big number like in the previous case
                                int last_slash_index = data.Data.Path.LastIndexOf('/');
                                iom.OnFilePathDelivered(GetImagePathWithMediaCursor(data.Data.Path.Substring(last_slash_index + 1)));
                                break;
                            default:
                                iom.OnFilePathDelivered(data.Data.Path); // no SAF
                                break;
                        }                    
                    }
                    break;

                case AndroidConstants.TaskCode_Load:
                    var parcelables = data.GetParcelableArrayExtra(AndroidConstants.RESULT_DESERIALIZED);

                    List<TimedPictureModel> future_schedule = null;
                    
                    if (parcelables != null)
                    {
                        future_schedule = ContextDependentObject.ParcelablesToList(parcelables);
                    }
                    iom.OnScheduleAdded(future_schedule);
                    break;

                case AndroidConstants.TaskCode_Save:
                    iom.OnScheduleSaved();
                    break;
            }
        }

        private void recreateDialogIfExists()
        {
            AndroidX.Fragment.App.Fragment f = SupportFragmentManager.FindFragmentByTag(AndroidConstants.WP_PREVIEW_DIALOG_TAG);
            if (f != null)
            {
                SupportFragmentManager.BeginTransaction().Detach(f).Commit();
                SupportFragmentManager.BeginTransaction().Attach(f).Commit();
            }
        }

        public override void OnConfigurationChanged(Configuration newConfig)
        {
            base.OnConfigurationChanged(newConfig);
            if(this.currentOrientation != newConfig.Orientation)
            {
                if ((int)Build.VERSION.SdkInt >= 17)
                {
                    IsOrientationChanging = true;
                }
                recreateDialogIfExists();
            }
            this.currentOrientation = newConfig.Orientation;
        }

        protected override void OnPause()
        {
            base.OnPause();
            InForeground = false;
            
            if ((int)Build.VERSION.SdkInt >= 17)
            {
                DisplayManager dsm = (DisplayManager)this.GetSystemService(DisplayService);
                dsm.UnregisterDisplayListener(rotationChangeListener);
            }
        }

        protected override void OnResume()
        {    
            base.OnResume();

            if ((int)Build.VERSION.SdkInt >= 17)
            {
                DisplayManager dsm = (DisplayManager)this.GetSystemService(DisplayService);
                if(rotationChangeListener==null)
                {
                    rotationChangeListener = new RotationChangeListener(recreateDialogIfExists, 
                                                                        () => IsOrientationChanging, 
                                                                        (b) => IsOrientationChanging = b) ;
                }
                dsm.RegisterDisplayListener(rotationChangeListener, null);
            }
            if (!StorageReadAllowed)
            {
                IPermitMediator permit = DependencyService.Get<IPermitMediator>();
                if(!permit.IsPermitted(InvolvedPermissions.StorageRead)) { return; }

                permit.OnFilesReadUnblocked();
                StorageReadAllowed = true;
            }

            InForeground = true;
            if (idleness_receiver == null)
                return;
            this.SendBroadcast(new Intent(AndroidConstants.START_IO_LOCAL_ACTION));
        }

        protected override void OnStop()
        {
            base.OnStop();

            if (!WPCNN.IsConnected) { return; }
            WPCNN._Binder.SetServiceToForeground();
        }

        protected override void OnStart()
        {
            base.OnStart();
            currentOrientation = Resources.Configuration.Orientation;

            (DependencyService.Get<IOMediator>() as IO_Implementation).OnUpdateWhichFilesExist();
            
            if (!WPCNN.IsConnected) { return; }
            WPCNN._Binder.SetServiceToBackground();
        }

        protected override void OnDestroy()
        {
            if(WPCNN.IsConnected) { this.UnbindService(WPCNN); }

            base.OnDestroy();
        }
    }
}