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


        public static readonly BindableProperty LabelTextColorProperty = BindableProperty.Create(nameof(LabelTextColor),typeof(Color), typeof(PickerCell),
                                                                                                 propertyChanged: LabelTextColorChanged);
        private static void LabelTextColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as PickerCell).title_here.TextColor = (Color)newValue;
        }
        public Color LabelTextColor
        {
            get { return title_here.TextColor; }
            set { title_here.TextColor = value; }
        }


        public static readonly BindableProperty PickerTextColorProperty = BindableProperty.Create(nameof(PickerTextColor), typeof(Color), typeof(PickerCell),
                                                                                                  propertyChanged: PickerTextColorChanged);
        private static void PickerTextColorChanged(BindableObject bindable, object oldValue, object newValue)
        {
            (bindable as PickerCell).variants_here.TextColor = (Color)newValue;
        }
        public Color PickerTextColor
        {
            get { return variants_here.TextColor; }
            set { variants_here.TextColor = value; }
        }

        public event EventHandler SelectedIndexChanged;
        public PickerCell()
        {
            InitializeComponent();
            variants_here.SelectedIndexChanged += (s, a) => { this.SelectedIndexChanged.Invoke(this, EventArgs.Empty); };
        }

        public void InitFromSettings(string keyword)//yeee encapsulation goals
        {
            this.SelectedIndex = (byte)Preferences.Get(keyword, 0);
        }
    }
}