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
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Newtonsoft.Json;

namespace DMI.Models
{
    public class BingLocationResponse
    {
        [JsonProperty("resourceSets")]
        [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] 
        public IList<ResourceSet> ResourceSets
        {
            get;
            set;
        }

        [JsonProperty("statusCode")]
        public string StatusCode
        {
            get;
            set;
        }

        [JsonProperty("statusDescription")]
        public string StatusDescription
        {
            get;
            set;
        }

        public class ResourceSet
        {
            [JsonProperty("resources")]
            [SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")] 
            public IList<Resource> Resources
            {
                get;
                set;
            }

            public class Resource
            {
                [JsonProperty("address")]
                public ResourceAddress Address
                {
                    get;
                    set;
                }

                public class ResourceAddress
                {
                    [JsonProperty("addressLine")]
                    public string AddressLine
                    {
                        get;
                        set;
                    }

                    [JsonProperty("adminDistrict")]
                    public string AdminDistrict
                    {
                        get;
                        set;
                    }

                    [JsonProperty("adminDistrict2")]
                    public string AdminDistrict2
                    {
                        get;
                        set;
                    }

                    [JsonProperty("countryRegion")]
                    public string CountryRegion
                    {
                        get;
                        set;
                    }

                    [JsonProperty("formattedAddress")]
                    public string FormattedAddress
                    {
                        get;
                        set;
                    }

                    [JsonProperty("locality")]
                    public string Locality
                    {
                        get;
                        set;
                    }

                    [JsonProperty("postalCode")]
                    public string PostalCode
                    {
                        get;
                        set;
                    }
                }
            }
        }
    }
}