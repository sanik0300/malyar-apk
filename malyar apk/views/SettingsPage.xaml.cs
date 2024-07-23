using malyar_apk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace malyar_apk.Shared
{
    public enum NoSuchImgHandling : byte { IgnoreAndWaitNext, PutNext, PutDefault }
}

namespace malyar_apk
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SettingsPage : ContentPage
    {
        private double width = 0, height = 0;
        public SettingsPage()
        {
            InitializeComponent();

            if(DependencyService.Get<I_UIMediator>().IsSystemDarkTheme()!=null)
            {
                appearanceSection.Remove(theme_switch);
                theme_setting_picker.SelectedIndexChanged += theme_setting_Changed;
                theme_setting_picker.InitFromSettings(Constants.THEME_KEY);
            }
            else {
                appearanceSection.Remove(theme_setting_picker);
                theme_switch.On = Preferences.Get(Constants.THEME_KEY, 1)==2;
                theme_switch.OnChanged += theme_switch_OnChanged;
            }

            autosave_switch.On = Preferences.Get(Constants.AUTOSAVE_SETTING, true);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (DependencyService.Get<I_UIMediator>().IsSystemDarkTheme() == null)
            {
                theme_switch.Text = theme_switch.On ? "Тема: тёмная" : "Тема: светлая";
            }
            no_wp_setting_picker.InitFromSettings(Constants.MISSING_IMG_HANDLING);
            hrz_dialog_setting_picker.InitFromSettings(Constants.HRZ_WP_DIALOG_MODE);
        }

        private void autosave_switch_OnChanged(object sender, ToggledEventArgs e)
        {
            Preferences.Set(Constants.AUTOSAVE_SETTING, (sender as SwitchCell).On);
        }

        private void NoWP_SettingChanged(object sender, EventArgs e)
        {
            Preferences.Set(Constants.MISSING_IMG_HANDLING, (sender as PickerCell).SelectedIndex);
        }

        private void HrzWP_DialogSettingChanged(object sender, EventArgs e)
        {
            Preferences.Set(Constants.HRZ_WP_DIALOG_MODE, (sender as PickerCell).SelectedIndex);
        }

        private void stop_changing_Clicked(object sender, EventArgs e)
        {
            DependencyService.Get<ISchedulingMediator>().CleanChangesSchedule(TimedPicturesLoader.prev_schedule_count);
        }

        private void theme_switch_OnChanged(object sender, ToggledEventArgs e)
        {
            bool switchOn = (sender as SwitchCell).On;
            int prevOption = Preferences.Get(Constants.THEME_KEY, 1),
                optionNow = switchOn ? 2 : 1;
            if(prevOption == optionNow) { return; }

            Preferences.Set(Constants.THEME_KEY, optionNow);
            (sender as SwitchCell).Text = switchOn ? "Тема: тёмная" : "Тема: светлая";

            DependencyService.Get<I_UIMediator>().ConfirmThemeChangeOnPlatform(optionNow);
            (App.Current as App).SwitchAppTheme(switchOn);
        }

        private void theme_setting_Changed(object sender, EventArgs e)
        {
            int prevOption = Preferences.Get(Constants.THEME_KEY, 0),
                optionNow =(sender as PickerCell).SelectedIndex;
            if (prevOption == optionNow) { return; }

            Preferences.Set(Constants.THEME_KEY, optionNow);
            I_UIMediator uimedi = DependencyService.Get<I_UIMediator>();
            App crtapp = App.Current as App;
            if (optionNow == 0)
            {
                bool systemdark = uimedi.IsSystemDarkTheme().Value;
                int themeId = 1 + Convert.ToInt32(systemdark);
                if(themeId == (int)crtapp.UserAppTheme) { return; }

                uimedi.ConfirmThemeChangeOnPlatform(optionNow);
                crtapp.SwitchAppTheme(systemdark);
            }
            else {
                uimedi.ConfirmThemeChangeOnPlatform(optionNow);
                crtapp.SwitchAppTheme(optionNow==2);
            }
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            if (this.width == width && this.height == height) { return; }
            bool horizontal = width > height;

            base.OnSizeAllocated(width, height);

            double marginTopPart = height/2;
            stopChangingBtn.Margin = new Thickness(5, marginTopPart, 5, 10);
        }
    }
}