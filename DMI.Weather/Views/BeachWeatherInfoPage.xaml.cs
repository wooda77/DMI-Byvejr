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
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;

namespace DMI.Views
{
    using DMI.Models;
    using DMI.Assets;

    public partial class BeachWeatherInfoPage : PhoneApplicationPage
    {
        private static string TemperatureImageSource = "http://servlet.dmi.dk/byvejr/servlet/byvejr_dag1?by={0}&tabel=dag1&mode=long";
        private static string WavesImageSource = "http://servlet.dmi.dk/byvejr/servlet/byvejr?by={0}&tabel=dag1&param=bolger";

        public BeachWeatherInfoPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string id = "";
            if (NavigationContext.QueryString.TryGetValue("ID", out id))
            {
                TemperatureImage.Source = new BitmapImage(
                    new Uri(string.Format(TemperatureImageSource, id), UriKind.Absolute));
                
                WavesImage.Source = new BitmapImage(
                    new Uri(string.Format(WavesImageSource, id), UriKind.Absolute));
            }
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            if (image != null)
                image.CropImageBorders();
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var image = sender as Image;
            if (image != null)
                image.CropImageBorders();
        }
    }
}