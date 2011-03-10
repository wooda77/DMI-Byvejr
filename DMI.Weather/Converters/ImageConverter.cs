//
// ImageConverter.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;
using System.Diagnostics;

namespace DMI.Converters
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
            catch (UriFormatException e)
            {
                Debug.WriteLine(e.Message);
            }

            return new BitmapImage();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
