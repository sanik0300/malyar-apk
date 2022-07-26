using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace malyar_apk
{
    class PhoneWidthconvert : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height)
            {
                return (double)value * DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Width;
            }
            else
            {
                return (double)value * DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Height;
            }         
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
