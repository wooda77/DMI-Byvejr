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
// Thanks to Chris Pietschmann 
// http://pietschsoft.com/post/2010/06/04/Resizing-and-Auto-Scaling-Pushpin-in-Bing-Maps-Silverlight.aspx
using System;
using System.Windows.Data;
using System.Windows.Media;

namespace DMI.Assets
{
    public class PushpinScaleTransform : IValueConverter
    {
        #region IValueConverter Members

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            double currentZoomLevel = (double)value;

            var scaleVal = (0.02 * (currentZoomLevel + 1)) + 0.3;

            if (currentZoomLevel >= 12)
            {
                scaleVal = 1.0;
            }

            var transform = new ScaleTransform();
            transform.ScaleX = scaleVal;
            transform.ScaleY = scaleVal;

            if (currentZoomLevel < 12)
            {
                    transform.CenterX = 13.2;
                    transform.CenterY = 48;
            }

            return transform;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
