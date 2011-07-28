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