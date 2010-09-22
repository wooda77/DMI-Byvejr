using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace DMI_Weather.Views
{
    using ViewModels;

    public partial class ImagePage : PageViewBase
    {
        public new ImageViewModel ViewModel
        {
            get
            {
                return (ImageViewModel)base.ViewModel;
            }
        }

        public ImagePage()
            : base(App.Resolve<ImageViewModel>())
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string imageSource = "";
            if (NavigationContext.QueryString.TryGetValue("ImageSource", out imageSource))
            {
                ViewModel.LoadImage(imageSource);
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