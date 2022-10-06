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
    public partial class PickerCell : ViewCell
    {
        public IList<string> Items { get { return variants_here.Items; } }
        public string Title { get { return title_here.Text; }
                              set { title_here.Text = value; }
        }
        public byte SelectedIndex { get { return (byte)variants_here.SelectedIndex; }
                                    private set { variants_here.SelectedIndex = value; }
        }

        public event EventHandler SelectedIndexChanged;
        //public event EventHandler<ValueChangedEventArgs> SelectedIndexChanged;
        //private byte old_value_picker_indx;
        public PickerCell()
        {
            InitializeComponent();
            variants_here.SelectedIndexChanged += this.SelectedIndexChanged;
            //variants_here.SelectedIndexChanged += (s, a) => { this.SelectedIndexChanged.Invoke(this, new ValueChangedEventArgs(old_value_picker_indx, variants_here.SelectedIndex)); };
            //old_value_picker_indx = (byte)variants_here.SelectedIndex;
        }

        public void InitFromSettings(string keyword)//yeee encapsulation goals
        {
            this.SelectedIndex = (byte)Preferences.Get(keyword, 0);
        }
    }
}