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
    public partial class DiversePivotItemControl : UserControl
    {
        public DiversePivotItemControl()
        {
            InitializeComponent();
        }

        private void RadarMenuItem_Tap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            App.Navigate(new Uri(AppSettings.RadarPageAddress, UriKind.Relative));
        }

        private void BeachWeatherMenuItem_Tap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            App.Navigate(new Uri(AppSettings.BeachWeatherPageAddress, UriKind.Relative));
        }

        private void UVIndexMenuItem_Tap(object sender, Microsoft.Phone.Controls.GestureEventArgs e)
        {
            App.Navigate(new Uri(AppSettings.UVIndexPageAddress, UriKind.Relative));
        }
    }
}
