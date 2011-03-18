//
// LocationProvider.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Device.Location;
using Newtonsoft.Json;
using System.Globalization;

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

                    if ((result.resourceSets.Length > 0) 
                     && (result.resourceSets[0].resources.Length > 0))
                    {
                        var resources = result.resourceSets[0].resources[0];
                    
                        civicAddress.AddressLine1 = resources.address.addressLine;
                        civicAddress.PostalCode = resources.address.postalCode;
                    }

                    callback(civicAddress, e.Error);
                }
            };

            client.DownloadStringAsync(new Uri(requestUriString));
        }
    }
}