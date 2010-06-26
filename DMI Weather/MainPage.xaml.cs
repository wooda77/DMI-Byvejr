using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.Windows.Navigation;

using Microsoft.Phone.Controls;

namespace DMI_Weather
{
    using ViewModels;

    public partial class MainPage : PhoneApplicationPage
    {
        private MainPageViewModel viewModel 
            = new MainPageViewModel();

        public MainPage()
        {
            InitializeComponent();

            DataContext = viewModel;
        }

        private void PhoneApplicationPage_Loaded(object sender, RoutedEventArgs e)
        {
            ResolveAddress();
            UpdateNewsFeed();
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Image)
            {
                var image = sender as Image;

                if ((image.ActualWidth > 0) && (image.ActualHeight > 0))
                {
                    // Crops 2 pixels on each side, to remove ugly rounded border from DMIs images.
                    image.Clip = new RectangleGeometry()
                    {
                        Rect = new Rect(2, 2, image.ActualWidth - 4, image.ActualHeight - 4)
                    };
                }
            }
        }

        private void ResolveAddress()
        {
            using (var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Low))
            {
                watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));

                var resolver = new CivicAddressResolver();

                if (!watcher.Position.Location.IsUnknown)
                {
                    var address = resolver.ResolveAddress(watcher.Position.Location);

                    if (!address.IsUnknown)
                    {
                        viewModel.Address = address;
                    }
                }
            }
        }

        private void UpdateNewsFeed()
        {
            var client = new WebClient();
            client.Encoding = new LatinEncoding();
            client.DownloadStringCompleted += WebClient_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri("http://www.dmi.dk/dmi/rss-nyheder"));
        }

        private void WebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }

            try
            {
                var document = XElement.Parse(e.Result);

                var items = document.Elements("channel").Elements("item");

                viewModel.NewsFeedItems.Clear();

                foreach (var item in items)
                {
                    viewModel.NewsFeedItems.Add(new FeedItemViewModel()
                    {
                        Title = item.Element("title").Value,
                        Description = item.Element("description").Value,
                        Link = new Uri(item.Element("link").Value)
                    });
                }
            }
            catch
            {                
            }
        }

        private void NewsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = (sender as ListBox).SelectedItem;
            if (selectedItem != null)
            {
                NavigationService.Navigate((selectedItem as FeedItemViewModel).Link);
            }
        }
    }
}
