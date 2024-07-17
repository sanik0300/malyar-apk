using malyar_apk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
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
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            autosave_switch.On = Preferences.Get(Constants.AUTOSAVE_SETTING, true);

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

        protected override void OnSizeAllocated(double width, double height)
        {
            if (this.width == width && this.height == height) { return; }
            bool horizontal = width > height;

            base.OnSizeAllocated(width, height);

            stopChangingBtn.Margin = horizontal ? new Thickness(5, Math.Max(100, height-stopChangingBtn.Height), 5, 5) : 40;


            if (horizontal)
            {
                settingsLayout.Children.Remove(stopChangingBtn);

                if (tableRoot.Count < 3)
                {
                    TableSection lastSection = new TableSection();
                    ViewCell cellForBtn = new ViewCell();
                    cellForBtn.View = stopChangingBtn;
                    lastSection.Add(cellForBtn);
                    tableRoot.Add(lastSection);
                }
            }
            else {
                settingsLayout.Children.Add(stopChangingBtn);

                if (tableRoot.Count > 2)
                {
                    tableRoot.RemoveAt(tableRoot.Count - 1);
                }
            }
        }
    }
}