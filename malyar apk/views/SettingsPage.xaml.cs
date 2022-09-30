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
        public SettingsPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            autosave_switch.On = Preferences.Get(Constants.AUTOSAVE_SETTING, true);
            picker_cell.InitFromSettings(Constants.MISSING_IMG_HANDLING);
        }

        private void autosave_switch_OnChanged(object sender, ToggledEventArgs e)
        {
            Preferences.Set(Constants.AUTOSAVE_SETTING, (sender as SwitchCell).On);
        }

        private void picker_cell_SelectedIndexChanged(object sender, EventArgs e)
        {
            Preferences.Set(Constants.MISSING_IMG_HANDLING, (sender as PickerCell).SelectedIndex);
        }

        private void stop_changing_Clicked(object sender, EventArgs e)
        {
            DependencyService.Get<ISchedulingMediator>().CleanChangesSchedule(TimedPicturesLoader.prev_schedule_count);
        }
    }
}