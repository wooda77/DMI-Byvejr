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
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using DMI.Data;
using DMI.Service;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Scheduler;

namespace DMI.ViewModels
{
    public class AddTilePageViewModel : ViewModelBase
    {
        public AddTilePageViewModel()
        {
            this.Tiles = new ObservableCollection<TileItem>();
            this.TileSelected = new RelayCommand<TileItem>(OnTileSelected);

            if (IsInDesignMode)
            {
                LoadCity(8260, Denmark.Name);
            }
        }

        public ICommand TileSelected
        {
            get;
            private set;
        }

        public ObservableCollection<TileItem> Tiles
        {
            get;
            private set;
        }

        public void LoadCity(int postalCode, string country)
        {
            var city = GetCityFromZipAndCountry(postalCode, country);

            Tiles.Clear();

            LiveTileWeatherProvider.GetForecast(city, DateTime.Now)
                .ContinueWith(task =>
                {
                    if (task.IsCompleted)
                    {
                        var result = task.Result;

                        AddLatestTile(city, result);
                        AddPlusTile(city, result, 6);
                        AddPlusTile(city, result, 8);
                    }
                });
        }

        private void AddLatestTile(GeoLocationCity city, List<LiveTileWeatherResponse> result)
        {
            var now = result.FirstOrDefault(x => x.Df.Hour == DateTime.Now.Hour);
            if (now != null)
            {
                var latestTile = new TileItem(city)
                {
                    LocationName = city.Name,
                    Title = string.Format(Properties.Resources.LatestTitle, now.Df),
                    CloudImage = ImageIdToUri(now.S),
                    Temperature = now.T + '°',
                    TileType = TileType.Latest,
                    Description = now.Prosa
                };

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.Tiles.Add(latestTile);
                });
            }
        }

        private void AddPlusTile(GeoLocationCity city, List<LiveTileWeatherResponse> result, int offset)
        {
            var custom = result.FirstOrDefault(x => x.Df.Hour == DateTime.Now.AddHours(offset).Hour);
            if (custom != null)
            {
                var customTile = new TileItem(city)
                {
                    LocationName = city.Name,
                    Title = string.Format(Properties.Resources.PlusTitleTitle, custom.Df, offset),
                    CloudImage = ImageIdToUri(custom.S),
                    Temperature = custom.T + '°',
                    TileType = TileType.PlusTile,
                    Offset = offset,
                    Description = custom.Prosa
                };

                Deployment.Current.Dispatcher.BeginInvoke(() =>
                {
                    this.Tiles.Add(customTile);
                });
            }
        }

        public void GenerateTile(TileItem item)
        {
            TileGenerator.RefreshTileTask();

#if DEBUG
            ScheduledActionService.LaunchForTest(AppSettings.PeriodicTaskName, TimeSpan.FromSeconds(1));
#endif

            TileGenerator.GenerateTile(item, delegate() {});
        }

        private void OnTileSelected(TileItem item)
        {
            GenerateTile(item);
        }

        private GeoLocationCity GetCityFromZipAndCountry(int postalCode, string country)
        {
            try
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
            } 
            catch (Exception)
            {
            }

            return Denmark.PostalCodes[Denmark.DefaultPostalCode];
        }

        private Uri ImageIdToUri(string imageId)
        {
            return new Uri(string.Format("/Resources/Weather/{0}.png", imageId), UriKind.Relative);
        }
    }
}

