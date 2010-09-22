// 
// MainViewModel.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Net;
using System.Device.Location;
using System.Windows.Navigation;

using Microsoft.Phone.Tasks;

namespace DMI_Weather.ViewModels
{
    using Models;

    public class ImageViewModel : ViewModelBase
    {        
        public ImageSource ImageSource
        {
            get;
            set;
        }

        public void CropImageBorders(Image image)
        {
            if ((image.ActualWidth > 0) && (image.ActualHeight > 0))
            {
                // Crops 2 pixels on each side, to remove ugly rounded border from DMIs images.
                image.Clip = new RectangleGeometry()
                {
                    Rect = new Rect(2, 2, image.ActualWidth - 4, image.ActualHeight - 4)
                };
            }
        }

        public void LoadImage(string imageSource)
        {
            ImageSource = new BitmapImage(new Uri(imageSource, UriKind.Absolute));
        }
    }
}