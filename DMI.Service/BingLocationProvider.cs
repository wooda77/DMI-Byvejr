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
using RestSharp;
using DMI.Service.Properties;

namespace DMI.Service
{
    public static class BingLocationProvider
    {
        /// <summary>
        /// Resolves an address from a <see cref="T:GeoCoordinate"/> using Bing Maps.
        /// </summary>
        /// <param name="geoCoordinate"></param>
        public static void ResolveLocation(GeoCoordinate geoCoordinate, Action<CivicAddress, Exception> callback)
        {
            if (geoCoordinate == null)
                throw new ArgumentException("Argument geoCoordinate cannot be null.");

            var client = new RestClient("https://dev.virtualearth.net");

            var request = new RestRequest();
            request.Resource = string.Format(CultureInfo.InvariantCulture,
                "REST/v1/Locations/{0},{1}?key={2}",
                geoCoordinate.Latitude,
                geoCoordinate.Longitude,
                AppResources.BingMapsKey
            );

            client.ExecuteAsync<BingLocationResponse>(request,
                (response) =>
                {
                    var result = response.Data;
                    var civicAddress = new CivicAddress();

                    if ((result.ResourceSets.Count > 0) &&
                        (result.ResourceSets[0].Resources.Count > 0))
                    {
                        var resources = result.ResourceSets[0].Resources[0];

                        civicAddress.CountryRegion = resources.Address.CountryRegion;
                        civicAddress.AddressLine1 = resources.Address.AddressLine;
                        civicAddress.PostalCode = resources.Address.PostalCode;
                    }

                    callback(civicAddress, response.ErrorException);
                });
        }
    }
}