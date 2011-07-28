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
using Microsoft.Phone.Shell;
using Microsoft.Phone.Scheduler;
using System.Windows;

namespace DMI.Common
{
    public class TileGenerator
    {
        public static void GenerateTile(TileItem item, Action completed)
        {
            if (item.TileType == TileType.PlusSix)
            {
                if (DateTime.Now.Hour < 6)
                    item.Title = string.Format(Properties.Resources.Tile_Morning, item.Time);
                else if (DateTime.Now.Hour < 12)
                    item.Title = string.Format(Properties.Resources.Tile_Afternoon, item.Time);
                else if (DateTime.Now.Hour < 18)
                    item.Title = string.Format(Properties.Resources.Tile_Evening, item.Time);
                else
                    item.Title = string.Format(Properties.Resources.Tile_Night, item.Time);
            }
            else if (item.TileType == TileType.PlusTwelve)
            {
                if (DateTime.Now.Hour < 6)
                    item.Title = string.Format(Properties.Resources.Tile_Afternoon, item.Time);
                else if (DateTime.Now.Hour < 12)
                    item.Title = string.Format(Properties.Resources.Tile_Evening, item.Time);
                else if (DateTime.Now.Hour < 18)
                    item.Title = string.Format(Properties.Resources.Tile_Night, item.Time);
                else
                    item.Title = string.Format(Properties.Resources.Tile_Morning, item.Time);
            }

            var fontFamily = new FontFamily("Segoe WP");
            var fontForeground = new SolidColorBrush(Colors.White);

            var backgroundRectangle = new Rectangle();
            backgroundRectangle.Height = 173;
            backgroundRectangle.Width = 173;
            backgroundRectangle.Fill = new SolidColorBrush(Color.FromArgb(255, 13, 45, 132));

            var source = new BitmapImage(item.CloudImage);
            source.CreateOptions = BitmapCreateOptions.None;
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
                    (int)item.TileType, 
                    item.City.PostalCode, 
                    item.City.ShortCountryName);

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
                    item.City.PostalCode, item.City.Country, (int)item.TileType);

                var dmiTile = ShellTile.ActiveTiles.FirstOrDefault(x => x.NavigationUri.ToString() == address);

                var tileData = new StandardTileData
                {
                    BackgroundImage = new Uri(isoStoreTileImage, UriKind.Absolute),
                    Title = item.City.Name,
                };

                if (dmiTile != null)
                {    
                    dmiTile.Update(tileData);    

                    completed();
                }
                else
                {
                    var navigationUri = new Uri(address, UriKind.Relative);

                    ShellTile.Create(navigationUri, tileData);

                    completed();
                }
            };
        }
    }
}
