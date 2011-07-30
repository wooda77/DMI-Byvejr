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
using DMI.Service;
using DMI.Common;
using DMI.Assets;

namespace DMI.ViewModel
{
    public class ChooseCityViewModel : ViewModelBase
    {
        private IEnumerable<GeoLocationCity> allCities = Enumerable.Empty<GeoLocationCity>();

        public ChooseCityViewModel()
        {
            this.SelectionChanged = new RelayCommand<SelectionChangedEventArgs>(SelectionChangedExecute);
            this.TextChanged = new RelayCommand<TextBox>(TextChangedExecute);

            this.allCities = Denmark.PostalCodes.Values
                .Concat(Greenland.PostalCodes.Values)
                .Concat(FaroeIslands.PostalCodes.Values);

            this.Cities = new LongListCollection<GeoLocationCity, char>(allCities, c => c.Name[0]);
        }

        public LongListCollection<GeoLocationCity, char> Cities
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

        private void SelectionChangedExecute(SelectionChangedEventArgs args)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            if (args.AddedItems != null &&
                args.AddedItems.Count > 0)
            {
                var city = args.AddedItems[0] as GeoLocationCity;
                var uri = string.Format(AppSettings.MainPageAddress, city.PostalCode, city.Country);

                App.Navigate(new Uri(uri, UriKind.Relative));
            }
        }

        private void TextChangedExecute(TextBox textBox)
        {
            if (textBox == null)
                throw new ArgumentNullException("textBox");

            var filter = textBox.Text;
            var filtered = allCities.Where(city => FilterItem(filter, city));

            this.Cities = new LongListCollection<GeoLocationCity, char>(filtered, e => e.Name[0]);
        }

        private bool FilterItem(string filter, object item)
        {
            if (string.IsNullOrEmpty(filter))
            {
                return true;
            }
            else if (item is GeoLocationCity)
            {
                var city = item as GeoLocationCity;
                if (city != null)
                    return city.Name.StartsWith(filter, StringComparison.CurrentCultureIgnoreCase);
            }

            return false;
        }
    }
}
