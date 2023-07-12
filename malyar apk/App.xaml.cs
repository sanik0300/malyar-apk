using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;
using malyar_apk.Shared;
using System.IO;
using System.Threading.Tasks;

namespace malyar_apk
{
    
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            if(Device.RuntimePlatform == Device.Android) {
                MainPage = new NavigationPage(new MainPage()) { BarBackgroundColor = Color.BlueViolet };
            }
            else { new MainPage(); }
        }

        protected override async void OnStart()
        {
            if (Preferences.Get(Constants.FIRST_LAUNCH_KEY, true))
            {
                IUXMediator ux_thing = DependencyService.Get<IUXMediator>();
                if (!ux_thing.BackgroundWorkAlreadyPrepared())
                {
                    ux_thing.InitializeBackgroundWork();
                }
            }

            if (await Permissions.CheckStatusAsync<Permissions.StorageRead>() != PermissionStatus.Granted)
            {
                SchedulePiece.worth_asking_for_file = false;
                await Permissions.RequestAsync<Permissions.StorageRead>();
                var status2 = await Permissions.CheckStatusAsync<Permissions.StorageRead>();
                IOMediator io_thing = DependencyService.Get<IOMediator>();
                if (status2 == PermissionStatus.Granted)
                {
                    if (!File.Exists(io_thing.PathToOriginalWP))
                    {
                        io_thing.RememberOriginalWP();
                    }
                    MessagingCenter.Send(io_thing, Constants.UpdateImg);
                    SchedulePiece.worth_asking_for_file = true;
                }
            }
            
            if(await Permissions.CheckStatusAsync<Permissions.StorageWrite>() != PermissionStatus.Granted)
            {
                await Permissions.RequestAsync<Permissions.StorageWrite>();
            }
            Preferences.Set(Constants.FIRST_LAUNCH_KEY, false);
        }
    }
}
