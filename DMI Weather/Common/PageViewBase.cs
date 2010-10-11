//
// PageViewBase.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Windows;
using Microsoft.Phone.Controls;

namespace DMI_Weather.Views
{    
    using ViewModels;

    public class PageViewBase : PhoneApplicationPage, IView
    {
        public IViewModel ViewModel
        {
            get;
            private set;
        }

        public PageViewBase(IViewModel viewModel)
            : base()
        {
            this.ViewModel = viewModel;
            this.Loaded += new RoutedEventHandler(PageViewBase_Loaded);
        }

        public PageViewBase()
            : base()
        {
        }

        private void PageViewBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (ViewModel != null)
            {
                this.DataContext = ViewModel;
            }
        }        
    }
}
