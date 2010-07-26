using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Windows.Navigation;
using System.Device.Location;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace Windcape.Phone.DanishWeather
{
    using ViewModels;
    using ViewModels.Models;

    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// ViewModel
        /// </summary>
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
            UpdatePollenFeed();
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
            using (var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default))
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
            client.Encoding = Encoding.GetEncoding("iso-8859-1"); 
            client.DownloadStringCompleted += NewsWebClient_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri("http://www.dmi.dk/dmi/rss-nyheder"));
        }

        private void NewsWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
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
                    viewModel.NewsFeedItems.Add(new NewsFeedItem()
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
            var selectedItem = (sender as StackPanel).DataContext;
            if (selectedItem != null)
            {
                var task = new WebBrowserTask();
                task.URL = (selectedItem as NewsFeedItem).Link.AbsoluteUri;
                task.Show();
            }
        }

        private void UpdatePollenFeed()
        {
            var client = new WebClient();
            client.Encoding = Encoding.GetEncoding("iso-8859-1"); 
            client.DownloadStringCompleted += PollenWebClient_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri("http://www.dmi.dk/dmi/pollen-feed.xml"));
        }

        private void PollenWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }

            try
            {
                var document = XElement.Parse(e.Result);

                var items = document.Elements("channel").Elements("item").ToList();

                viewModel.PollenFeedItems.Clear();

                for (int i = 0; i < 4; i += 2)
                {
                    viewModel.PollenFeedItems.Add(new PollenItem()
                    {
                        City = items[i].Element("title").Value,
                        Data = ParsePollenData(items[i].Element("description").Value),
                        Forecast = items[i + 1].Element("description").Value
                    });
                }
            }
            catch
            {
            }
        }

        private string ParsePollenData(string data)
        {
            string input = data.Replace("\n", "");
            input = input.Replace(" ", "");

            var result = new StringBuilder();

            string[] parts = input.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var partValues = part.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if ((partValues.Length == 2) && (partValues[1] != "-"))
                {
                    result.AppendFormat("{0}: {1} , ", partValues[0], partValues[1]);
                }
            }

            string output = result.ToString();
            
            if (output != string.Empty)
            {
                output = output.Substring(0, output.Length - 3);
            }

            return output;
        }
    }
}
