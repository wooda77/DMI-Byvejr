using System;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Device.Location;

namespace DMI_Weather.ViewModels
{
    using Models;

    public class MainPageViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<NewsFeedItem> newsFeedItems
             = new ObservableCollection<NewsFeedItem>();

        public ObservableCollection<NewsFeedItem> NewsFeedItems
        {
            get
            {
                return newsFeedItems;
            }
            set
            {
                if (newsFeedItems != value)
                {
                    newsFeedItems = value;
                    NotifyPropertyChanged("NewsFeedItems");
                }
            }
        }

        private ObservableCollection<PollenItem> pollenFeedItems
             = new ObservableCollection<PollenItem>();

        public ObservableCollection<PollenItem> PollenFeedItems
        {
            get
            {
                return pollenFeedItems;
            }
            set
            {
                if (pollenFeedItems != value)
                {
                    pollenFeedItems = value;
                    NotifyPropertyChanged("PollenFeedItems");
                }
            }
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
                    
                    NotifyPropertyChanged("Address");
                    NotifyPropertyChanged("CityWeather2days");
                    NotifyPropertyChanged("CityWeather7days");
                    NotifyPropertyChanged("Pollen");
                }
            }
        }

        public Uri CityWeather2days
        {
            get
            {
                string uri = "http://servlet.dmi.dk/byvejr/servlet/byvejr_dag1?by={0}&mode=long";

                return new Uri(string.Format(uri, PostalCode));
            }
        }

        public Uri CityWeather7days
        {
            get
            {
                string uri = "http://servlet.dmi.dk/byvejr/servlet/byvejr?by={0}&tabel=dag3_9";

                return new Uri(string.Format(uri, PostalCode));
            }
        }

        public Uri Pollen
        {
            get
            {
                string uri = "http://servlet.dmi.dk/byvejr/servlet/pollen_dag1?by={0}";

                return new Uri(string.Format(uri, PostalCode));
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = (s, e) => { };

        protected void NotifyPropertyChanged(string property)
        {
            PropertyChanged(this, new PropertyChangedEventArgs(property));
        }

        #endregion
    }
}
