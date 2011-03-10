// 
// ImageUtility.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

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

        /// <summary>
        /// Crops the Image Borders two pixels on each side.
        /// </summary>
        public static void CropImageBorders(Image image, Size newSize)
        {
            if ((image.ActualWidth > 0) && (image.ActualHeight > 0))
            {
                image.Clip = new RectangleGeometry()
                {
                    Rect = new Rect(2, 2,
                        newSize.Width - 4,
                        newSize.Height - 4
                    )
                };
            }
        }
    }
}
