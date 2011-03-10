// 
// MainViewModel.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Collections.ObjectModel;
using System.Device.Location;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using System.Threading;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Phone.Tasks;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using DMI.Models;
using DMI.Properties;
using System.Net;
using System.Net.NetworkInformation;

namespace DMI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private const string CurrentLocationPropertyName = "CurrentLocation";
        private const string CityWeather2daysGraphPropertyName = "CityWeather2daysGraph";
        private const string CityWeather7daysGraphPropertyName = "CityWeather7daysGraph";
        private const string PollenGraphPropertyName = "PollenGraph";

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
        private GeoCoordinateWatcher watcher;
        private CivicAddress currentLocation;
        private bool loading = false;

        public MainViewModel()
        {
            this.Favorites = new ObservableCollection<City>();
            this.PollenData = new ObservableCollection<PollenItem>();
            this.NewsItems = new ObservableCollection<NewsItem>();

            if (IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites))
            {
                this.Favorites = (ObservableCollection<City>)
                    IsolatedStorageSettings.ApplicationSettings[App.Favorites];
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

        public string PostalCode
        {
            get
            {
                if (currentLocation != null)
                {
                    return currentLocation.PostalCode;
                }

                return string.Empty;
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

                    if (!string.IsNullOrEmpty(currentLocation.PostalCode))
                    {
                        UpdateCurrentLocation();
                    }

                    RaisePropertyChanged(CurrentLocationPropertyName);
                }
            }
        }

        public ObservableCollection<PollenItem> PollenData
        {
            get;
            set;
        }

        public ObservableCollection<NewsItem> NewsItems
        {
            get;
            set;
        }

        public ObservableCollection<City> Favorites
        {
            get;
            set;
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
            var postal = int.Parse(PostalCode);

            if (!Favorites.Any(c => c.PostalCode == postal))
            {
                Favorites.Add(new City()
                {
                    PostalCode = postal,
                    Name = Denmark.PostalCodes[postal]
                });
            }

            SaveFavorites();
        }

        private void RemoveFromFavoritesExecute(City city)
        {
            Favorites.Remove(city);
            SaveFavorites();
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
                MessageBox.Show("No internet connection is available. Please try again later.");
                return false;
            }

            return true;
        }

        private void UpdateCurrentLocation()
        {
            if (InternetIsAvailable())
            {
                CityWeather2daysGraph = new Uri(string.Format(
                    AppResources.CityWeather2daysGraph, currentLocation.PostalCode));

                CityWeather7daysGraph = new Uri(string.Format(
                    AppResources.CityWeather7daysGraph, currentLocation.PostalCode));

                PollenGraph = new Uri(string.Format(
                    AppResources.PollenGraph, currentLocation.PostalCode));
            }
        }

        private void ResolveAddressFromGeoPosition()
        {
            Loading = true;

            var timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(8);
            timer.Tick += (s, e) =>
            {
                if (Loading)
                {
                    Loading = false;
                    MessageBox.Show("Could not resolve the location.");
                }
            };
            timer.Start();

            watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.High);
            watcher.MovementThreshold = 1.0;
            watcher.PositionChanged += GeoCoordinateWatcher_PositionChanged;
            watcher.Start();
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
                            CurrentLocation = address;
                            Loading = false;
                        });
                }
            }
        }

        private void SaveFavorites()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites))
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