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
using System.Linq;
using System.Collections.Generic;

namespace DMI.Models
{
    public class CityGroups : List<CityGroup>
    {
        private static readonly string Groups = "abcdefghijklmnopqrstuvæøå";

        public CityGroups(List<City> cities)
        {
            if (cities == null)
            {
                throw new ArgumentException("cities");
            }

            cities.Sort();

            var groups = new Dictionary<string, CityGroup>();

            foreach (char c in Groups)
            {
                var group = new CityGroup(c.ToString());                
                this.Add(group);
                groups[c.ToString()] = group;
            }

            foreach (City city in cities)
            {
                if (city.Name.Length > 0) 
                {
                    groups[char.ToLower(city.Name[0]).ToString()].Add(city);
                }
            }
        }
    }
}
