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
using System.Collections.Generic;
using System.Linq;
using DMI.Common;
using DMI.Service;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace DMI.TaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        protected override void OnInvoke(ScheduledTask scheduledTask)
        {
            var latestTile = GetTile(TileType.Latest).FirstOrDefault();
            var customTiles = GetTile(TileType.Custom);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var task = Task.Factory.StartNew(() => RefreshTile(latestTile, TileType.Latest));
                    
                foreach (var customTile in customTiles)
                    task.ContinueWith(t => RefreshTile(customTile, TileType.Custom));
                    
                task.ContinueWith(t => NotifyComplete());
            });
        }

        private void RefreshTile(ShellTile tile, TileType type)
        {
            var queryString = tile.NavigationUri.ToString();
            queryString = queryString.Substring(queryString.IndexOf('?'));
            var segments = Utils.ParseQueryString(queryString);

            int postalCode = 0;
            if (int.TryParse(segments["PostalCode"], out postalCode))
            {
                var country = segments["Country"];
                var offset = segments["Offset"];

                GeoLocationCity city = null;

                switch (country)
                {
                    case Denmark.Name:
                        city = Denmark.PostalCodes[postalCode];
                        break;
                    case Greenland.Name:
                        city = Greenland.PostalCodes[postalCode];
                        break;
                    case FaroeIslands.Name:
                        city = FaroeIslands.PostalCodes[postalCode];
                        break;
                }

                if (city != null)
                {
                    if (type == TileType.Latest)
                    {
                        LiveTileWeatherProvider.GetForecast(city, DateTime.Now)
                            .ContinueWith(task =>
                            {
                                if (task.IsCompleted)
                                {
                                    var now = task.Result.FirstOrDefault(x => x.Df.Hour == DateTime.Now.Hour);
                                    if (now != null)
                                        Deployment.Current.Dispatcher.BeginInvoke(
                                            () => TileGenerator.GenerateTile(CreateTileItem(city, now, type)));
                                }
                            });
                    }
                    else if (type == TileType.Custom)
                    {
                        int offsetHour = int.Parse(offset);
                        var date = DateTime.Today.AddHours(offsetHour);

                        if (DateTime.Now.Hour > offsetHour)
                            date = date.AddDays(1);

                        LiveTileWeatherProvider.GetForecast(city, date)
                            .ContinueWith(task =>
                            {
                                if (task.IsCompleted)
                                {
                                    var custom = task.Result.FirstOrDefault(x => x.Df.Hour == date.Hour);
                                    if (custom != null)
                                    {
                                        var customTile = CreateTileItem(city, custom, type);
                                        customTile.Offset = offsetHour;

                                        Deployment.Current.Dispatcher.BeginInvoke(
                                            () => TileGenerator.GenerateTile(customTile));
                                    }
                                }
                            });
                    }
                }
            }
        }

        private ShellTile[] GetTile(TileType type)
        {
            string urlSegment = string.Format(AppSettings.TileTypeUrlSegment, (int)type, "");

            return ShellTile.ActiveTiles.Where(x => x.NavigationUri.ToString().Contains(urlSegment)).ToArray();
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

        private Uri ImageIdToUri(string imageId)
        {
            return new Uri(string.Format("/Resources/Weather/{0}.png", imageId), UriKind.Relative);
        }

        private TileItem CreateTileItem(GeoLocationCity city, LiveTileWeatherResponse response, TileType type)
        {
            return new TileItem(city)
            {
                TileType = type,
                LocationName = city.Name,
                Title = string.Format(Properties.Resources.LatestTitle, response.Df),
                CloudImage = ImageIdToUri(response.S),
                Temperature = response.T + '°',
            };
        }
    }
}
