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
using System.Xml.Linq;
using DMI.Service.Properties;
using DMI.Common;

namespace DMI.Service
{
    public class Greenland : IWeatherProvider
    {
        public const string Name = "Greenland";

        private static Greenland instance;

        public static Greenland Instance
        {
            get
            {
                if (instance == null)
                    instance = new Greenland();

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
                    Resources.Greenland_CityWeatherThreeDaysImage, postalCode)),
                CityWeatherSevenDaysImage = new Uri(string.Format(
                    Resources.Greenland_CityWeatherSevenDaysImage, postalCode)),
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

            // TODO: Replace with HttpWebRequest
            var client = new WebClient()
            {
                Encoding = Encoding.GetEncoding("iso-8859-1")
            };

            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    callback(new CountryWeatherResult(), e.Error);
                }
                else
                {
                    var input = HttpUtility.HtmlDecode(e.Result);

                    var pattern = @"<td class=""broedtekst"">(?<content>.*?)</td>";

                    var regex = new Regex(pattern, RegexOptions.Singleline);
                    var matches = regex.Matches(input);

                    var description = new StringBuilder();

                    foreach (var match in matches.Cast<Match>().Skip(1))
                    {
                        var content = match.Groups["content"].Value;
                        content = content.Replace("<br>", Environment.NewLine).Trim();

                        if (string.IsNullOrEmpty(content) == false)
                        {
                            description.AppendLine(content);
                            description.AppendLine();
                        }
                    }

                    var result = new CountryWeatherResult()
                    {
                        Image = null,
                        Items = new List<CountryWeatherItem>()
                        {
                            new CountryWeatherItem() 
                            {
                                Title = Resources.Greenland_CountryWeatherTitle,
                                Description = description.ToString()
                            }
                        }
                    };

                    callback(result, e.Error);
                }
            };

            client.DownloadStringAsync(new Uri(Resources.Greenland_CountryFeed));
        }

        public void GetPollenData(GeoCoordinate location, string postalCode, Action<PollenResult, Exception> callback)
        {
            throw new NotImplementedException();            
        }

        #endregion

        private static int GetPostalCodeFromGeoCoordinate(GeoCoordinate location)
        {
            GeoLocationCity nearestCity = PostalCodes[4214];
            double shortestDistance = nearestCity.Location.GetDistanceTo(location);

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

        #region List of known cities on Greenland

        public static IDictionary<int, GeoLocationCity> PostalCodes = new Dictionary<int, GeoLocationCity>()
        {
            { 4285, new GeoLocationCity("Greenland", 4285, 3423771, "Angissoq", 59.983339, -45.183313) },
            { 4351, new GeoLocationCity("Greenland", 4351, 3424495, "Aputiteeq", 66.016745, -35.866671) },
            { 9997, new GeoLocationCity("Greenland", 9997, 3424452, "Arsuk", 61.178219, -48.459213) },
            { 4228, new GeoLocationCity("Greenland", 4228, 2724324, "Attu", 67.940812, -53.620834) },
            { 4330, new GeoLocationCity("Greenland", 4330, 2760249, "Daneborg (Sirius-patruljen)", 74.295463, -20.247803) },
            { 4320, new GeoLocationCity("Greenland", 4320, 2760249, "Danmarkshavn", 78.453226, -19.193115) },
            { 4207, new GeoLocationCity("Greenland", 4207, 5055312, "Hall Land", 81.456474, -58.557129) },
            { 4313, new GeoLocationCity("Greenland", 4313, 2760249, "Henrik Krøyer Holme", 80.683566, -13.750076) },
            { 4373, new GeoLocationCity("Greenland", 4373, 3419291, "Ikermit", 64.78336, -40.316648) },
            { 4221, new GeoLocationCity("Greenland", 4221, 3423146, "Ilulissat", 69.21708, -51.100502) },
            { 4339, new GeoLocationCity("Greenland", 4339, 3422891, "Ittoqqortoormiit", 70.485312, -21.966648) },
            { 9998, new GeoLocationCity("Greenland", 9998, 3422861, "Ivittuut", 61.203036, -48.179941) },
            { 4231, new GeoLocationCity("Greenland", 4231, 3419842, "Kangerlussuaq ", 67.008618, -50.689158) },
            { 9995, new GeoLocationCity("Greenland", 9995, 3422700, "Kangaamiut", 65.824983, -53.33755) },
            { 4825, new GeoLocationCity("Greenland", 4825, 3422683, "Kangaatsiaq", 68.307076, -53.463593) },
            { 4301, new GeoLocationCity("Greenland", 4301, 2760249, "Kap Morris Jesup", 83.633339, -32.516613) },
            { 4208, new GeoLocationCity("Greenland", 4208, 3422228, "Kitsissorsuit", 74.140084, -57.128906) },
            { 4203, new GeoLocationCity("Greenland", 4203, 3420101, "Kitsissut/Careyøer", 59.987482, -45.186939) },
            { 4241, new GeoLocationCity("Greenland", 4241, 3421982, "Maniitsoq", 65.416657, -52.900007) },
            { 4283, new GeoLocationCity("Greenland", 4283, 3421765, "Nanortalik", 60.142327, -45.244961) },
            { 4280, new GeoLocationCity("Greenland", 4280, 3421717, "Narsaq", 60.914998, -46.050911) },
            { 4270, new GeoLocationCity("Greenland", 4270, 3421712, "Narsarsuaq", 61.155991, -45.42263) },
            { 4266, new GeoLocationCity("Greenland", 4266, 3424452, "Nunarsuit", 60.749431, -47.963562) },
            { 4250, new GeoLocationCity("Greenland", 4250, 3421319, "Nuuk", 64.183617, -51.721407) },
            { 4214, new GeoLocationCity("Greenland", 4214, 3831252, "Nuussuaq", 64.182425, -51.710259) },
            { 4202, new GeoLocationCity("Greenland", 4202, 3831683, "Pituffik (Thule AirBase)", 76.530101, -68.705578) },
            { 4260, new GeoLocationCity("Greenland", 4260, 3421193, "Paamiut", 61.994215, -49.666958) },
            { 4272, new GeoLocationCity("Greenland", 4272, 3420846, "Qaqortoq", 60.720102, -46.034689) },
            { 4817, new GeoLocationCity("Greenland", 4817, 3420769, "Qasigiannguit", 68.819463, -51.193371) },
            { 4219, new GeoLocationCity("Greenland", 4219, 3420636, "Qeqertarsuaq", 69.243803, -53.552942) },
            { 9996, new GeoLocationCity("Greenland", 9996, 3419310, "Qeqertarsuatsiaat", 69.247575, -53.535433) },
            { 4205, new GeoLocationCity("Greenland", 4205, 3831208, "Qaanaaq (Thule)", 77.482707, -69.345016) },
            { 9994, new GeoLocationCity("Greenland", 9994, 3831146, "Savissivik", 76.019453, -65.114336) },
            { 4242, new GeoLocationCity("Greenland", 4242, 3419291, "Sioralik", 66.366597, -35.45022) },
            { 4234, new GeoLocationCity("Greenland", 4234, 3419842, "Sisimiut", 66.938889, -53.672209) },
            { 4312, new GeoLocationCity("Greenland", 4312, 2760249, "Station Nord", 81.534461, -16.54541) },
            { 4360, new GeoLocationCity("Greenland", 4360, 3424607, "Tasiilaq (Angmagsalik)", 65.60433, -37.707224) },
            { 9999, new GeoLocationCity("Greenland", 9999, 3419310, "Timmiarmiut", 64.31373, -40.770264) },
            { 4253, new GeoLocationCity("Greenland", 4253, 3420101, "Ukiivik", 60.766643, -47.616677) },
            { 4211, new GeoLocationCity("Greenland", 4211, 3418910, "Upernavik", 72.787142, -56.147604) },
            { 4213, new GeoLocationCity("Greenland", 4213, 3418845, "Uummannaq", 70.674858, -52.126522) },
            { 4220, new GeoLocationCity("Greenland", 4220, 3424904, "Aasiaat", 68.709785, -52.869473) },
        };

        #endregion
    }
}
