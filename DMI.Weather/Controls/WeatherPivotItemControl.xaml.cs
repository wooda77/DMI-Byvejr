using System;
using System.Windows.Controls;
using System.Windows.Navigation;
using DMI.Common;

namespace DMI.Controls
{
    public partial class WeatherPivotItemControl : UserControl
    {
        public WeatherPivotItemControl()
        {
            InitializeComponent();
        }

        private void OpenInLandscapeMode(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            var image = sender as Image;
            if (image != null)
            {
                var source = image.Tag as Uri;
                if (string.IsNullOrEmpty(source.ToString()) == false)
                {
                    var address = string.Format(AppSettings.ImagePageAddress, Uri.EscapeDataString(source.ToString()));
                    App.Navigate(new Uri(address, UriKind.Relative));
                }
            }
        }
    }
}
