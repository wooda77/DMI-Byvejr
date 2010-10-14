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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace DMI.ViewModels
{
    using Microsoft.Phone.Tasks;
    using Models;
    using Properties;

    public class MainViewModel : ViewModelBase
    {
        private const string CurrentLocationPropertyName = "CurrentLocation";
        private const string CityWeather2daysGraphPropertyName = "CityWeather2daysGraph";
        private const string CityWeather7daysGraphPropertyName = "CityWeather7daysGraph";
        private const string PollenGraphPropertyName = "PollenGraph";

        private CivicAddress currentLocation 
            = new CivicAddress();

        private WeatherDataProvider weatherDataProvider 
            = new WeatherDataProvider();

        public MainViewModel()
        {
            Favorites = new ObservableCollection<City>();
            PollenData = new ObservableCollection<PollenItem>();
            NewsItems = new ObservableCollection<NewsItem>();

            if (IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites))
            {
                Favorites = (ObservableCollection<City>)
                    IsolatedStorageSettings.ApplicationSettings[App.Favorites];
            }
        }

        #region Properties

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

        public string PostalCode
        {
            get
            {
                if ((currentLocation == null) || string.IsNullOrEmpty(currentLocation.PostalCode))
                {
                    return AppResources.DefaultPostal;
                }
                else
                {
                    return currentLocation.PostalCode;
                }
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
                else
                {
                    return AppResources.DefaultCity;
                }
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

                    RaisePropertyChanged(CurrentLocationPropertyName);
                    RaisePropertyChanged(CityWeather2daysGraphPropertyName);
                    RaisePropertyChanged(CityWeather7daysGraphPropertyName);
                    RaisePropertyChanged(PollenGraphPropertyName);
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

        #endregion

        #region Commands

        private ICommand loadWeatherInformation;

        public ICommand LoadWeatherInformation
        {
            get
            {
                if (loadWeatherInformation == null)
                {
                    loadWeatherInformation = new RelayCommand(() =>
                    {
                        CurrentLocation = ResolveAddressFromGeoPosition();
                    });
                }

                return loadWeatherInformation;
            }
        }

        private ICommand loadPollenInformation;

        public ICommand LoadPollenInformation
        {
            get
            {
                if (loadPollenInformation == null)
                {
                    loadPollenInformation = new RelayCommand(() =>
                    {
                        weatherDataProvider.GetPollenData((items, e) =>
                        {
                            PollenData.Clear();

                            foreach (var item in items)
                            {
                                PollenData.Add(item);
                            }
                        });
                    });
                }

                return loadPollenInformation;
            }
        }

        private ICommand loadNewsFeed;

        public ICommand LoadNewsFeed
        {
            get
            {
                if (loadNewsFeed == null)
                {
                    loadNewsFeed = new RelayCommand(() =>
                    {
                        weatherDataProvider.GetNewsItems((items, e) =>
                        {
                            NewsItems.Clear();

                            foreach (var item in items)
                            {
                                NewsItems.Add(item);
                            }
                        });
                    });
                }

                return loadNewsFeed;
            }
        }

        private ICommand loadFavorites;

        public ICommand LoadFavorites
        {
            get
            {
                if (loadFavorites == null)
                {
                    loadFavorites = new RelayCommand(() =>
                    {
                        if (IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites))
                        {
                            Favorites = (ObservableCollection<City>)
                                IsolatedStorageSettings.ApplicationSettings[App.Favorites];
                        }
                    });
                }

                return loadFavorites;
            }
        }

        private ICommand addToFavorites;

        public ICommand AddToFavorites
        {
            get
            {
                if (addToFavorites == null)
                {
                    addToFavorites = new RelayCommand(() =>
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
                    });
                }

                return addToFavorites;
            }
        }

        private ICommand cropBorders;

        public ICommand CropBorders
        {
            get
            {
                if (cropBorders == null)
                {
                    cropBorders = new RelayCommand<Image>(image =>
                    {
                        ImageUtility.CropImageBorders(image);
                    });
                }

                return cropBorders;
            }
        }

        private ICommand newsItemSelected;

        public ICommand NewsItemSelected
        {
            get
            {
                if (newsItemSelected == null)
                {
                    newsItemSelected = new RelayCommand<NewsItem>(item =>
                    {
                        var task = new WebBrowserTask()
                        {
                            URL = item.Link.AbsoluteUri
                        };
                        task.Show();
                    });
                }

                return newsItemSelected;
            }
        }

        private ICommand favoriteItemSelected;

        public ICommand FavoriteItemSelected
        {
            get
            {
                if (favoriteItemSelected == null)
                {
                    favoriteItemSelected = new RelayCommand<City>(city =>
                    {
                        var uri = string.Format("/Views/MainPage.xaml?PostalCode={0}", city.PostalCode);

                        App.Navigate(new Uri(uri, UriKind.Relative));
                    });
                }

                return favoriteItemSelected;
            }
        }

        #endregion

        #region Utility

        private CivicAddress ResolveAddressFromGeoPosition()
        {
            using (var watcher = new GeoCoordinateWatcher(GeoPositionAccuracy.Default))
            {
                watcher.TryStart(false, TimeSpan.FromMilliseconds(1000));

                var resolver = new CivicAddressResolver();
                if (!watcher.Position.Location.IsUnknown)
                {
                    return resolver.ResolveAddress(watcher.Position.Location);
                }
            }

            return default(CivicAddress);
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