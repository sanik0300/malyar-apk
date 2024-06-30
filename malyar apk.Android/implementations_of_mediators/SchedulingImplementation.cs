using Android.App;
using Android.Content;
using Android.OS;
using System;
using System.Collections.Generic;
using Xamarin.Forms;
using malyar_apk.Shared;
using Android.Graphics;

[assembly: Dependency(typeof(malyar_apk.Droid.SchedulingImplementation))]
namespace malyar_apk.Droid
{
    internal class SchedulingImplementation : ContextDependentObject, ISchedulingMediator
    {
        public void AssignActionsOfChange(IList<TimedPictureModel> tpms)
        {
            if(BaseContext.WPCNN.IsConnected)
            {
                BaseContext.WPCNN._Binder.SendNewScheduleToService(tpms);
            }
            else {
                Intent pack_me_for_waiting_service = new Intent(Android.App.Application.Context, typeof(WaitForWPChangeService))
                                                                                .PutExtra(AndroidConstants.LIST_KEY, IlistToParcelables(tpms)); 
                BaseContext.StartService(pack_me_for_waiting_service);
                BaseContext.BindService(pack_me_for_waiting_service, BaseContext.WPCNN, Bind.AutoCreate);
            }
        }

        public void SetWallpaperConstant(string path_to_wp)
        {
            using (var WM = WallpaperManager.GetInstance(BaseContext))
            {
                WM.SetBitmap(BitmapFactory.DecodeFile(path_to_wp));
            }
        }


        public void CleanChangesSchedule(int num_of_actions_to_remove)
        {
            Intent pack_me_to_end_service = new Intent(Android.App.Application.Context, typeof(WaitForWPChangeService));
            BaseContext.UnbindService(BaseContext.WPCNN);
            BaseContext.StopService(pack_me_to_end_service);      
        }
    }
}