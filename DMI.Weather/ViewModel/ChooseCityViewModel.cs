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
using System.Collections.Generic;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Input;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using DMI.Model;
using DMI.Service;

namespace DMI.ViewModel
{
    public class ChooseCityViewModel : ViewModelBase
    {
        private List<City> allCities;

        public ChooseCityViewModel()
        {
            this.SelectionChanged = new RelayCommand<SelectionChangedEventArgs>(SelectionChangedExecute);
            this.TextChanged = new RelayCommand<TextBox>(TextChangedExecute);

            var danishCities =
                from city in Denmark.PostalCodes
                select new City()
                {
                    Country = "Denmark",
                    Name = city.Value,
                    PostalCode = city.Key
                };

            var greenlandCities =
                from city in Greenland.PostalCodes
                select new City()
                {
                    Country = "Greenland",
                    Name = city.Value.Name,
                    PostalCode = city.Key
                };

            var faroeislandsCities =
                from city in FaroeIslands.PostalCodes
                select new City()
                {
                    Country = "Faroe Islands",
                    Name = city.Value.Name,
                    PostalCode = city.Key
                };

            this.allCities = danishCities
                .Concat(greenlandCities)
                .Concat(faroeislandsCities)
                .ToList();

            this.Cities = new CityGroups(allCities);
        }

        public List<CityGroup> Cities
        {
            get;
            private set;
        }

        public ICommand SelectionChanged
        {
            get;
            private set;
        }

        public ICommand TextChanged
        {
            get;
            private set;
        }

        private void SelectionChangedExecute(SelectionChangedEventArgs e)
        {
            if (e == null)
            {
                throw new ArgumentException("e");
            }

            if (e.AddedItems != null &&
                e.AddedItems.Count > 0)
            {
                var city = e.AddedItems[0] as City;
                var uri = string.Format("/View/MainPage.xaml?PostalCode={0}&Country={1}", city.PostalCode, city.Country);

                App.Navigate(new Uri(uri, UriKind.Relative));
            }
        }

        private void TextChangedExecute(TextBox textBox)
        {
            if (textBox == null)
            {
                throw new ArgumentException("textBox");
            }

            var filter = textBox.Text;
            var filtered = allCities.Where(city => FilterItem(filter, city));

            this.Cities = new CityGroups(filtered.ToList());
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
                if (city != null)
                    return city.Name.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase);
            }

            return false;
        }
    }
}
