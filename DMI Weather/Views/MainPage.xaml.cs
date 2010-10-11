//
// MainPage.xaml.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Device.Location;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DMI_Weather.Views
{
    using Models;
    using ViewModels;
    using System.IO.IsolatedStorage;

    public partial class MainPage : PageViewBase
    {
        public new MainViewModel ViewModel
        {
            get
            {
                return (MainViewModel)base.ViewModel;
            }
        }

        public MainPage()
            : base(new MainViewModel())
        {
            InitializeComponent();

            if (IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites))
            {
                ViewModel.Favorites = (ObservableCollection<City>)
                    IsolatedStorageSettings.ApplicationSettings[App.Favorites];
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string postalCode = "";
            if (NavigationContext.QueryString.TryGetValue("PostalCode", out postalCode))
            {
                ViewModel.Address.PostalCode = postalCode;
            }
        }

        private void WeatherPivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            BuildApplicationBar();
            ViewModel.ResolveAddressFromGeoPosition();
        }

        private void PollenPivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdatePollenFeed();
        }

        private void NewsPivotItem_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.UpdateNewsFeed();
        }

        private void BuildApplicationBar()
        {
            ApplicationBar = new ApplicationBar();

            Uri homeImage;
            Uri favImage;
            Uri addToFavImage;

            if (App.CurrentThemeBackground == App.ThemeBackground.ThemeBackgroundDark)
            {
                homeImage = new Uri("/Images/appbar.home.light.png", UriKind.Relative);
                favImage = new Uri("/Images/appbar.favs.light.png", UriKind.Relative);
                addToFavImage = new Uri("/Images/appbar.addtofavs.light.png", UriKind.Relative);
            }
            else
            {
                homeImage = new Uri("/Images/appbar.home.dark.png", UriKind.Relative);
                favImage = new Uri("/Images/appbar.favs.dark.png", UriKind.Relative);
                addToFavImage = new Uri("/Images/appbar.addtofavs.dark.png", UriKind.Relative);
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

            ApplicationBar.Buttons.Add(chooseCityAppBarButton);
            ApplicationBar.Buttons.Add(showFavoritesAppBarButton);
            ApplicationBar.Buttons.Add(addtoFavoritesAppBarButton);
        }

        private void AddtoFavoritesAppBarButton_Click(object sender, EventArgs e)
        {
            var postal = int.Parse(ViewModel.PostalCode);

            if (!ViewModel.Favorites.Any(c => c.PostalCode == postal))
            {
                ViewModel.Favorites.Add(new City()
                {
                    PostalCode = postal,
                    Name = Denmark.PostalCodes[postal]
                });
            }

            SaveFavorites();
        }

        private void SaveFavorites()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(App.Favorites))
            {
                IsolatedStorageSettings.ApplicationSettings.Add(App.Favorites, ViewModel.Favorites);
            }
            else
            {
                IsolatedStorageSettings.ApplicationSettings[App.Favorites] = ViewModel.Favorites;
            }
        }

        private void ShowFavoritesAppBarButton_Click(object sender, EventArgs e)
        {
            PivotLayout.SelectedItem = FavoritesPivotItem;
        }

        private void ChooseCityAppBarButton_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new Uri("/Views/ChooseCityPage.xaml", UriKind.Relative));
        }

        private void Pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ApplicationBar != null)
            {
                ApplicationBar.IsVisible = (PivotLayout.SelectedItem == WeatherPivotItem);
            }
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Image)
            {
                ViewModel.CropImageBorders(sender as Image);
            }
        }

        private void Image_DoubleTap(object sender, GestureEventArgs e)
        {
            var image = (sender as Image).Source as BitmapImage;

            var uri = "/Views/ImagePage.xaml?ImageSource=" +
                Uri.EscapeDataString(image.UriSource.ToString());

            NavigationService.Navigate(new Uri(uri, UriKind.Relative));
        }

        private void NewsItem_Tap(object sender, GestureEventArgs e)
        {
            var selectedItem = (sender as ListBox).SelectedItem;

            if ((selectedItem != null) && (selectedItem is NewsItem))
            {
                ViewModel.OpenUrlInBrowser((selectedItem as NewsItem).Link);
            }
        }

        private void FavoritesGestureListener_Tap(object sender, GestureEventArgs e)
        {
            var item = (City)FavoritesListBox.SelectedItem;      
            if (item != null)
            {
                ViewModel.Address = new CivicAddress()
                {
                    PostalCode = item.PostalCode.ToString()
                };
                PivotLayout.SelectedItem = WeatherPivotItem;

                FavoritesListBox.SelectedIndex = -1;
            }
        }

        private void FavoritesContextMenuItem_Click(object sender, RoutedEventArgs e)
        {          
            var item = sender as MenuItem;
            if (item != null)
            {
                ViewModel.Favorites.Remove((City)item.DataContext);

                SaveFavorites();   
            }
        }
    }
}