using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using DMI.Properties;

namespace DMI.View
{
    public partial class BeachWeatherInfoPage
    {
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
                    new Uri(string.Format(AppResources.TemperatureImageSource, id), UriKind.Absolute));

                WavesImage.Source = new BitmapImage(
                    new Uri(string.Format(AppResources.WavesImageSource, id), UriKind.Absolute));
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