using System;
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;

namespace DMI.Views
{
    using DMI.Models;
    using DMI.ViewModels;

    public partial class BeachWeatherPage
    {
        public BeachWeatherPage()
        {
            InitializeComponent();
        }

        public BeachWeatherViewModel ViewModel
        {
            get
            {
                return DataContext as BeachWeatherViewModel;
            }
        }

        private void Pushpin_Tap(object sender, GestureEventArgs e)
        {
            var pushpin = sender as Pushpin;
            if (pushpin != null)
            {
                var beach = pushpin.DataContext as Beach;

                if (beach != null)
                {
                    NavigationService.Navigate(new Uri(string.Format(
                        "/Views/BeachWeatherInfoPage.xaml?ID={0}", beach.ID), UriKind.Relative));
                }
            }
        }
    }
}