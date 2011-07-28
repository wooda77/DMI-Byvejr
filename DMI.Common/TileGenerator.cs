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
using System.Windows.Media.Imaging;
using System.IO.IsolatedStorage;
using Microsoft.Phone.Shell;
using System.Linq;

namespace DMI.Common
{
    public class TileGenerator
    {
        public static void GenerateTile(TileItem item)
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

            var source = new BitmapImage(new Uri("/Resources/Weather/3.png", UriKind.Relative));
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

                var tileImage = string.Format("/Shared/ShellContent/{0}.jpg", (int)item.TileType);
                var isoStoreTileImage = string.Format("isostore:/Shared/ShellContent/{0}.jpg", (int)item.TileType);

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

                var tileTypeUrlSegment = string.Format(AppSettings.TileTypeUrlSegment, (int)item.TileType);

                var dmiTile = ShellTile.ActiveTiles.FirstOrDefault(
                    x => x.NavigationUri.ToString().Contains(tileTypeUrlSegment));

                if (dmiTile != null)
                    dmiTile.Delete();

                var tileData = new StandardTileData
                {
                    BackgroundImage = new Uri(isoStoreTileImage, UriKind.Absolute),
                    Title = item.City.Name,
                };

                var address = string.Format(AppSettings.MainPageWithTileAddress,
                    item.City.PostalCode, item.City.Country, (int)item.TileType);

                var navigationUri = new Uri(address, UriKind.Relative);

                ShellTile.Create(navigationUri, tileData);
            };
        }
    }
}
