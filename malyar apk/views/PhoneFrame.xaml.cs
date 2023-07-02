using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace malyar_apk
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PhoneFrame : Frame
    {
        static private double ratio = -1;
        public PhoneFrame() {
            if (ratio > 0)
                return;
            ratio = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Height;
            if(ratio > 1)
            {
                ratio = 1 / ratio;
            }
        }

        protected override SizeRequest OnMeasure(double widthConstraint, double heightConstraint)
        {
            if (widthConstraint/heightConstraint >= ratio)
            {
                widthConstraint = heightConstraint * ratio;
            }
            else {
                heightConstraint = widthConstraint / ratio;
            }

            return new SizeRequest(new Size(widthConstraint, heightConstraint));
        }
    }
}