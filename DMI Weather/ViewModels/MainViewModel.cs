// 
// MainViewModel.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Net;
using System.Device.Location;
using Microsoft.Phone.Tasks;

namespace DMI_Weather.ViewModels
{
    using Models;

    public class MainViewModel : ViewModelBase
    {
        #region Properties

        private PollenFeed pollenFeed = new PollenFeed();
        private NewsFeed newsFeed = new NewsFeed();
        private CivicAddress address = new CivicAddress();
        private ObservableCollection<City> favorites 
            = new ObservableCollection<City>();

        public string PostalCode
        {
            get
            {
                if (string.IsNullOrEmpty(address.PostalCode))
                {
                    return AppResources.DefaultPostal;
                }
                else
                {
                    return address.PostalCode;
                }
            }
        }

        public Uri CityWeather2daysGraph
        {
            get
            {
                return new Uri(string.Format(AppResources.CityWeather2daysGraph, PostalCode));
            }
        }

        public Uri CityWeather7daysGraph
        {
            get
            {
                return new Uri(string.Format(AppResources.CityWeather7daysGraph, PostalCode));
            }
        }

        public Uri PollenGraph
        {
            get
            {
                return new Uri(string.Format(AppResources.PollenGraph, PostalCode));
            }
        }

        public string City
        {
            get
            {
                if (address != null)
                {
                    return address.City;
                }
                else
                {
                    return AppResources.DefaultCity;
                }
            }
        }

        public CivicAddress Address
        {
            get
            {
                return address;
            }
            set
            {
                if (address != value)
                {
                    address = value;

                    OnPropertyChanged("Address");
                    OnPropertyChanged("CityWeather2daysGraph");
                    OnPropertyChanged("CityWeather7daysGraph");
                    OnPropertyChanged("PollenGraph");
                }
            }
        }

        public ObservableCollection<PollenItem> PollenData
        {
            get
            {
                return pollenFeed.PollenData;
            }
        }

        public ObservableCollection<NewsItem> NewsItems
        {
            get
            {
                return newsFeed.NewsItems;
            }
        }
        
        public ObservableCollection<City> Favorites
        {
            get
            {
                return favorites;
            }
            set
            {
                favorites = value;
            }
        }

        #endregion

        #region Public Methods

        public void CropImageBorders(Image image)
        {
            if ((image.ActualWidth > 0) && (image.ActualHeight > 0))
            {
                // Crops 2 pixels on each side, to remove ugly rounded border from DMIs images.
                image.Clip = new RectangleGeometry()
                {
                    Rect = new Rect(2, 2, image.ActualWidth - 4, image.ActualHeight - 4)
                };
            }
        }

        public void ResolveAddressFromGeoPosition()
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
                        this.address = address;
                    }
                }
            }
        }

        public void OpenUrlInBrowser(Uri uri)
        {
            var task = new WebBrowserTask();
            task.URL = uri.AbsoluteUri;
            task.Show();
        }

        public void UpdatePollenFeed()
        {
            pollenFeed.Update();
        }

        public void UpdateNewsFeed()
        {
            newsFeed.Update();
        }

        #endregion
    }
}