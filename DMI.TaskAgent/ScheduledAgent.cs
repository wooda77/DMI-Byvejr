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

namespace DMI.TaskAgent
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        protected override void OnInvoke(ScheduledTask task)
        {
            var latestTiles = GetTiles(TileType.Latest);
            var plus6Tiles = GetTiles(TileType.PlusSix);
            var plus12Tiles = GetTiles(TileType.PlusTwelve);
            if (latestTiles.Any() || plus6Tiles.Any() || plus12Tiles.Any())
            {
                foreach (var tile in latestTiles)
                    RefreshTile(tile, TileType.Latest);

                foreach (var tile in plus6Tiles)
                    RefreshTile(tile, TileType.PlusSix);

                foreach (var tile in plus12Tiles)
                    RefreshTile(tile, TileType.PlusTwelve);
            }
            else
            {
                NotifyComplete();
            }
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
                        LiveTileWeatherProvider.GetForecast(city, DateTime.Now,
                            (response, exception) =>
                            {
                                var now = response.FirstOrDefault(x => x.Df.Hour == DateTime.Now.Hour);
                                if (now != null)
                                {
                                    TileGenerator.GenerateTile(CreateTileItem(city, now, type));

                                    NotifyComplete();
                                }
                            });
                    }
                    else if (type == TileType.PlusSix)
                    {
                        var date = DateTime.Now;
                        
                        if (DateTime.Now.Hour >= 18)
                            date = date.AddDays(1);

                        LiveTileWeatherProvider.GetForecast(city, date,
                            (response, exception) =>
                            {
                                var plus6 = response.FirstOrDefault(x => x.Df.Hour == DateTime.Now.Hour);
                                if (plus6 != null)
                                {
                                    TileGenerator.GenerateTile(CreateTileItem(city, plus6, type));

                                    NotifyComplete();
                                }
                            });
                    }
                    else if (type == TileType.PlusTwelve)
                    {
                        var date = DateTime.Now;

                        if (DateTime.Now.Hour >= 12)
                            date = date.AddDays(1);

                        LiveTileWeatherProvider.GetForecast(city, date,
                            (response, exception) =>
                            {
                                var plus12 = response.FirstOrDefault(x => x.Df.Hour == DateTime.Now.Hour);
                                if (plus12 != null)
                                {
                                    TileGenerator.GenerateTile(CreateTileItem(city, plus12, type));

                                    NotifyComplete();
                                }
                            });
                    }
                }
                else
                {
                    NotifyComplete();
                }
            }
            else
            {
                NotifyComplete();
            }
        }

        private IEnumerable<ShellTile> GetTiles(TileType type)
        {
            string urlSegment = string.Format(AppSettings.TileTypeUrlSegment, (int)type);

            return ShellTile.ActiveTiles.Where(x => x.NavigationUri.ToString().Contains(urlSegment));
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
