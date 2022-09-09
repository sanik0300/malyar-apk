using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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

        protected override void OnStart()
        {

        }

        protected override void OnSleep()
        {
        }

        protected override void OnResume()
        {
        }
    }
}
