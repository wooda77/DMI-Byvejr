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

namespace DMI.ViewModels
{
    using DMI.Models;

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
            private set
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
