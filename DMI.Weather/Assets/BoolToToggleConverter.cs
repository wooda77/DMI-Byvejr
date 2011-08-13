using System;
using System.Windows.Data;

namespace DMI.Assets
{
    public class BoolToToggleConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return (bool)value ? Properties.Resources.Toggle_On : Properties.Resources.Toggle_Off;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
