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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Phone;
using System.Windows.Media.Imaging;

namespace DMI.Models
{
    public static class ImageUtility
    {
        /// <summary>
        /// Crops the Image Borders two pixels on each side.
        /// </summary>
        public static Image CropImageBorders(this Image image)
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

            return image;
        }

        /// <summary>
        /// Crops the Image Borders two pixels on each side.
        /// </summary>
        public static Image CropImageBorders(this Image image, Size newSize)
        {
            if ((image.ActualWidth > 0) && (image.ActualHeight > 0) &&
                (newSize.Width > 0) && (newSize.Height > 0))
            {
                image.Clip = new RectangleGeometry()
                {
                    Rect = new Rect(2, 2,
                        newSize.Width - 4,
                        newSize.Height - 4
                    )
                };
            }

            return image;
        }

        /// <summary>
        /// Saves a image to local storage.
        /// </summary>
        /// <param name="image">The image.</param>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static BitmapSource SaveToLocalStorage(this BitmapSource image, string filename)
        {
            if (image.PixelWidth == 0 || image.PixelHeight == 0)
            {
                return image;
            }

            var bytes = image.ConvertToBytes();

            var store = IsolatedStorageFile.GetUserStoreForApplication();

            if (store.DirectoryExists(App.ImageFolder) == false)
            {
                store.CreateDirectory(App.ImageFolder);
            }

            string path = Path.Combine(App.ImageFolder, filename);
            
            if (store.FileExists(path))
            {
                store.DeleteFile(path);
            }

            using (var stream = store.CreateFile(path))
            {
                stream.Write(bytes, 0, bytes.Length);
            }

            return image;
        }

        /// <summary>
        /// Loads a image from local storage.
        /// </summary>
        /// <param name="filename">The filename.</param>
        /// <returns></returns>
        public static BitmapSource LoadFromLocalStorage(string filename)
        {
            var store = IsolatedStorageFile.GetUserStoreForApplication();

            if (store.DirectoryExists(App.ImageFolder) == false)
            {
                store.CreateDirectory(App.ImageFolder);
            }

            string path = Path.Combine(App.ImageFolder, filename);

            if (store.FileExists(path))
            {
                using (var imageStream = store.OpenFile(path, FileMode.Open, FileAccess.Read))
                {
                    return PictureDecoder.DecodeJpeg(imageStream);
                }
            }

            return new WriteableBitmap(0, 0);
        }

        public static byte[] ConvertToBytes(this BitmapSource image)
        {
            using (var stream = new MemoryStream())
            {
                var writeableBitmap = new WriteableBitmap(image.PixelWidth, image.PixelHeight);

                Extensions.SaveJpeg(writeableBitmap, 
                    stream, image.PixelWidth, image.PixelHeight, 0, 100);

                return stream.ToArray();
            }
        }
    }
}
