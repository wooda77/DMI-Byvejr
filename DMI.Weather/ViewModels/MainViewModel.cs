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
using System.Collections.Generic;
using System.Windows.Resources;
using System.Windows.Media.Imaging;

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

            // Debug

            WeatherItems.Add(new WeatherItem() 
            {
                Title = "Oversigt",
                Description = "Et højtryk over Vesteuropa breder sig mod nordøst, og det vil i den kommende periode stabiliserer vejret over Danmark"
            });

            WeatherItems.Add(new WeatherItem()
            {
                Title = "Udsigt, der gælder til fredag morgen:",
                Description = "I de østlige egne skyet og endnu stedvis lidt regn. I resten af landet tørt og mest klart vejr, men i løbet af natten mere skyet og stedvis tåget. Temp. mellem frysepunktet og 5 graders varme, i Jylland lokalt let frost. Svag til jævn vind fra nordvest og vest. Torsdag først mest skyet og stedvis tåget, men i løbet af dagen kommer der nogen sol de fleste steder. I løbet af eftermiddagen er der mulighed for lokale byger. Temp. op mellem 8 og 13 grader, og svag til let vind mest omkring nordvest. Natten til fredag tørt og mest klart vejr, men stedvis tåget eller skyet. Temp. ned mellem 0 og 5 grader, lokalt let frost. Ret svag skiftende vind."
            });

            WeatherItems.Add(new WeatherItem()
            {
                Title = "Fredag",
                Description = "Først stedvis skyet eller tåget, men ellers nogen eller en del sol de fleste steder. I løbet af eftermiddagen er der mulighed for lokale byger. Dagtemp. op mellem 8 og 13 grader, og svag til let vind mest omkring sydvest. Om natten mest tørt og ret skyet. Temp. ned mellem 1 og 5 grader, og svag til jævn vind fra sydvest."
            });

            WeatherItems.Add(new WeatherItem()
            {
                Title = "Lørdag",
                Description = "Mest tørt og ret skyet, men især i de sydøstlige egne også lidt sol. Temp. op mellem 10 og 15 grader. Svag til jævn vind fra sydvest, i Nordvestjylland op til frisk vind. Om natten efterhånden mest klart vejr. Temp. ned mellem 3 og 7 grader, og svag til jævn vind fra vest, i Nordjylland op til frisk vind."
            });

            WeatherItems.Add(new WeatherItem()
            {
                Title = "Søndag",
                Description = "Først på dagen stedvis skyet, men ellers en del sol de fleste steder. Temp. op mellem 10 og 15 grader. og svag til frisk vind fra vest og nordvest. Om natten klart vejr, og temp. ned mellem 3 og 7 grader. Svag til jævn vind fra vest og nordvest."
            });

            WeatherItems.Add(new WeatherItem()
            {
                Title = "Mandag",
                Description = "Tørt vejr med en del sol de fleste steder. Dagtemp. mellem 13 og 18 grader, men ved kyster med pålandsvind kun omkring 10 grader. Svag til jævn vind omkring vest. Om natten mest klart vejr, og temp. ned mellem 1 og 5 grader. Ret svag vind."
            });

            WeatherItems.Add(new WeatherItem()
            {
                Title = "Tirsdag og onsdag:",
                Description = "Antagelig tørt med ret uændrede temp."
            });
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

        public ObservableCollection<WeatherItem> WeatherItems
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
            UpdateCurrentLocation();

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
                    AppResources.CityWeather2daysGraph, PostalCode));

                CityWeather7daysGraph = new Uri(string.Format(
                    AppResources.CityWeather7daysGraph, PostalCode));

                PollenGraph = new Uri(string.Format(
                    AppResources.PollenGraph, PostalCode));
            }
        }

        private void ResolveAddressFromGeoPosition()
        {
            if (App.IsGPSEnabled == false)
            {
                MessageBox.Show("Could not resolve the location, because Location Servies aren't enabled.");
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
                        MessageBox.Show("Could not resolve the location.");
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
                                else
                                {
                                    MessageBox.Show("No internet connection is available. Please try again later.");
                                    Loading = false;
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

                return "1000";
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