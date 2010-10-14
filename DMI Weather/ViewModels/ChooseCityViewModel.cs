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

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace DMI.ViewModels
{
    using Models;

    public class ChooseCityViewModel : ViewModelBase
    {
        /// <summary>
        /// Internal collection of all available cities.
        /// </summary>
        private List<City> allCities;

        /// <summary>
        /// Collection of displayed cities (allowing filtering)
        /// </summary>
        private IEnumerable<City> cities;

        public IEnumerable<City> Cities
        {
            get
            {
                return cities;
            }
            set
            {
                cities = value;
                RaisePropertyChanged("Cities");
            }
        }

        private ICommand loadCities;

        public ICommand LoadCities
        {
            get
            {
                if (loadCities == null)
                {
                    loadCities = new RelayCommand(() =>
                    {
                        allCities = Denmark.PostalCodes.
                            Select(city => new City()
                            {
                                Name = city.Value,
                                PostalCode = city.Key
                            })
                            .ToList();
                        Cities = allCities;
                    });
                }

                return loadCities;
            }
        }

        private ICommand itemSelected;

        public ICommand ItemSelected
        {
            get
            {
                if (itemSelected == null)
                {
                    itemSelected = new RelayCommand<object>(item =>
                    {
                        if (item is City)
                        {
                            var city = item as City;
                            var uri = string.Format("/Views/MainPage.xaml?PostalCode={0}", city.PostalCode);

                            App.Navigate(new Uri(uri, UriKind.Relative));
                        }
                    });
                }

                return itemSelected;
            }
        }

        public void FilterItems(string filter)
        {
            var danish1 = allCities.Where(city => FilterItem(filter, city)).ToList();

            // Untill WP7 get a Danish Keyboard
            filter = filter.Replace("ae", "æ");
            filter = filter.Replace("oe", "ø");
            filter = filter.Replace("aa", "å");

            var danish2 = allCities.Where(city => FilterItem(filter, city)).ToList();
             
            Cities = danish1.Concat(danish2);
        }

        private bool FilterItem(string filter, object item)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return false;
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
