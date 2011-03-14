//
// MainPage.xaml.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Device.Location;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.IO.IsolatedStorage;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

using GalaSoft.MvvmLight.Messaging;

using DMI.Properties;
using DMI.ViewModels;
using DMI.Models;

namespace DMI.Views
{
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
            BuildApplicationBar();
        }

        /// <summary>
        /// Creates a theme and localization aware Application Bar.
        /// </summary>
        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

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

            ApplicationBar.Buttons.Add(chooseCityAppBarButton);
            ApplicationBar.Buttons.Add(goToLocationAppBarButton);
            ApplicationBar.Buttons.Add(showFavoritesAppBarButton);
            ApplicationBar.Buttons.Add(addtoFavoritesAppBarButton);
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
            PivotLayout.SelectedItem = FavoritesPivotItem;
        }

        /// <summary>
        /// Executes the AddToFavorites Command.
        /// </summary>
        private void AddtoFavoritesAppBarButton_Click(object sender, EventArgs e)
        {
            (DataContext as MainViewModel).AddToFavorites.Execute(null);
        }

        /// <summary>
        /// Executes the GoToLocation Command.
        /// </summary>
        private void GoToLocationAppBarButton_Click(object sender, EventArgs e)
        {
            (DataContext as MainViewModel).GoToLocation.Execute(null);
        }

        /// <summary>
        /// Toggles the ApplicationBar on/off depending on the selected pivot item.
        /// </summary>
        private void PivotLayout_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ApplicationBar != null)
            {
                ApplicationBar.IsVisible = (PivotLayout.SelectedItem == WeatherPivotItem);
            }
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
            base.OnNavigatedTo(e);

            if (State.ContainsKey(App.PivotItem))
            {
                PivotLayout.SelectedIndex = (int)State[App.PivotItem];
            }

            string postalCode = "";
            if (NavigationContext.QueryString.TryGetValue("PostalCode", out postalCode))
            {
                (DataContext as MainViewModel).CurrentLocation = new CivicAddress()
                {
                    PostalCode = postalCode
                };
            }
        }

        /// <summary>
        /// Called when a page is no longer the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            base.OnNavigatedFrom(e);

            if (State.ContainsKey(App.PivotItem))
            {
                State[App.PivotItem] = PivotLayout.SelectedIndex;
            }
            else
            {
                State.Add(App.PivotItem, PivotLayout.SelectedIndex);
            }
        }

        /// <summary>
        /// Handles the SizeChanged event of the CityWeather2daysGraphImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void CityWeather2daysGraphImage_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            ImageUtility.CropImageBorders(CityWeather2daysGraphImage, e.NewSize);
        }

        /// <summary>
        /// Handles the SizeChanged event of the CityWeather7daysGraphImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void CityWeather7daysGraphImage_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            ImageUtility.CropImageBorders(CityWeather7daysGraphImage, e.NewSize);
        }

        /// <summary>
        /// Handles the SizeChanged event of the PollenGraphImage control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.Windows.SizeChangedEventArgs"/> instance containing the event data.</param>
        private void PollenGraphImage_SizeChanged(object sender, System.Windows.SizeChangedEventArgs e)
        {
            ImageUtility.CropImageBorders(PollenGraphImage, e.NewSize);
        }
    }
}