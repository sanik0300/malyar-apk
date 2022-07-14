using System;
using System.Globalization;
using Xamarin.Forms;
using Xamarin.Essentials;

namespace malyar_apk
{
    class PhoneHeightConverter : IValueConverter
    {

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToDouble(value) * PeriodicalPicture.bigger_screen_dimension / PeriodicalPicture.smoller_screen_dimension;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
    class PhoneWidthConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return System.Convert.ToInt32(value) * PeriodicalPicture.smoller_screen_dimension / PeriodicalPicture.bigger_screen_dimension;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
