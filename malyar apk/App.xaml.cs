using System;
using Xamarin.Forms;
using Xamarin.Essentials;
using Xamarin.Forms.Xaml;
using malyar_apk.Shared;
using System.IO;
using System.Threading.Tasks;
using System.Reflection;

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

            I_UIMediator uim = DependencyService.Get<I_UIMediator>();
            bool? themeIndicator = uim.IsSystemDarkTheme();
            int option = Preferences.Get(Constants.THEME_KEY, themeIndicator == null ? 1 : 0);
            if (option == 0)
            {
                (App.Current as App).SwitchAppTheme(themeIndicator.Value);
            }
            else {
                uim.ConfirmThemeChangeOnPlatform(option);
                (App.Current as App).SwitchAppTheme(option == 2);
            }
            /*since confirm...onplatform is needed to enforce right pair of styles.xml + colors.xml,
             * and option==0 is basically to depend on system theme
             * then in case of option 0, styles and colors are chosen automatically here at launch
             * so no need to call said method in the "if" part
             * "else" part is for when theme should be constant and/or os version doesnt have themes
             */

            if (Device.RuntimePlatform == Device.Android) {
                MainPage = new NavigationPage(new MainPage());
            }
            else { MainPage = new MainPage(); }
        }

        public event EventHandler UserAppThemeChanged;
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

        public void SwitchAppTheme(bool dark)
        {
            (App.Current as App).UserAppTheme = dark ? OSAppTheme.Dark : OSAppTheme.Light;
            UserAppThemeChanged?.Invoke(this, EventArgs.Empty);
        }
    }
}
