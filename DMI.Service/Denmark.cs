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
using System.IO;

namespace DMI.Service
{
    public class Denmark : IWeatherProvider
    {
        public const int DefaultPostalCode = 1000; // Copenhagen
        public const string Name = "Denmark";

        private static Denmark instance;

        public static Denmark Instance
        {
            get
            {
                if (instance == null)
                    instance = new Denmark();

                return instance;
            }
        }

        public static GeoCoordinate CenterCoordinate = new GeoCoordinate(56.183782, 10.395813);

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
            {
                GetPostalCodeFromGeoCoordinate(location,
                    (result) =>
                    {
                        GetCityWeatherFromPostalCode(result, callback);
                    });
            }
            else
            {
                int result = 0;
                if (int.TryParse(postalCode, out result))
                {
                    GetCityWeatherFromPostalCode(GetValidPostalCode(result), callback);
                }
            }
        }

        public void GetRegionalWeather(GeoCoordinate location, string postalCode, Action<RegionalWeatherResult, Exception> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            if (string.IsNullOrEmpty(postalCode))
            {
                GetPostalCodeFromGeoCoordinate(location,
                    (result) =>
                    {
                        GetRegionalWeatherFromPostalCode(result, callback);
                    });
            }
            else
            {
                int result = 0;
                if (int.TryParse(postalCode, out result))
                {
                    GetRegionalWeatherFromPostalCode(GetValidPostalCode(result), callback);
                }
            }
        }

        public void GetCountryWeather(Action<CountryWeatherResult, Exception> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var client = HttpWebRequest.Create(Resources.Denmark_CountryFeed);
            client.DownloadStringAsync(html =>
            {
                    var input = HttpUtility.HtmlDecode(html);

                    var pattern = @"<td class=""mellemrubrik"">(?<title>.*?)</td>";
                    pattern += @"(.*?)<td class=""broedtekst"">(?<description>.*?)</td>";

                    var regex = new Regex(pattern, RegexOptions.Singleline);
                    var matches = regex.Matches(input);

                    var items = new List<CountryWeatherItem>();

                    foreach (var match in matches.Cast<Match>())
                    {
                        var title = match.Groups["title"].Value;
                        title = title.Trim();
                        title = title.Replace(":", "");

                        var description = match.Groups["description"].Value;
                        description = description.Trim();

                        if (string.IsNullOrEmpty(title) == false)
                        {
                            items.Add(new CountryWeatherItem()
                            {
                                Title = title,
                                Description = description
                            });
                        }
                    }

                    var countryWeatherResult = new CountryWeatherResult()
                    {
                        Image = new Uri(Resources.Denmark_CountryImage),
                        Items = items
                    };

                    callback(countryWeatherResult, null);
            });
        }

        public void GetPollenData(GeoCoordinate location, string postalCode, Action<PollenResult, Exception> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            if (string.IsNullOrEmpty(postalCode))
            {            
                GetPostalCodeFromGeoCoordinate(location,
                    (result) =>
                    {
                        GetPollenDataFromPostalCode(result, callback);
                    });
            }
            else
            {
                int result = 0;
                if (int.TryParse(postalCode, out result))
                {
                    GetPollenDataFromPostalCode(GetValidPostalCode(result), callback);
                }
            }
        }

        #endregion

        private static void GetCityWeatherFromPostalCode(int postalCode, Action<CityWeatherResult, Exception> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var result = new CityWeatherResult()
            {
                CityWeatherThreeDaysImage = new Uri(string.Format(
                    Resources.Denmark_CityWeatherThreeDaysImage, postalCode)),
                CityWeatherSevenDaysImage = new Uri(string.Format(
                    Resources.Denmark_CityWeatherSevenDaysImage, postalCode)),
            };

            callback(result, null);
        }

        private static void GetRegionalWeatherFromPostalCode(int postalCode, Action<RegionalWeatherResult, Exception> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var client = HttpWebRequest.Create(GetRegionalContentFromPostalCode(postalCode));
            client.DownloadStringAsync(html =>
            {
                var input = HttpUtility.HtmlDecode(html);

                var textPattern = @"<td class=""broedtekst"">(?<text>.*?)</td>";
                var textRegex = new Regex(textPattern, RegexOptions.Singleline);
                var textMatches = textRegex.Matches(input);

                var namePattern = @"<font class=""mellemrubrik"">(?<text>.*?)</font>";
                var nameRegex = new Regex(namePattern, RegexOptions.Singleline);
                var nameMatches = nameRegex.Matches(input);

                var region = new RegionalWeatherResult();

                region.Image = new Uri(GetRegionalImageFromPostalCode(postalCode));

                if (nameMatches.Count >= 1)
                    region.Name = nameMatches[0].Groups["text"].Value;

                if (textMatches.Count >= 3)
                    region.Content = textMatches[2].Groups["text"].Value;

                callback(region, null);
            });
        }

        private static void GetPollenDataFromPostalCode(int postalCode, Action<PollenResult, Exception> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var client = HttpWebRequest.Create(Resources.PollenFeed);
            client.DownloadStringAsync(html =>
            {
                var allItems = XElement.Parse(html)
                    .Elements("channel")
                    .Elements("item")
                    .ToArray();

                var pollenItems = allItems.Take(4).Chunks(2).ToArray()
                    .Where(x => x.Length >= 1)
                    .Select(x => new PollenItem()
                    {
                        City = x[0].Element("title").TryGetValue(),
                        Data = Utils.ParsePollenData(x[0].Element("description").TryGetValue()),
                        Forecast = x[1].Element("description").TryGetValue()
                    });

                var result = new PollenResult()
                {
                    Image = new Uri(string.Format(Resources.PollenImage, postalCode)),
                    Items = pollenItems
                };

                callback(result, null);
            });
        }

        private static void GetPostalCodeFromGeoCoordinate(GeoCoordinate location, Action<int> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            BingLocationProvider.ResolveLocation(location,
                (address, exception) =>
                {
                    if (string.IsNullOrEmpty(address.PostalCode))
                    {
                        callback(GetValidPostalCode(DefaultPostalCode));
                    }
                    else
                    {
                        int postalCode = DefaultPostalCode;
                        
                        if (int.TryParse(address.PostalCode, out postalCode))
                            callback(GetValidPostalCode(postalCode));
                    }
                });
        }

        private static string GetRegionalContentFromPostalCode(int postalCode)
        {
            if (postalCode >= 1000 && postalCode <= 2999)
                return Resources.RegionalText_NorthZealand;
            else if (postalCode >= 3000 && postalCode <= 3699)
                return Resources.RegionalText_NorthZealand;
            else if (postalCode >= 4000 && postalCode <= 4999)
                return Resources.RegionalText_SouthZealand;
            else if (postalCode >= 3700 && postalCode <= 3799)
                return Resources.RegionalText_Bornholm;
            else if (postalCode >= 5000 && postalCode <= 5999)
                return Resources.RegionalText_Fyn;
            else if (postalCode >= 6000 && postalCode <= 6999)
                return Resources.RegionalText_SouthJytland;
            else if (postalCode >= 7000 && postalCode <= 7999)
                return Resources.RegionalText_MiddleJytland;
            else if (postalCode >= 8000 && postalCode <= 8999)
                return Resources.RegionalText_EastJytland;
            else if (postalCode >= 9000 && postalCode <= 9999)
                return Resources.RegionalImage_NorthJytland;
            else
                return Resources.RegionalText_NorthZealand; // Default to Copenhagen
        }

        private static string GetRegionalImageFromPostalCode(int postalCode)
        {
            if (postalCode >= 1000 && postalCode <= 2999)
                return Resources.RegionalImage_NorthZealand;
            else if (postalCode >= 3000 && postalCode <= 3699)
                return Resources.RegionalImage_NorthZealand;
            else if (postalCode >= 4000 && postalCode <= 4999)
                return Resources.RegionalImage_SouthZealand;
            else if (postalCode >= 3700 && postalCode <= 3799)
                return Resources.RegionalImage_Bornholm;
            else if (postalCode >= 5000 && postalCode <= 5999)
                return Resources.RegionalImage_Fyn;
            else if (postalCode >= 6000 && postalCode <= 6999)
                return Resources.RegionalImage_SouthJytland;
            else if (postalCode >= 7000 && postalCode <= 7999)
                return Resources.RegionalImage_MiddleJytland;
            else if (postalCode >= 8000 && postalCode <= 8999)
                return Resources.RegionalImage_EastJytland;
            else if (postalCode >= 9000 && postalCode <= 9999)
                return Resources.RegionalImage_NorthJytland;
            else
                return Resources.RegionalImage_NorthZealand; // Default to Copenhagen
        }

        private static int GetValidPostalCode(int postalCode)
        {
            if (postalCode < 1800)
                return 1000;
            else if (postalCode < 2000)
                return 2000;
            else if (postalCode > 2000 && postalCode < 2500)
                return 1000;
            else if (postalCode > 5000 && postalCode < 5280)
                return 5000;
            else if (postalCode > 6000 && postalCode < 6020)
                return 6000;
            else if (postalCode > 6700 && postalCode < 6720)
                return 6700;
            else if (postalCode > 7100 && postalCode < 7130)
                return 7100;
            else if (postalCode > 8000 && postalCode < 8220)
                return 8000;
            else if (postalCode > 8900 && postalCode < 8950)
                return 8900;
            else if (postalCode == 8920 || postalCode == 8930 || postalCode == 8940 || postalCode == 8960)
                return 8900;
            else if (postalCode > 9000 && postalCode < 9230)
                return 9000;
            else if (postalCode > 9999)
                return 1000;

            return postalCode;
        }

        #region Beaches

        public static List<Beach> Beaches = new List<Beach>() 
        {
            new Beach { Latitude=57.735684, Longitude=10.583954, ID = 9033, Name = "Skagen" }, 
            new Beach { Latitude=57.33764, Longitude=10.513916, ID = 9030, Name = "Sæby" }, 
            new Beach { Latitude=57.279785, Longitude=10.991135, ID = 9019, Name = "Læsø", HasBlueFlag = false }, 
            new Beach { Latitude=57.373938, Longitude=9.720154, ID = 9021, Name = "Løkken" }, 
            new Beach { Latitude=57.255652, Longitude=9.584541, ID = 9031, Name = "Blokhus" }, 
            new Beach { Latitude=56.950966, Longitude=8.388062, ID = 9018, Name = "Vorupør" }, 
            new Beach { Latitude=56.701301, Longitude=8.211594, ID = 9036, Name = "Thyborøn" }, 
            new Beach { Latitude=56.024483, Longitude=8.12439, ID = 9012, Name = "Hvide Sande" }, 
            new Beach { Latitude=55.737017, Longitude=8.17606, ID = 9009, Name = "Henne" }, 
            new Beach { Latitude=55.420831, Longitude=8.415527, ID = 9006, Name = "Fanø" }, 
            new Beach { Latitude=55.145526, Longitude=8.510284, ID = 9029, Name = "Rømø" }, 
            new Beach { Latitude=56.79948, Longitude=10.278203, ID = 9043, Name = "Øster Hurup" }, 
            new Beach { Latitude=56.512154, Longitude=10.49675, ID = 9044, Name = "Rygaard" }, 
            new Beach { Latitude=56.412382, Longitude=10.894318, ID = 9008, Name = "Grenå" }, 
            new Beach { Latitude=56.088799, Longitude=10.248184, ID = 9028, Name = "Moesgaard" }, 
            new Beach { Latitude=55.710093, Longitude=9.998245, ID = 9013, Name = "Juelsminde" }, 
            new Beach { Latitude=55.511527, Longitude=9.899368, ID = 9004, Name = "Båring Vig" }, 
            new Beach { Latitude=54.896453, Longitude=9.715519, ID = 9039, Name = "Vemmingbund" }, 
            new Beach { Latitude=55.567087, Longitude=10.093689, ID = 9003, Name = "Bogense", HasBlueFlag = false }, 
            new Beach { Latitude=55.621018, Longitude=10.296249, ID = 9007, Name = "Flyvesandet", HasBlueFlag = false }, 
            new Beach { Latitude=55.27051, Longitude=9.901428, ID = 9045, Name = "Assens" }, 
            new Beach { Latitude=55.088291, Longitude=10.255737, ID = 9005, Name = "Faaborg/Klinten" }, 
            new Beach { Latitude=54.856059, Longitude=10.509109, ID = 9023, Name = "Marstal", HasBlueFlag = false }, 
            new Beach { Latitude=55.042483, Longitude=10.706778, ID = 9034, Name = "Smørmosen" }, 
            new Beach { Latitude=55.133752, Longitude=10.910025, ID = 9020, Name = "Lohals" }, 
            new Beach { Latitude=55.324457, Longitude=10.798874, ID = 9025, Name = "Nyborg" }, 
            new Beach { Latitude=55.453941, Longitude=10.653992, ID = 9016, Name = "Kerteminde" }, 
            new Beach { Latitude=55.257012, Longitude=11.251373, ID = 9017, Name = "Kobæk" }, 
            new Beach { Latitude=55.184356, Longitude=11.646881, ID = 9015, Name = "Karrebæksminde" }, 
            new Beach { Latitude=55.030974, Longitude=12.297134, ID = 9037, Name = "Ulvshale" }, 
            new Beach { Latitude=54.689858, Longitude=11.969776, ID = 9022, Name = "Marielyst", HasBlueFlag = false }, 
            new Beach { Latitude=54.632406, Longitude=11.40928, ID = 9027, Name = "Østersøbadet" }, 
            new Beach { Latitude=54.656159, Longitude=11.350079, ID = 9047, Name = "Rødbyhavn" }, 
            new Beach { Latitude=54.833918, Longitude=11.096878, ID = 9010, Name = "Hestehovedet" }, 
            new Beach { Latitude=55.533488, Longitude=12.22126, ID = 9035, Name = "Solrød", HasBlueFlag = false }, 
            new Beach { Latitude=55.680682, Longitude=11.072845, ID = 9014, Name = "Kalundborg" }, 
            new Beach { Latitude=55.830601, Longitude=11.393509, ID = 9040, Name = "Sejerøbugten" }, 
            new Beach { Latitude=55.945163, Longitude=11.756058, ID = 9026, Name = "Rørvig", HasBlueFlag = false }, 
            new Beach { Latitude=55.678504, Longitude=12.07818, ID = 9038, Name = "Veddelev", HasBlueFlag = false }, 
            new Beach { Latitude=56.059578, Longitude=12.071228, ID = 9049, Name = "Tisvilde" }, 
            new Beach { Latitude=56.0933, Longitude=12.455406, ID = 9011, Name = "Hornbæk" }, 
            new Beach { Latitude=55.931414, Longitude=12.519264, ID = 9024, Name = "Nivå", HasBlueFlag = false }, 
            new Beach { Latitude=55.756003, Longitude=12.580547, ID = 9046, Name = "Charlottenlund", HasBlueFlag = false }, 
            new Beach { Latitude=55.655897, Longitude=12.642517, ID = 9041, Name = "Amager Strandpark" }, 
            new Beach { Latitude=55.25689, Longitude=14.819269, ID = 9032, Name = "Sandvig", HasBlueFlag = false }, 
            new Beach { Latitude=55.084459, Longitude=14.706659, ID = 9048, Name = "Antoinette", HasBlueFlag = false }, 
            new Beach { Latitude=55.006924, Longitude=15.103884, ID = 9001, Name = "Dueodde" },  
        };

        #endregion

        #region List of known cities in Denmark

        public static IDictionary<int, GeoLocationCity> PostalCodes = new Dictionary<int, GeoLocationCity>()
        {
            { 5320, new GeoLocationCity("Denmark", 5320, 2624988, "Agedrup", 55.4166667, 10.5) },
            { 6753, new GeoLocationCity("Denmark", 6753, 2624986, "Agerbæk", 55.6, 8.8) },
            { 6534, new GeoLocationCity("Denmark", 6534, 2624973, "Agerskov", 55.1166667, 9.1333333) },
            { 2620, new GeoLocationCity("Denmark", 2620, 2624906, "Albertslund", 55.6569140460036, 12.3638141155243) },
            { 3450, new GeoLocationCity("Denmark", 3450, 2624843, "Allerød", 55.8666667, 12.3833333) },
            { 3770, new GeoLocationCity("Denmark", 3770, 2624816, "Allinge/Sandvig", 55.2, 14.916667) },
            { 8961, new GeoLocationCity("Denmark", 8961, 2624819, "Allingåbro", 56.4666667, 10.3333333) },
            { 6051, new GeoLocationCity("Denmark", 6051, 2624809, "Almind", 55.5666667, 9.5) },
            { 8592, new GeoLocationCity("Denmark", 8592, 2624693, "Anholt", 55.4, 9.3833333) },
            { 8643, new GeoLocationCity("Denmark", 8643, 2624679, "Ans By", 56.3166667, 9.6) },
            { 6823, new GeoLocationCity("Denmark", 6823, 2624678, "Ansager", 55.7, 8.75) },
            { 9510, new GeoLocationCity("Denmark", 9510, 2624660, "Arden", 56.7666667, 9.8833333) },
            { 4792, new GeoLocationCity("Denmark", 4792, 2624545, "Askeby", 54.9166667, 12.1833333) },
            { 4550, new GeoLocationCity("Denmark", 4550, 2624521, "Asnæs", 55.8166667, 11.5166667) },
            { 5466, new GeoLocationCity("Denmark", 5466, 2624511, "Asperup", 55.4833333, 9.9166667) },
            { 5610, new GeoLocationCity("Denmark", 5610, 2624502, "Assens", 55.2702259618287, 9.90080595016479) },
            { 9340, new GeoLocationCity("Denmark", 9340, 5455821, "Asaa", 35.9666778, -108.9339807) },
            { 6440, new GeoLocationCity("Denmark", 6440, 2624439, "Augustenborg", 54.95, 9.8833333) },
            { 7490, new GeoLocationCity("Denmark", 7490, 2624419, "Aulum", 56.2666667, 8.8) },
            { 8963, new GeoLocationCity("Denmark", 8963, 2624435, "Auning", 56.4333333, 10.3833333) },
            { 5935, new GeoLocationCity("Denmark", 5935, 2624398, "Bagenkop", 54.75, 10.6833333) },
            { 2880, new GeoLocationCity("Denmark", 2880, 2624388, "Bagsværd", 55.7666667, 12.4666667) },
            { 8444, new GeoLocationCity("Denmark", 8444, 2624361, "Balle på Djursland", 56.3, 10.2666667) },
            { 2750, new GeoLocationCity("Denmark", 2750, 2624341, "Ballerup", 55.7316456911196, 12.3632830381393) },
            { 7150, new GeoLocationCity("Denmark", 7150, 2624289, "Barrit", 55.7166667, 9.9) },
            { 8330, new GeoLocationCity("Denmark", 8330, 2624212, "Beder", 56.0602486926966, 10.2117919921875) },
            { 7755, new GeoLocationCity("Denmark", 7755, 2624208, "Bedsted Thy", 56.8166667, 8.4166667) },
            { 6541, new GeoLocationCity("Denmark", 6541, 2624163, "Bevtoft", 55.2, 9.2166667) },
            { 6852, new GeoLocationCity("Denmark", 6852, 2624148, "Billum", 55.6166667, 8.3333333) },
            { 7190, new GeoLocationCity("Denmark", 7190, 2624145, "Billund", 55.25, 9.2833333) },
            { 9881, new GeoLocationCity("Denmark", 9881, 2616357, "Bindslev", 57.55, 10.2) },
            { 3460, new GeoLocationCity("Denmark", 3460, 2624112, "Birkerød", 55.8475902458178, 12.4279081821442) },
            { 8850, new GeoLocationCity("Denmark", 8850, 2624019, "Bjerringbro", 56.3833333, 9.6666667) },
            { 6091, new GeoLocationCity("Denmark", 6091, 2624014, "Bjert", 55.4333333, 9.6166667) },
            { 4632, new GeoLocationCity("Denmark", 4632, 2623978, "Bjæverskov", 55.45, 12.0333333) },
            { 9492, new GeoLocationCity("Denmark", 9492, 2623911, "Blokhus", 57.25, 9.5833333) },
            { 5491, new GeoLocationCity("Denmark", 5491, 2623909, "Blommenslyst", 55.3833333, 10.2666667) },
            { 6857, new GeoLocationCity("Denmark", 6857, 2623928, "Blåvand", 55.55, 8.1333333) },
            { 4242, new GeoLocationCity("Denmark", 4242, 2623861, "Boeslunde", 55.3, 11.2833333) },
            { 5400, new GeoLocationCity("Denmark", 5400, 2623857, "Bogense", 55.5669139235959, 10.0886303186417) },
            { 4793, new GeoLocationCity("Denmark", 4793, 2623847, "Bogø By", 54.9333333, 12.05) },
            { 6392, new GeoLocationCity("Denmark", 6392, 2623837, "Bolderslev", 54.9833333, 9.3) },
            { 7441, new GeoLocationCity("Denmark", 7441, 2623694, "Bording", 56.1666667, 9.25) },
            { 4791, new GeoLocationCity("Denmark", 4791, 2623659, "Borre", 55, 12.4666667) },
            { 4140, new GeoLocationCity("Denmark", 4140, 2623619, "Borup på Sjælland", 55.1833333, 11.8) },
            { 8220, new GeoLocationCity("Denmark", 8220, 2623586, "Brabrand", 56.1530557219913, 10.1057052612305) },
            { 6740, new GeoLocationCity("Denmark", 6740, 2623570, "Bramming", 55.4666667, 8.7) },
            { 7330, new GeoLocationCity("Denmark", 7330, 2623554, "Brande", 55.95, 9.1166667) },
            { 6535, new GeoLocationCity("Denmark", 6535, 2623547, "Branderup J", 55.1166667, 9.0833333) },
            { 6261, new GeoLocationCity("Denmark", 6261, 2623505, "Bredebro", 55.05, 8.8333333) },
            { 7182, new GeoLocationCity("Denmark", 7182, 2623487, "Bredsten", 54.85, 9.95) },
            { 5464, new GeoLocationCity("Denmark", 5464, 2623433, "Brenderup Fyn", 55.1833333, 10.6666667) },
            { 6310, new GeoLocationCity("Denmark", 6310, 2623393, "Broager", 54.883333, 9.666667) },
            { 5672, new GeoLocationCity("Denmark", 5672, 2719212, "Broby", 56.255211784484, 14.0779709815979) },
            { 9460, new GeoLocationCity("Denmark", 9460, 2623306, "Brovst", 57.1, 9.5333333) },
            { 8654, new GeoLocationCity("Denmark", 8654, 2623270, "Bryrup", 56.0166667, 9.5166667) },
            { 8740, new GeoLocationCity("Denmark", 8740, 2623516, "Brædstrup", 55.9666667, 9.6166667) },
            { 2605, new GeoLocationCity("Denmark", 2605, 2623352, "Brøndby", 55.65, 12.4333333) },
            { 2660, new GeoLocationCity("Denmark", 2660, 2623341, "Brøndby Strand", 55.6166667, 12.35) },
            { 9700, new GeoLocationCity("Denmark", 9700, 2623337, "Brønderslev", 57.2702091402345, 9.94101762771606) },
            { 2700, new GeoLocationCity("Denmark", 2700, 2623322, "Brønshøj", 55.7, 12.5166667) },
            { 6650, new GeoLocationCity("Denmark", 6650, 2623321, "Brørup", 55.4833333, 9.0166667) },
            { 6372, new GeoLocationCity("Denmark", 6372, 2623208, "Bylderup - Bov", 54.9333333, 9.0833333) },
            { 6622, new GeoLocationCity("Denmark", 6622, 2623973, "Bække", 55.5666667, 9.15) },
            { 7660, new GeoLocationCity("Denmark", 7660, 2623964, "Bækmarksbro", 56.4166667, 8.3166667) },
            { 9574, new GeoLocationCity("Denmark", 9574, 2623894, "Bælum", 56.8333333, 10.1166667) },
            { 7080, new GeoLocationCity("Denmark", 7080, 2623736, "Børkop", 55.65, 9.65) },
            { 7650, new GeoLocationCity("Denmark", 7650, 2623711, "Bøvlingbjerg", 56.4333333, 8.2166667) },
            { 2920, new GeoLocationCity("Denmark", 2920, 2623188, "Charlottenlund", 55.75, 12.5833333) },
            { 6070, new GeoLocationCity("Denmark", 6070, 2623183, "Christiansfeld", 55.35, 9.4833333) },
            { 5380, new GeoLocationCity("Denmark", 5380, 2623140, "Dalby", 55.3166667, 12.0833333) },
            { 4261, new GeoLocationCity("Denmark", 4261, 2623117, "Dalmose", 55.3, 11.4333333) },
            { 4983, new GeoLocationCity("Denmark", 4983, 2623073, "Dannemare", 54.75, 11.2) },
            { 8721, new GeoLocationCity("Denmark", 8721, 2623060, "Daugård", 55.7333333, 9.7166667) },
            { 4293, new GeoLocationCity("Denmark", 4293, 2623028, "Dianalund", 55.5333333, 11.5) },
            { 2791, new GeoLocationCity("Denmark", 2791, 2622937, "Dragør", 55.5945464305369, 12.6663780212402) },
            { 9330, new GeoLocationCity("Denmark", 9330, 2622888, "Dronninglund", 57.15, 10.3) },
            { 3120, new GeoLocationCity("Denmark", 3120, 2622884, "Dronningmølle", 56.1, 12.4) },
            { 9352, new GeoLocationCity("Denmark", 9352, 2622844, "Dybvad", 57.2833333, 10.3666667) },
            { 2870, new GeoLocationCity("Denmark", 2870, 2610742, "Dyssegård", 55.75, 12.5333333) },
            { 5631, new GeoLocationCity("Denmark", 5631, 2622799, "Ebberup", 55.25, 9.9833333) },
            { 8400, new GeoLocationCity("Denmark", 8400, 2622793, "Ebeltoft", 56.1944211845909, 10.6821012496948) },
            { 6320, new GeoLocationCity("Denmark", 6320, 2622735, "Egernsund", 54.9, 9.6166667) },
            { 6040, new GeoLocationCity("Denmark", 6040, 2622686, "Egtved", 55.6166667, 9.3) },
            { 8250, new GeoLocationCity("Denmark", 8250, 2622780, "Egå", 56.2166667, 10.2666667) },
            { 5592, new GeoLocationCity("Denmark", 5592, 2622667, "Ejby", 55.4, 10.4333333) },
            { 7361, new GeoLocationCity("Denmark", 7361, 2622641, "Ejstrupholm", 55.9833333, 9.2833333) },
            { 7442, new GeoLocationCity("Denmark", 7442, 2622504, "Engesvang", 56.1666667, 9.35) },
            { 4895, new GeoLocationCity("Denmark", 4895, 2622457, "Errindlev", 54.6666667, 11.5) },
            { 7950, new GeoLocationCity("Denmark", 7950, 2622453, "Erslev", 56.366667, 10.083333) },
            { 6700, new GeoLocationCity("Denmark", 6700, 2622447, "Esbjerg", 55.4666667, 8.45) },
            { 4593, new GeoLocationCity("Denmark", 4593, 2622439, "Eskebjerg", 55.7166667, 11.3166667) },
            { 4863, new GeoLocationCity("Denmark", 4863, 2622431, "Eskilstrup", 54.85, 11.9) },
            { 3060, new GeoLocationCity("Denmark", 3060, 2622418, "Espergærde", 56, 12.5666667) },
            { 5600, new GeoLocationCity("Denmark", 5600, 2622383, "Faaborg", 55.0951046891091, 10.2422645688057) },
            { 6720, new GeoLocationCity("Denmark", 6720, 2622341, "Fanø", 55.4166667, 8.4166667) },
            { 9640, new GeoLocationCity("Denmark", 9640, 2622310, "Farsø", 56.7833333, 9.35) },
            { 3520, new GeoLocationCity("Denmark", 3520, 2622306, "Farum", 55.8085814748315, 12.3606598377228) },
            { 4640, new GeoLocationCity("Denmark", 4640, 6543931, "Faxe", 55.2944444333333, 12.0611111333333) },
            { 4654, new GeoLocationCity("Denmark", 4654, 6543931, "Faxe Ladeplads", 55.2944444333333, 12.0611111333333) },
            { 4944, new GeoLocationCity("Denmark", 4944, 2610475, "Fejø", 54.95, 11.4) },
            { 5863, new GeoLocationCity("Denmark", 5863, 2622221, "Ferritslev Fyn", 55.3, 10.6) },
            { 4173, new GeoLocationCity("Denmark", 4173, 2622144, "Fjenneslev", 55.4333333, 11.6666667) },
            { 9690, new GeoLocationCity("Denmark", 9690, 2622139, "Fjerritslev", 57.0833333, 9.2666667) },
            { 8762, new GeoLocationCity("Denmark", 8762, 2622097, "Flemming", 55.8666667, 9.6666667) },
            { 3480, new GeoLocationCity("Denmark", 3480, 2621957, "Fredensborg", 55.0833333, 14.7166667) },
            { 7000, new GeoLocationCity("Denmark", 7000, 2621951, "Fredericia", 55.5656763280375, 9.75256562232971) },
            { 2000, new GeoLocationCity("Denmark", 2000, 2621945, "Frederiksberg", 55.4166667, 11.5666667) },
            { 9900, new GeoLocationCity("Denmark", 9900, 2621927, "Frederikshavn", 57.4407346907764, 10.536607503891) },
            { 3600, new GeoLocationCity("Denmark", 3600, 2621912, "Frederikssund", 55.8395635993135, 12.0689588785172) },
            { 3300, new GeoLocationCity("Denmark", 3300, 2621910, "Frederiksværk", 55.9707302663499, 12.0224976539612) },
            { 5871, new GeoLocationCity("Denmark", 5871, 2621839, "Frørup", 55.25, 10.7166667) },
            { 7741, new GeoLocationCity("Denmark", 7741, 2621829, "Frøstrup", 55.7333333, 8.4166667) },
            { 4250, new GeoLocationCity("Denmark", 4250, 2621805, "Fuglebjerg", 55.3, 11.5666667) },
            { 7884, new GeoLocationCity("Denmark", 7884, 2713428, "Fur", 56.4666667, 15.5833333) },
            { 4591, new GeoLocationCity("Denmark", 4591, 2622017, "Føllenslev", 55.7166667, 11.3666667) },
            { 6683, new GeoLocationCity("Denmark", 6683, 2622005, "Føvling", 55.45, 8.95) },
            { 4540, new GeoLocationCity("Denmark", 4540, 2622326, "Fårevejle St.", 55.8, 11.45) },
            { 8990, new GeoLocationCity("Denmark", 8990, 2622303, "Fårup", 55.1666667, 11.9) },
            { 8882, new GeoLocationCity("Denmark", 8882, 2622291, "Fårvang", 56.2666667, 9.7333333) },
            { 7321, new GeoLocationCity("Denmark", 7321, 2621735, "Gadbjerg", 55.7666667, 9.3333333) },
            { 4621, new GeoLocationCity("Denmark", 4621, 2621728, "Gadstrup", 55.4333333, 9.8833333) },
            { 8464, new GeoLocationCity("Denmark", 8464, 2621710, "Galten", 56.15, 9.9166667) },
            { 9362, new GeoLocationCity("Denmark", 9362, 2621617, "Gandrup", 56.7333333, 9.8333333) },
            { 4874, new GeoLocationCity("Denmark", 4874, 2621551, "Gedser", 54.576439791073, 11.9300365447998) },
            { 9631, new GeoLocationCity("Denmark", 9631, 2621547, "Gedsted", 56.6833333, 9.35) },
            { 8751, new GeoLocationCity("Denmark", 8751, 2621546, "Gedved", 55.9333333, 9.85) },
            { 5591, new GeoLocationCity("Denmark", 5591, 2621523, "Gelsted", 55.3, 11.7666667) },
            { 2820, new GeoLocationCity("Denmark", 2820, 2621515, "Gentofte", 55.75, 12.55) },
            { 6621, new GeoLocationCity("Denmark", 6621, 2621485, "Gesten", 55.5166667, 9.2) },
            { 3250, new GeoLocationCity("Denmark", 3250, 2621471, "Gilleleje", 56.1166667, 12.3166667) },
            { 5854, new GeoLocationCity("Denmark", 5854, 2621458, "Gislev", 55.2166667, 10.6166667) },
            { 4532, new GeoLocationCity("Denmark", 4532, 2621456, "Gislinge", 55.7333333, 11.55) },
            { 9260, new GeoLocationCity("Denmark", 9260, 2621449, "Gistrup", 57, 10) },
            { 7323, new GeoLocationCity("Denmark", 7323, 2621448, "Give", 55.85, 9.25) },
            { 8983, new GeoLocationCity("Denmark", 8983, 2621432, "Gjerlev J", 56.5833333, 10.1333333) },
            { 8883, new GeoLocationCity("Denmark", 8883, 2621431, "Gjern", 56.2333333, 9.75) },
            { 5620, new GeoLocationCity("Denmark", 5620, 2621393, "Glamsbjerg", 55.2666667, 10.1166667) },
            { 6752, new GeoLocationCity("Denmark", 6752, 2621382, "Glejbjerg", 55.55, 8.8333333) },
            { 8585, new GeoLocationCity("Denmark", 8585, 2621376, "Glesborg", 56.4774722407594, 10.7233428955078) },
            { 2600, new GeoLocationCity("Denmark", 2600, 2621356, "Glostrup", 55.6666667, 12.4) },
            { 4171, new GeoLocationCity("Denmark", 4171, 2621348, "Glumsø", 55.35, 11.7) },
            { 6510, new GeoLocationCity("Denmark", 6510, 2621279, "Gram", 55.2894780621891, 9.04912948608398) },
            { 6771, new GeoLocationCity("Denmark", 6771, 2621236, "Gredstedbro", 55.4, 8.75) },
            { 8500, new GeoLocationCity("Denmark", 8500, 2621227, "Grenå", 56.366667, 10.833333) },
            { 2670, new GeoLocationCity("Denmark", 2670, 2621215, "Greve Strand", 55.5833333, 12.3) },
            { 4571, new GeoLocationCity("Denmark", 4571, 2621213, "Grevinge", 55.8, 11.5666667) },
            { 7200, new GeoLocationCity("Denmark", 7200, 2621193, "Grindsted", 55.75, 8.9333333) },
            { 6300, new GeoLocationCity("Denmark", 6300, 2621258, "Gråsten", 54.9166667, 9.6) },
            { 3230, new GeoLocationCity("Denmark", 3230, 2621101, "Græsted", 56.0666667, 12.2833333) },
            { 5892, new GeoLocationCity("Denmark", 5892, 2621076, "Gudbjerg Sydfyn", 55.15, 10.6666667) },
            { 3760, new GeoLocationCity("Denmark", 3760, 2621070, "Gudhjem", 55.2166667, 14.9833333) },
            { 5884, new GeoLocationCity("Denmark", 5884, 2621067, "Gudme", 55.15, 10.7166667) },
            { 4862, new GeoLocationCity("Denmark", 4862, 2621042, "Guldborg", 54.8666667, 11.75) },
            { 6690, new GeoLocationCity("Denmark", 6690, 2621307, "Gørding", 55.4833333, 8.8) },
            { 4281, new GeoLocationCity("Denmark", 4281, 2621304, "Gørlev Sjælland", 55.5392558932959, 11.2270832061768) },
            { 3330, new GeoLocationCity("Denmark", 3330, 2621303, "Gørløse", 55.8833333, 12.2) },
            { 6100, new GeoLocationCity("Denmark", 6100, 2620964, "Haderslev", 55.2537705628178, 9.48982313275337) },
            { 8370, new GeoLocationCity("Denmark", 8370, 2620956, "Hadsten", 56.3281855510411, 10.0493144989014) },
            { 9560, new GeoLocationCity("Denmark", 9560, 2620954, "Hadsund", 56.75, 10.2333333) },
            { 9370, new GeoLocationCity("Denmark", 9370, 2620871, "Hals", 57, 10.3166667) },
            { 8450, new GeoLocationCity("Denmark", 8450, 2620835, "Hammel", 56.25, 9.8666667) },
            { 7362, new GeoLocationCity("Denmark", 7362, 2620813, "Hampen", 56.0166667, 9.3666667) },
            { 7730, new GeoLocationCity("Denmark", 7730, 2620786, "Hanstholm", 57.1166667, 8.6166667) },
            { 7673, new GeoLocationCity("Denmark", 7673, 2620767, "Harboøre", 56.6175208445297, 8.18069458007812) },
            { 8462, new GeoLocationCity("Denmark", 8462, 2620750, "Harlev J", 56.141054628234, 9.99601364135742) },
            { 5463, new GeoLocationCity("Denmark", 5463, 2620747, "Harndrup", 55.4666667, 10.0333333) },
            { 4912, new GeoLocationCity("Denmark", 4912, 2620746, "Harpelunde", 54.8833333, 11.1) },
            { 3790, new GeoLocationCity("Denmark", 3790, 2620716, "Hasle", 55.1833333, 14.7166667) },
            { 4690, new GeoLocationCity("Denmark", 4690, 2620712, "Haslev", 55.3234649560937, 11.9638913869858) },
            { 8361, new GeoLocationCity("Denmark", 8361, 2620704, "Hasselager", 56.1, 10.1166667) },
            { 4622, new GeoLocationCity("Denmark", 4622, 2620664, "Havdrup", 55.5333333, 12.1333333) },
            { 8970, new GeoLocationCity("Denmark", 8970, 2620637, "Havndal", 56.65, 10.2) },
            { 2640, new GeoLocationCity("Denmark", 2640, 2620587, "Hedehusene", 55.65, 12.2) },
            { 8722, new GeoLocationCity("Denmark", 8722, 2620583, "Hedensted", 55.7704297921124, 9.70109939575195) },
            { 6094, new GeoLocationCity("Denmark", 6094, 2620556, "Hejls", 55.3833333, 9.6) },
            { 7250, new GeoLocationCity("Denmark", 7250, 2620549, "Hejnsvig", 55.6833333, 8.9833333) },
            { 3150, new GeoLocationCity("Denmark", 3150, 2620528, "Hellebæk", 56.0666667, 12.5666667) },
            { 2900, new GeoLocationCity("Denmark", 2900, 2620516, "Hellerup", 55.7320398697566, 12.5709342956543) },
            { 3200, new GeoLocationCity("Denmark", 3200, 2620476, "Helsinge", 56.0228341568646, 12.197517156601) },
            { 3000, new GeoLocationCity("Denmark", 3000, 2620473, "Helsingør", 56.0360649372914, 12.6136028766632) },
            { 6893, new GeoLocationCity("Denmark", 6893, 2620451, "Hemmet", 55.85, 8.3833333) },
            { 6854, new GeoLocationCity("Denmark", 6854, 2620448, "Henne", 55.7333333, 8.25) },
            { 4681, new GeoLocationCity("Denmark", 4681, 2620433, "Herfølge", 55.4166667, 12.1666667) },
            { 2730, new GeoLocationCity("Denmark", 2730, 2620431, "Herlev", 55.7236600738204, 12.4399781227112) },
            { 4160, new GeoLocationCity("Denmark", 4160, 2620428, "Herlufmagle", 55.3166667, 11.7666667) },
            { 7400, new GeoLocationCity("Denmark", 7400, 2620425, "Herning", 56.1393151149957, 8.97378087043762) },
            { 5874, new GeoLocationCity("Denmark", 5874, 2620387, "Hesselager", 55.1666667, 10.75) },
            { 3400, new GeoLocationCity("Denmark", 3400, 2620320, "Hillerød", 55.9266656211426, 12.3109102249146) },
            { 8382, new GeoLocationCity("Denmark", 8382, 2620287, "Hinnerup", 56.7333333, 9.0666667) },
            { 9850, new GeoLocationCity("Denmark", 9850, 2620279, "Hirtshals", 57.5881173328479, 9.95922446250916) },
            { 9320, new GeoLocationCity("Denmark", 9320, 2620275, "Hjallerup", 57.1666667, 10.15) },
            { 7560, new GeoLocationCity("Denmark", 7560, 2620229, "Hjerm", 56.4333333, 8.65) },
            { 8530, new GeoLocationCity("Denmark", 8530, 2620186, "Hjortshøj", 56.25, 10.2666667) },
            { 9800, new GeoLocationCity("Denmark", 9800, 2620214, "Hjørring", 57.464169442516, 9.98229146003723) },
            { 9500, new GeoLocationCity("Denmark", 9500, 2620167, "Hobro", 56.6430615893198, 9.79028820991516) },
            { 4300, new GeoLocationCity("Denmark", 4300, 2620147, "Holbæk", 55.7166667, 11.7166667) },
            { 4960, new GeoLocationCity("Denmark", 4960, 2620134, "Holeby", 54.7166667, 11.4666667) },
            { 4684, new GeoLocationCity("Denmark", 4684, 2620082, "Holme-Olstrup", 55.25, 11.85) },
            { 7500, new GeoLocationCity("Denmark", 7500, 2620046, "Holstebro", 56.3600915537719, 8.61607074737549) },
            { 6670, new GeoLocationCity("Denmark", 6670, 2620043, "Holsted", 55.25, 11.8) },
            { 2840, new GeoLocationCity("Denmark", 2840, 2620032, "Holte", 55.2833333, 10.15) },
            { 4871, new GeoLocationCity("Denmark", 4871, 2619809, "Horbelev", 54.8166667, 12.0666667) },
            { 3100, new GeoLocationCity("Denmark", 3100, 2619807, "Hornbæk", 56.0833333, 12.4666667) },
            { 8543, new GeoLocationCity("Denmark", 8543, 2619787, "Hornslet", 56.3166667, 10.3333333) },
            { 8783, new GeoLocationCity("Denmark", 8783, 2619782, "Hornsyld", 55.75, 9.85) },
            { 8700, new GeoLocationCity("Denmark", 8700, 2619771, "Horsens", 55.8606582358663, 9.85033750534058) },
            { 4913, new GeoLocationCity("Denmark", 4913, 2619760, "Horslunde", 54.9, 11.2333333) },
            { 6682, new GeoLocationCity("Denmark", 6682, 2619733, "Hovborg", 55.6, 8.95) },
            { 8732, new GeoLocationCity("Denmark", 8732, 2619726, "Hovedgård", 55.95, 9.9666667) },
            { 5932, new GeoLocationCity("Denmark", 5932, 2619670, "Humble", 54.8333333, 10.7) },
            { 3050, new GeoLocationCity("Denmark", 3050, 2619669, "Humlebæk", 55.9618012405348, 12.5341022014618) },
            { 3390, new GeoLocationCity("Denmark", 3390, 2619650, "Hundested", 55.9666667, 11.8666667) },
            { 7760, new GeoLocationCity("Denmark", 7760, 2619624, "Hurup Thy", 56.75, 8.4166667) },
            { 4330, new GeoLocationCity("Denmark", 4330, 2619584, "Hvalsø", 55.583333, 11.866667) },
            { 7790, new GeoLocationCity("Denmark", 7790, 2619553, "Hvidbjerg", 55.65, 9.75) },
            { 6960, new GeoLocationCity("Denmark", 6960, 2619537, "Hvide Sande", 55.9833333, 8.1333333) },
            { 2650, new GeoLocationCity("Denmark", 2650, 2619528, "Hvidovre", 55.657186413939, 12.4736398458481) },
            { 8270, new GeoLocationCity("Denmark", 8270, 2619974, "Højbjerg", 55.2666667, 10.1666667) },
            { 4573, new GeoLocationCity("Denmark", 4573, 2619966, "Højby Sjælland", 55.6, 12) },
            { 6280, new GeoLocationCity("Denmark", 6280, 2620154, "Højer", 54.966667, 8.733333) },
            { 7840, new GeoLocationCity("Denmark", 7840, 2619908, "Højslev", 56.5833333, 9.15) },
            { 4270, new GeoLocationCity("Denmark", 4270, 2619882, "Høng", 55.5116482581683, 11.2903189659119) },
            { 8362, new GeoLocationCity("Denmark", 8362, 2619860, "Hørning", 56.0833333, 10.05) },
            { 2970, new GeoLocationCity("Denmark", 2970, 2619856, "Hørsholm", 55.8833333, 12.5) },
            { 4534, new GeoLocationCity("Denmark", 4534, 2619844, "Hørve", 55.75, 11.4666667) },
            { 5683, new GeoLocationCity("Denmark", 5683, 2620765, "Haarby", 55.2166667, 10.1166667) },
            { 4652, new GeoLocationCity("Denmark", 4652, 2620751, "Hårlev", 55.35, 12.25) },
            { 4872, new GeoLocationCity("Denmark", 4872, 2619439, "Idestrup", 54.7333333, 11.9666667) },
            { 7430, new GeoLocationCity("Denmark", 7430, 2619426, "Ikast", 56.138830906585, 9.15768384933472) },
            { 2635, new GeoLocationCity("Denmark", 2635, 2619377, "Ishøj", 55.6154254311285, 12.3518192768097) },
            { 6851, new GeoLocationCity("Denmark", 6851, 2619363, "Janderup Vestjylland", 55.6166667, 8.3833333) },
            { 7300, new GeoLocationCity("Denmark", 7300, 2619340, "Jelling", 55.75, 9.4333333) },
            { 9740, new GeoLocationCity("Denmark", 9740, 2619303, "Jerslev J", 57.2833333, 10.1) },
            { 4490, new GeoLocationCity("Denmark", 4490, 2619304, "Jerslev S", 55.6104991101051, 11.2406444549561) },
            { 9981, new GeoLocationCity("Denmark", 9981, 2619300, "Jerup", 57.5333333, 10.4333333) },
            { 6064, new GeoLocationCity("Denmark", 6064, 2619261, "Jordrup", 55.55, 9.3166667) },
            { 7130, new GeoLocationCity("Denmark", 7130, 2619251, "Juelsminde", 55.7066667, 10.0152778) },
            { 4450, new GeoLocationCity("Denmark", 4450, 2619222, "Jyderup", 55.2166667, 12.0833333) },
            { 4040, new GeoLocationCity("Denmark", 4040, 2619216, "Jyllinge", 55.75, 12.1166667) },
            { 4174, new GeoLocationCity("Denmark", 4174, 2619211, "Jystrup Midtsjælland", 55.5166667, 11.8833333) },
            { 3630, new GeoLocationCity("Denmark", 3630, 2619287, "Jægerspris", 55.85, 11.9833333) },
            { 4400, new GeoLocationCity("Denmark", 4400, 2619154, "Kalundborg", 55.6795443823581, 11.0886383056641) },
            { 4771, new GeoLocationCity("Denmark", 4771, 2619144, "Kalvehave", 54.8333333, 10.4333333) },
            { 7960, new GeoLocationCity("Denmark", 7960, 2619107, "Karby", 56.75, 8.5666667) },
            { 4653, new GeoLocationCity("Denmark", 4653, 2619102, "Karise", 55.3, 12.2166667) },
            { 2690, new GeoLocationCity("Denmark", 2690, 2619087, "Karlslunde", 55.5666667, 12.2333333) },
            { 4736, new GeoLocationCity("Denmark", 4736, 2619078, "Karrebæksminde", 55.1833333, 11.6666667) },
            { 7470, new GeoLocationCity("Denmark", 7470, 2619068, "Karup", 56.3067294370937, 9.16834831237793) },
            { 2770, new GeoLocationCity("Denmark", 2770, 2619032, "Kastrup", 55.0333333, 11.9166667) },
            { 5300, new GeoLocationCity("Denmark", 5300, 2618944, "Kerteminde", 55.4490251628357, 10.6576931476593) },
            { 4892, new GeoLocationCity("Denmark", 4892, 2618936, "Kettinge", 54.7, 11.75) },
            { 6933, new GeoLocationCity("Denmark", 6933, 2618931, "Kibæk", 56.0333333, 8.85) },
            { 4360, new GeoLocationCity("Denmark", 4360, 2618887, "Kirke Eskilstrup", 55.5666667, 11.7666667) },
            { 4070, new GeoLocationCity("Denmark", 4070, 2619456, "Kirke Hyllinge", 55.2666667, 11.6166667) },
            { 4060, new GeoLocationCity("Denmark", 4060, 2618866, "Kirke Såby", 55.6166667, 11.8666667) },
            { 8620, new GeoLocationCity("Denmark", 8620, 2618814, "Kjellerup", 56.2833333, 9.4333333) },
            { 2930, new GeoLocationCity("Denmark", 2930, 2618794, "Klampenborg", 55.7666667, 12.6) },
            { 9270, new GeoLocationCity("Denmark", 9270, 2618787, "Klarup", 57.0166667, 10.05) },
            { 3782, new GeoLocationCity("Denmark", 3782, 2618764, "Klemensker", 55.1666667, 14.8166667) },
            { 4672, new GeoLocationCity("Denmark", 4672, 2618726, "Klippinge", 55.35, 12.3333333) },
            { 8765, new GeoLocationCity("Denmark", 8765, 2618680, "Klovborg", 55.9333333, 9.5166667) },
            { 8420, new GeoLocationCity("Denmark", 8420, 2618646, "Knebel", 56.2166667, 10.5) },
            { 2980, new GeoLocationCity("Denmark", 2980, 2618545, "Kokkedal", 55.9, 12.5166667) },
            { 6000, new GeoLocationCity("Denmark", 6000, 2618528, "Kolding", 55.4903994823623, 9.47216480970383) },
            { 8560, new GeoLocationCity("Denmark", 8560, 2618515, "Kolind", 56.366667, 10.6) },
            { 2800, new GeoLocationCity("Denmark", 2800, 2618468, "Kongens Lyngby", 55.7704418622691, 12.5037825107574) },
            { 9293, new GeoLocationCity("Denmark", 9293, 2618464, "Kongerslev", 56.8833333, 10.1166667) },
            { 4220, new GeoLocationCity("Denmark", 4220, 2618361, "Korsør", 55.3299256127534, 11.1385703086853) },
            { 6340, new GeoLocationCity("Denmark", 6340, 2618148, "Kruså", 54.85, 9.4166667) },
            { 3490, new GeoLocationCity("Denmark", 3490, 2618091, "Kvistgård", 55.9833333, 12.5) },
            { 5772, new GeoLocationCity("Denmark", 5772, 2618070, "Kværndrup", 55.1666667, 10.5333333) },
            { 1000, new GeoLocationCity("Denmark", 1000, 2618425, "København", 55.6776812020993, 12.5709342956543) },
            { 4600, new GeoLocationCity("Denmark", 4600, 2618415, "Køge", 55.4580173891012, 12.1821373701096) },
            { 4772, new GeoLocationCity("Denmark", 4772, 2617961, "Langebæk", 55, 12.1) },
            { 5550, new GeoLocationCity("Denmark", 5550, 2617936, "Langeskov", 55.3666667, 10.6) },
            { 8870, new GeoLocationCity("Denmark", 8870, 2617982, "Langå", 55.1833333, 10.7333333) },
            { 4320, new GeoLocationCity("Denmark", 4320, 2617832, "Lejre", 55.6046145872378, 11.9747650623322) },
            { 6940, new GeoLocationCity("Denmark", 6940, 2617824, "Lem St.", 56.0166667, 8.4) },
            { 8632, new GeoLocationCity("Denmark", 8632, 2617819, "Lemming", 56.1333333, 10.1166667) },
            { 7620, new GeoLocationCity("Denmark", 7620, 2617812, "Lemvig", 56.5485607971269, 8.31019163131714) },
            { 4623, new GeoLocationCity("Denmark", 4623, 2617648, "Lille Skensved", 55.5166667, 12.15) },
            { 6660, new GeoLocationCity("Denmark", 6660, 2617549, "Lintrup", 55.4, 8.9833333) },
            { 3360, new GeoLocationCity("Denmark", 3360, 2617538, "Liseleje", 56.0166667, 11.9833333) },
            { 4750, new GeoLocationCity("Denmark", 4750, 2617340, "Lundby", 54.8666667, 11.8333333) },
            { 6640, new GeoLocationCity("Denmark", 6640, 2617312, "Lunderskov", 55.4833333, 9.3) },
            { 3540, new GeoLocationCity("Denmark", 3540, 2617221, "Lynge", 55.4, 11.5833333) },
            { 8520, new GeoLocationCity("Denmark", 8520, 2617179, "Lystrup", 55.3666667, 12.25) },
            { 9940, new GeoLocationCity("Denmark", 9940, 2623199, "Læsø/Byrum", 57.2552778, 11.0025) },
            { 8831, new GeoLocationCity("Denmark", 8831, 2617464, "Løgstrup", 56.5, 9.3333333) },
            { 9670, new GeoLocationCity("Denmark", 9670, 2617467, "Løgstør", 56.9666667, 9.25) },
            { 6240, new GeoLocationCity("Denmark", 6240, 2617497, "Løgumkloster", 55.05, 9.066667) },
            { 9480, new GeoLocationCity("Denmark", 9480, 2617443, "Løkken", 57.3704675580465, 9.71466064453125) },
            { 8723, new GeoLocationCity("Denmark", 8723, 2617423, "Løsning", 55.8, 9.7) },
            { 8670, new GeoLocationCity("Denmark", 8670, 2617879, "Låsby", 56.15, 9.8166667) },
            { 8340, new GeoLocationCity("Denmark", 8340, 2617114, "Malling", 56.0333333, 10.1666667) },
            { 9550, new GeoLocationCity("Denmark", 9550, 2617076, "Mariager", 56.65, 10) },
            { 4930, new GeoLocationCity("Denmark", 4930, 2617072, "Maribo", 54.7766175185167, 11.500169634819) },
            { 5290, new GeoLocationCity("Denmark", 5290, 2617033, "Marslev", 55.3833333, 10.5166667) },
            { 5960, new GeoLocationCity("Denmark", 5960, 2617030, "Marstal", 54.85, 10.5166667) },
            { 5390, new GeoLocationCity("Denmark", 5390, 2617023, "Martofte", 55.55, 10.6666667) },
            { 3370, new GeoLocationCity("Denmark", 3370, 2616976, "Melby", 54.9833333, 10.5833333) },
            { 4735, new GeoLocationCity("Denmark", 4735, 2616945, "Mern", 55.05, 12.0666667) },
            { 5370, new GeoLocationCity("Denmark", 5370, 2616938, "Mesinge Fyn", 55.5, 10.65) },
            { 5500, new GeoLocationCity("Denmark", 5500, 2616933, "Middelfart", 55.5059099250131, 9.73054200410843) },
            { 5642, new GeoLocationCity("Denmark", 5642, 2616893, "Millinge", 55.1333333, 10.2166667) },
            { 5462, new GeoLocationCity("Denmark", 5462, 2616752, "Morud", 55.45, 10.1833333) },
            { 4190, new GeoLocationCity("Denmark", 4190, 2616673, "Munke Bjergby", 55.5, 11.5333333) },
            { 5330, new GeoLocationCity("Denmark", 5330, 2616672, "Munkebo", 55.45, 10.5666667) },
            { 9632, new GeoLocationCity("Denmark", 9632, 2616821, "Møldrup", 56.6166667, 9.5) },
            { 8544, new GeoLocationCity("Denmark", 8544, 2616773, "Mørke", 56.3333333, 10.3833333) },
            { 4440, new GeoLocationCity("Denmark", 4440, 2616767, "Mørkøv", 55.6666667, 11.5166667) },
            { 2760, new GeoLocationCity("Denmark", 2760, 2617112, "Måløv", 55.75, 12.3333333) },
            { 8320, new GeoLocationCity("Denmark", 8320, 2617034, "Mårslet", 56.0666667, 10.1666667) },
            { 4900, new GeoLocationCity("Denmark", 4900, 2616599, "Nakskov", 54.8333333, 11.15) },
            { 3730, new GeoLocationCity("Denmark", 3730, 2616504, "Nexø/Dueodde", 55.0666667, 15.15) },
            { 9240, new GeoLocationCity("Denmark", 9240, 2616483, "Nibe", 56.9833333, 9.6333333) },
            { 8581, new GeoLocationCity("Denmark", 8581, 2616465, "Nimtofte", 56.4166667, 10.5666667) },
            { 2990, new GeoLocationCity("Denmark", 2990, 2616450, "Nivå", 55.9333333, 12.5166667) },
            { 6430, new GeoLocationCity("Denmark", 6430, 2616176, "Nordborg", 55.05, 9.75) },
            { 8355, new GeoLocationCity("Denmark", 8355, 2613254, "Ny-Solbjerg", 56.0427055949913, 10.0863075256348) },
            { 5800, new GeoLocationCity("Denmark", 5800, 2616015, "Nyborg", 55.3127355595366, 10.7896363735199) },
            { 4800, new GeoLocationCity("Denmark", 4800, 2615961, "Nykøbing Falster", 54.7690581396173, 11.87424659729) },
            { 7900, new GeoLocationCity("Denmark", 7900, 2615964, "Nykøbing Mors", 56.793340470572, 8.85282397270203) },
            { 4500, new GeoLocationCity("Denmark", 4500, 2615962, "Nykøbing Sjælland", 55.9249134133599, 11.671085357666) },
            { 4880, new GeoLocationCity("Denmark", 4880, 2615911, "Nysted", 54.6666667, 11.75) },
            { 2850, new GeoLocationCity("Denmark", 2850, 2616069, "Nærum", 55.8166667, 12.55) },
            { 4700, new GeoLocationCity("Denmark", 4700, 2616038, "Næstved", 55.2299178968098, 11.7609179019928) },
            { 9610, new GeoLocationCity("Denmark", 9610, 2616413, "Nørager", 56.4833333, 10.5166667) },
            { 4840, new GeoLocationCity("Denmark", 4840, 2616366, "Nørre Alslev", 54.8666667, 11.8333333) },
            { 4572, new GeoLocationCity("Denmark", 4572, 2616361, "Nørre Asmindrup", 55.8833333, 11.6333333) },
            { 5580, new GeoLocationCity("Denmark", 5580, 2616368, "Nørre Aaby", 55.45, 9.9) },
            { 6830, new GeoLocationCity("Denmark", 6830, 2616279, "Nørre-Nebel", 55.7833333, 8.3) },
            { 8766, new GeoLocationCity("Denmark", 8766, 2616248, "Nørre-Snede", 55.9666667, 9.4166667) },
            { 4951, new GeoLocationCity("Denmark", 4951, 2616360, "Nørreballe", 54.8, 10.7) },
            { 9400, new GeoLocationCity("Denmark", 9400, 2616235, "Nørresundby", 57.0666667, 9.9166667) },
            { 8300, new GeoLocationCity("Denmark", 8300, 2615886, "Odder", 55.9731258009686, 10.1529979705811) },
            { 5000, new GeoLocationCity("Denmark", 5000, 2615876, "Odense", 55.3959381271672, 10.3883135318756) },
            { 6840, new GeoLocationCity("Denmark", 6840, 2615860, "Oksbøl", 55.0333333, 9.7666667) },
            { 5450, new GeoLocationCity("Denmark", 5450, 2615351, "Otterup", 55.5166667, 10.4) },
            { 5883, new GeoLocationCity("Denmark", 5883, 2615343, "Oure", 55.1166667, 10.7333333) },
            { 6855, new GeoLocationCity("Denmark", 6855, 2615267, "Ovtrup", 55.7166667, 8.35) },
            { 6330, new GeoLocationCity("Denmark", 6330, 2615242, "Padborg", 54.8166667, 9.3666667) },
            { 9490, new GeoLocationCity("Denmark", 9490, 2615222, "Pandrup", 57.2333333, 9.6833333) },
            { 4720, new GeoLocationCity("Denmark", 4720, 2615089, "Præstø", 55.1166667, 12.05) },
            { 7183, new GeoLocationCity("Denmark", 7183, 2615009, "Randbøl", 55.7, 9.2666667) },
            { 8900, new GeoLocationCity("Denmark", 8900, 2615006, "Randers", 56.4666667, 10.05) },
            { 9681, new GeoLocationCity("Denmark", 9681, 2614977, "Ranum", 56.9, 9.2333333) },
            { 8763, new GeoLocationCity("Denmark", 8763, 2614970, "Rask Mølle", 55.8666667, 9.6166667) },
            { 7970, new GeoLocationCity("Denmark", 7970, 2614903, "Redsted M", 56.7333333, 8.65) },
            { 4420, new GeoLocationCity("Denmark", 4420, 2614883, "Regstrup", 55.35, 10.7666667) },
            { 6760, new GeoLocationCity("Denmark", 6760, 2614813, "Ribe", 55.35, 8.7666667) },
            { 5750, new GeoLocationCity("Denmark", 5750, 2614790, "Ringe", 55.2333333, 10.4833333) },
            { 6950, new GeoLocationCity("Denmark", 6950, 2614776, "Ringkøbing", 56.0900620302018, 8.24401617050171) },
            { 4100, new GeoLocationCity("Denmark", 4100, 2614764, "Ringsted", 55.4425991502195, 11.7901057004929) },
            { 8240, new GeoLocationCity("Denmark", 8240, 2614718, "Risskov", 56.1833333, 10.2333333) },
            { 4000, new GeoLocationCity("Denmark", 4000, 2614481, "Roskilde", 55.6415191517441, 12.0803475379944) },
            { 7870, new GeoLocationCity("Denmark", 7870, 2614477, "Roslev", 56.7, 8.9833333) },
            { 4243, new GeoLocationCity("Denmark", 4243, 2614440, "Rude", 55.2333333, 11.4833333) },
            { 5900, new GeoLocationCity("Denmark", 5900, 2614432, "Rudkøbing", 54.9363898099085, 10.7101947069168) },
            { 4291, new GeoLocationCity("Denmark", 4291, 2614427, "Ruds-Vedby", 55.55, 11.3833333) },
            { 2960, new GeoLocationCity("Denmark", 2960, 2614400, "Rungsted Kyst", 55.8841667, 12.5419444) },
            { 8680, new GeoLocationCity("Denmark", 8680, 2614357, "Ry ", 56.083333, 9.75) },
            { 5350, new GeoLocationCity("Denmark", 5350, 2614356, "Rynkeby", 55.25, 10.4666667) },
            { 8550, new GeoLocationCity("Denmark", 8550, 2614353, "Ryomgård", 56.3833333, 10.5) },
            { 5856, new GeoLocationCity("Denmark", 5856, 2614349, "Ryslinge", 55.25, 10.55) },
            { 4970, new GeoLocationCity("Denmark", 4970, 2614626, "Rødby", 54.7, 11.4) },
            { 6630, new GeoLocationCity("Denmark", 6630, 2614623, "Rødding", 55.3666667, 9.0666667) },
            { 6230, new GeoLocationCity("Denmark", 6230, 2614611, "Rødekro", 55.0666667, 9.35) },
            { 8840, new GeoLocationCity("Denmark", 8840, 2614602, "Rødkærsbro", 56.35, 9.5166667) },
            { 2610, new GeoLocationCity("Denmark", 2610, 2614600, "Rødovre", 55.6806211149001, 12.4537324905396) },
            { 4673, new GeoLocationCity("Denmark", 4673, 2614595, "Rødvig Stevns", 55.25, 12.3833333) },
            { 6792, new GeoLocationCity("Denmark", 6792, 2614568, "Rømø/Havneby", 55.1, 8.55) },
            { 8410, new GeoLocationCity("Denmark", 8410, 2614565, "Rønde", 56.3, 10.4833333) },
            { 3700, new GeoLocationCity("Denmark", 3700, 2614553, "Rønne", 55.1, 14.7) },
            { 4683, new GeoLocationCity("Denmark", 4683, 2614547, "Rønnede", 55.25, 12) },
            { 4581, new GeoLocationCity("Denmark", 4581, 2614515, "Rørvig", 55.95, 11.7666667) },
            { 8471, new GeoLocationCity("Denmark", 8471, 2614343, "Sabro", 56.2166667, 10.05) },
            { 4990, new GeoLocationCity("Denmark", 4990, 2614328, "Sakskøbing", 54.8, 11.65) },
            { 9493, new GeoLocationCity("Denmark", 9493, 2614286, "Saltum", 57.2666667, 9.7) },
            { 8305, new GeoLocationCity("Denmark", 8305, 2611311, "Samsø/Tranebjerg", 55.8333333, 10.6) },
            { 4592, new GeoLocationCity("Denmark", 4592, 2614122, "Sejerø", 55.8833333, 11.15) },
            { 8600, new GeoLocationCity("Denmark", 8600, 2614030, "Silkeborg", 56.1697004358941, 9.54508066177368) },
            { 9870, new GeoLocationCity("Denmark", 9870, 2614011, "Sindal", 57.5, 10.2333333) },
            { 4583, new GeoLocationCity("Denmark", 4583, 2609929, "Sjællands Odde", 55.9833333, 11.35) },
            { 6093, new GeoLocationCity("Denmark", 6093, 2613970, "Sjølund", 55.4, 9.5333333) },
            { 9990, new GeoLocationCity("Denmark", 9990, 2613939, "Skagen", 57.7209333147178, 10.5839377641678) },
            { 8832, new GeoLocationCity("Denmark", 8832, 2613896, "Skals", 56.55, 9.4166667) },
            { 5485, new GeoLocationCity("Denmark", 5485, 2613891, "Skamby", 55.5166667, 10.2833333) },
            { 8660, new GeoLocationCity("Denmark", 8660, 2613887, "Skanderborg", 56.0399128222392, 9.92719888687134) },
            { 4050, new GeoLocationCity("Denmark", 4050, 2613766, "Skibby", 55.75, 11.9666667) },
            { 7800, new GeoLocationCity("Denmark", 7800, 2613731, "Skive", 56.5666667, 9.0333333) },
            { 6900, new GeoLocationCity("Denmark", 6900, 2613715, "Skjern", 55.95, 8.5) },
            { 2942, new GeoLocationCity("Denmark", 2942, 2613685, "Skodsborg", 55.8166667, 12.5666667) },
            { 2740, new GeoLocationCity("Denmark", 2740, 2613588, "Skovlunde", 55.7166667, 12.4) },
            { 5881, new GeoLocationCity("Denmark", 5881, 2613845, "Skårup Fyn", 55.0833333, 10.7) },
            { 4230, new GeoLocationCity("Denmark", 4230, 2613694, "Skælskør", 55.25, 11.3166667) },
            { 6780, new GeoLocationCity("Denmark", 6780, 2613539, "Skærbæk", 55.15, 8.7666667) },
            { 3320, new GeoLocationCity("Denmark", 3320, 2613471, "Skævinge", 55.9166667, 12.1666667) },
            { 8541, new GeoLocationCity("Denmark", 8541, 2613675, "Skødstrup", 55.6166667, 8.2666667) },
            { 9520, new GeoLocationCity("Denmark", 9520, 2613672, "Skørping", 56.8333333, 9.8833333) },
            { 4200, new GeoLocationCity("Denmark", 4200, 2613460, "Slagelse", 55.4027616181201, 11.3545900583267) },
            { 3550, new GeoLocationCity("Denmark", 3550, 2613451, "Slangerup", 55.85, 12.1833333) },
            { 2765, new GeoLocationCity("Denmark", 2765, 2613345, "Smørum", 55.7333333, 12.3) },
            { 7752, new GeoLocationCity("Denmark", 7752, 2613327, "Snedsted", 56.9, 8.5333333) },
            { 3070, new GeoLocationCity("Denmark", 3070, 2613319, "Snekkersten", 56, 12.6) },
            { 4460, new GeoLocationCity("Denmark", 4460, 2613313, "Snertinge", 55.0666667, 11.9166667) },
            { 2680, new GeoLocationCity("Denmark", 2680, 2613233, "Solrød Strand", 55.532850656309, 12.2222685813904) },
            { 6560, new GeoLocationCity("Denmark", 6560, 2613224, "Sommersted", 55.3166667, 9.3166667) },
            { 8641, new GeoLocationCity("Denmark", 8641, 2612861, "Sorring", 56.1833333, 9.7833333) },
            { 4180, new GeoLocationCity("Denmark", 4180, 2612862, "Sorø", 55.4318396261791, 11.5554714202881) },
            { 8981, new GeoLocationCity("Denmark", 8981, 2612823, "Spentrup", 56.55, 10.0333333) },
            { 6971, new GeoLocationCity("Denmark", 6971, 2612815, "Spjald", 56.1166667, 8.5166667) },
            { 8472, new GeoLocationCity("Denmark", 8472, 2620835, "Sporup", 56.25, 9.8666667) },
            { 7270, new GeoLocationCity("Denmark", 7270, 2612757, "Stakroge", 55.8833333, 8.85) },
            { 4780, new GeoLocationCity("Denmark", 4780, 2612689, "Stege", 54.9833333, 12.3) },
            { 8781, new GeoLocationCity("Denmark", 8781, 2612664, "Stenderup", 54.9333333, 9.7) },
            { 4295, new GeoLocationCity("Denmark", 4295, 2612633, "Stenlille", 55.5333333, 11.6) },
            { 3660, new GeoLocationCity("Denmark", 3660, 2612630, "Stenløse", 55.3333333, 10.3666667) },
            { 5771, new GeoLocationCity("Denmark", 5771, 2612595, "Stenstrup", 55.1, 12.1333333) },
            { 4773, new GeoLocationCity("Denmark", 4773, 2612588, "Stensved", 55, 12.0333333) },
            { 7850, new GeoLocationCity("Denmark", 7850, 2612529, "Stoholm Jyll.", 56.4833333, 9.1666667) },
            { 4952, new GeoLocationCity("Denmark", 4952, 2612519, "Stokkemarke", 54.8333333, 11.3833333) },
            { 4480, new GeoLocationCity("Denmark", 4480, 2612427, "Store Fuglede", 55.5852336621194, 11.2453651428223) },
            { 4660, new GeoLocationCity("Denmark", 4660, 2612411, "Store Heddinge", 55.3096489848695, 12.3888471722603) },
            { 4370, new GeoLocationCity("Denmark", 4370, 2612381, "Store Merløse", 55.55, 11.7166667) },
            { 9280, new GeoLocationCity("Denmark", 9280, 2612301, "Storvorde", 57, 10.1) },
            { 7140, new GeoLocationCity("Denmark", 7140, 2612300, "Stouby", 55.7, 9.8) },
            { 9970, new GeoLocationCity("Denmark", 9970, 2612274, "Strandby Vends.", 57.5, 10.5) },
            { 7600, new GeoLocationCity("Denmark", 7600, 2612204, "Struer", 56.491216119714, 8.58375549316406) },
            { 4671, new GeoLocationCity("Denmark", 4671, 2612213, "Strøby", 55.0616580988303, 14.9065589904785) },
            { 4850, new GeoLocationCity("Denmark", 4850, 2612192, "Stubbekøbing", 54.8887527372959, 12.041015625) },
            { 9530, new GeoLocationCity("Denmark", 9530, 2612501, "Støvring", 56.5, 10.1833333) },
            { 9541, new GeoLocationCity("Denmark", 9541, 2612133, "Suldrup", 56.85, 9.7) },
            { 7451, new GeoLocationCity("Denmark", 7451, 2612107, "Sunds", 56.2, 9.0166667) },
            { 3740, new GeoLocationCity("Denmark", 3740, 2612078, "Svaneke", 55.135739410692, 15.1413917541504) },
            { 4470, new GeoLocationCity("Denmark", 4470, 2612057, "Svebølle", 55.65, 11.3) },
            { 5700, new GeoLocationCity("Denmark", 5700, 2612045, "Svendborg", 55.0598193609648, 10.6067714095116) },
            { 9230, new GeoLocationCity("Denmark", 9230, 2612026, "Svenstrup J", 56.25, 9.8166667) },
            { 4520, new GeoLocationCity("Denmark", 4520, 2611999, "Svinninge", 55.7166667, 11.4666667) },
            { 6470, new GeoLocationCity("Denmark", 6470, 2611971, "Sydals", 54.916667, 10) },
            { 9300, new GeoLocationCity("Denmark", 9300, 2614175, "Sæby", 55.5470864715623, 11.3087511062622) },
            { 2860, new GeoLocationCity("Denmark", 2860, 2613205, "Søborg", 55.7333333, 12.5166667) },
            { 5985, new GeoLocationCity("Denmark", 5985, 2613196, "Søby Ærø", 54.9333333, 10.2666667) },
            { 4920, new GeoLocationCity("Denmark", 4920, 2613136, "Søllested", 54.8166667, 11.2833333) },
            { 7280, new GeoLocationCity("Denmark", 7280, 2613064, "Sønder Felding", 55.95, 8.7833333) },
            { 7260, new GeoLocationCity("Denmark", 7260, 2612996, "Sønder Omme", 55.8333333, 8.9) },
            { 6092, new GeoLocationCity("Denmark", 6092, 2612951, "Sønder Stenderup", 55.4666667, 9.6166667) },
            { 6400, new GeoLocationCity("Denmark", 6400, 2613102, "Sønderborg", 54.9092625383989, 9.80736583471298) },
            { 5471, new GeoLocationCity("Denmark", 5471, 2612955, "Søndersø", 55.4833333, 10.2666667) },
            { 7550, new GeoLocationCity("Denmark", 7550, 2612894, "Sørvad", 56.2666667, 8.6666667) },
            { 2630, new GeoLocationCity("Denmark", 2630, 2611828, "Taastrup", 55.6517326417309, 12.2921562194824) },
            { 4733, new GeoLocationCity("Denmark", 4733, 2611872, "Tappernøje", 55.1666667, 11.9833333) },
            { 6880, new GeoLocationCity("Denmark", 6880, 2611865, "Tarm", 55.9166667, 8.5333333) },
            { 9575, new GeoLocationCity("Denmark", 9575, 2611783, "Terndrup", 56.8166667, 10.0666667) },
            { 8653, new GeoLocationCity("Denmark", 8653, 2611761, "Them", 56.0942576373958, 9.54866409301758) },
            { 7700, new GeoLocationCity("Denmark", 7700, 2611755, "Thisted", 56.9552254432748, 8.69490623474121) },
            { 8881, new GeoLocationCity("Denmark", 8881, 2611747, "Thorsø", 56.3, 9.8) },
            { 7680, new GeoLocationCity("Denmark", 7680, 2611738, "Thyborøn", 56.7, 8.2166667) },
            { 3080, new GeoLocationCity("Denmark", 3080, 2611723, "Tikøb", 56.0166667, 12.4666667) },
            { 8381, new GeoLocationCity("Denmark", 8381, 2611720, "Tilst", 56.1937645589944, 10.112829208374) },
            { 6980, new GeoLocationCity("Denmark", 6980, 2611718, "Tim", 56.2, 8.3166667) },
            { 6360, new GeoLocationCity("Denmark", 6360, 2611684, "Tinglev", 54.9333333, 9.25) },
            { 6862, new GeoLocationCity("Denmark", 6862, 2611653, "Tistrup", 55.7166667, 8.6166667) },
            { 3220, new GeoLocationCity("Denmark", 3220, 2611649, "Tisvildeleje", 56.05, 12.0833333) },
            { 6731, new GeoLocationCity("Denmark", 6731, 2611610, "Tjæreborg", 55.4666667, 8.5833333) },
            { 6520, new GeoLocationCity("Denmark", 6520, 2611565, "Toftlund", 55.1833333, 9.0666667) },
            { 5690, new GeoLocationCity("Denmark", 5690, 2611520, "Tommerup", 55.3166667, 10.2166667) },
            { 4891, new GeoLocationCity("Denmark", 4891, 2611452, "Toreby L", 54.75, 11.8) },
            { 4943, new GeoLocationCity("Denmark", 4943, 2611404, "Torrig L", 54.9, 11.3333333) },
            { 5953, new GeoLocationCity("Denmark", 5953, 2611307, "Tranekær", 55, 10.85) },
            { 8570, new GeoLocationCity("Denmark", 8570, 2611171, "Trustrup", 55.9, 10.05) },
            { 4030, new GeoLocationCity("Denmark", 4030, 2611132, "Tune", 55.6, 12.1833333) },
            { 4682, new GeoLocationCity("Denmark", 4682, 2611116, "Tureby", 55.3666667, 12.0833333) },
            { 4340, new GeoLocationCity("Denmark", 4340, 2611509, "Tølløse", 55.6166667, 11.75) },
            { 6270, new GeoLocationCity("Denmark", 6270, 2611497, "Tønder", 54.9330598082844, 8.86673927307129) },
            { 7160, new GeoLocationCity("Denmark", 7160, 2611486, "Tørring", 55.85, 9.4833333) },
            { 9830, new GeoLocationCity("Denmark", 9830, 2611854, "Tårs", 54.8333333, 11.65) },
            { 4350, new GeoLocationCity("Denmark", 4350, 2610989, "Ugerløse", 55.5833333, 11.6666667) },
            { 7171, new GeoLocationCity("Denmark", 7171, 2610960, "Uldum", 55.85, 9.6) },
            { 6990, new GeoLocationCity("Denmark", 6990, 2610959, "Ulfborg", 56.2666667, 8.3333333) },
            { 5540, new GeoLocationCity("Denmark", 5540, 2610943, "Ullerslev", 54.8166667, 11.2166667) },
            { 8860, new GeoLocationCity("Denmark", 8860, 2610913, "Ulstrup", 55.0833333, 8.95) },
            { 9430, new GeoLocationCity("Denmark", 9430, 2610824, "Vadum", 56.6333333, 8.7666667) },
            { 2500, new GeoLocationCity("Denmark", 2500, 2610802, "Valby", 55.65, 12.5166667) },
            { 2625, new GeoLocationCity("Denmark", 2625, 2610789, "Vallensbæk", 55.6333333, 12.3666667) },
            { 2665, new GeoLocationCity("Denmark", 2665, 2610789, "Vallensbæk Strand", 55.6333333, 12.3666667) },
            { 6580, new GeoLocationCity("Denmark", 6580, 2610760, "Vamdrup", 55.4166667, 9.2833333) },
            { 7184, new GeoLocationCity("Denmark", 7184, 2610755, "Vandel", 55.7, 9.2166667) },
            { 2720, new GeoLocationCity("Denmark", 2720, 2610735, "Vanløse", 55.55, 11.6833333) },
            { 6800, new GeoLocationCity("Denmark", 6800, 2610726, "Varde", 55.621120529006, 8.48069429397583) },
            { 2950, new GeoLocationCity("Denmark", 2950, 2610680, "Vedbæk", 55.85, 12.5666667) },
            { 5474, new GeoLocationCity("Denmark", 5474, 2610649, "Veflinge", 55.4833333, 10.1666667) },
            { 3210, new GeoLocationCity("Denmark", 3210, 2610640, "Vejby", 56.0666667, 12.15) },
            { 6600, new GeoLocationCity("Denmark", 6600, 2610634, "Vejen", 55.4833333, 9.15) },
            { 6853, new GeoLocationCity("Denmark", 6853, 2610625, "Vejers Strand", 55.6333333, 8.1333333) },
            { 7100, new GeoLocationCity("Denmark", 7100, 2610613, "Vejle", 55.7092706640481, 9.53570365905762) },
            { 5882, new GeoLocationCity("Denmark", 5882, 2610572, "Vejstrup", 55.1, 10.7166667) },
            { 3670, new GeoLocationCity("Denmark", 3670, 2610566, "Veksø Sjælland", 55.75, 12.25) },
            { 7570, new GeoLocationCity("Denmark", 7570, 2610543, "Vemb", 56.35, 8.35) },
            { 4241, new GeoLocationCity("Denmark", 4241, 2610541, "Vemmelev", 55.3666667, 11.2666667) },
            { 7742, new GeoLocationCity("Denmark", 7742, 2610503, "Vesløs", 57.0166667, 8.9666667) },
            { 9380, new GeoLocationCity("Denmark", 9380, 2610499, "Vestbjerg", 57.1333333, 9.9833333) },
            { 5762, new GeoLocationCity("Denmark", 5762, 2610380, "Vester Skerninge", 55.0666667, 10.45) },
            { 4953, new GeoLocationCity("Denmark", 4953, 2610479, "Vesterborg", 54.85, 11.2833333) },
            { 7770, new GeoLocationCity("Denmark", 7770, 2610348, "Vestervig", 56.7666667, 8.3333333) },
            { 8800, new GeoLocationCity("Denmark", 8800, 2610319, "Viborg", 56.4531891019637, 9.40201163291931) },
            { 8260, new GeoLocationCity("Denmark", 8260, 2610310, "Viby J", 56.122591449212, 10.1514530181885) },
            { 4130, new GeoLocationCity("Denmark", 4130, 2610311, "Viby Sjælland", 55.55, 12.0333333) },
            { 6920, new GeoLocationCity("Denmark", 6920, 2610307, "Videbæk", 56.0833333, 8.6333333) },
            { 4560, new GeoLocationCity("Denmark", 4560, 2610284, "Vig St.", 55.85, 11.6) },
            { 7480, new GeoLocationCity("Denmark", 7480, 2610259, "Vildbjerg", 56.2, 8.7666667) },
            { 7980, new GeoLocationCity("Denmark", 7980, 2610227, "Vils", 56.7666667, 8.7333333) },
            { 7830, new GeoLocationCity("Denmark", 7830, 2610197, "Vinderup", 55.35, 11.7333333) },
            { 4390, new GeoLocationCity("Denmark", 4390, 2610162, "Vipperød", 55.6666667, 11.75) },
            { 2830, new GeoLocationCity("Denmark", 2830, 2610155, "Virum", 55.8, 12.4666667) },
            { 5492, new GeoLocationCity("Denmark", 5492, 2610140, "Vissenbjerg", 55.3833333, 10.1333333) },
            { 6052, new GeoLocationCity("Denmark", 6052, 2610118, "Viuf", 55.5833333, 9.5) },
            { 9310, new GeoLocationCity("Denmark", 9310, 2610095, "Vodskov", 57.1, 10.0333333) },
            { 6500, new GeoLocationCity("Denmark", 6500, 2610075, "Vojens", 55.25, 9.3166667) },
            { 7173, new GeoLocationCity("Denmark", 7173, 2610037, "Vonge", 55.8666667, 9.4333333) },
            { 6623, new GeoLocationCity("Denmark", 6623, 2610025, "Vorbasse", 55.6333333, 9.0833333) },
            { 4760, new GeoLocationCity("Denmark", 4760, 2610021, "Vordingborg", 54.9833333, 11.9) },
            { 9760, new GeoLocationCity("Denmark", 9760, 2609994, "Vrå", 55.5333333, 9.35) },
            { 4873, new GeoLocationCity("Denmark", 4873, 2610328, "Væggerløse", 54.7, 11.9333333) },
            { 3500, new GeoLocationCity("Denmark", 3500, 2609956, "Værløse", 55.2666667, 12.15) },
            { 5970, new GeoLocationCity("Denmark", 5970, 2625001, "Ærøskøbing", 54.8880291974563, 10.4111739993095) },
            { 6870, new GeoLocationCity("Denmark", 6870, 2615749, "Ølgod", 55.8166667, 8.6166667) },
            { 3310, new GeoLocationCity("Denmark", 3310, 2615737, "Ølsted", 55.25, 10.3) },
            { 3650, new GeoLocationCity("Denmark", 3650, 2615730, "Ølstykke", 55.7833333, 12.1833333) },
            { 5853, new GeoLocationCity("Denmark", 5853, 2615718, "Ørbæk", 55.2666667, 10.6833333) },
            { 6973, new GeoLocationCity("Denmark", 6973, 2615684, "Ørnhøj", 56.2, 8.5833333) },
            { 8950, new GeoLocationCity("Denmark", 8950, 2615651, "Ørsted", 55.3333333, 10.0666667) },
            { 8586, new GeoLocationCity("Denmark", 8586, 2615645, "Ørum Djursland", 56.45, 10.683333) },
            { 8752, new GeoLocationCity("Denmark", 8752, 2615626, "Østbirk", 55.9666667, 9.7666667) },
            { 4894, new GeoLocationCity("Denmark", 4894, 2615459, "Øster Ulslev", 54.7, 11.6333333) },
            { 7990, new GeoLocationCity("Denmark", 7990, 2615613, "Øster-Assels", 56.7, 8.7) },
            { 3751, new GeoLocationCity("Denmark", 3751, 2615506, "Østermarie", 55.1406700135066, 15.0124740600586) },
            { 9750, new GeoLocationCity("Denmark", 9750, 2615448, "Østervrå", 57.35, 10.25) },
            { 6200, new GeoLocationCity("Denmark", 6200, 2625070, "Aabenraa", 55.0443426591741, 9.41740751266479) },
            { 9440, new GeoLocationCity("Denmark", 9440, 2625037, "Aabybro", 57.15, 9.75) },
            { 8230, new GeoLocationCity("Denmark", 8230, 2625033, "Åbyhøj", 56.1562585675595, 10.1652717590332) },
            { 3720, new GeoLocationCity("Denmark", 3720, 2624929, "Aakirkeby", 55.0707998420085, 14.9197769165039) },
            { 9000, new GeoLocationCity("Denmark", 9000, 2624886, "Aalborg", 57.0479990421469, 9.91870164871216) },
            { 9982, new GeoLocationCity("Denmark", 9982, 2624900, "Ålbæk", 55.4833333, 8.65) },
            { 9620, new GeoLocationCity("Denmark", 9620, 2624864, "Aalestrup", 56.7, 9.5) },
            { 3140, new GeoLocationCity("Denmark", 3140, 2624784, "Ålsgårde", 56.0833333, 12.55) },
            { 8000, new GeoLocationCity("Denmark", 8000, 2624652, "Aarhus", 56.1567365813134, 10.2107620239258) },
            { 9600, new GeoLocationCity("Denmark", 9600, 2624602, "Aars", 56.8039858355313, 9.51440691947937) },
            { 5792, new GeoLocationCity("Denmark", 5792, 2624596, "Årslev", 55.0333333, 9.35) },
            { 5560, new GeoLocationCity("Denmark", 5560, 2624587, "Aarup", 55.3833333, 10.0666667) },
        };

        #endregion
    }
}
