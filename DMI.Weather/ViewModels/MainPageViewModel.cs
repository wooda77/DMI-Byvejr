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
using System.Windows.Threading;
using DMI.Service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Tasks;
using DMI.Resources;
using DMI.Data;

namespace DMI.ViewModels
{
    public class MainPageViewModel : ViewModelBase
    {
        private GeoCoordinateWatcher geoCoordinateWatcher;

        public MainPageViewModel()
        {
            this.Favorites = new ObservableCollection<GeoLocationCity>();
            this.NewsItems = new ObservableCollection<NewsItem>();
            this.IsInitialized = false;
            this.Loading = false;

            if (AppSettings.IsFirstStart)
            {
                var result = MessageBox.Show(Properties.Resources.GPSHelpText, "Allow GPS Services", MessageBoxButton.OKCancel);
                if (result == MessageBoxResult.OK)
                {
                    AppSettings.IsGPSEnabled = true;
                }

                AppSettings.IsFirstStart = false;
            }
        }

        public void Initialize()
        {
            this.IsInitialized = true;

            TileGenerator.RefreshTileTask();

            if (IsolatedStorageSettings.ApplicationSettings.Contains(AppSettings.FavoritesKey))
            {
                this.Favorites = (ObservableCollection<GeoLocationCity>)
                    IsolatedStorageSettings.ApplicationSettings[AppSettings.FavoritesKey];

                this.Favorites = this.Favorites ?? new ObservableCollection<GeoLocationCity>();
            }

            this.LoadNewsFeed();
            this.LoadFavorites();

            this.AddToFavorites = new RelayCommand(AddToFavoritesExecute);
            this.RemoveFromFavorites = new RelayCommand<GeoLocationCity>(RemoveFromFavoritesExecute);
            this.FavoriteItemSelected = new RelayCommand<GeoLocationCity>(FavoriteItemSelectedExecute);
            this.NewsItemSelected = new RelayCommand<NewsItem>(NewsItemSelectedExecute);
            this.GoToLocation = new RelayCommand(GoToLocationExecute);

            if (InternetIsAvailable())
            {
                ResolveAddress();
            }
        }

        public int PivotSelectedIndex
        {
            get;
            set;
        }

        public ICommand PivotSelectionChanged
        {
            get;
            private set;
        }

        public bool IsInitialized
        {
            get;
            private set;
        }

        public Uri ThreeDaysImage
        {
            get;
            private set;
        }

        public Uri SevenDaysImage
        {
            get;
            private set;
        }

        public Uri FourteenDaysImage
        {
            get;
            private set;
        }

        public Uri FifteenDaysImage
        {
            get;
            private set;
        }

        public Uri PollenImage
        {
            get;
            private set;
        }

        public Uri RegionalImage
        {
            get;
            private set;
        }

        public Uri CountryImage
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

        private void LoadNewsFeed()
        {
            NewsProvider.GetVideos((videos, exception) =>
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    NewsItems.Clear();

                    if (videos.Count >= 2)
                    {
                        NewsItems.Add(new NewsItem()
                        {
                            Title = Properties.Resources.WebTV_DMI,
                            WebTVItem = videos.FirstOrDefault(v => v.Category == "DMI"),
                        });

                        NewsItems.Add(new NewsItem()
                        {
                            Title = Properties.Resources.WebTV_3D,
                            WebTVItem = videos.FirstOrDefault(v => v.Category == "3D"),
                        });
                    }

                    NewsProvider.GetNewsItems((items, e) =>
                    {
                        SmartDispatcher.BeginInvoke(() => 
                        {
                            foreach (var item in items)
                            {
                                NewsItems.Add(item);
                            }
                        });
                    });
                });
            });
        }

        private void LoadFavorites()
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

        private void FavoriteItemSelectedExecute(GeoLocationCity city)
        {
            if (city != null)
            {
                ResolveLocation(city.PostalCode.ToString(), city.Country);
                PivotSelectedIndex = 0;
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
            else if (item.Link != null)
            {
                var task = new WebBrowserTask()
                {
                    Uri = item.Link
                };
                task.Show();
            }
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
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            ThreeDaysImage = result.CityWeatherThreeDaysImage;
                            SevenDaysImage = result.CityWeatherSevenDaysImage;
                            FourteenDaysImage = result.CityWeatherFourteenDaysImage;
                        });
                    });

                Greenland.Instance.GetCountryWeather(
                    (result, exception) =>
                    {
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            CountryWeather = result;
                            CountryImage = result.Image;
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
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            ThreeDaysImage = result.CityWeatherThreeDaysImage;
                            SevenDaysImage = result.CityWeatherSevenDaysImage;
                        });
                    });

                FaroeIslands.Instance.GetCountryWeather(
                    (result, exception) =>
                    {
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            CountryWeather = result;
                            CountryImage = result.Image;
                        });
                    });
            }
            else if (CurrentAddress != null)
            {
                Denmark.Instance.GetCityWeather(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            ThreeDaysImage = result.CityWeatherThreeDaysImage;
                            SevenDaysImage = result.CityWeatherSevenDaysImage;
                            FourteenDaysImage = result.CityWeatherFourteenDaysImage;
                            FifteenDaysImage = result.CityWeatherFifteenDaysImage;
                        });
                    });

                Denmark.Instance.GetPollenData(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            PollenImage = result.Image;
                            PollenData = result;
                        });
                    });

                Denmark.Instance.GetRegionalWeather(CurrentGeoCoordinate, CurrentAddress.PostalCode,
                    (result, exception) =>
                    {
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            RegionalWeather = result;
                            RegionalImage = result.Image;
                        });
                    });

                Denmark.Instance.GetCountryWeather(
                    (result, exception) =>
                    {
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            CountryWeather = result;
                            CountryImage = result.Image;
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