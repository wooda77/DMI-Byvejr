using System;
using System.Collections.ObjectModel;
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
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Data;
using System.ComponentModel;

using Microsoft.Phone.Controls;

namespace DMI_Weather.Views
{
    using Models;
    using ViewModels;

    public partial class ChooseCityPage : PageViewBase
    {
        public ObservableCollection<City> cities
        {
            get;
            set;
        }

        public ChooseCityPage()
        {
            InitializeComponent();
        }

        private void LayoutRoot_Loaded(object sender, RoutedEventArgs e)
        {
            cities = new ObservableCollection<City>();

            foreach (var item in Denmark.PostalCodes.OrderBy(v => v.Value))
            {
                cities.Add(new City()
                {
                    Name = item.Value,
                    PostalCode = item.Key
                });
            }

            CitiesJumpGrid.DataSource = cities;
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            CitiesJumpGrid.DataSource = cities.Where(c => Filter(c));
        }

        private bool Filter(object item)
        {
            if (item is City)
            {
                var city = item as City;

                return city.Name.StartsWith(SearchTextBox.Text,
                    StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                return false;
            }
        }

        private void QuickJumpGrid_OverlayClosed(object sender, RoutedEventArgs e)
        {

        }

        private void QuickJumpGrid_OverlayOpened(object sender, RoutedEventArgs e)
        {

        }

        private void CitiesJumpGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var item = (City)CitiesJumpGrid.SelectedItem;
            var uri  = "/Views/MainPage.xaml?PostalCode=" + item.PostalCode;

            NavigationService.Navigate(new Uri(uri, UriKind.Relative));
        }       
    }
}