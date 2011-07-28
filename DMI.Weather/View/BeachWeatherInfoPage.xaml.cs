using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using DMI.Assets;

namespace DMI.View
{
    public partial class BeachWeatherInfoPage
    {
        public BeachWeatherInfoPage()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string beachId = NavigationContext.TryGetStringKey("ID");
            
            if (string.IsNullOrEmpty(beachId) == false)
            {
                if (TemperatureImage != null)
                    TemperatureImage.Source = new BitmapImage(new Uri(string.Format(
                        Properties.Resources.TemperatureImageSource, beachId), UriKind.Absolute));

                if (WavesImage != null)
                    WavesImage.Source = new BitmapImage(new Uri(string.Format(
                        Properties.Resources.WavesImageSource, beachId), UriKind.Absolute));
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