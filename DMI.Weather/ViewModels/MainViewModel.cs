﻿#region License
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
using System.Windows.Threading;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Tasks;

namespace DMI.ViewModels
{
    using DMI.Models;
    using DMI.Properties;

    public class MainViewModel : ViewModelBase
    {
        private const string CurrentLocationPropertyName = "CurrentLocation";
        private const string CityWeather2daysGraphPropertyName = "CityWeather2daysGraph";
        private const string CityWeather7daysGraphPropertyName = "CityWeather7daysGraph";
        private const string PollenGraphPropertyName = "PollenGraph";
        private const string RegionalImagePropertyName = "RegionalImage";
        private const string CountryImagePropertyName = "CountryImage";
        private const string RegionPropertyName = "Region";

        private readonly ICommand loadWeatherInformation;
        private readonly ICommand loadPollenInformation;
        private readonly ICommand loadNewsFeed;
        private readonly ICommand addToFavorites;
        private readonly ICommand loadFavorites;
        private readonly ICommand removeFromFavorites;
        private readonly ICommand newsItemSelected;
        private readonly ICommand favoriteItemSelected;
        private readonly ICommand goToLocation;

        private Uri cityWeather2daysGraph;
        private Uri cityWeather7daysGraph;
        private Uri pollenGraph;
        private Uri regionalImage;
        private Uri countryImage;
        private GeoCoordinateWatcher watcher;
        private CivicAddress currentLocation;
        private Region region;
        private bool loading = false;

        public MainViewModel()
        {
            this.Favorites = new ObservableCollection<City>();
            this.PollenData = new ObservableCollection<PollenItem>();
            this.NewsItems = new ObservableCollection<NewsItem>();
            this.WeatherItems = new ObservableCollection<WeatherItem>();

            if (IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites))
            {
                this.Favorites = (ObservableCollection<City>)
                    IsolatedStorageSettings.ApplicationSettings[App.Favorites];

                this.Favorites = this.Favorites ?? new ObservableCollection<City>();
            }

            this.loadWeatherInformation = new RelayCommand(LoadWeatherInformationExecute);
            this.loadPollenInformation = new RelayCommand(LoadPollenInformationExecute);
            this.loadNewsFeed = new RelayCommand(LoadNewsFeedExecute);
            this.addToFavorites = new RelayCommand(AddToFavoritesExecute);
            this.loadFavorites = new RelayCommand(LoadFavoritesExecute);
            this.removeFromFavorites = new RelayCommand<City>(RemoveFromFavoritesExecute);
            this.newsItemSelected = new RelayCommand<NewsItem>(NewsItemSelectedExecute);
            this.favoriteItemSelected = new RelayCommand<City>(FavoriteItemSelectedExecute);
            this.goToLocation = new RelayCommand(GoToLocationExecute);
        }

        #region Properties

        public IEnumerable<City> Cities
        {
            get
            {
                return Denmark.Cities;
            }
        }

        public Uri CityWeather2daysGraph
        {
            get
            {
                return cityWeather2daysGraph;
            }
            set
            {
                if (cityWeather2daysGraph != value)
                {
                    cityWeather2daysGraph = value;
                    RaisePropertyChanged(CityWeather2daysGraphPropertyName);
                }
            }
        }

        public Uri CityWeather7daysGraph
        {
            get
            {
                return cityWeather7daysGraph;
            }
            set
            {
                if (cityWeather7daysGraph != value)
                {
                    cityWeather7daysGraph = value;
                    RaisePropertyChanged(CityWeather7daysGraphPropertyName);
                }
            }
        }

        public Uri PollenGraph
        {
            get
            {
                return pollenGraph;
            }
            set
            {
                if (pollenGraph != value)
                {
                    pollenGraph = value;
                    RaisePropertyChanged(PollenGraphPropertyName);
                }
            }
        }

        public Uri RegionalImage
        {
            get
            {
                return regionalImage;
            }
            set
            {
                if (regionalImage != value)
                {
                    regionalImage = value;
                    RaisePropertyChanged(RegionalImagePropertyName);
                }
            }
        }

        public Uri CountryImage
        {
            get
            {
                return countryImage;
            }
            set
            {
                if (countryImage != value)
                {
                    countryImage = value;
                    RaisePropertyChanged(CountryImagePropertyName);
                }
            }
        }

        public string PostalCode
        {
            get
            {
                if (currentLocation != null)
                {
                    int postal = 1000;
                    if (int.TryParse(currentLocation.PostalCode, out postal))
                    {
                        return Denmark.GetValidPostalCode(postal).ToString();
                    }

                    return currentLocation.PostalCode;
                }

                return LastCity;
            }
        }

        public string City
        {
            get
            {
                if (currentLocation != null)
                {
                    return currentLocation.City;
                }

                return string.Empty;
            }
        }

        public CivicAddress CurrentLocation
        {
            get
            {
                return currentLocation;
            }
            set
            {
                if (currentLocation != value && value != null)
                {
                    currentLocation = value;

                    if (string.IsNullOrEmpty(currentLocation.PostalCode) == false)
                    {
                        SaveLastCity();
                        UpdateCurrentLocation();
                        Loading = false;
                    }

                    RaisePropertyChanged(CurrentLocationPropertyName);
                }
            }
        }

        public Region Region
        {
            get
            {
                return region;
            }
            set
            {
                if (region != value)
                {
                    region = value;

                    RaisePropertyChanged(RegionPropertyName);
                }
            }
        }

        public ObservableCollection<PollenItem> PollenData
        {
            get;
            private set;
        }

        public ObservableCollection<NewsItem> NewsItems
        {
            get;
            private set;
        }

        public ObservableCollection<WeatherItem> WeatherItems
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
            get
            {
                return loading;
            }
            set
            {
                if (loading != value)
                {
                    loading = value;
                    RaisePropertyChanged("Loading");
                }
            }
        }

        public ICommand LoadWeatherInformation
        {
            get
            {
                return loadWeatherInformation;
            }
        }

        public ICommand LoadPollenInformation
        {
            get
            {
                return loadPollenInformation;
            }
        }

        public ICommand LoadNewsFeed
        {
            get
            {
                return loadNewsFeed;
            }
        }

        public ICommand LoadFavorites
        {
            get
            {
                return loadFavorites;
            }
        }

        public ICommand AddToFavorites
        {
            get
            {
                return addToFavorites;
            }
        }

        public ICommand RemoveFromFavorites
        {
            get
            {
                return removeFromFavorites;
            }
        }

        public ICommand NewsItemSelected
        {
            get
            {
                return newsItemSelected;
            }
        }

        public ICommand FavoriteItemSelected
        {
            get
            {
                return favoriteItemSelected;
            }
        }

        public ICommand GoToLocation
        {
            get
            {
                return goToLocation;
            }
        }

        #endregion

        #region Commands

        private void LoadWeatherInformationExecute()
        {
            UpdateCurrentLocation();

            WeatherDataProvider.GetWeatherData((items, e) =>
            {
                WeatherItems.Clear();

                foreach (var item in items)
                {
                    WeatherItems.Add(item);
                }
            });

            if (currentLocation == null
            || string.IsNullOrEmpty(currentLocation.PostalCode))
            {
                ResolveAddressFromGeoPosition();
            }
        }

        private void LoadPollenInformationExecute()
        {
            WeatherDataProvider.GetPollenData((items, e) =>
            {
                PollenData.Clear();

                foreach (var item in items)
                {
                    PollenData.Add(item);
                }
            });
        }

        private void LoadNewsFeedExecute()
        {
            WeatherDataProvider.GetNewsItems((items, e) =>
            {
                NewsItems.Clear();

                foreach (var item in items)
                {
                    NewsItems.Add(item);
                }
            });
        }

        private void LoadFavoritesExecute()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites))
            {
                Favorites = (ObservableCollection<City>)
                    IsolatedStorageSettings.ApplicationSettings[App.Favorites];
            }
        }

        private void AddToFavoritesExecute()
        {
            if (string.IsNullOrEmpty(PostalCode) == false)
            {
                int postal;
                if (int.TryParse(PostalCode, out postal) == true)
                {
                    if (Favorites != null &&
                        Favorites.Any(c => c.PostalCode == postal) == false &&
                        Denmark.PostalCodes.ContainsKey(postal))
                    {
                        Favorites.Add(new City()
                        {
                            PostalCode = postal,
                            Name = Denmark.PostalCodes[postal]
                        });

                        SaveFavorites();
                    }
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
            var task = new WebBrowserTask()
            {
                URL = item.Link.AbsoluteUri
            };
            task.Show();
        }

        private void FavoriteItemSelectedExecute(City city)
        {
            var uri = string.Format("/Views/MainPage.xaml?PostalCode={0}&Time={1}",
                city.PostalCode,
                DateTime.Now.ToLongTimeString()
            );

            App.Navigate(new Uri(uri, UriKind.Relative));
        }

        private void GoToLocationExecute()
        {
            currentLocation = null;

            ResolveAddressFromGeoPosition();
        }

        #endregion

        #region Utility

        private bool InternetIsAvailable()
        {
            var available = NetworkInterface.GetIsNetworkAvailable();

            //#if DEBUG
            //    available = false;
            //#endif

            if (available == false)
            {
                MessageBox.Show(AppResources.InternetError);
                return false;
            }

            return true;
        }

        private void UpdateCurrentLocation()
        {
            if (InternetIsAvailable())
            {
                CityWeather2daysGraph = new Uri(string.Format(
                    AppResources.CityWeather2daysGraph, PostalCode));

                CityWeather7daysGraph = new Uri(string.Format(
                    AppResources.CityWeather7daysGraph, PostalCode));

                PollenGraph = new Uri(string.Format(
                    AppResources.PollenGraph, PostalCode));
                
                int postalCode = 1000;
                if (int.TryParse(PostalCode, out postalCode))
                {
                    RegionalImage = new Uri(Denmark.GetRegionImageFromPostalCode(postalCode));
                    UpdateRegion(Denmark.GetRegionTextFromPostalCode(postalCode));
                }

                CountryImage = new Uri(AppResources.CountryImage);
            }
        }

        private void UpdateRegion(string regionUrl)
        {
            WeatherDataProvider.GetRegionData(regionUrl, 
                (region, e) => 
                {
                    Region = region;
                });
        }

        private void ResolveAddressFromGeoPosition()
        {
            if (App.IsGPSEnabled == false)
            {
                MessageBox.Show(AppResources.GPSDisabledError);
            }
            else
            {
                Loading = true;

                var timer = new DispatcherTimer();
                timer.Interval = TimeSpan.FromSeconds(15);
                timer.Tick += (s, e) =>
                {
                    if (Loading)
                    {
                        Loading = false;
                        MessageBox.Show(AppResources.GPSResolveError);
                    }
                };
                timer.Start();

                watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
                watcher.MovementThreshold = 1.0;
                watcher.PositionChanged += GeoCoordinateWatcher_PositionChanged;
                watcher.Start();
            }
        }

        private void GeoCoordinateWatcher_PositionChanged(object sender,
            GeoPositionChangedEventArgs<GeoCoordinate> e)
        {
            if (currentLocation == null)
            {
                if (e.Position.Location.IsUnknown == false)
                {
                    LocationProvider.ResolveLocation(watcher.Position.Location,
                        (address, exception) =>
                        {
                            if (Loading)
                            {
                                if (address != null)
                                {
                                    Loading = false;
                                    CurrentLocation = address;
                                }
                            }
                        });
                }
            }
        }

        private string LastCity
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(App.LastCity))
                {
                    var lastCity = IsolatedStorageSettings.ApplicationSettings[App.LastCity];
                    return lastCity.ToString();
                }

                return AppResources.DefaultCity;
            }
        }

        private void SaveLastCity()
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(App.LastCity) == false)
            {
                IsolatedStorageSettings.ApplicationSettings.Add(App.LastCity, PostalCode);
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings[App.LastCity] = PostalCode;
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
        }

        #endregion
    }
}