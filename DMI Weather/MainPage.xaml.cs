using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Device.Location;
using System.Xml;
using System.Xml.Linq;

using Microsoft.Phone.Controls;

namespace DMI_Weather
{
    public partial class MainPage : PhoneApplicationPage
    {
        private string bingMapsKey = "AiRmp6pZjy3sk-7JUt4Q66Va2JQ33fp6uT6zTS37NT3i6hjCNBUPJyxaptAIWSvy";
        private int postalCode = 2000;

        public MainPage()
        {
            InitializeComponent();

            SupportedOrientations = SupportedPageOrientation.Portrait | SupportedPageOrientation.Landscape;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            double latitude  = 56.12029;
            double longitude = 10.149;

            string url = string.Format(
                "http://dev.virtualearth.net/REST/v1/Locations/{0},{1}?o=xml&key={2}",
                latitude, longitude, bingMapsKey
            );

            try
            {
                WebClient client = new WebClient();
                client.OpenReadCompleted += client_OpenReadCompleted;
                client.OpenReadAsync(new Uri(url, UriKind.Absolute));
            }
            catch (Exception)
            {
            }
        }

        private void client_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }

            using (var stream = e.Result)
            using (var reader = XmlReader.Create(stream))
            {
                while (reader.ReadToFollowing("PostalCode"))
                {
                    string content = reader.ReadElementContentAsString();
                    postalCode = int.Parse(content);

                    CityWeatherImage1.Source = new BitmapImage(new Uri(string.Format(
                        "http://servlet.dmi.dk/byvejr/servlet/byvejr_dag1?by={0}&mode=long", postalCode),
                        UriKind.Absolute
                    ));
                    Image_Loaded(CityWeatherImage1, null);

                    string url = string.Format(
                        "http://servlet.dmi.dk/byvejr/servlet/byvejr?by={0}&tabel=dag3_9", postalCode);

                    CityWeatherImage2.Source = new BitmapImage(new Uri(url, UriKind.Absolute));
                    Image_Loaded(CityWeatherImage2, null);

                    PollenImage.Source = new BitmapImage(new Uri(string.Format(
                        "http://servlet.dmi.dk/byvejr/servlet/pollen_dag1?by={0}", postalCode),
                        UriKind.Absolute
                    ));
                    Image_Loaded(PollenImage, null);
                }
            }
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Image)
            {
                var image = sender as Image;

                if ((image.ActualWidth > 0) && (image.ActualHeight > 0))
                {
                    image.Clip = new RectangleGeometry()
                    {
                        Rect = new Rect(2, 2, image.ActualWidth - 4, image.ActualHeight - 4)
                    };
                }
            }
        }
    }
}
