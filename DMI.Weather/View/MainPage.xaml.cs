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
using System.Linq;
using System.Device.Location;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using DMI.Model;
using DMI.Properties;
using DMI.ViewModel;
using System.Windows.Threading;

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

            var homeImage = new Uri("/Resources/Images/appbar.home.png", UriKind.Relative);
            var favImage = new Uri("/Resources/Images/appbar.favs.png", UriKind.Relative);
            var addToFavImage = new Uri("/Resources/Images/appbar.addtofavs.png", UriKind.Relative);
            var gotoLocationImage = new Uri("/Resources/Images/appbar.location.png", UriKind.Relative);

            var chooseCityAppBarButton = new ApplicationBarIconButton(homeImage);
            chooseCityAppBarButton.Text = AppResources.AppBar_ChooseCity;
            chooseCityAppBarButton.Click += new EventHandler(ChooseCityAppBarButton_Click);

            var showFavoritesAppBarButton = new ApplicationBarIconButton(favImage);
            showFavoritesAppBarButton.Text = AppResources.AppBar_Favorites;
            showFavoritesAppBarButton.Click += new EventHandler(ShowFavoritesAppBarButton_Click);

            var addtoFavoritesAppBarButton = new ApplicationBarIconButton(addToFavImage);
            addtoFavoritesAppBarButton.Text = AppResources.AppBar_AddToFavorites;
            addtoFavoritesAppBarButton.Click += new EventHandler(AddtoFavoritesAppBarButton_Click);

            var goToLocationAppBarButton = new ApplicationBarIconButton(gotoLocationImage);
            goToLocationAppBarButton.Text = AppResources.AppBar_GoToLocation;
            goToLocationAppBarButton.Click += new EventHandler(GoToLocationAppBarButton_Click);

            var supportMenu = new ApplicationBarMenuItem();
            supportMenu.Text = AppResources.AppBar_Support;
            supportMenu.Click += new EventHandler(SettingsMenu_Click);

            ApplicationBar.Buttons.Add(chooseCityAppBarButton);
            ApplicationBar.Buttons.Add(goToLocationAppBarButton);
            ApplicationBar.Buttons.Add(showFavoritesAppBarButton);
            ApplicationBar.Buttons.Add(addtoFavoritesAppBarButton);
            ApplicationBar.MenuItems.Add(supportMenu);

            ApplicationBar.IsVisible = (PivotLayout.SelectedItem == WeatherPivotItem);
        }

        private void SettingsMenu_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/SupportPage.xaml", UriKind.Relative));
        }

        private void ChooseCityAppBarButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/ChooseCityPage.xaml", UriKind.Relative));
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
                var uri = "/View/ImagePage.xaml?ImageSource=" +
                    Uri.EscapeDataString(image.UriSource.ToString());

                NavigationService.Navigate(new Uri(uri, UriKind.Relative));
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.IsFirstStart)
            {
                NavigationService.Navigate(new Uri("/View/SupportPage.xaml", UriKind.Relative));

                return;
            }
            else
            {
                if (ViewModel.IsInitialized == false)
                {
                    ViewModel.Initialize();
                }
            }

            base.OnNavigatedTo(e);

            var queryString = NavigationContext.QueryString.Values.ToList();

            string postalCode = "";
            if (NavigationContext.QueryString.TryGetValue("PostalCode", out postalCode))
            {
                string country = "Denmark";
                NavigationContext.QueryString.TryGetValue("Country", out country);

                SmartDispatcher.BeginInvoke(() =>
                {
                    ViewModel.ResolveLocation(postalCode, country);
                });
            }
        }

        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            try
            {
                if (State.ContainsKey(App.PivotItem))
                {
                    State[App.PivotItem] = PivotLayout.SelectedIndex;
                }
                else
                {
                    State.Add(App.PivotItem, PivotLayout.SelectedIndex);
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
                if (State.ContainsKey(App.PivotItem))
                {
                    var index = (int)State[App.PivotItem];
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

        private void TideWaterMenuItem_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/TideWaterPage.xaml", UriKind.Relative));
        }

        private void WaterHeightMenuItem_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/WaterHeightPage.xaml", UriKind.Relative));
        }

        private void MapsMenuItem_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/MapsPage.xaml", UriKind.Relative));
        }

        private void BorgerVejrMenuItem_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/PeopleWeatherPage.xaml", UriKind.Relative));
        }

        private void RadarMenuItem_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/RadarPage.xaml", UriKind.Relative));
        }

        private void BeachWeatherMenuItem_Tap(object sender, GestureEventArgs e)
        {
            NavigationService.Navigate(new Uri("/View/BeachWeatherPage.xaml", UriKind.Relative));
        }
    }
}