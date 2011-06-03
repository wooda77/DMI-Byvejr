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
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Tasks;

namespace DMI.ViewModels
{
    using DMI.Models;
    using DMI.Properties;

    public class MainViewModel : ViewModelBase
    {
        #region Constants

        private const int DefaultPostalCode = 1000;

        private const string CurrentLocationPropertyName = "CurrentLocation";
        private const string PostalCodePropertyName = "PostalCode";
        private const string TwoDaysImagePropertyName = "TwoDaysImage";
        private const string SevenDaysImagePropertyName = "SevenDaysImage";
        private const string PollenImagePropertyName = "PollenImage";
        private const string RegionalImagePropertyName = "RegionalImage";
        private const string CountryImagePropertyName = "CountryImage";
        private const string RegionPropertyName = "Region";

        private const string TwoDaysFileName = "2days.jpg";
        private const string SevenDaysFileName = "7days.jpg";
        private const string PollenFileName = "pollen.jpg";
        private const string RegionalFileName = "regional.jpg";
        private const string CountryFileName = "country.jpg";

        #endregion

        #region Properties

        private BitmapSource twoDaysImage;
        private BitmapSource sevenDaysImage;
        private BitmapSource pollenImage;
        private BitmapSource regionalImage;
        private BitmapSource countryImage;

        private Region currentRegion;
        private CivicAddress currentLocation;

        private GeoCoordinateWatcher watcher;
        private bool isLoading = false;

        #endregion

        #region Constructor

        public MainViewModel()
        {
            this.Favorites = new ObservableCollection<City>();
            this.PollenData = new ObservableCollection<PollenItem>();
            this.NewsItems = new ObservableCollection<NewsItem>();
            this.WeatherItems = new ObservableCollection<WeatherItem>();

            this.TwoDaysImage = ImageUtility.LoadFromLocalStorage(TwoDaysFileName);
            this.SevenDaysImage = ImageUtility.LoadFromLocalStorage(SevenDaysFileName);
            this.PollenImage = ImageUtility.LoadFromLocalStorage(PollenFileName);
            this.RegionalImage = ImageUtility.LoadFromLocalStorage(RegionalFileName);
            this.CountryImage = ImageUtility.LoadFromLocalStorage(CountryFileName);

            if (IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites))
            {
                this.Favorites = (ObservableCollection<City>)
                    IsolatedStorageSettings.ApplicationSettings[App.Favorites];

                this.Favorites = this.Favorites ?? new ObservableCollection<City>();
            }

            this.LoadWeatherInformation = new RelayCommand(LoadWeatherInformationExecute);
            this.LoadPollenInformation = new RelayCommand(LoadPollenInformationExecute);
            this.LoadNewsFeed = new RelayCommand(LoadNewsFeedExecute);
            this.AddToFavorites = new RelayCommand(AddToFavoritesExecute);
            this.LoadFavorites = new RelayCommand(LoadFavoritesExecute);
            this.RemoveFromFavorites = new RelayCommand<City>(RemoveFromFavoritesExecute);
            this.NewsItemSelected = new RelayCommand<NewsItem>(NewsItemSelectedExecute);
            this.FavoriteItemSelected = new RelayCommand<City>(FavoriteItemSelectedExecute);
            this.GoToLocation = new RelayCommand(GoToLocationExecute);
        }

        #endregion

        #region Properties


        public string TwoDaysImageFilename
        {
            get
            {
                return TwoDaysFileName;
            }
        }

        public string SevenDaysImageFilename
        {
            get
            {
                return SevenDaysFileName;
            }
        }

        public string PollenImageFilename
        {
            get
            {
                return PollenFileName;
            }
        }

        public string RegionalImageFilename
        {
            get
            {
                return RegionalFileName;
            }
        }

        public string CountryImageFilename
        {
            get
            {
                return CountryFileName;
            }
        }

        public IEnumerable<City> Cities
        {
            get
            {
                return Denmark.Cities;
            }
        }

        public BitmapSource TwoDaysImage
        {
            get
            {
                return twoDaysImage;
            }
            set
            {
                if (twoDaysImage != value)
                {
                    twoDaysImage = value;
                    RaisePropertyChanged(TwoDaysImagePropertyName);
                }
            }
        }

        public BitmapSource SevenDaysImage
        {
            get
            {
                return sevenDaysImage;
            }
            set
            {
                if (sevenDaysImage != value)
                {
                    sevenDaysImage = value;
                    RaisePropertyChanged(SevenDaysImagePropertyName);
                }
            }
        }

        public BitmapSource PollenImage
        {
            get
            {
                return pollenImage;
            }
            set
            {
                if (pollenImage != value)
                {
                    pollenImage = value;
                    RaisePropertyChanged(PollenImagePropertyName);
                }
            }
        }

        public BitmapSource RegionalImage
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

        public BitmapSource CountryImage
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

        public int PostalCode
        {
            get
            {
                if (currentLocation != null)
                {
                    int postal = DefaultPostalCode;
                    if (int.TryParse(currentLocation.PostalCode, out postal))
                    {
                        return Denmark.GetValidPostalCode(postal);
                    }
                    else
                    {
                        return postal;
                    }
                }

                return DefaultPostalCode;
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
                if (currentLocation != value)
                {
                    currentLocation = value;
                    
                    RaisePropertyChanged(CurrentLocationPropertyName);
                    
                    UpdateCurrentLocation();                    
                }
            }
        }

        public Region Region
        {
            get
            {
                return currentRegion;
            }
            set
            {
                if (currentRegion != value)
                {
                    currentRegion = value;
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
                return isLoading;
            }
            set
            {
                if (isLoading != value)
                {
                    isLoading = value;
                    RaisePropertyChanged("Loading");
                }
            }
        }

        public ICommand LoadWeatherInformation
        {
            get;
            private set;
        }

        public ICommand LoadPollenInformation
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

        #endregion

        #region Commands

        private void LoadWeatherInformationExecute()
        {
            if (InternetIsAvailable())
            {
                CountryImage = new BitmapImage(new Uri(AppResources.CountryImage));

                if (CurrentLocation == null)
                {
                    ResolveAddressFromGeoPosition();
                }
            }

            WeatherDataProvider.GetWeatherData((items, e) =>
            {
                WeatherItems.Clear();

                foreach (var item in items)
                {
                    WeatherItems.Add(item);
                }
            });
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
            if (Favorites != null &&
                Favorites.Any(c => c.PostalCode == PostalCode) == false &&
                Denmark.PostalCodes.ContainsKey(PostalCode))
            {
                Favorites.Add(new City()
                {
                    PostalCode = PostalCode,
                    Name = Denmark.PostalCodes[PostalCode]
                });

                SaveFavorites();
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
            {
                throw new ArgumentException("item");
            }

            var task = new WebBrowserTask()
            {
                URL = item.Link.AbsoluteUri
            };
            task.Show();
        }

        private void FavoriteItemSelectedExecute(City city)
        {
            if (city == null)
            {
                throw new ArgumentException("city");
            }

            var uri = string.Format("/Views/MainPage.xaml?PostalCode={0}&Time={1}",
                city.PostalCode,
                DateTime.Now.ToLongTimeString()
            );

            App.Navigate(new Uri(uri, UriKind.Relative));
        }

        private void GoToLocationExecute()
        {
            if (InternetIsAvailable())
            {
                CurrentLocation = null;
                ResolveAddressFromGeoPosition();
            }
        }

        #endregion

        #region Utility

        private bool InternetIsAvailable()
        {
            var available = NetworkInterface.GetIsNetworkAvailable();

            if (available == false)
            {
                isLoading = false;
                MessageBox.Show(AppResources.InternetError);

                return false;
            }

            return true;
        }

        private void UpdateCurrentLocation()
        {
            TwoDaysImage = new BitmapImage(new Uri(string.Format(
                AppResources.CityWeather2daysGraph, PostalCode)));

            SevenDaysImage = new BitmapImage(new Uri(string.Format(
                AppResources.CityWeather7daysGraph, PostalCode)));

            PollenImage = new BitmapImage(new Uri(string.Format(
                AppResources.PollenGraph, PostalCode)));

            RegionalImage = new BitmapImage(
                new Uri(Denmark.GetRegionImageFromPostalCode(PostalCode)));

            UpdateRegion(Denmark.GetRegionTextFromPostalCode(PostalCode));
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
                                Loading = false;

                                if (address != null)
                                {
                                    CurrentLocation = address;
                                    UpdateCurrentLocation();
                                }
                            }
                        });
                }
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