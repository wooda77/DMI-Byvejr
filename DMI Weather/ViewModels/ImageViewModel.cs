// 
// ImageViewModel.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;

namespace DMI.ViewModels
{
    using Models;

    public class ImageViewModel : ViewModelBase
    {
        public ImageSource ImageSource
        {
            get;
            set;
        }

        private ICommand cropBorders;

        public ICommand CropBorders
        {
            get
            {
                if (cropBorders == null)
                {
                    cropBorders = new RelayCommand<Image>(image =>
                    {
                        ImageUtility.CropImageBorders(image);
                    });
                }

                return cropBorders;
            }
        }

        public void LoadImage(string imageSource)
        {
            ImageSource = new BitmapImage(new Uri(imageSource, UriKind.Absolute));
        }
    }
}