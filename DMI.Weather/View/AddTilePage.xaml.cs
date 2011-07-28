using System;
using DMI.ViewModel;
using Microsoft.Phone.Controls;
using DMI.Common;
using System.Windows.Navigation;
using System.Windows.Markup;
using System.Windows;

namespace DMI.View
{
    public partial class AddTilePage : PhoneApplicationPage
    {
        public AddTilePage()
        {
            InitializeComponent();

//            string XamlTile = @"
//                <Grid xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation"">
//                    <Grid Width=""173""                  
//                          Height=""173""
//                          HorizontalAlignment=""Left""
//                          VerticalAlignment=""Top""
//                          Background=""#0C2D83"">
//                        <Image Margin=""8,54,0,0""
//                               Width=""100""
//                               Height=""64"" 
//                               HorizontalAlignment=""Left""
//                               VerticalAlignment=""Top""
//                               Source=""{0}""
//                               Stretch=""None"" />
//                        <TextBlock Margin=""12,6,0,0""
//                                   FontFamily=""Segoe WP""
//                                   FontSize=""20""
//                                   HorizontalAlignment=""Left""
//                                   VerticalAlignment=""Top""
//                                   Foreground=""White""
//                                   Text=""{1}""
//                                   TextWrapping=""Wrap"" />
//                        <TextBlock Margin=""124,63,0,0""
//                                   FontFamily=""Segoe WP""
//                                   HorizontalAlignment=""Left""
//                                   VerticalAlignment=""Top""
//                                   FontSize=""29.333""
//                                   Foreground=""White""
//                                   Text=""{2}""
//                                   TextWrapping=""Wrap"" />
//                    </Grid>
//                </Grid>
//            ";

//            var xaml = string.Format(XamlTile, "/Resources/Weather/3.png", "Seneste (09:00)", "20°");
//            var element = XamlReader.Load(xaml) as FrameworkElement;

//            LayoutRoot.Children.Add(element);
        }

        private AddTileViewModel ViewModel
        {
            get
            {
                return DataContext as AddTileViewModel;
            }
        }

        private void LatestGrid_Tap(object sender, GestureEventArgs e)
        {
            ViewModel.GenerateTile(LatestGrid.DataContext as TileItem);
        }

        private void PlusSixHoursGrid_Tap(object sender, GestureEventArgs e)
        {
            ViewModel.GenerateTile(PlusSixHoursGrid.DataContext as TileItem);
        }

        private void PlusTwelveHoursGrid_Tap(object sender, GestureEventArgs e)
        {
            ViewModel.GenerateTile(PlusTwelveHoursGrid.DataContext as TileItem);
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            var postalCode = NavigationContext.TryGetKey("PostalCode");
            var country = NavigationContext.TryGetStringKey("Country");

            if (postalCode.HasValue && string.IsNullOrEmpty(country) == false)
            {
                ViewModel.LoadCity(postalCode.Value, country);
            }
        }
    }
}