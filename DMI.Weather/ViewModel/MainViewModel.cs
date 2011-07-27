#region License
// Copyright (c) 2011 Claus Jørgensen <10229@iha.dk>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
#endregion
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DMI.Model;
using DMI.Properties;
using DMI.Service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Tasks;
using System.Windows.Controls;

namespace DMI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private GeoCoordinateWatcher geoCoordinateWatcher;

        public MainViewModel()
        {
            this.Favorites = new ObservableCollection<City>();
            this.NewsItems = new ObservableCollection<NewsItem>();
            this.IsInitialized = false;
            this.Loading = false;

            if (App.IsFirstStart == false)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            this.IsInitialized = true;

            if (IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites))
            {
                this.Favorites = (ObservableCollection<City>)
                    IsolatedStorageSettings.ApplicationSettings[App.Favorites];

                this.Favorites = this.Favorites ?? new ObservableCollection<City>();
            }

            this.LoadNewsFeed = new RelayCommand(LoadNewsFeedExecute);
            this.LoadFavorites = new RelayCommand(LoadFavoritesExecute);
            this.AddToFavorites = new RelayCommand(AddToFavoritesExecute);
            this.RemoveFromFavorites = new RelayCommand<City>(RemoveFromFavoritesExecute);
            this.NewsItemSelected = new RelayCommand<NewsItem>(NewsItemSelectedExecute);
            this.FavoriteItemSelected = new RelayCommand<City>(FavoriteItemSelectedExecute);
            this.GoToLocation = new RelayCommand(GoToLocationExecute);

            if (InternetIsAvailable())
            {
                ResolveAddress();
            }
        }

        public bool IsInitialized
        {
            get;
            set;
        }

        public BitmapSource ThreeDaysImage
        {
            get;
            private set;
        }

        public BitmapSource SevenDaysImage
        {
            get;
            private set;
        }

        public BitmapSource PollenImage
        {
            get;
            private set;
        }

        public BitmapSource RegionalImage
        {
            get;
            private set;
        }

        public BitmapSource CountryImage
        {
            get;
            private set;
        }

        public GeoCoordinate CurrentGeoCoordinate
        {
            get;
            private set;
        }

        public CivicAddress CurrentAddress
        {
            get;
            private set;
        }

        public CountryWeatherResult CountryWeather
        {
            get;
            private set;
        }

        public RegionalWeatherResult RegionalWeather
        {
            get;
            set;
        }

        public PollenResult PollenData
        {
            get;
            private set;
        }

        public ObservableCollection<NewsItem> NewsItems
        {
            get;
            private set;
        }

        public ObservableCollection<City> Favorites
        {
            get;
            private set;
        }

        public bool Loading
        {
            get;
            private set;
        }

        public ICommand LoadNewsFeed
        {
            get;
            private set;
        }

        public ICommand LoadFavorites
        {
            get;
            private set;
        }

        public ICommand AddToFavorites
        {
            get;
            private set;
        }

        public ICommand RemoveFromFavorites
        {
            get;
            private set;
        }

        public ICommand NewsItemSelected
        {
            get;
            private set;
        }

        public ICommand FavoriteItemSelected
        {
            get;
            private set;
        }

        public ICommand GoToLocation
        {
            get;
            private set;
        }

        public void ResolveLocation(string postalCode, string country)
        {
            CurrentAddress = new CivicAddress()
            {
                PostalCode = postalCode,
                CountryRegion = country,
            };

            UpdateCurrentLocation();
        }

        private void LoadNewsFeedExecute()
        {
            NewsProvider.GetVideos((videos, exception) =>
            {
                NewsItems.Clear();

                if (videos.Count >= 3)
                {
                    NewsItems.Add(new NewsItem()
                    {
                        Title = "WebTV - Dagens Vejrudsigt",
                        WebTVItem = videos.FirstOrDefault(v => v.Category == "DMI"),
                    });

                    NewsItems.Add(new NewsItem()
                    {
                        Title = "WebTV - Sejlervejret",
                        WebTVItem = videos.FirstOrDefault(v => v.Category == "SEJL"),
                    });

                    NewsItems.Add(new NewsItem()
                    {
                        Title = "WebTV - 3-døgnsudsigten",
                        WebTVItem = videos.FirstOrDefault(v => v.Category == "3D"),
                    });
                }

                NewsProvider.GetNewsItems((items, e) =>
                {
                    foreach (var item in items)
                    {
                        NewsItems.Add(item);
                    }
                });
            });
        }

        private void LoadFavoritesExecute()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites))
                Favorites = (ObservableCollection<City>)IsolatedStorageSettings.ApplicationSettings[App.Favorites];
        }

        private void AddToFavoritesExecute()
        {
            if (CurrentAddress == null)
            {
                return;
            }
            
            int postalCode = 0;
            if (int.TryParse(CurrentAddress.PostalCode, out postalCode))
            {
                string cityName = string.Empty;

                switch (CurrentAddress.CountryRegion)
                {
                    case "Greenland":
                        cityName = Greenland.PostalCodes[postalCode].Name;
                        break;
                    case "Faroe Islands":
                        cityName = FaroeIslands.PostalCodes[postalCode].Name;
                        break;
                    case "Denmark":
                        cityName = Denmark.PostalCodes[postalCode];
                        break;
                }

                if (string.IsNullOrEmpty(cityName) == false && Favorites.Any(c => c.Name == cityName) == false)
                {
                    Favorites.Add(new City()
                    {
                        Country = CurrentAddress.CountryRegion,
                        PostalCode = postalCode,
                        Name = cityName
                    });

                    SaveFavorites();
                }
            }
        }

        private void RemoveFromFavoritesExecute(City city)
        {
            if ((city != null) && (city is City) && (Favorites != null))
            {
                Favorites.Remove(city);
                SaveFavorites();
            }
        }

        private void NewsItemSelectedExecute(NewsItem item)
        {
            if (item == null)
                throw new ArgumentException("item");

            if (item.WebTVItem != null)
            {
                var task = new MediaPlayerLauncher()
                {
                    Media = new Uri("http://tv.dmi.dk" + item.WebTVItem.Video, UriKind.Absolute)
                };
                task.Show();
            }
            else
            {
                var task = new WebBrowserTask()
                {
                    Uri = item.Link
                };
                task.Show();
            }
        }

        private void FavoriteItemSelectedExecute(City city)
        {
            if (city == null)
                throw new ArgumentException("city");

            var uri = string.Format("/View/MainPage.xaml?PostalCode={0}", city.PostalCode);

            App.Navigate(new Uri(uri, UriKind.Relative));
        }

        private void GoToLocationExecute()
        {
            if (InternetIsAvailable())
            {
                CurrentAddress = null;
                ResolveAddress();
            }
        }

        private bool InternetIsAvailable()
        {
            var available = NetworkInterface.GetIsNetworkAvailable();

//#if DEBUG
//    available = false;
//#endif

            if (available == false)
            {
                Loading = false;
                MessageBox.Show(AppResources.InternetError);

                return false;
            }

            return true;
        }

        private void UpdateCurrentLocation()
        {
            if (CurrentAddress != null &&
                CurrentAddress.CountryRegion == "Greenland")
            {
                RegionalWeather = new RegionalWeatherResult()
                {
                    Name = "Grønland",
                    Content = "Der er ingen regionaludsigt for din region"
                };

                Greenland.Instance.GetCityWeather(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        ThreeDaysImage = new BitmapImage(result.CityWeatherThreeDaysImage);
                        SevenDaysImage = new BitmapImage(result.CityWeatherSevenDaysImage);
                    });

                Greenland.Instance.GetCountryWeather(
                    (result, exception) =>
                    {
                        CountryWeather = result;
                        CountryImage = new BitmapImage(result.Image);
                    });
            }
            else if (CurrentAddress != null &&
                     CurrentAddress.CountryRegion == "Faroe Islands")
            {
                RegionalWeather = new RegionalWeatherResult()
                {
                    Name = "Færøerne",
                    Content = "Der er ingen regionaludsigt for din region"
                };

                FaroeIslands.Instance.GetCityWeather(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        ThreeDaysImage = new BitmapImage(result.CityWeatherThreeDaysImage);
                        SevenDaysImage = new BitmapImage(result.CityWeatherSevenDaysImage);
                    });

                FaroeIslands.Instance.GetCountryWeather(
                    (result, exception) =>
                    {
                        CountryWeather = result;
                        CountryImage = new BitmapImage(result.Image);
                    });
            }
            else if (CurrentAddress != null)
            {
                Denmark.Instance.GetCityWeather(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        ThreeDaysImage = new BitmapImage(result.CityWeatherThreeDaysImage);
                        SevenDaysImage = new BitmapImage(result.CityWeatherSevenDaysImage);
                    });

                Denmark.Instance.GetPollenData(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        PollenImage = new BitmapImage(result.Image);
                        PollenData = result;
                    });

                Denmark.Instance.GetRegionalWeather(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        RegionalWeather = result;
                        RegionalImage = new BitmapImage(result.Image);
                    });

                Denmark.Instance.GetCountryWeather(
                    (result, exception) =>
                    {
                        CountryWeather = result;
                        CountryImage = new BitmapImage(result.Image);
                    });
            }
        }

        private void ResolveAddress()
        {
            if (App.IsGPSEnabled == false)
            {
                MessageBox.Show(AppResources.GPSDisabledError);
            }
            else
            {
                Loading = true;

                geoCoordinateWatcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                geoCoordinateWatcher.MovementThreshold = 1.0;
                geoCoordinateWatcher.PositionChanged += GeoCoordinateWatcher_PositionChanged;
                geoCoordinateWatcher.Start();
            }
        }

        private void GeoCoordinateWatcher_PositionChanged(object sender, GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (CurrentAddress == null)
            {
                CurrentGeoCoordinate = geoCoordinateWatcher.Position.Location;

                BingLocationProvider.ResolveLocation(geoCoordinateWatcher.Position.Location,
                    (address, exception) =>
                    {
                        if (Loading)
                        {
                            Loading = false;

                            if (address != null)
                            {
                                CurrentAddress = address;
                                UpdateCurrentLocation();
                            }
                        }
                    });
            }
            else
            {
                Loading = false;
            }
        }

        private void SaveFavorites()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites) == false)
            {
                IsolatedStorageSettings.ApplicationSettings.Add(App.Favorites, Favorites);
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings[App.Favorites] = Favorites;
            }

            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}