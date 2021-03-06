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
using System.ComponentModel;
using DMI.Data;

namespace DMI.TaskAgent
{
    public class TileItem : INotifyPropertyChanged
    {
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67

        public TileItem()
        {
        }

        public TileItem(GeoLocationCity city)
        {
            this.City = city;
        }

        public int Offset
        {
            get;
            set;
        }

        public GeoLocationCity City
        {
            get;
            set;
        }

        public Uri CloudImage
        {
            get;
            set;
        }

        public string LocationName
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public string Temperature
        {
            get;
            set;
        }

        public TileType TileType
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }
    }
}
