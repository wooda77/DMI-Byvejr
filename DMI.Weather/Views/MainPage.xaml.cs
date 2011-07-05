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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DMI.Views
{
    using DMI.Models;
    using DMI.Properties;
    using DMI.ViewModels;
    using System.Windows.Threading;

    /// <summary>
    /// Interaction logic for MainPage.xaml
    /// </summary>
    public partial class MainPage : PhoneApplicationPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:MainPage"/> class.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Creates a theme and localization aware Application Bar.
        /// </summary>
        private void BuildApplicationBar()
        {
            if (ApplicationBar != null)
            {
                return;
            }
            
            var appBar = new ApplicationBar();

            Uri homeImage;
            Uri favImage;
            Uri addToFavImage;
            Uri gotoLocationImage;

            if (App.CurrentThemeBackground == App.ThemeBackground.ThemeBackgroundDark)
            {
                homeImage = new Uri("/Images/appbar.home.light.png", UriKind.Relative);
                favImage = new Uri("/Images/appbar.favs.light.png", UriKind.Relative);
                addToFavImage = new Uri("/Images/appbar.addtofavs.light.png", UriKind.Relative);
                gotoLocationImage = new Uri("/Images/appbar.location.light.png", UriKind.Relative);
            }
            else
            {
                homeImage = new Uri("/Images/appbar.home.dark.png", UriKind.Relative);
                favImage = new Uri("/Images/appbar.favs.dark.png", UriKind.Relative);
                addToFavImage = new Uri("/Images/appbar.addtofavs.dark.png", UriKind.Relative);
                gotoLocationImage = new Uri("/Images/appbar.location.dark.png", UriKind.Relative);
            }

            var chooseCityAppBarButton = new ApplicationBarIconButton(homeImage)
            {
                Text = AppResources.AppBar_ChooseCity
            };
            chooseCityAppBarButton.Click += new EventHandler(ChooseCityAppBarButton_Click);

            var showFavoritesAppBarButton = new ApplicationBarIconButton(favImage)
            {
                Text = AppResources.AppBar_Favorites
            };
            showFavoritesAppBarButton.Click += new EventHandler(ShowFavoritesAppBarButton_Click);

            var addtoFavoritesAppBarButton = new ApplicationBarIconButton(addToFavImage)
            {
                Text = AppResources.AppBar_AddToFavorites
            };
            addtoFavoritesAppBarButton.Click += new EventHandler(AddtoFavoritesAppBarButton_Click);

            var goToLocationAppBarButton = new ApplicationBarIconButton(gotoLocationImage)
            {
                Text = AppResources.AppBar_GoToLocation
            };
            goToLocationAppBarButton.Click += new EventHandler(GoToLocationAppBarButton_Click);

            var supportMenu = new ApplicationBarMenuItem()
            {
                Text = AppResources.AppBar_Support
            };
            supportMenu.Click += new EventHandler(SettingsMenu_Click);

            appBar.Buttons.Add(chooseCityAppBarButton);
            appBar.Buttons.Add(goToLocationAppBarButton);
            appBar.Buttons.Add(showFavoritesAppBarButton);
            appBar.Buttons.Add(addtoFavoritesAppBarButton);
            appBar.MenuItems.Add(supportMenu);

            ApplicationBar = appBar;
        }

        /// <summary>
        /// Handles the Click event of the SettingsMenu control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        private void SettingsMenu_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/SupportPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Executes the ChooseCity Command.
        /// </summary>
        private void ChooseCityAppBarButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/ChooseCityPage.xaml", UriKind.Relative));
        }

        /// <summary>
        /// Executes the ShowFavorites Command.
        /// </summary>
        private void ShowFavoritesAppBarButton_Click(object sender, EventArgs e)
        {
            if (PivotLayout != null)
            {
                PivotLayout.SelectedItem = FavoritesPivotItem;
            }
        }

        /// <summary>
        /// Executes the AddToFavorites Command.
        /// </summary>
        private void AddtoFavoritesAppBarButton_Click(object sender, EventArgs e)
        {
            if (DataContext != null && DataContext is MainViewModel)
            {
                (DataContext as MainViewModel).AddToFavorites.Execute(null);
            }
        }

        /// <summary>
        /// Executes the GoToLocation Command.
        /// </summary>
        private void GoToLocationAppBarButton_Click(object sender, EventArgs e)
        {
            if (DataContext != null && DataContext is MainViewModel)
            {
                (DataContext as MainViewModel).GoToLocation.Execute(null);
            }
        }

        /// <summary>
        /// Toggles the ApplicationBar on/off depending on the selected pivot item.
        /// </summary>
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

        /// <summary>
        /// Opens a image in landscape mode.
        /// </summary>
        private void OpenInLandscapeMode(object sender, GestureEventArgs e)
        {
            if (e.OriginalSource is Image)
            {
                var image = (e.OriginalSource as Image).Source as BitmapImage;
                var uri = "/Views/ImagePage.xaml?ImageSource=" +
                    Uri.EscapeDataString(image.UriSource.ToString());

                NavigationService.Navigate(new Uri(uri, UriKind.Relative));
            }
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (App.IsFirstStart)
            {
                NavigationService.Navigate(new Uri("/Views/SupportPage.xaml", UriKind.Relative));                                

                return;
            }

            base.OnNavigatedTo(e);

            string postalCode = "";
            if (NavigationContext.QueryString.TryGetValue("PostalCode", out postalCode))
            {
                SmartDispatcher.BeginInvoke(() =>
                {
                    (DataContext as MainViewModel).CurrentLocation = new CivicAddress()
                    {
                        PostalCode = postalCode
                    };
                });
            }
        }

        /// <summary>
        /// Called when a page is no longer the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
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
            } catch (InvalidOperationException)
            {
                // Fix for Emulator-only crashes.
            }
        }

        /// <summary>
        /// Handles the SizeChanged event of a Image control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Image)
            {
                var image = sender as Image;
                var source = image.Source as BitmapSource;
                var filename = (string)image.Tag;

                image.CropImageBorders(e.NewSize);
                source.SaveToLocalStorage(filename);
            }
        }

        /// <summary>
        /// Handles the Loaded event of the PivotLayout control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.RoutedEventArgs"/> instance containing the event data.</param>
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

        /// <summary>
        /// Handles the OrientationChanged event of the PhoneApplicationPage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="Microsoft.Phone.Controls.OrientationChangedEventArgs"/> instance containing the event data.</param>
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
    }
}