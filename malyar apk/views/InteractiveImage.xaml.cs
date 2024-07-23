using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace malyar_apk
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class InteractiveImage : Image
    {
        public event EventHandler Tapped;
        public void OnTapped() => Tapped?.Invoke(this, EventArgs.Empty);
        public event EventHandler TappedTwice;
        public void OnTappedTwice() => TappedTwice?.Invoke(this, EventArgs.Empty);
        public event EventHandler LongPressed;
        public void OnLongPressed() => LongPressed?.Invoke(this, EventArgs.Empty);
        public InteractiveImage()
        {
            InitializeComponent();
        }
    }
}