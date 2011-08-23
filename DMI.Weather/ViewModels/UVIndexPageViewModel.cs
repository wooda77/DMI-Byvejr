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
using System;
using System.Windows.Threading;
using DMI.Service;
using GalaSoft.MvvmLight;
using ImageTools.IO;
using ImageTools.IO.Gif;

namespace DMI.ViewModels
{
    public class UVIndexPageViewModel : ViewModelBase
    {
        public UVIndexPageViewModel()
        {
            Decoders.AddDecoder<GifDecoder>();

            if (IsInDesignMode == false)
            {
                this.ImageSource = new Uri("http://www.dmi.dk/dmi/1daymap.gif", UriKind.Absolute);

                UVIndexProvider.GetUVIndex((result) =>
                    {
                        SmartDispatcher.BeginInvoke(() =>
                        {
                            NorthJytland = result[0];
                            MiddleAndWestJytland = result[1];
                            EastJytland = result[2];
                            SouthJytland = result[3];
                            Fyn = result[4];
                            SouthAndWestZealand = result[5];
                            Copenhagen = result[6];
                            Bornholm = result[7];
                        });
                    });
            }
        }

        public Uri ImageSource
        {
            get;
            private set;
        }

        public UVIndex Bornholm
        {
            get;
            private set;
        }

        public UVIndex Fyn
        {
            get;
            private set;
        }

        public UVIndex Copenhagen
        {
            get;
            private set;
        }

        public UVIndex MiddleAndWestJytland
        {
            get;
            private set;
        }

        public UVIndex NorthJytland
        {
            get;
            private set;
        }

        public UVIndex EastJytland
        {
            get;
            private set;
        }

        public UVIndex SouthAndWestZealand
        {
            get;
            private set;
        }

        public UVIndex SouthJytland
        {
            get;
            private set;
        }
    }
}
