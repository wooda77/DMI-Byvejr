using System;
using DMI.ViewModel;
using DMI.Model;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.Maps;

namespace DMI.View
{
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
                        "/View/BeachWeatherInfoPage.xaml?ID={0}", beach.ID), UriKind.Relative));
                }
            }
        }
    }
}