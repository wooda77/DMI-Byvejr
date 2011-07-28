using System;
using System.ComponentModel;

namespace DMI.Common
{
    public class TileItem : INotifyPropertyChanged
    {
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        public TileItem()
        {
        }

        public TileItem(GeoLocationCity city)
        {
            this.City = city;
        }

        public DateTime Time
        {
            get;
            set;
        }

        public GeoLocationCity City
        {
            get;
            set;
        }

        public Uri CloudImage
        {
            get;
            set;
        }

        public string LocationName
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string Temperature
        {
            get;
            set;
        }

        public TileType TileType
        {
            get;
            set;
        }
    }
}
