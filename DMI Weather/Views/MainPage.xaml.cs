//
// MainPage.xaml.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DMI_Weather.Views
{
    using Models;
    using ViewModels;
    using System.Windows.Media;

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

        private void NewsListBox_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var selectedItem = (sender as StackPanel).DataContext;
            
            if ((selectedItem != null) && (selectedItem is NewsItem))
            {
                ViewModel.OpenUrlInBrowser((selectedItem as NewsItem).Link);
            }
        }

        private void Image_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (sender is Image)
            {
                ViewModel.CropImageBorders(sender as Image);
            }
        }
    }
}