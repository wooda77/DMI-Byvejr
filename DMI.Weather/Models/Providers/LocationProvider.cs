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
using System.Globalization;
using System.Net;
using Newtonsoft.Json;

namespace DMI.Models
{
    public static class LocationProvider
    {
        private static readonly string bingMapsKey = "AlqAO_r2pYHWEKTnYolKAITA9Ix8p5mHO26jZ2bWZbmn1FHMU6eEJKRgqnS2v-P4";
        private static readonly string bingMapsRESTUri = "https://dev.virtualearth.net/REST/v1/Locations/{0},{1}?key={2}";

        /// <summary>
        /// Resolves a geo-location using Bing Maps.
        /// </summary>
        /// <param name="geoCoordinate"></param>
        public static void ResolveLocation(GeoCoordinate geoCoordinate, Action<CivicAddress, Exception> callback)
        {
            if (geoCoordinate == null)
            {
                throw new ArgumentException("geoCoordinate");
            }

            var requestUriString = string.Format(CultureInfo.InvariantCulture, bingMapsRESTUri,
                geoCoordinate.Latitude, geoCoordinate.Longitude, bingMapsKey);

            var client = new WebClient();

            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    callback(null, e.Error);
                }
                else
                {
                    var result = JsonConvert.DeserializeObject<BingLocationResponse>(e.Result);

                    var civicAddress = new CivicAddress();

                    if ((result.ResourceSets.Count > 0) 
                     && (result.ResourceSets[0].Resources.Count > 0))
                    {
                        var resources = result.ResourceSets[0].Resources[0];

                        civicAddress.CountryRegion = resources.Address.CountryRegion;

                        if (resources.Address.CountryRegion == "Faroe Islands")                            
                        {
                            civicAddress.PostalCode = "6011";
                        }
                        else if(resources.Address.CountryRegion == "Greenland")
                        {
                            civicAddress.PostalCode = "4250";
                        }
                        else
                        {
                            civicAddress.AddressLine1 = resources.Address.AddressLine;
                            civicAddress.PostalCode = resources.Address.PostalCode;
                        }
                    }

                    callback(civicAddress, e.Error);
                }
            };

            client.DownloadStringAsync(new Uri(requestUriString));
        }
    }
}