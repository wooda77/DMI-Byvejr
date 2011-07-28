using System;
using System.Net;
using System.Collections.Generic;
using System.Device.Location;
using DMI.Service;

namespace DMI.ViewModel
{
    public class BeachWeatherViewModel
    {
        public BeachWeatherViewModel()
        {
            this.Center = Denmark.CenterCoordinate;
        }

        public GeoCoordinate Center
        {
            get;
            private set;
        }

        public IEnumerable<Beach> Beaches
        {
            get
            {
                return Denmark.Beaches;
            }
        }
    }
}
