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
using System.Device.Location;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Threading;
using DMI.Assets;
using DMI.Common;
using DMI.Properties;
using DMI.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Data;
using System.Windows.Media;

namespace DMI.Views
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();

            if (ApplicationBar == null)
            {
                SmartDispatcher.BeginInvoke(BuildApplicationBar);
            }
        }

        public MainPageViewModel ViewModel
        {
            get
            {
                return (DataContext as MainPageViewModel);
            }
        }

        private void BuildApplicationBar()
        {
            if (ApplicationBar != null)
                return;

            ApplicationBar = new ApplicationBar();
            
            if (App.CurrentThemeBackground == ThemeBackground.ThemeBackgroundDark)
                ApplicationBar.BackgroundColor = Colors.Black;
            else
                ApplicationBar.BackgroundColor = Colors.White;

            ApplicationBar.Opacity = 0.8;
            ApplicationBar.StateChanged += (sender, e) =>
                {
                    ApplicationBar.Opacity = e.IsMenuVisible ? 1 : 0.8;
                };

            var chooseCityAppBarButton = new ApplicationBarIconButton(
                new Uri("/Resources/Images/appbar.home.png", UriKind.Relative));
            chooseCityAppBarButton.Text = Properties.Resources.AppBar_ChooseCity;
            chooseCityAppBarButton.Click += ChooseCityAppBarButton_Click;

            var showFavoritesAppBarButton = new ApplicationBarIconButton(
                new Uri("/Resources/Images/appbar.favs.png", UriKind.Relative));
            showFavoritesAppBarButton.Text = Properties.Resources.AppBar_Favorites;
            showFavoritesAppBarButton.Click += ShowFavoritesAppBarButton_Click;

            var addtoFavoritesAppBarButton = new ApplicationBarIconButton(
                new Uri("/Resources/Images/appbar.addtofavs.png", UriKind.Relative));
            addtoFavoritesAppBarButton.Text = Properties.Resources.AppBar_AddToFavorites;
            addtoFavoritesAppBarButton.Click += AddtoFavoritesAppBarButton_Click;

            var goToLocationAppBarButton = new ApplicationBarIconButton(
                new Uri("/Resources/Images/appbar.location.png", UriKind.Relative));
            goToLocationAppBarButton.Text = Properties.Resources.AppBar_GoToLocation;
            goToLocationAppBarButton.Click += GoToLocationAppBarButton_Click;

            var liveTileMenu = new ApplicationBarMenuItem();
            liveTileMenu.Text = Properties.Resources.AppBar_LiveTile;
            liveTileMenu.Click += LiveTileMenu_Click;

            var supportMenu = new ApplicationBarMenuItem();
            supportMenu.Text = Properties.Resources.AppBar_Support;
            supportMenu.Click += SettingsMenu_Click;

            ApplicationBar.Buttons.Add(chooseCityAppBarButton);
            ApplicationBar.Buttons.Add(goToLocationAppBarButton);
            ApplicationBar.Buttons.Add(showFavoritesAppBarButton);
            ApplicationBar.Buttons.Add(addtoFavoritesAppBarButton);

            ApplicationBar.MenuItems.Add(liveTileMenu);
            ApplicationBar.MenuItems.Add(supportMenu);

            ApplicationBar.IsVisible = (PivotLayout.SelectedItem == WeatherPivotItem);

            if ((this.Orientation & PageOrientation.Landscape) == PageOrientation.Landscape)
            {
                ApplicationBar.IsVisible = false;
            }
        }

        private void Navigate(Uri source)
        {
            Dispatcher.BeginInvoke(() => NavigationService.Navigate(source));
        }

        private void LiveTileMenu_Click(object sender, EventArgs e)
        {
            if (ViewModel != null && 
                ViewModel.IsInitialized && 
                ViewModel.CurrentAddress != null)
            {
                string address = string.Format(AppSettings.AddTilePageAddress, 
                    ViewModel.CurrentAddress.PostalCode, ViewModel.CurrentAddress.CountryRegion);

                Navigate(new Uri(address, UriKind.Relative));
            }
        }

        private void SettingsMenu_Click(object sender, EventArgs e)
        {
            Navigate(new Uri(AppSettings.SupportPageAdress, UriKind.Relative));
        }

        private void ChooseCityAppBarButton_Click(object sender, EventArgs e)
        {
            Navigate(new Uri(AppSettings.ChooseCityPageAddress, UriKind.Relative));
        }

        private void ShowFavoritesAppBarButton_Click(object sender, EventArgs e)
        {
            if (PivotLayout != null)
            {
                PivotLayout.SelectedItem = FavoritesPivotItem;
            }
        }

        private void AddtoFavoritesAppBarButton_Click(object sender, EventArgs e)
        {
            if (DataContext != null && DataContext is MainPageViewModel)
            {
                (DataContext as MainPageViewModel).AddToFavorites.Execute(null);
            }
        }

        private void GoToLocationAppBarButton_Click(object sender, EventArgs e)
        {
            if (DataContext != null && DataContext is MainPageViewModel)
            {
                (DataContext as MainPageViewModel).GoToLocation.Execute(null);
            }
        }

        private void PivotLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SmartDispatcher.BeginInvoke(() =>
            {
                if (ApplicationBar != null && (this.Orientation & PageOrientation.Landscape) != PageOrientation.Landscape)
                {
                    ApplicationBar.IsVisible = (PivotLayout.SelectedItem == WeatherPivotItem);
                }
            });
        }

        private void OpenInLandscapeMode(object sender, GestureEventArgs e)
        {
            var image = sender as Image;
            if (image != null)
            {
                var source = image.Tag as Uri;
                if (string.IsNullOrEmpty(source.ToString()) == false)
                {
                    var address = string.Format(AppSettings.ImagePageAddress, Uri.EscapeDataString(source.ToString()));
                    Navigate(new Uri(address, UriKind.Relative));
                }
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (AppSettings.IsFirstStart)
            {
                Navigate(new Uri(AppSettings.SupportPageAdress, UriKind.Relative));
            }
            else
            {
                if (ViewModel.IsInitialized == false)
                    ViewModel.Initialize();

                var postalCode = NavigationContext.TryGetStringKey("PostalCode");
                var country = NavigationContext.TryGetStringKey("Country");

                if (string.IsNullOrEmpty(postalCode) == false && 
                    string.IsNullOrEmpty(country) == false)
                {
                    SmartDispatcher.BeginInvoke(() =>
                    {
                        ViewModel.ResolveLocation(postalCode, country);
                    });
                }
            }
        }

        private void Page_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (ApplicationBar != null)
            {
                if ((e.Orientation & PageOrientation.Landscape) == PageOrientation.Landscape)
                {
                    ApplicationBar.IsVisible = false;
                }
                else
                {
                    ApplicationBar.IsVisible = true;
                }
            }
        }

        private void RadarMenuItem_Tap(object sender, GestureEventArgs e)
        {
            Navigate(new Uri(AppSettings.RadarPageAddress, UriKind.Relative));
        }

        private void BeachWeatherMenuItem_Tap(object sender, GestureEventArgs e)
        {
            Navigate(new Uri(AppSettings.BeachWeatherPageAddress, UriKind.Relative));
        }

        private void UVIndexMenuItem_Tap(object sender, GestureEventArgs e)
        {
            Navigate(new Uri(AppSettings.UVIndexPageAddress, UriKind.Relative));
        }
    }
}