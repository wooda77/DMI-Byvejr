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
using System.Collections.ObjectModel;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using DMI.Common;
using DMI.Service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Tasks;
using Microsoft.Phone.Shell;

namespace DMI.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        private GeoCoordinateWatcher geoCoordinateWatcher;

        public MainViewModel()
        {
            this.Favorites = new ObservableCollection<GeoLocationCity>();
            this.NewsItems = new ObservableCollection<NewsItem>();
            this.IsInitialized = false;
            this.Loading = false;

            if (AppSettings.IsFirstStart == false)
            {
                Initialize();
            }
        }

        public void Initialize()
        {
            this.IsInitialized = true;

            if (IsolatedStorageSettings.ApplicationSettings.Contains(AppSettings.FavoritesKey))
            {
                this.Favorites = (ObservableCollection<GeoLocationCity>)
                    IsolatedStorageSettings.ApplicationSettings[AppSettings.FavoritesKey];

                this.Favorites = this.Favorites ?? new ObservableCollection<GeoLocationCity>();
            }

            this.LoadNewsFeed = new RelayCommand(LoadNewsFeedExecute);
            this.LoadFavorites = new RelayCommand(LoadFavoritesExecute);
            this.AddToFavorites = new RelayCommand(AddToFavoritesExecute);
            this.RemoveFromFavorites = new RelayCommand<GeoLocationCity>(RemoveFromFavoritesExecute);
            this.NewsItemSelected = new RelayCommand<NewsItem>(NewsItemSelectedExecute);
            this.FavoriteItemSelected = new RelayCommand<GeoLocationCity>(FavoriteItemSelectedExecute);
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

        public ObservableCollection<GeoLocationCity> Favorites
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
                        Title = Properties.Resources.WebTV_DMI,
                        WebTVItem = videos.FirstOrDefault(v => v.Category == "DMI"),
                    });

                    NewsItems.Add(new NewsItem()
                    {
                        Title = Properties.Resources.WebTV_SEJL,
                        WebTVItem = videos.FirstOrDefault(v => v.Category == "SEJL"),
                    });

                    NewsItems.Add(new NewsItem()
                    {
                        Title = Properties.Resources.WebTV_3D,
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
            if (IsolatedStorageSettings.ApplicationSettings.Contains(AppSettings.FavoritesKey))
                Favorites = (ObservableCollection<GeoLocationCity>)IsolatedStorageSettings.ApplicationSettings[AppSettings.FavoritesKey];
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
                GeoLocationCity city = null;

                switch (CurrentAddress.CountryRegion)
                {
                    case Greenland.Name:
                        city = Greenland.PostalCodes[postalCode];
                        break;
                    case FaroeIslands.Name:
                        city = FaroeIslands.PostalCodes[postalCode];
                        break;
                    case Denmark.Name:
                        city = Denmark.PostalCodes[postalCode];
                        break;
                    default:
                        city = Denmark.PostalCodes[Denmark.DefaultPostalCode];
                        break;
                }

                if (city != null && Favorites.Any(c => c.Name == city.Name) == false)
                {
                    Favorites.Add(city);
                    SaveFavorites();
                }
            }
        }

        private void RemoveFromFavoritesExecute(GeoLocationCity city)
        {
            if ((city != null) && (city is GeoLocationCity) && (Favorites != null))
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

        private void FavoriteItemSelectedExecute(GeoLocationCity city)
        {
            if (city == null)
                throw new ArgumentException("city");

            var uri = string.Format(AppSettings.MainPageAddress, city.PostalCode, city.Country);

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
                MessageBox.Show(Properties.Resources.InternetError);

                return false;
            }

            return true;
        }

        private void UpdateCurrentLocation()
        {
            if (CurrentAddress != null &&
                CurrentAddress.CountryRegion == Greenland.Name)
            {
                RegionalWeather = new RegionalWeatherResult()
                {
                    Name = Properties.Resources.Country_Greenland,
                    Content = Properties.Resources.NoRegionalForecast
                };

                Greenland.Instance.GetCityWeather(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            ThreeDaysImage = new BitmapImage(result.CityWeatherThreeDaysImage);
                            SevenDaysImage = new BitmapImage(result.CityWeatherSevenDaysImage);
                        });
                    });

                Greenland.Instance.GetCountryWeather(
                    (result, exception) =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            CountryWeather = result;
                            CountryImage = new BitmapImage(result.Image);
                        });
                    });
            }
            else if (CurrentAddress != null &&
                     CurrentAddress.CountryRegion == FaroeIslands.Name)
            {
                RegionalWeather = new RegionalWeatherResult()
                {
                    Name = Properties.Resources.Country_FaroeIslands,
                    Content = Properties.Resources.NoRegionalForecast
                };

                FaroeIslands.Instance.GetCityWeather(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            ThreeDaysImage = new BitmapImage(result.CityWeatherThreeDaysImage);
                            SevenDaysImage = new BitmapImage(result.CityWeatherSevenDaysImage);
                        });
                    });

                FaroeIslands.Instance.GetCountryWeather(
                    (result, exception) =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            CountryWeather = result;
                            CountryImage = new BitmapImage(result.Image);
                        });
                    });
            }
            else if (CurrentAddress != null)
            {
                Denmark.Instance.GetCityWeather(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            ThreeDaysImage = new BitmapImage(result.CityWeatherThreeDaysImage);
                            SevenDaysImage = new BitmapImage(result.CityWeatherSevenDaysImage);
                        });
                    });

                Denmark.Instance.GetPollenData(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            PollenImage = new BitmapImage(result.Image);
                            PollenData = result;
                        });
                    });

                Denmark.Instance.GetRegionalWeather(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            RegionalWeather = result;
                            RegionalImage = new BitmapImage(result.Image);
                        });
                    });

                Denmark.Instance.GetCountryWeather(
                    (result, exception) =>
                    {
                        Deployment.Current.Dispatcher.BeginInvoke(() =>
                        {
                            CountryWeather = result;
                            CountryImage = new BitmapImage(result.Image);
                        });
                    });
            }
        }

        private void ResolveAddress()
        {
            if (AppSettings.IsGPSEnabled == false)
            {
                MessageBox.Show(Properties.Resources.GPSDisabledError);
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
            if (IsolatedStorageSettings.ApplicationSettings.Contains(AppSettings.FavoritesKey) == false)
            {
                IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.FavoritesKey, Favorites);
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings[AppSettings.FavoritesKey] = Favorites;
            }

            IsolatedStorageSettings.ApplicationSettings.Save();
        }
    }
}