﻿#region License
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
using System.Windows;
using System.Windows.Input;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace DMI.Assets
{
    public static class ImageExtension
    {
        public static readonly DependencyProperty SizeChangedCommandProperty =
            DependencyProperty.RegisterAttached("SizeChangedCommand",
                typeof(bool), typeof(ImageExtension),
                new PropertyMetadata(false, OnSizeChanged));

        public static bool GetSizeChangedCommand(Image selector)
        {
            return (bool)selector.GetValue(SizeChangedCommandProperty);
        }

        public static void SetSizeChangedCommand(Image selector, bool value)
        {
            selector.SetValue(SizeChangedCommandProperty, value);
        }

        private static void OnSizeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var image = d as Image;
            if (image == null)
            {
                throw new ArgumentException(
                    "You must set the Command attached property on an element that derives from Image.");
            }

            var oldCommand = (bool)e.OldValue;
            if (oldCommand)
            {
                image.Loaded -= OnLoaded;
                image.SizeChanged -= OnSizeChanged;
            }

            var newCommand = (bool)e.NewValue;
            if (newCommand)
            {
                image.Loaded += OnLoaded;
                image.SizeChanged += OnSizeChanged;
            }

            image.CropImageBorders();
        }

        private static void OnLoaded(object sender, RoutedEventArgs e)
        {
            var image = sender as Image;
            if (image != null)
                image.CropImageBorders();
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            var image = sender as Image;
            if (image != null)
                image.CropImageBorders(e.NewSize);
        }
    }
}
