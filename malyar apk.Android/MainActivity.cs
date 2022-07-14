using System;

using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Android.OS;
using System.Text;

namespace malyar_apk.Droid
{
    [Activity(Label = "malyar_apk", Icon = "@mipmap/icon", Theme = "@style/MainTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize )]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        static internal MainActivity Current;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Current = this;

            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            global::Xamarin.Forms.Forms.Init(this, savedInstanceState);
            LoadApplication(new App());

            const string filename = @"/schedule.json";//на случай если захочу название файла менять
            StringBuilder make_Path = new StringBuilder(this.FilesDir.AbsolutePath, this.FilesDir.AbsolutePath.Length+filename.Length);
            make_Path.Append(filename);
            ToolForImages.path_to_schedule = make_Path.ToString();
        }
        /*protected  override void OnStart()
        {
            base.OnStart();
            this.RequestPermissions(new string[] { "android.permission.READ_EXTERNAL_STORAGE" }, 1);

        }*/

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}