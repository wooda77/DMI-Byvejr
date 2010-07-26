using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Windcape.Phone.DanishWeather.Converters
{
    public sealed class ImageConverter : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            try
            {
                if ((value != null) && (value is Uri))
                {
                    return new BitmapImage((Uri)value);
                }
            }
            catch (UriFormatException) { }

            return new BitmapImage();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
