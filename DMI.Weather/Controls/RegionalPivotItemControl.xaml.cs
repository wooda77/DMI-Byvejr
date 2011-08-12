using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using DMI.Common;

namespace DMI.Controls
{
    public partial class RegionalPivotItemControl : UserControl
    {
        public RegionalPivotItemControl()
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
