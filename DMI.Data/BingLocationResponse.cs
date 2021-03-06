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
using System.Collections.Generic;

namespace DMI.Data
{
    public class BingLocationResponse
    {
        public List<BingResourceSet> ResourceSets
        {
            get;
            set;
        }

        public string StatusCode
        {
            get;
            set;
        }

        public string StatusDescription
        {
            get;
            set;
        }
    }

    public class BingResourceSet
    {
        public List<BingResource> Resources
        {
            get;
            set;
        }
    }

    public class BingResource
    {
        public BingResourceAddress Address
        {
            get;
            set;
        }
    }

    public class BingResourceAddress
    {
        public string AddressLine
        {
            get;
            set;
        }

        public string AdminDistrict
        {
            get;
            set;
        }

        public string AdminDistrict2
        {
            get;
            set;
        }

        public string CountryRegion
        {
            get;
            set;
        }

        public string FormattedAddress
        {
            get;
            set;
        }

        public string Locality
        {
            get;
            set;
        }

        public string PostalCode
        {
            get;
            set;
        }
    }
}