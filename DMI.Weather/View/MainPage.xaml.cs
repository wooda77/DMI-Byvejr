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
using DMI.ViewModel;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DMI.View
{
    public partial class MainPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        public MainViewModel ViewModel
        {
            get
            {
                return (DataContext as MainViewModel);
            }
        }

        private void BuildApplicationBar()
        {
            if (ApplicationBar != null)
                return;

            ApplicationBar = new ApplicationBar();

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
        }

        private void LiveTileMenu_Click(object sender, EventArgs e)
        {
            string address = string.Format(AppSettings.AddTilePageAddress, 
                ViewModel.CurrentAddress.PostalCode, ViewModel.CurrentAddress.CountryRegion);

            NavigationService.Navigate(new Uri(address, UriKind.Relative));
        }

        private void SettingsMenu_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(AppSettings.SupportPageAdress, UriKind.Relative));
        }

        private void ChooseCityAppBarButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri(AppSettings.ChooseCityPageAddress, UriKind.Relative));
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
            if (DataContext != null && DataContext is MainViewModel)
            {
                (DataContext as MainViewModel).AddToFavorites.Execute(null);
            }
        }

        private void GoToLocationAppBarButton_Click(object sender, EventArgs e)
        {
            if (DataContext != null && DataContext is MainViewModel)
            {
                (DataContext as MainViewModel).GoToLocation.Execute(null);
            }
        }

        private void PivotLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SmartDispatcher.BeginInvoke(() =>
            {
                if (ApplicationBar != null)
                {
                    ApplicationBar.IsVisible = (PivotLayout.SelectedItem == WeatherPivotItem);
                }
            });
        }

        private void OpenInLandscapeMode(object sender, GestureEventArgs e)
        {
            if (e.OriginalSource is Image)
            {
                var image = (e.OriginalSource as Image).Source as BitmapImage;
                var address = string.Format(AppSettings.ImagePageAddress, Uri.EscapeDataString(image.UriSource.ToString()));

                NavigationService.Navigate(new Uri(address, UriKind.Relative));
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            if (AppSettings.IsFirstStart)
            {
                NavigationService.Navigate(new Uri(AppSettings.SupportPageAdress, UriKind.Relative));
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

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            try
            {
                if (State.ContainsKey(AppSettings.PivotItemKey))
                {
                    State[AppSettings.PivotItemKey] = PivotLayout.SelectedIndex;
                }
                else
                {
                    State.Add(AppSettings.PivotItemKey, PivotLayout.SelectedIndex);
                }
            }
            catch (InvalidOperationException)
            {
                // Fix for Emulator-only crashes.
            }
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Image)
            {
                var image = sender as Image;
                var source = image.Source as BitmapSource;
                var filename = (string)image.Tag;

                image.CropImageBorders(e.NewSize);
            }
        }

        private void PivotLayout_Loaded(object sender, RoutedEventArgs e)
        {
            try
            {
                if (State.ContainsKey(AppSettings.PivotItemKey))
                {
                    var index = (int)State[AppSettings.PivotItemKey];
                    PivotLayout.SelectedIndex = index;
                }
            }
            catch (Exception)
            {
                // Fix for loading bug in the emulator.
            }

            if (ApplicationBar == null)
            {
                SmartDispatcher.BeginInvoke(BuildApplicationBar);
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
            NavigationService.Navigate(new Uri(AppSettings.RadarPageAddress, UriKind.Relative));
        }

        private void BeachWeatherMenuItem_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri(AppSettings.BeachWeatherPageAddress, UriKind.Relative));
        }

        private void NewsPivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsInitialized)
                ViewModel.LoadNewsFeed.Execute(null);
        }

        private void NewsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.IsInitialized)
                ViewModel.NewsItemSelected.Execute(NewsListBox.SelectedItem);
        }

        private void FavoritesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ViewModel.IsInitialized)
                ViewModel.FavoriteItemSelected.Execute(FavoritesListBox.SelectedItem);
        }

        private void FavoritesPivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel.IsInitialized)
                ViewModel.LoadFavorites.Execute(null);
        }
    }
}