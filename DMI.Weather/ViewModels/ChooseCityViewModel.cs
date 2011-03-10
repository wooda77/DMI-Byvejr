// 
// ChooseCityViewModel.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using System.Windows.Controls;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

using DMI.Models;

namespace DMI.ViewModels
{
    public class ChooseCityViewModel : ViewModelBase
    {
        private readonly ICommand selectionChanged;
        private readonly ICommand textChanged;

        private List<City> allCities;
        private List<CityGroup> currentCities;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:ChooseCityViewModel"/> class.
        /// </summary>
        public ChooseCityViewModel()
        {
            this.selectionChanged = new RelayCommand<SelectionChangedEventArgs>(SelectionChangedExecute);
            this.textChanged = new RelayCommand<TextBox>(TextChangedExecute);

            this.allCities = Denmark.Cities;
            this.currentCities = new AllCities(Denmark.Cities);
        }

        public List<CityGroup> Cities
        {
            get
            {
                return currentCities;
            }
            set
            {
                currentCities = value;
                RaisePropertyChanged("Cities");
            }
        }

        public ICommand SelectionChanged
        {
            get
            {
                return selectionChanged;
            }
        }

        public ICommand TextChanged
        {
            get
            {
                return textChanged;
            }
        }

        private void SelectionChangedExecute(SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var city = e.AddedItems[0] as City;
                var uri = string.Format("/Views/MainPage.xaml?PostalCode={0}", city.PostalCode);

                App.Navigate(new Uri(uri, UriKind.Relative));
            }
        }

        private void TextChangedExecute(TextBox textbox)
        {
            var filter = textbox.Text;
            var filtered = allCities.Where(city => FilterItem(filter, city));

            this.Cities = new AllCities(filtered.ToList());
        }

        private bool FilterItem(string filter, object item)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }
            else if (item is City)
            {
                var city = item as City;

                return city.Name.StartsWith(filter,
                    StringComparison.CurrentCultureIgnoreCase);
            }
            else
            {
                return false;
            }
        }
    }
}
