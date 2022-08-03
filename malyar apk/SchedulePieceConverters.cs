using System;
using System.Collections.Generic;
using System.Globalization;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace malyar_apk
{
    class VerticalConverter : IMultiValueConverter
    {
        public const double not_square_ratio = 0.8;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //values[0] - multiplier property
            //values[1] - width of stacklayout
            //values[2] - height of stacklayout
            if (values[0] == null || values[1] == null || values[2]==null)
                return not_square_ratio;
            
            if (DeviceDisplay.MainDisplayInfo.Width > DeviceDisplay.MainDisplayInfo.Height)//if phone is situated horizontally, let's expand height to the maximum
                return (double)values[2];   
            
            return (double)values[0] * (double)values[1]* not_square_ratio;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    class HorizontalConverter : IMultiValueConverter
    {
        public const double not_square_ratio = 0.9;
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //values[0] - multiplier property
            //values[1] - width of stacklayout
            //values[2] - height of stacklayout
            if (values[0] == null || values[1] == null || values[2] == null)
                return not_square_ratio;
            
            if (DeviceDisplay.MainDisplayInfo.Width < DeviceDisplay.MainDisplayInfo.Height)//if phone is situated vertically, let's expand width to the maximum
                return (double)values[1];

            return (double)values[0] * (double)values[2] * not_square_ratio;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
