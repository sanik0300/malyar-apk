using malyar_apk.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

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
            picker_cell.InitFromSettings(Constants.MISSING_IMG_HANDLE_SETTING);
        }

        private void autosave_switch_OnChanged(object sender, ToggledEventArgs e)
        {
            Preferences.Set(Constants.AUTOSAVE_SETTING, (sender as SwitchCell).On);
        }

        /*private void PickerCell_SelectedIndexChanged(object sender, ValueChangedEventArgs e)
        {
            byte max = (byte)((sender as PickerCell).Items.Count - 1);
            if(e.OldValue < max && e.NewValue == max)
            {
                //показать imagecell
            }
            else if(e.OldValue == max && e.NewValue < max)
            {
                //спрятать imagecell
            }            
        }*/

        private void picker_cell_SelectedIndexChanged(object sender, EventArgs e)
        {
            Preferences.Set(Constants.MISSING_IMG_HANDLE_SETTING, (sender as PickerCell).SelectedIndex);
        }
    }
}