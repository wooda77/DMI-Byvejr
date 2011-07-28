using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DMI.Service;
using GalaSoft.MvvmLight;
using System.ComponentModel;
using DMI.Properties;
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using System.Windows.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;
using System.Windows;
using DMI.Common;
using System.Windows.Markup;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using System.Windows.Shapes;

namespace DMI.ViewModel
{
    public class AddTileViewModel : ViewModelBase
    {
        public AddTileViewModel()
        {
            if (IsInDesignMode)
            {
                LoadCity(8260, Denmark.Name);
            }
        }

        public TileItem Latest
        {
            get;
            set;
        }

        public TileItem PlusSixHours
        {
            get;
            set;
        }

        public TileItem PlusTwelveHours
        {
            get;
            set;
        }

        public void LoadCity(int postalCode, string country)
        {
            var city = GetCityFromZipAndCountry(postalCode, country);

            // Today
            LiveTileWeatherProvider.GetForecast(city, DateTime.Now,
                (response, exception) =>
                {
                    var now = response.FirstOrDefault(x => x.Df.Hour == DateTime.Now.Hour);
                    if (now != null)
                        this.Latest = CreateTileItem(city, now, TileType.Latest);

                    if (DateTime.Now.Hour < 18)
                    {
                        var plus6 = response.FirstOrDefault(x => x.Df.Hour == DateTime.Now.AddHours(6).Hour);
                        if (plus6 != null)
                            this.PlusSixHours = CreateTileItem(city, plus6, TileType.PlusSix);
                    }

                    if (DateTime.Now.Hour < 12)
                    {
                        var plus12 = response.FirstOrDefault(x => x.Df.Hour == DateTime.Now.AddHours(12).Hour);
                        if (plus12 != null)
                            this.PlusTwelveHours = CreateTileItem(city, plus12, TileType.PlusTwelve);
                    }
                });

            // Tomorrow, if necessary
            if (DateTime.Now.Hour >= 12)
            {
                LiveTileWeatherProvider.GetForecast(city, DateTime.Now.AddDays(1),
                    (response, exception) =>
                    {
                        if (DateTime.Now.Hour >= 18)
                        {
                            var plus6 = response.FirstOrDefault(x => x.Df.Hour == DateTime.Now.AddHours(6).Hour);
                            if (plus6 != null)
                                this.PlusSixHours = CreateTileItem(city, plus6, TileType.PlusSix);
                        }

                        if (DateTime.Now.Hour >= 12)
                        {
                            var plus12 = response.FirstOrDefault(x => x.Df.Hour == DateTime.Now.AddHours(12).Hour);
                            if (plus12 != null)
                                this.PlusTwelveHours = CreateTileItem(city, plus12, TileType.PlusTwelve);
                        }
                    });
            }
        }        

        public void AddTile(TileItem item)
        {
            TileGenerator.GenerateTile(item);
        }

        private GeoLocationCity GetCityFromZipAndCountry(int postalCode, string country)
        {
            switch (country)
            {
                case Denmark.Name:
                    return Denmark.PostalCodes[postalCode];
                case Greenland.Name:
                    return Greenland.PostalCodes[postalCode];
                case FaroeIslands.Name:
                    return FaroeIslands.PostalCodes[postalCode];
            }

            return Denmark.PostalCodes[postalCode];
        }

        private TileItem CreateTileItem(GeoLocationCity city, LiveTileWeatherResponse response, TileType type)
        {
            string title = string.Format(Properties.Resources.LatestTitle, response.Df);

            switch (type)
            {
                case TileType.Latest:
                    title = string.Format(Properties.Resources.LatestTitle, response.Df);
                    break;
                case TileType.PlusSix:
                    title = string.Format(Properties.Resources.PlusSixHoursTitle, response.Df);
                    break;
                case TileType.PlusTwelve:
                    title = string.Format(Properties.Resources.PlusTwelveHoursTitle, response.Df);
                    break;
            }

            return new TileItem(city)
            {
                Time = response.Df,
                TileType = type,
                LocationName = city.Name,
                Title = title,
                CloudImage = ImageIdToUri(response.S),
                Temperature = response.T + '°',
            };
        }
    }
}

