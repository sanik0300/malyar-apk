using Android;
using Android.App;
using Android.Net;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Android.Provider;
using System;
using Xamarin.Forms;

[assembly: Dependency(typeof(malyar_apk.Droid.PermitImplementation))]
namespace malyar_apk.Droid
{
    internal class PermitImplementation : ContextDependentObject, IPermitMediator 
    {
        private static string[] OldStoragePermissions = new string[2] { Manifest.Permission.ReadExternalStorage, Manifest.Permission.WriteExternalStorage };
        private static string[] ModernStoragePermissions = new string[2] { Manifest.Permission.ReadMediaImages, Manifest.Permission.ManageExternalStorage };

        public event EventHandler FilesReadUnblocked;
        public void OnFilesReadUnblocked() => FilesReadUnblocked?.Invoke(this, EventArgs.Empty);
        public void AskPermission(InvolvedPermissions perm)
        {
            switch (perm)
            {
                case InvolvedPermissions.ExactAlarm:
                    Intent ask_for_permission = new Intent(Settings.ActionRequestScheduleExactAlarm);
                    BaseContext.StartActivity(ask_for_permission);
                    break;
                default: {
                        string readPerm = (int)Build.VERSION.SdkInt >= 33 ? ModernStoragePermissions[(int)perm] : OldStoragePermissions[(int)perm];

                        if ((int)Build.VERSION.SdkInt >= 23 && BaseContext.ShouldShowRequestPermissionRationale(readPerm))
                        {
                            Intent ask_for_perm = new Intent(Settings.ActionApplicationDetailsSettings, 
                                                             Android.Net.Uri.FromParts("package", BaseContext.PackageName, null))
                                                  .AddFlags(ActivityFlags.NewTask);
                            BaseContext.StartActivity(ask_for_perm);
                        }
                        else {
                            BaseContext.RequestPermissions(new string[] { readPerm }, 9);
                        }   
                    }
                    break;
            }
        }

        public bool IsPermitted(InvolvedPermissions perm)
        {
            switch (perm)
            {
                case InvolvedPermissions.ExactAlarm:
                    if ((int)Build.VERSION.SdkInt >= 31)
                    {
                        return ((AlarmManager)BaseContext.GetSystemService(Context.AlarmService)).CanScheduleExactAlarms();
                    }
                    else { return true; }            
                default:
                    return BaseContext.CheckSelfPermission((int)Build.VERSION.SdkInt >= 33? ModernStoragePermissions[(int)perm] 
                                                                                          : OldStoragePermissions[(int)perm]) == Permission.Granted;
            }
        }
    }
}