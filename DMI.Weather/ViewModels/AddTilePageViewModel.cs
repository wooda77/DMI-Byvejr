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
using System.Linq;
using DMI.Common;
using DMI.Service;
using GalaSoft.MvvmLight;
using Microsoft.Phone.Scheduler;

namespace DMI.ViewModels
{
    public class AddTilePageViewModel : ViewModelBase
    {
        public AddTilePageViewModel()
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

        public void GenerateTile(TileItem item)
        {
            TileGenerator.RefreshTileTask();

#if DEBUG
            ScheduledActionService.LaunchForTest(AppSettings.PeriodicTaskName, TimeSpan.FromSeconds(1));
#endif

            TileGenerator.GenerateTile(item, () => {}, true);
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

            return Denmark.PostalCodes[Denmark.DefaultPostalCode];
        }

        private Uri ImageIdToUri(string imageId)
        {
            return new Uri(string.Format("/Resources/Weather/{0}.png", imageId), UriKind.Relative);
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

