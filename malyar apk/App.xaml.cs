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
            DependencyService.Get<IPermitMediator>().FilesReadUnblocked += (s, a) =>
            {
                IOMediator iom = DependencyService.Get<IOMediator>();
                if (!File.Exists(iom.PathToOriginalWP))
                {
                    iom.RememberOriginalWP();
                }
                MessagingCenter.Send(iom, Constants.UpdateImg);
                SchedulePiece.worth_asking_for_file = true;
            };

            InitializeComponent();

            if(Device.RuntimePlatform == Device.Android) {
                MainPage = new NavigationPage(new MainPage()) { BarBackgroundColor = Color.BlueViolet };
            }
            else { new MainPage(); }
        }

        public static bool IsRunning { get; private set; }

        protected override async void OnStart()
        {
            base.OnStart();

            IPermitMediator permit = DependencyService.Get<IPermitMediator>();
            if(!permit.IsPermitted(InvolvedPermissions.StorageRead))
            {
                SchedulePiece.worth_asking_for_file = false;
                permit.AskPermission(InvolvedPermissions.StorageRead);
            }
            if(!permit.IsPermitted(InvolvedPermissions.StorageWrite)) {
                permit.AskPermission(InvolvedPermissions.StorageWrite);
            }
            if(!permit.IsPermitted(InvolvedPermissions.ExactAlarm)) {
                permit.AskPermission(InvolvedPermissions.ExactAlarm);
            }
            IsRunning = true;
        }
    }
}
