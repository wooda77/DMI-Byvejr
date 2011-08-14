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
using System.Windows.Media;
using System.Windows.Navigation;
using DMI.Common;
using DMI.ViewModels;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using System.Windows.Threading;

namespace DMI.Views
{
    public partial class AddTilePage
    {
        public AddTilePage()
        {
            InitializeComponent();
        }

        private void BuildApplicationBar()
        {
            // Feature disabled untill the Mango release of the Silverlight Toolkit is ready.


            //if (ApplicationBar != null)
            //    return;
            
            //this.ApplicationBar = new ApplicationBar();
            //this.ApplicationBar.BackgroundColor = Colors.Transparent;
            
            //var addTileButton = new ApplicationBarIconButton();
            //addTileButton.IconUri = new Uri("/Resources/Images/appbar.add.png", UriKind.Relative);
            //addTileButton.Text = Properties.Resources.AppBar_NewTile;
            //addTileButton.Click += (sender, e) =>
            //    {
            //        NavigationService.Navigate(new Uri("/Views/AddCustomTilePage.xaml", UriKind.Relative));
            //    };

            //this.ApplicationBar.Buttons.Add(addTileButton);
            
            //this.ApplicationBar.IsMenuEnabled = true;
            //this.ApplicationBar.IsVisible = true;
        }

        private AddTilePageViewModel ViewModel
        {
            get
            {
                return DataContext as AddTilePageViewModel;
            }
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            SmartDispatcher.BeginInvoke(() =>
            {
                if (ApplicationBar == null)
                    BuildApplicationBar();
                else
                    ApplicationBar.IsVisible = true;
            });

            SmartDispatcher.BeginInvoke(() =>
            {
                var postalCode = NavigationContext.TryGetKey("PostalCode");
                var country = NavigationContext.TryGetStringKey("Country");

                if (postalCode.HasValue && string.IsNullOrEmpty(country) == false)
                {
                    ViewModel.LoadCity(postalCode.Value, country);
                }
            });

            base.OnNavigatedTo(e);
        }
    }
}