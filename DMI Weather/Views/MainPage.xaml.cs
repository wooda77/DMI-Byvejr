//
// MainPage.xaml.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

using Microsoft.Phone.Controls;

namespace DMI_Weather.Views
{
    using Models;
    using ViewModels;
    using System.Windows.Media.Imaging;

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
            : base(App.Resolve<MainViewModel>())
        {
            InitializeComponent();
        }

        private void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.ResolveAddressFromGeoPosition();
            ViewModel.UpdatePollenFeed();
            ViewModel.UpdateNewsFeed();
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

        private void NewsItem_DoubleTap(object sender, GestureEventArgs e)
        {
            var selectedItem = (sender as StackPanel).DataContext;

            if ((selectedItem != null) && (selectedItem is NewsItem))
            {
                ViewModel.OpenUrlInBrowser((selectedItem as NewsItem).Link);
            }
        }
    }
}