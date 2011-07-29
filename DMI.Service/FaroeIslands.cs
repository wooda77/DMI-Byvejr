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
using System.Collections.Generic;
using System.Device.Location;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using DMI.Service.Properties;
using DMI.Common;

namespace DMI.Service
{
    public class FaroeIslands : IWeatherProvider
    {
        public const string Name = "Faroe Islands";

        private static FaroeIslands instance;

        public static FaroeIslands Instance
        {
            get
            {
                if (instance == null)
                    instance = new FaroeIslands();

                return instance;
            }
        }

        #region IWeatherProvider Members

        public bool HasRegionalWeather
        {
            get
            {
                return false;
            }
        }

        public bool HasPollenData
        {
            get
            {
                return false;
            }
        }

        public void GetCityWeather(GeoCoordinate location, string postalCode, Action<CityWeatherResult, Exception> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            if (string.IsNullOrEmpty(postalCode))
                postalCode = GetPostalCodeFromGeoCoordinate(location).ToString();

            var result = new CityWeatherResult()
            {
                CityWeatherThreeDaysImage = new Uri(string.Format(
                    Resources.FaroeIslands_CityWeatherThreeDaysImage, postalCode)),
                CityWeatherSevenDaysImage = new Uri(string.Format(
                    Resources.FaroeIslands_CityWeatherSevenDaysImage, postalCode)),
            };

            callback(result, null);
        }

        public void GetRegionalWeather(GeoCoordinate location, string postalCode, Action<RegionalWeatherResult, Exception> callback)
        {
            throw new NotImplementedException();
        }

        public void GetCountryWeather(Action<CountryWeatherResult, Exception> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var client = HttpWebRequest.Create(Resources.FaroeIslands_CountryFeed);
            client.DownloadStringAsync(html =>
            {
                var input = HttpUtility.HtmlDecode(html);

                var pattern = @"<td class=""broedtekst"">(?<content>.*?)</td>";

                var regex = new Regex(pattern, RegexOptions.Singleline);
                var matches = regex.Matches(input);

                var description = new StringBuilder();

                foreach (var match in matches.Cast<Match>().Skip(1))
                {
                    var content = match.Groups["content"].Value;
                    content = content.Replace("<br>", Environment.NewLine).Trim();

                    if (string.IsNullOrEmpty(content) == false)
                        description.AppendLine(content);
                }

                var result = new CountryWeatherResult()
                {
                    Image = null,
                    Items = new List<CountryWeatherItem>()
                    {
                        new CountryWeatherItem() 
                        {
                            Title = "Udsigt for Færøerne",
                            Description = description.ToString()
                        }
                    }
                };

                callback(result, null);
            });
        }

        public void GetPollenData(GeoCoordinate location, string postalCode, Action<PollenResult, Exception> callback)
        {
            throw new NotImplementedException();
        }

        #endregion

        private static int GetPostalCodeFromGeoCoordinate(GeoCoordinate location)
        {
            var shortestDistance = 0.0;
            var nearestCity = PostalCodes[6011];

            foreach (var city in PostalCodes.Values)
            {
                var distance = city.Location.GetDistanceTo(location);
                if (distance < shortestDistance)
                {
                    shortestDistance = distance;
                    nearestCity = city;
                }
            }

            return nearestCity.PostalCode;
        }

        #region List of known cities on the Fareo Islands

        public static IDictionary<int, GeoLocationCity> PostalCodes = new Dictionary<int, GeoLocationCity>()
        {
            { 6009, new GeoLocationCity("Faroe Islands", 6009, 2624923, "Akraberg", 61.400663, -6.687927) },
            { 6012, new GeoLocationCity("Faroe Islands", 6012, 2621795, "Fugloy", 62.321555, -6.325378) },
            { 6005, new GeoLocationCity("Faroe Islands", 6005, 2616648, "Mykines", 62.097457, -7.676697) },
            { 6010, new GeoLocationCity("Faroe Islands", 6010, 2612890, "Sørvágur/Vágar", 62.082833, -7.318611) },
            { 6011, new GeoLocationCity("Faroe Islands", 6011, 2611396, "Tórshavn", 62.017985, -6.782684) },
        };

        #endregion
    }
}
