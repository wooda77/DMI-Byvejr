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
using System.Device.Location;

namespace DMI.Data
{
    public class GeoLocationCity : IComparable<GeoLocationCity>
    {
        public GeoLocationCity()
        {
        }

        public GeoLocationCity(string country, int postalCode, int id, string name, double latitude, double longitude)
        {
            this.Country = country;
            this.Id = id;
            this.PostalCode = postalCode;
            this.Name = name;
            this.Location = new GeoCoordinate(latitude, longitude);
        }

        public string Country
        {
            get;
            set;
        }

        public string ShortCountryName
        {
            get
            {
                if (Country == "Denmark")
                    return "DK";
                else if (Country == "Greenland")
                    return "GR";
                else if (Country == "Faroe Islands")
                    return "FR";
                else
                    return "__";
            }
        }

        public int Id
        {
            get;
            set;
        }

        public string Name
        {
            get;
            set;
        }

        public int PostalCode
        {
            get;
            set;
        }

        public GeoCoordinate Location
        {
            get;
            set;
        }

        public int CompareTo(GeoLocationCity other)
        {
            if (other == null)
                return -1;

            return this.Name.CompareTo(other.Name);
        }
    }
}
