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

namespace DMI_Weather
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();

            SupportedOrientations = SupportedPageOrientation.Portrait | SupportedPageOrientation.Landscape;
        }

        private void Image_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Image)
            {
                var image = sender as Image;

                if ((image.ActualWidth > 0) && (image.ActualHeight > 0))
                {
                    image.Clip = new RectangleGeometry()
                    {
                        Rect = new Rect(2, 2, image.ActualWidth - 4, image.ActualHeight - 4)
                    };
                }
            }
        }
    }
}
