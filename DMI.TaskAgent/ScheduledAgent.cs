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
            var latestTiles = GetTile(TileType.Latest);
            var plusTiles = GetTile(TileType.PlusTile);
            var customTiles = GetTile(TileType.Custom);

            Deployment.Current.Dispatcher.BeginInvoke(() =>
            {
                var task = RefreshTile(latestTiles[0], TileType.Latest);

                var last = task;

                if (latestTiles.Length > 1)
                {
                    foreach (var tile in latestTiles.Skip(1))
                    {
                        var latestTile = tile;
                        task = task.ContinueWith(t => RefreshTile(latestTile, TileType.Latest));
                        last = task;
                    }
                }

                foreach (var tile in customTiles)
                {
                    var customTile = tile;
                    task = task.ContinueWith(t => RefreshTile(customTile, TileType.Custom));
                    last = task;
                }

                foreach (var tile in plusTiles)
                {
                    var plusTile = tile;
                    task = task.ContinueWith(t => RefreshTile(plusTile, TileType.PlusTile));
                    last = task;
                }

                // repeat last task, to ensure it's actually updated.
                task = task.ContinueWith(t => last);

                task.ContinueWith(OnNotifyComplete);
            });
        }

        private void OnNotifyComplete(Task task)
        {
            if (task.IsCompleted)
            {            
                Deployment.Current.Dispatcher.BeginInvoke(() => NotifyComplete());
            }
        }

        private Task RefreshTile(ShellTile tile, TileType type)
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
                    switch (type)
                    {
                        case TileType.Latest:
                            return RefreshLatestTile(city);
                        case TileType.Custom:
                            return RefreshCustomTile(offset, city);
                        case TileType.PlusTile:
                            return RefreshPlusTile(offset, city);
                    }
                }
            }

            throw new InvalidOperationException("Attempted to refresh a non-existant tile");
        }

        private Task RefreshLatestTile(GeoLocationCity city)
        {
            return LiveTileWeatherProvider.GetForecast(city, DateTime.Now)
                        .ContinueWith<Task>(task =>
                        {
                            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.AttachedToParent);

                            if (task.IsCompleted)
                            {
                                var now = task.Result.FirstOrDefault(x => x.Df.Hour == DateTime.Now.Hour);
                                if (now != null)               
                                {
                                    var latestTile = CreateTileItem(city, now, TileType.Latest);
                                    
                                    Deployment.Current.Dispatcher.BeginInvoke(
                                        () => TileGenerator.GenerateTile(latestTile, () => tcs.SetResult(true)));
                                }
                            }

                            return tcs.Task;
                        });
        }

        private Task RefreshCustomTile(string offset, GeoLocationCity city)
        {
            int offsetHour = int.Parse(offset);
            var date = DateTime.Today.AddHours(offsetHour);

            if (DateTime.Now.Hour > offsetHour)
                date = date.AddDays(1);

            return LiveTileWeatherProvider.GetForecast(city, date)
                        .ContinueWith<Task>(task =>
                        {
                            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.AttachedToParent);

                            if (task.IsCompleted)
                            {
                                var custom = task.Result.FirstOrDefault(x => x.Df.Hour == date.Hour);
                                if (custom != null)
                                {
                                    var customTile = CreateTileItem(city, custom, TileType.Custom);
                                    customTile.Offset = offsetHour;

                                    Deployment.Current.Dispatcher.BeginInvoke(
                                        () => TileGenerator.GenerateTile(customTile, () => tcs.SetResult(true)));
                                }
                            }

                            return tcs.Task;
                        });
        }

        private Task RefreshPlusTile(string offset, GeoLocationCity city)
        {
            int offsetHour = int.Parse(offset);
            var date = DateTime.Now.AddHours(offsetHour);

            return LiveTileWeatherProvider.GetForecast(city, date)
                        .ContinueWith<Task>(task =>
                        {
                            var tcs = new TaskCompletionSource<bool>(TaskCreationOptions.AttachedToParent);

                            if (task.IsCompleted)
                            {
                                var result = task.Result.FirstOrDefault(x => x.Df.Hour == date.Hour);
                                if (result != null)
                                {
                                    var plusTile = CreateTileItem(city, result, TileType.PlusTile);
                                    plusTile.Offset = offsetHour;

                                    Deployment.Current.Dispatcher.BeginInvoke(
                                        () => TileGenerator.GenerateTile(plusTile, () => tcs.SetResult(true)));
                                }
                            }

                            return tcs.Task;
                        });
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
                Description = response.Prosa,
            };
        }
    }
}
