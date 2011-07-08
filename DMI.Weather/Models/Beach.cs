using System;
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

namespace DMI.Models
{
    public class Beach
    {
        private GeoCoordinate location;
        
        public Beach()
        {
            this.HasBlueFlag = true;
        }

        public GeoCoordinate Location
        {
            get
            {
                if (location == null)
                {
                    location = new GeoCoordinate(this.Latitude, this.Longitude);
                }

                return location;
            }
        }

        public int ID
        {
            get;
            set;
        }

        public double Latitude
        {
            get;
            set;
        }

        public double Longitude
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public bool HasBlueFlag
        {
            get;
            set;
        }
    }
}
