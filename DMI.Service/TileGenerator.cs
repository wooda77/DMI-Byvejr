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
using System.IO.IsolatedStorage;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;

namespace DMI.Service
{
    public class TileGenerator
    {
        public static void RefreshTile(TileItem item, Action completed)
        {
            if (item.TileType == TileType.Custom)
            {
                var time = DateTime.Today.AddHours(item.Offset);

                if (item.Offset < 6)
                    item.Title = string.Format(Properties.Resources.Tile_Night, time);
                else if (item.Offset < 12)
                    item.Title = string.Format(Properties.Resources.Tile_Morning, time);
                else if (item.Offset < 18)
                    item.Title = string.Format(Properties.Resources.Tile_Afternoon, time);
                else
                    item.Title = string.Format(Properties.Resources.Tile_Evening, time);
            }
            else if (item.TileType == TileType.PlusTile)
            {
                var hour = DateTime.Now.AddHours(item.Offset);
                var time = DateTime.Today.AddHours(hour.Hour);

                if (time.Hour < 6)
                    item.Title = string.Format(Properties.Resources.Tile_Night, time);
                else if (time.Hour < 12)
                    item.Title = string.Format(Properties.Resources.Tile_Morning, time);
                else if (time.Hour < 18)
                    item.Title = string.Format(Properties.Resources.Tile_Afternoon, time);
                else
                    item.Title = string.Format(Properties.Resources.Tile_Evening, time);
            }

            var fontFamily = new FontFamily("Segoe WP");
            var fontForeground = new SolidColorBrush(Colors.White);

            var backgroundRectangle = new Rectangle();
            backgroundRectangle.Height = 173;
            backgroundRectangle.Width = 173;
            backgroundRectangle.Fill = new SolidColorBrush(Color.FromArgb(255, 13, 45, 132));

            var source = new BitmapImage(item.CloudImage);
            source.CreateOptions = BitmapCreateOptions.None;
            
            source.ImageFailed += (sender, e) =>
            {
                System.Diagnostics.Debugger.Break();
            };

            source.ImageOpened += (sender, e) => 
            {
                var cloudImage = new Image();
                cloudImage.Source = source;
                cloudImage.Width = 100;
                cloudImage.Height = 64;

                TextBlock titleTextBlock = new TextBlock();
                titleTextBlock.Text = item.Title;
                titleTextBlock.FontSize = 20;
                titleTextBlock.Foreground = fontForeground;
                titleTextBlock.FontFamily = fontFamily;

                TextBlock tempTextBlock = new TextBlock();
                tempTextBlock.Text = item.Temperature;
                tempTextBlock.FontSize = 30;
                tempTextBlock.Foreground = fontForeground;
                tempTextBlock.FontFamily = fontFamily;

                string fileName = string.Format("{0}_{1}_{2}.jpg",
                    item.Offset, item.City.PostalCode, item.City.ShortCountryName);

                var tileImage = string.Format("/Shared/ShellContent/{0}", fileName);
                var isoStoreTileImage = string.Format("isostore:/Shared/ShellContent/{0}", fileName);

                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var stream = store.CreateFile(tileImage);

                    var bitmap = new WriteableBitmap(173, 173);

                    bitmap.Render(backgroundRectangle, new TranslateTransform());

                    bitmap.Render(cloudImage, new TranslateTransform()
                    {
                        X = 8,
                        Y = 54
                    });

                    bitmap.Render(tempTextBlock, new TranslateTransform()
                    {
                        X = 124,
                        Y = 63
                    });

                    bitmap.Render(titleTextBlock, new TranslateTransform()
                    {
                        X = 12,
                        Y = 6
                    });

                    bitmap.Invalidate();
                    bitmap.SaveJpeg(stream, 173, 173, 0, 100);

                    stream.Close();
                }

                var address = string.Format(AppSettings.MainPageWithTileAddress,
                    item.City.PostalCode, item.City.Country, (int)item.TileType, item.Offset);

                var dmiTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString() == address);

                var tileData = new StandardTileData
                {
                    BackgroundImage = new Uri(isoStoreTileImage, UriKind.Absolute),
                    Title = item.City.Name,
                };

                var navigationUri = new Uri(address, UriKind.Relative);

                if (dmiTile != null)
                {
                    dmiTile.Update(tileData);
                }

                completed();
            };
        }

        public static void RefreshTileTask()
        {
            PeriodicTask task = new PeriodicTask(AppSettings.PeriodicTaskName);
            task.Description = Properties.Resources.PeriodicTaskHelpMessage;
            task.ExpirationTime = DateTime.Now.AddDays(14);

            try
            {
                ScheduledActionService.Remove(AppSettings.PeriodicTaskName);
            }
            catch (InvalidOperationException)
            {
            }

            try
            {
                ScheduledActionService.Add(task);
            }
            catch (InvalidOperationException)
            {
            }
        }

        public static void GenerateTile(TileItem item, Action completed)
        {
            if (item.TileType == TileType.Custom)
            {
                var time = DateTime.Today.AddHours(item.Offset);

                if (item.Offset < 6)
                    item.Title = string.Format(Properties.Resources.Tile_Night, time);
                else if (item.Offset < 12)
                    item.Title = string.Format(Properties.Resources.Tile_Morning, time);
                else if (item.Offset < 18)
                    item.Title = string.Format(Properties.Resources.Tile_Afternoon, time);
                else
                    item.Title = string.Format(Properties.Resources.Tile_Evening, time);
            }
            else if (item.TileType == TileType.PlusTile)
            {
                var hour = DateTime.Now.AddHours(item.Offset);
                var time = DateTime.Today.AddHours(hour.Hour);

                if (time.Hour < 6)
                    item.Title = string.Format(Properties.Resources.Tile_Night, time);
                else if (time.Hour < 12)
                    item.Title = string.Format(Properties.Resources.Tile_Morning, time);
                else if (time.Hour < 18)
                    item.Title = string.Format(Properties.Resources.Tile_Afternoon, time);
                else
                    item.Title = string.Format(Properties.Resources.Tile_Evening, time);
            }

            var fontFamily = new FontFamily("Segoe WP");
            var fontForeground = new SolidColorBrush(Colors.White);

            var backgroundRectangle = new Rectangle();
            backgroundRectangle.Height = 173;
            backgroundRectangle.Width = 173;
            backgroundRectangle.Fill = new SolidColorBrush(Color.FromArgb(255, 13, 45, 132));

            var source = new BitmapImage(item.CloudImage);
            source.CreateOptions = BitmapCreateOptions.None;

            source.ImageFailed += (sender, e) =>
            {
                System.Diagnostics.Debugger.Break();
            };

            source.ImageOpened += (sender, e) =>
            {
                var cloudImage = new Image();
                cloudImage.Source = source;
                cloudImage.Width = 100;
                cloudImage.Height = 64;

                TextBlock titleTextBlock = new TextBlock();
                titleTextBlock.Text = item.Title;
                titleTextBlock.FontSize = 20;
                titleTextBlock.Foreground = fontForeground;
                titleTextBlock.FontFamily = fontFamily;

                TextBlock tempTextBlock = new TextBlock();
                tempTextBlock.Text = item.Temperature;
                tempTextBlock.FontSize = 30;
                tempTextBlock.Foreground = fontForeground;
                tempTextBlock.FontFamily = fontFamily;

                string fileName = string.Format("{0}_{1}_{2}.jpg",
                    item.Offset, item.City.PostalCode, item.City.ShortCountryName);

                var tileImage = string.Format("/Shared/ShellContent/{0}", fileName);
                var isoStoreTileImage = string.Format("isostore:/Shared/ShellContent/{0}", fileName);

                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    var stream = store.CreateFile(tileImage);

                    var bitmap = new WriteableBitmap(173, 173);

                    bitmap.Render(backgroundRectangle, new TranslateTransform());

                    bitmap.Render(cloudImage, new TranslateTransform()
                    {
                        X = 8,
                        Y = 54
                    });

                    bitmap.Render(tempTextBlock, new TranslateTransform()
                    {
                        X = 124,
                        Y = 63
                    });

                    bitmap.Render(titleTextBlock, new TranslateTransform()
                    {
                        X = 12,
                        Y = 6
                    });

                    bitmap.Invalidate();
                    bitmap.SaveJpeg(stream, 173, 173, 0, 100);

                    stream.Close();
                }

                var address = string.Format(AppSettings.MainPageWithTileAddress,
                    item.City.PostalCode, item.City.Country, (int)item.TileType, item.Offset);

                var dmiTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString() == address);

                var tileData = new StandardTileData
                {
                    BackgroundImage = new Uri(isoStoreTileImage, UriKind.Absolute),
                    Title = item.City.Name,
                };

                var navigationUri = new Uri(address, UriKind.Relative);

                if (dmiTile != null)
                {
                    dmiTile.Delete();
                    ShellTile.Create(navigationUri, tileData);
                }
                else
                {
                    ShellTile.Create(navigationUri, tileData);
                }

                completed();
            };
        }
    }
}
