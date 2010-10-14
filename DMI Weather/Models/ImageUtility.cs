using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DMI.Models
{
    public class ImageUtility
    {
        /// <summary>
        /// Crops the Image Borders two pixels on each side.
        /// </summary>
        public static void CropImageBorders(Image image)
        {
            if ((image.ActualWidth > 0) && (image.ActualHeight > 0))
            {
                image.Clip = new RectangleGeometry()
                {
                    Rect = new Rect(2, 2,
                        image.ActualWidth - 4,
                        image.ActualHeight - 4
                    )
                };
            }
        }
    }
}
