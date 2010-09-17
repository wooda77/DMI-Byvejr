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

        public int SelectedPivotPage
        {
            get;
            set;
        }

        public string PostalCode
        {
            get
            {
                if (address == null)
                {
                    return "8000";
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
                string uri = "http://servlet.dmi.dk/byvejr/servlet/byvejr_dag1?by={0}&mode=long";

                return new Uri(string.Format(uri, PostalCode));
            }
        }

        public Uri CityWeather7daysGraph
        {
            get
            {
                string uri = "http://servlet.dmi.dk/byvejr/servlet/byvejr?by={0}&tabel=dag3_9";

                return new Uri(string.Format(uri, PostalCode));
            }
        }

        public Uri PollenGraph
        {
            get
            {
                string uri = "http://servlet.dmi.dk/byvejr/servlet/pollen_dag1?by={0}";

                return new Uri(string.Format(uri, PostalCode));
            }
        }

        private CivicAddress address;

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
                    OnPropertyChanged("CityWeather2days");
                    OnPropertyChanged("CityWeather7days");
                    OnPropertyChanged("Pollen");
                }
            }
        }

        private PollenFeed pollenFeed = new PollenFeed();

        public ObservableCollection<PollenItem> PollenData
        {
            get
            {
                return pollenFeed.PollenData;
            }
        }

        private NewsFeed newsFeed = new NewsFeed();

        public ObservableCollection<NewsItem> NewsItems
        {
            get
            {
                return newsFeed.NewsItems;
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