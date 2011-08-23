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
using DMI.Service;
using DMI.Properties;
using DMI.ViewModels;
using DMI.Controls;
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
        }

        private MainPageViewModel ViewModel
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
            chooseCityAppBarButton.Click += (s, e) =>
                {
                    App.Navigate(new Uri(AppSettings.ChooseCityPageAddress, UriKind.Relative));
                };

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
            supportMenu.Click += (s, e) => 
                {
                    App.Navigate(new Uri(AppSettings.SupportPageAdress, UriKind.Relative));
                };

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

        private void LiveTileMenu_Click(object sender, EventArgs e)
        {
            if (ViewModel != null && 
                ViewModel.IsInitialized && 
                ViewModel.CurrentAddress != null)
            {
                string address = string.Format(AppSettings.AddTilePageAddress, 
                    ViewModel.CurrentAddress.PostalCode, ViewModel.CurrentAddress.CountryRegion);

                App.Navigate(new Uri(address, UriKind.Relative));
            }
        }

        private void ShowFavoritesAppBarButton_Click(object sender, EventArgs e)
        {
            if (PivotLayout != null)
                PivotLayout.SelectedItem = FavoritesPivotItem;
        }

        private void AddtoFavoritesAppBarButton_Click(object sender, EventArgs e)
        {
            if (DataContext != null && DataContext is MainPageViewModel)
                (DataContext as MainPageViewModel).AddToFavorites.Execute(null);
        }

        private void GoToLocationAppBarButton_Click(object sender, EventArgs e)
        {
            if (DataContext != null && DataContext is MainPageViewModel)
                (DataContext as MainPageViewModel).GoToLocation.Execute(null);
        }

        private void PivotLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SmartDispatcher.BeginInvoke(() =>
            {
                if (ApplicationBar != null && (this.Orientation & PageOrientation.Landscape) != PageOrientation.Landscape)
                    ApplicationBar.IsVisible = (PivotLayout.SelectedItem == WeatherPivotItem);
            });
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SmartDispatcher.BeginInvoke(() =>
            {
                if (DataContext == null)
                    DataContext = new MainPageViewModel();

                if (ViewModel != null && ViewModel.IsInitialized == false)
                    ViewModel.Initialize();

                if (ApplicationBar == null)
                    BuildApplicationBar();
            });

            SmartDispatcher.BeginInvoke(() =>
            {
                var postalCode = NavigationContext.TryGetStringKey("PostalCode");
                var country = NavigationContext.TryGetStringKey("Country");

                if (string.IsNullOrEmpty(postalCode) == false &&
                    string.IsNullOrEmpty(country) == false)
                {
                    ViewModel.ResolveLocation(postalCode, country);
                }
            });

            base.OnNavigatedTo(e);
        }

        private void Page_OrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            if (ApplicationBar != null)
            {
                if ((e.Orientation & PageOrientation.Landscape) == PageOrientation.Landscape)
                    ApplicationBar.IsVisible = false;
                else
                    ApplicationBar.IsVisible = true;
            }
        }
        
        private void PivotLayout_LoadingPivotItem(object sender, PivotItemEventArgs e)
        {
            if (e.Item.Content != null)
                return;

            var pivot = (Pivot)sender;

            if (e.Item == WeatherPivotItem)
                e.Item.Content = new WeatherPivotItemControl();
            else if (e.Item == RegionalPivotItem)
                e.Item.Content = new RegionalPivotItemControl();
            else if (e.Item == CountryPivotItem)
                e.Item.Content = new CountryPivotItemControl();
            else if (e.Item == PollenPivotItem)
                e.Item.Content = new PollenPivotItemControl();
            else if (e.Item == DiversePivotItem)
                e.Item.Content = new DiversePivotItemControl();
            else if (e.Item == FavoritesPivotItem)
                e.Item.Content = new FavoritesPivotItemControl();
            else if (e.Item == NewsPivotItem)
                e.Item.Content = new NewsPivotItemControl();

            if (e.Item.Content != null)
                (e.Item.Content as FrameworkElement).DataContext = this.DataContext;
        }
    }
}