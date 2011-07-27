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

namespace DMI.Service
{
    public class Denmark : IWeatherProvider
    {
        public const int DefaultPostalCode = 1000; // Copenhagen

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

                    var result = new CountryWeatherResult()
                    {
                        Image = new Uri(AppResources.Denmark_CountryImage),
                        Items = items
                    };

                    callback(result, e.Error);
                }
            };

            client.DownloadStringAsync(new Uri(AppResources.Denmark_CountryFeed));
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
                    AppResources.Denmark_CityWeatherThreeDaysImage, postalCode)),
                CityWeatherSevenDaysImage = new Uri(string.Format(
                    AppResources.Denmark_CityWeatherSevenDaysImage, postalCode)),
            };

            callback(result, null);
        }

        private static void GetRegionalWeatherFromPostalCode(int postalCode, Action<RegionalWeatherResult, Exception> callback)
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
                    callback(new RegionalWeatherResult(), e.Error);
                }
                else
                {
                    var input = HttpUtility.HtmlDecode(e.Result);

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

                    callback(region, e.Error);
                }
            };

            var address = new Uri(GetRegionalContentFromPostalCode(postalCode));

            client.DownloadStringAsync(address);
        }

        private static void GetPollenDataFromPostalCode(int postalCode, Action<PollenResult, Exception> callback)
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
                    callback(new PollenResult(), e.Error);
                }
                else
                {
                    var allItems = XElement.Parse(e.Result)
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
                        Image = new Uri(string.Format(AppResources.PollenImage, postalCode)),
                        Items = pollenItems
                    };

                    callback(result, null);
                }
            };

            client.DownloadStringAsync(new Uri(AppResources.PollenFeed));
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
                return AppResources.RegionalText_NorthZealand;
            else if (postalCode >= 3000 && postalCode <= 3699)
                return AppResources.RegionalText_NorthZealand;
            else if (postalCode >= 4000 && postalCode <= 4999)
                return AppResources.RegionalText_SouthZealand;
            else if (postalCode >= 3700 && postalCode <= 3799)
                return AppResources.RegionalText_Bornholm;
            else if (postalCode >= 5000 && postalCode <= 5999)
                return AppResources.RegionalText_Fyn;
            else if (postalCode >= 6000 && postalCode <= 6999)
                return AppResources.RegionalText_SouthJytland;
            else if (postalCode >= 7000 && postalCode <= 7999)
                return AppResources.RegionalText_MiddleJytland;
            else if (postalCode >= 8000 && postalCode <= 8999)
                return AppResources.RegionalText_EastJytland;
            else if (postalCode >= 9000 && postalCode <= 9999)
                return AppResources.RegionalImage_NorthJytland;
            else
                return AppResources.RegionalText_NorthZealand; // Default to Copenhagen
        }

        private static string GetRegionalImageFromPostalCode(int postalCode)
        {
            if (postalCode >= 1000 && postalCode <= 2999)
                return AppResources.RegionalImage_NorthZealand;
            else if (postalCode >= 3000 && postalCode <= 3699)
                return AppResources.RegionalImage_NorthZealand;
            else if (postalCode >= 4000 && postalCode <= 4999)
                return AppResources.RegionalImage_SouthZealand;
            else if (postalCode >= 3700 && postalCode <= 3799)
                return AppResources.RegionalImage_Bornholm;
            else if (postalCode >= 5000 && postalCode <= 5999)
                return AppResources.RegionalImage_Fyn;
            else if (postalCode >= 6000 && postalCode <= 6999)
                return AppResources.RegionalImage_SouthJytland;
            else if (postalCode >= 7000 && postalCode <= 7999)
                return AppResources.RegionalImage_MiddleJytland;
            else if (postalCode >= 8000 && postalCode <= 8999)
                return AppResources.RegionalImage_EastJytland;
            else if (postalCode >= 9000 && postalCode <= 9999)
                return AppResources.RegionalImage_NorthJytland;
            else
                return AppResources.RegionalImage_NorthZealand; // Default to Copenhagen
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

        #region List of known cities in Denmark

        //const int MISSING = -1;

        public static IDictionary<int, CityWithId> PostalCodes = new Dictionary<int, CityWithId>()
        {
            { 5320, new CityWithId(5320, "Agedrup", 2624988) },
            { 6753, new CityWithId(6753, "Agerbæk", 2624986) },
            { 6534, new CityWithId(6534, "Agerskov", 2624973) },
            { 2620, new CityWithId(2620, "Albertslund", 2624906) },
            { 3450, new CityWithId(3450, "Allerød", 2624843) },
            { 3770, new CityWithId(3770, "Allinge/Sandvig", 2624816) },
            { 8961, new CityWithId(8961, "Allingåbro", 2624819) },
            { 6051, new CityWithId(6051, "Almind", 2624809) },
            { 8592, new CityWithId(8592, "Anholt", 2624693) },
            { 8643, new CityWithId(8643, "Ans By", 2624679) },
            { 6823, new CityWithId(6823, "Ansager", 2624678) },
            { 9510, new CityWithId(9510, "Arden", 2624660) },
            { 4792, new CityWithId(4792, "Askeby", 2624545) },
            { 4550, new CityWithId(4550, "Asnæs", 2624521) },
            { 5466, new CityWithId(5466, "Asperup", 2624511) },
            { 5610, new CityWithId(5610, "Assens", 2624502) },
            { 9340, new CityWithId(9340, "Asaa", 5455821) },
            { 6440, new CityWithId(6440, "Augustenborg", 2624439) },
            { 7490, new CityWithId(7490, "Aulum", 2624419) },
            { 8963, new CityWithId(8963, "Auning", 2624435) },
            { 5935, new CityWithId(5935, "Bagenkop", 2624398) },
            { 2880, new CityWithId(2880, "Bagsværd", 2624388) },
            { 8444, new CityWithId(8444, "Balle på Djursland", 2624361) },
            { 2750, new CityWithId(2750, "Ballerup", 2624341) },
            { 7150, new CityWithId(7150, "Barrit", 2624289) },
            { 8330, new CityWithId(8330, "Beder", 2624212) },
            { 7755, new CityWithId(7755, "Bedsted Thy", 2624208) },
            { 6541, new CityWithId(6541, "Bevtoft", 2624163) },
            { 6852, new CityWithId(6852, "Billum", 2624148) },
            { 7190, new CityWithId(7190, "Billund", 2624145) },
            { 9881, new CityWithId(9881, "Bindslev", 2616357) },
            { 3460, new CityWithId(3460, "Birkerød", 2624112) },
            { 8850, new CityWithId(8850, "Bjerringbro", 2624019) },
            { 6091, new CityWithId(6091, "Bjert", 2624014) },
            { 4632, new CityWithId(4632, "Bjæverskov", 2623978) },
            { 9492, new CityWithId(9492, "Blokhus", 2623911) },
            { 5491, new CityWithId(5491, "Blommenslyst", 2623909) },
            { 6857, new CityWithId(6857, "Blåvand", 2623928) },
            { 4242, new CityWithId(4242, "Boeslunde", 2623861) },
            { 5400, new CityWithId(5400, "Bogense", 2623857) },
            { 4793, new CityWithId(4793, "Bogø By", 2623847) },
            { 6392, new CityWithId(6392, "Bolderslev", 2623837) },
            { 7441, new CityWithId(7441, "Bording", 2623694) },
            { 4791, new CityWithId(4791, "Borre", 2623659) },
            { 4140, new CityWithId(4140, "Borup på Sjælland", 2623619) },
            { 8220, new CityWithId(8220, "Brabrand", 2623586) },
            { 6740, new CityWithId(6740, "Bramming", 2623570) },
            { 7330, new CityWithId(7330, "Brande", 2623554) },
            { 6535, new CityWithId(6535, "Branderup J", 2623547) },
            { 6261, new CityWithId(6261, "Bredebro", 2623505) },
            { 7182, new CityWithId(7182, "Bredsten", 2623487) },
            { 5464, new CityWithId(5464, "Brenderup Fyn", 2623433) },
            { 6310, new CityWithId(6310, "Broager", 2623393) },
            { 5672, new CityWithId(5672, "Broby", 2719212) },
            { 9460, new CityWithId(9460, "Brovst", 2623306) },
            { 8654, new CityWithId(8654, "Bryrup", 2623270) },
            { 8740, new CityWithId(8740, "Brædstrup", 2623516) },
            { 2605, new CityWithId(2605, "Brøndby", 2623352) },
            { 2660, new CityWithId(2660, "Brøndby Strand", 2623341) },
            { 9700, new CityWithId(9700, "Brønderslev", 2623337) },
            { 2700, new CityWithId(2700, "Brønshøj", 2623322) },
            { 6650, new CityWithId(6650, "Brørup", 2623321) },
            { 6372, new CityWithId(6372, "Bylderup - Bov", 2623208) },
            { 6622, new CityWithId(6622, "Bække", 2623973) },
            { 7660, new CityWithId(7660, "Bækmarksbro", 2623964) },
            { 9574, new CityWithId(9574, "Bælum", 2623894) },
            { 7080, new CityWithId(7080, "Børkop", 2623736) },
            { 7650, new CityWithId(7650, "Bøvlingbjerg", 2623711) },
            { 2920, new CityWithId(2920, "Charlottenlund", 2623188) },
            { 6070, new CityWithId(6070, "Christiansfeld", 2623183) },
            { 5380, new CityWithId(5380, "Dalby", 2623140) },
            { 4261, new CityWithId(4261, "Dalmose", 2623117) },
            { 4983, new CityWithId(4983, "Dannemare", 2623073) },
            { 8721, new CityWithId(8721, "Daugård", 2623060) },
            { 4293, new CityWithId(4293, "Dianalund", 2623028) },
            { 2791, new CityWithId(2791, "Dragør", 2622937) },
            { 9330, new CityWithId(9330, "Dronninglund", 2622888) },
            { 3120, new CityWithId(3120, "Dronningmølle", 2622884) },
            { 9352, new CityWithId(9352, "Dybvad", 2622844) },
            { 2870, new CityWithId(2870, "Dyssegård", 2610742) },
            { 5631, new CityWithId(5631, "Ebberup", 2622799) },
            { 8400, new CityWithId(8400, "Ebeltoft", 2622793) },
            { 6320, new CityWithId(6320, "Egernsund", 2622735) },
            { 6040, new CityWithId(6040, "Egtved", 2622686) },
            { 8250, new CityWithId(8250, "Egå", 2622780) },
            { 5592, new CityWithId(5592, "Ejby", 2622667) },
            { 7361, new CityWithId(7361, "Ejstrupholm", 2622641) },
            { 7442, new CityWithId(7442, "Engesvang", 2622504) },
            { 4895, new CityWithId(4895, "Errindlev", 2622457) },
            { 7950, new CityWithId(7950, "Erslev", 2622453) },
            { 6700, new CityWithId(6700, "Esbjerg", 2622447) },
            { 4593, new CityWithId(4593, "Eskebjerg", 2622439) },
            { 4863, new CityWithId(4863, "Eskilstrup", 2622431) },
            { 3060, new CityWithId(3060, "Espergærde", 2622418) },
            { 5600, new CityWithId(5600, "Faaborg", 2622383) },
            { 6720, new CityWithId(6720, "Fanø", 2622341) },
            { 9640, new CityWithId(9640, "Farsø", 2622310) },
            { 3520, new CityWithId(3520, "Farum", 2622306) },
            { 4640, new CityWithId(4640, "Faxe", 6543931) },
            { 4654, new CityWithId(4654, "Faxe Ladeplads", 6543931) },
            { 4944, new CityWithId(4944, "Fejø", 2610475) },
            { 5863, new CityWithId(5863, "Ferritslev Fyn", 2622221) },
            { 4173, new CityWithId(4173, "Fjenneslev", 2622144) },
            { 9690, new CityWithId(9690, "Fjerritslev", 2622139) },
            { 8762, new CityWithId(8762, "Flemming", 2622097) },
            { 3480, new CityWithId(3480, "Fredensborg", 2621957) },
            { 7000, new CityWithId(7000, "Fredericia", 2621951) },
            { 2000, new CityWithId(2000, "Frederiksberg", 2621945) },
            { 9900, new CityWithId(9900, "Frederikshavn", 2621927) },
            { 3600, new CityWithId(3600, "Frederikssund", 2621912) },
            { 3300, new CityWithId(3300, "Frederiksværk", 2621910) },
            { 5871, new CityWithId(5871, "Frørup", 2621839) },
            { 7741, new CityWithId(7741, "Frøstrup", 2621829) },
            { 4250, new CityWithId(4250, "Fuglebjerg", 2621805) },
            { 7884, new CityWithId(7884, "Fur", 2713428) },
            { 4591, new CityWithId(4591, "Føllenslev", 2622017) },
            { 6683, new CityWithId(6683, "Føvling", 2622005) },
            { 4540, new CityWithId(4540, "Fårevejle St.", 2622326) },
            { 8990, new CityWithId(8990, "Fårup", 2622303) },
            { 8882, new CityWithId(8882, "Fårvang", 2622291) },
            { 7321, new CityWithId(7321, "Gadbjerg", 2621735) },
            { 4621, new CityWithId(4621, "Gadstrup", 2621728) },
            { 8464, new CityWithId(8464, "Galten", 2621710) },
            { 9362, new CityWithId(9362, "Gandrup", 2621617) },
            { 4874, new CityWithId(4874, "Gedser", 2621551) },
            { 9631, new CityWithId(9631, "Gedsted", 2621547) },
            { 8751, new CityWithId(8751, "Gedved", 2621546) },
            { 5591, new CityWithId(5591, "Gelsted", 2621523) },
            { 2820, new CityWithId(2820, "Gentofte", 2621515) },
            { 6621, new CityWithId(6621, "Gesten", 2621485) },
            { 3250, new CityWithId(3250, "Gilleleje", 2621471) },
            { 5854, new CityWithId(5854, "Gislev", 2621458) },
            { 4532, new CityWithId(4532, "Gislinge", 2621456) },
            { 9260, new CityWithId(9260, "Gistrup", 2621449) },
            { 7323, new CityWithId(7323, "Give", 2621448) },
            { 8983, new CityWithId(8983, "Gjerlev J", 2621432) },
            { 8883, new CityWithId(8883, "Gjern", 2621431) },
            { 5620, new CityWithId(5620, "Glamsbjerg", 2621393) },
            { 6752, new CityWithId(6752, "Glejbjerg", 2621382) },
            { 8585, new CityWithId(8585, "Glesborg", 2621376) },
            { 2600, new CityWithId(2600, "Glostrup", 2621356) },
            { 4171, new CityWithId(4171, "Glumsø", 2621348) },
            { 6510, new CityWithId(6510, "Gram", 2621279) },
            { 6771, new CityWithId(6771, "Gredstedbro", 2621236) },
            { 8500, new CityWithId(8500, "Grenå", 2621227) },
            { 2670, new CityWithId(2670, "Greve Strand", 2621215) },
            { 4571, new CityWithId(4571, "Grevinge", 2621213) },
            { 7200, new CityWithId(7200, "Grindsted", 2621193) },
            { 6300, new CityWithId(6300, "Gråsten", 2621258) },
            { 3230, new CityWithId(3230, "Græsted", 2621101) },
            { 5892, new CityWithId(5892, "Gudbjerg Sydfyn", 2621076) },
            { 3760, new CityWithId(3760, "Gudhjem", 2621070) },
            { 5884, new CityWithId(5884, "Gudme", 2621067) },
            { 4862, new CityWithId(4862, "Guldborg", 2621042) },
            { 6690, new CityWithId(6690, "Gørding", 2621307) },
            { 4281, new CityWithId(4281, "Gørlev Sjælland", 2621304) },
            { 3330, new CityWithId(3330, "Gørløse", 2621303) },
            { 6100, new CityWithId(6100, "Haderslev", 2620964) },
            { 8370, new CityWithId(8370, "Hadsten", 2620956) },
            { 9560, new CityWithId(9560, "Hadsund", 2620954) },
            { 9370, new CityWithId(9370, "Hals", 2620871) },
            { 8450, new CityWithId(8450, "Hammel", 2620835) },
            { 7362, new CityWithId(7362, "Hampen", 2620813) },
            { 7730, new CityWithId(7730, "Hanstholm", 2620786) },
            { 7673, new CityWithId(7673, "Harboøre", 2620767) },
            { 8462, new CityWithId(8462, "Harlev J", 2620750) },
            { 5463, new CityWithId(5463, "Harndrup", 2620747) },
            { 4912, new CityWithId(4912, "Harpelunde", 2620746) },
            { 3790, new CityWithId(3790, "Hasle", 2620716) },
            { 4690, new CityWithId(4690, "Haslev", 2620712) },
            { 8361, new CityWithId(8361, "Hasselager", 2620704) },
            { 4622, new CityWithId(4622, "Havdrup", 2620664) },
            { 8970, new CityWithId(8970, "Havndal", 2620637) },
            { 2640, new CityWithId(2640, "Hedehusene", 2620587) },
            { 8722, new CityWithId(8722, "Hedensted", 2620583) },
            { 6094, new CityWithId(6094, "Hejls", 2620556) },
            { 7250, new CityWithId(7250, "Hejnsvig", 2620549) },
            { 3150, new CityWithId(3150, "Hellebæk", 2620528) },
            { 2900, new CityWithId(2900, "Hellerup", 2620516) },
            { 3200, new CityWithId(3200, "Helsinge", 2620476) },
            { 3000, new CityWithId(3000, "Helsingør", 2620473) },
            { 6893, new CityWithId(6893, "Hemmet", 2620451) },
            { 6854, new CityWithId(6854, "Henne", 2620448) },
            { 4681, new CityWithId(4681, "Herfølge", 2620433) },
            { 2730, new CityWithId(2730, "Herlev", 2620431) },
            { 4160, new CityWithId(4160, "Herlufmagle", 2620428) },
            { 7400, new CityWithId(7400, "Herning", 2620425) },
            { 5874, new CityWithId(5874, "Hesselager", 2620387) },
            { 3400, new CityWithId(3400, "Hillerød", 2620320) },
            { 8382, new CityWithId(8382, "Hinnerup", 2620287) },
            { 9850, new CityWithId(9850, "Hirtshals", 2620279) },
            { 9320, new CityWithId(9320, "Hjallerup", 2620275) },
            { 7560, new CityWithId(7560, "Hjerm", 2620229) },
            { 8530, new CityWithId(8530, "Hjortshøj", 2620186) },
            { 9800, new CityWithId(9800, "Hjørring", 2620214) },
            { 9500, new CityWithId(9500, "Hobro", 2620167) },
            { 4300, new CityWithId(4300, "Holbæk", 2620147) },
            { 4960, new CityWithId(4960, "Holeby", 2620134) },
            { 4684, new CityWithId(4684, "Holme-Olstrup", 2620082) },
            { 7500, new CityWithId(7500, "Holstebro", 2620046) },
            { 6670, new CityWithId(6670, "Holsted", 2620043) },
            { 2840, new CityWithId(2840, "Holte", 2620032) },
            { 4871, new CityWithId(4871, "Horbelev", 2619809) },
            { 3100, new CityWithId(3100, "Hornbæk", 2619807) },
            { 8543, new CityWithId(8543, "Hornslet", 2619787) },
            { 8783, new CityWithId(8783, "Hornsyld", 2619782) },
            { 8700, new CityWithId(8700, "Horsens", 2619771) },
            { 4913, new CityWithId(4913, "Horslunde", 2619760) },
            { 6682, new CityWithId(6682, "Hovborg", 2619733) },
            { 8732, new CityWithId(8732, "Hovedgård", 2619726) },
            { 5932, new CityWithId(5932, "Humble", 2619670) },
            { 3050, new CityWithId(3050, "Humlebæk", 2619669) },
            { 3390, new CityWithId(3390, "Hundested", 2619650) },
            { 7760, new CityWithId(7760, "Hurup Thy", 2619624) },
            { 4330, new CityWithId(4330, "Hvalsø", 2619584) },
            { 7790, new CityWithId(7790, "Hvidbjerg", 2619553) },
            { 6960, new CityWithId(6960, "Hvide Sande", 2619537) },
            { 2650, new CityWithId(2650, "Hvidovre", 2619528) },
            { 8270, new CityWithId(8270, "Højbjerg", 2619974) },
            { 4573, new CityWithId(4573, "Højby Sjælland", 2619966) },
            { 6280, new CityWithId(6280, "Højer", 2620154) },
            { 7840, new CityWithId(7840, "Højslev", 2619908) },
            { 4270, new CityWithId(4270, "Høng", 2619882) },
            { 8362, new CityWithId(8362, "Hørning", 2619860) },
            { 2970, new CityWithId(2970, "Hørsholm", 2619856) },
            { 4534, new CityWithId(4534, "Hørve", 2619844) },
            { 5683, new CityWithId(5683, "Haarby", 2620765) },
            { 4652, new CityWithId(4652, "Hårlev", 2620751) },
            { 4872, new CityWithId(4872, "Idestrup", 2619439) },
            { 7430, new CityWithId(7430, "Ikast", 2619426) },
            { 2635, new CityWithId(2635, "Ishøj", 2619377) },
            { 6851, new CityWithId(6851, "Janderup Vestjylland", 2619363) },
            { 7300, new CityWithId(7300, "Jelling", 2619340) },
            { 9740, new CityWithId(9740, "Jerslev J", 2619303) },
            { 4490, new CityWithId(4490, "Jerslev S", 2619304) },
            { 9981, new CityWithId(9981, "Jerup", 2619300) },
            { 6064, new CityWithId(6064, "Jordrup", 2619261) },
            { 7130, new CityWithId(7130, "Juelsminde", 2619251) },
            { 4450, new CityWithId(4450, "Jyderup", 2619222) },
            { 4040, new CityWithId(4040, "Jyllinge", 2619216) },
            { 4174, new CityWithId(4174, "Jystrup Midtsjælland", 2619211) },
            { 3630, new CityWithId(3630, "Jægerspris", 2619287) },
            { 4400, new CityWithId(4400, "Kalundborg", 2619154) },
            { 4771, new CityWithId(4771, "Kalvehave", 2619144) },
            { 7960, new CityWithId(7960, "Karby", 2619107) },
            { 4653, new CityWithId(4653, "Karise", 2619102) },
            { 2690, new CityWithId(2690, "Karlslunde", 2619087) },
            { 4736, new CityWithId(4736, "Karrebæksminde", 2619078) },
            { 7470, new CityWithId(7470, "Karup", 2619068) },
            { 2770, new CityWithId(2770, "Kastrup", 2619032) },
            { 5300, new CityWithId(5300, "Kerteminde", 2618944) },
            { 4892, new CityWithId(4892, "Kettinge", 2618936) },
            { 6933, new CityWithId(6933, "Kibæk", 2618931) },
            { 4360, new CityWithId(4360, "Kirke Eskilstrup", 2618887) },
            { 4070, new CityWithId(4070, "Kirke Hyllinge", 2619456) },
            { 4060, new CityWithId(4060, "Kirke Såby", 2618866) },
            { 8620, new CityWithId(8620, "Kjellerup", 2618814) },
            { 2930, new CityWithId(2930, "Klampenborg", 2618794) },
            { 9270, new CityWithId(9270, "Klarup", 2618787) },
            { 3782, new CityWithId(3782, "Klemensker", 2618764) },
            { 4672, new CityWithId(4672, "Klippinge", 2618726) },
            { 8765, new CityWithId(8765, "Klovborg", 2618680) },
            { 8420, new CityWithId(8420, "Knebel", 2618646) },
            { 2980, new CityWithId(2980, "Kokkedal", 2618545) },
            { 6000, new CityWithId(6000, "Kolding", 2618528) },
            { 8560, new CityWithId(8560, "Kolind", 2618515) },
            { 2800, new CityWithId(2800, "Kongens Lyngby", 2618468) },
            { 9293, new CityWithId(9293, "Kongerslev", 2618464) },
            { 4220, new CityWithId(4220, "Korsør", 2618361) },
            { 6340, new CityWithId(6340, "Kruså", 2618148) },
            { 3490, new CityWithId(3490, "Kvistgård", 2618091) },
            { 5772, new CityWithId(5772, "Kværndrup", 2618070) },
            { 1000, new CityWithId(1000, "København", 2618425) },
            { 4600, new CityWithId(4600, "Køge", 2618415) },
            { 4772, new CityWithId(4772, "Langebæk", 2617961) },
            { 5550, new CityWithId(5550, "Langeskov", 2617936) },
            { 8870, new CityWithId(8870, "Langå", 2617982) },
            { 4320, new CityWithId(4320, "Lejre", 2617832) },
            { 6940, new CityWithId(6940, "Lem St.", 2617824) },
            { 8632, new CityWithId(8632, "Lemming", 2617819) },
            { 7620, new CityWithId(7620, "Lemvig", 2617812) },
            { 4623, new CityWithId(4623, "Lille Skensved", 2617648) },
            { 6660, new CityWithId(6660, "Lintrup", 2617549) },
            { 3360, new CityWithId(3360, "Liseleje", 2617538) },
            { 4750, new CityWithId(4750, "Lundby", 2617340) },
            { 6640, new CityWithId(6640, "Lunderskov", 2617312) },
            { 3540, new CityWithId(3540, "Lynge", 2617221) },
            { 8520, new CityWithId(8520, "Lystrup", 2617179) },
            { 9940, new CityWithId(9940, "Læsø/Byrum", 2623199) },
            { 8831, new CityWithId(8831, "Løgstrup", 2617464) },
            { 9670, new CityWithId(9670, "Løgstør", 2617467) },
            { 6240, new CityWithId(6240, "Løgumkloster", 2617497) },
            { 9480, new CityWithId(9480, "Løkken", 2617443) },
            { 8723, new CityWithId(8723, "Løsning", 2617423) },
            { 8670, new CityWithId(8670, "Låsby", 2617879) },
            { 8340, new CityWithId(8340, "Malling", 2617114) },
            { 9550, new CityWithId(9550, "Mariager", 2617076) },
            { 4930, new CityWithId(4930, "Maribo", 2617072) },
            { 5290, new CityWithId(5290, "Marslev", 2617033) },
            { 5960, new CityWithId(5960, "Marstal", 2617030) },
            { 5390, new CityWithId(5390, "Martofte", 2617023) },
            { 3370, new CityWithId(3370, "Melby", 2616976) },
            { 4735, new CityWithId(4735, "Mern", 2616945) },
            { 5370, new CityWithId(5370, "Mesinge Fyn", 2616938) },
            { 5500, new CityWithId(5500, "Middelfart", 2616933) },
            { 5642, new CityWithId(5642, "Millinge", 2616893) },
            { 5462, new CityWithId(5462, "Morud", 2616752) },
            { 4190, new CityWithId(4190, "Munke Bjergby", 2616673) },
            { 5330, new CityWithId(5330, "Munkebo", 2616672) },
            { 9632, new CityWithId(9632, "Møldrup", 2616821) },
            { 8544, new CityWithId(8544, "Mørke", 2616773) },
            { 4440, new CityWithId(4440, "Mørkøv", 2616767) },
            { 2760, new CityWithId(2760, "Måløv", 2617112) },
            { 8320, new CityWithId(8320, "Mårslet", 2617034) },
            { 4900, new CityWithId(4900, "Nakskov", 2616599) },
            { 3730, new CityWithId(3730, "Nexø/Dueodde", 2616504) },
            { 9240, new CityWithId(9240, "Nibe", 2616483) },
            { 8581, new CityWithId(8581, "Nimtofte", 2616465) },
            { 2990, new CityWithId(2990, "Nivå", 2616450) },
            { 6430, new CityWithId(6430, "Nordborg", 2616176) },
            { 8355, new CityWithId(8355, "Ny-Solbjerg", 2613254) },
            { 5800, new CityWithId(5800, "Nyborg", 2616015) },
            { 4800, new CityWithId(4800, "Nykøbing Falster", 2615961) },
            { 7900, new CityWithId(7900, "Nykøbing Mors", 2615964) },
            { 4500, new CityWithId(4500, "Nykøbing Sjælland", 2615962) },
            { 4880, new CityWithId(4880, "Nysted", 2615911) },
            { 2850, new CityWithId(2850, "Nærum", 2616069) },
            { 4700, new CityWithId(4700, "Næstved", 2616038) },
            { 9610, new CityWithId(9610, "Nørager", 2616413) },
            { 4840, new CityWithId(4840, "Nørre Alslev", 2616366) },
            { 4572, new CityWithId(4572, "Nørre Asmindrup", 2616361) },
            { 5580, new CityWithId(5580, "Nørre Aaby", 2616368) },
            { 6830, new CityWithId(6830, "Nørre-Nebel", 2616279) },
            { 8766, new CityWithId(8766, "Nørre-Snede", 2616248) },
            { 4951, new CityWithId(4951, "Nørreballe", 2616360) },
            { 9400, new CityWithId(9400, "Nørresundby", 2616235) },
            { 8300, new CityWithId(8300, "Odder", 2615886) },
            { 5000, new CityWithId(5000, "Odense", 2615876) },
            { 6840, new CityWithId(6840, "Oksbøl", 2615860) },
            { 5450, new CityWithId(5450, "Otterup", 2615351) },
            { 5883, new CityWithId(5883, "Oure", 2615343) },
            { 6855, new CityWithId(6855, "Ovtrup", 2615267) },
            { 6330, new CityWithId(6330, "Padborg", 2615242) },
            { 9490, new CityWithId(9490, "Pandrup", 2615222) },
            { 4720, new CityWithId(4720, "Præstø", 2615089) },
            { 7183, new CityWithId(7183, "Randbøl", 2615009) },
            { 8900, new CityWithId(8900, "Randers", 2615006) },
            { 9681, new CityWithId(9681, "Ranum", 2614977) },
            { 8763, new CityWithId(8763, "Rask Mølle", 2614970) },
            { 7970, new CityWithId(7970, "Redsted M", 2614903) },
            { 4420, new CityWithId(4420, "Regstrup", 2614883) },
            { 6760, new CityWithId(6760, "Ribe", 2614813) },
            { 5750, new CityWithId(5750, "Ringe", 2614790) },
            { 6950, new CityWithId(6950, "Ringkøbing", 2614776) },
            { 4100, new CityWithId(4100, "Ringsted", 2614764) },
            { 8240, new CityWithId(8240, "Risskov", 2614718) },
            { 4000, new CityWithId(4000, "Roskilde", 2614481) },
            { 7870, new CityWithId(7870, "Roslev", 2614477) },
            { 4243, new CityWithId(4243, "Rude", 2614440) },
            { 5900, new CityWithId(5900, "Rudkøbing", 2614432) },
            { 4291, new CityWithId(4291, "Ruds-Vedby", 2614427) },
            { 2960, new CityWithId(2960, "Rungsted Kyst", 2614400) },
            { 8680, new CityWithId(8680, "Ry ", 2614357) },
            { 5350, new CityWithId(5350, "Rynkeby", 2614356) },
            { 8550, new CityWithId(8550, "Ryomgård", 2614353) },
            { 5856, new CityWithId(5856, "Ryslinge", 2614349) },
            { 4970, new CityWithId(4970, "Rødby", 2614626) },
            { 6630, new CityWithId(6630, "Rødding", 2614623) },
            { 6230, new CityWithId(6230, "Rødekro", 2614611) },
            { 8840, new CityWithId(8840, "Rødkærsbro", 2614602) },
            { 2610, new CityWithId(2610, "Rødovre", 2614600) },
            { 4673, new CityWithId(4673, "Rødvig Stevns", 2614595) },
            { 6792, new CityWithId(6792, "Rømø/Havneby", 2614568) },
            { 8410, new CityWithId(8410, "Rønde", 2614565) },
            { 3700, new CityWithId(3700, "Rønne", 2614553) },
            { 4683, new CityWithId(4683, "Rønnede", 2614547) },
            { 4581, new CityWithId(4581, "Rørvig", 2614515) },
            { 8471, new CityWithId(8471, "Sabro", 2614343) },
            { 4990, new CityWithId(4990, "Sakskøbing", 2614328) },
            { 9493, new CityWithId(9493, "Saltum", 2614286) },
            { 8305, new CityWithId(8305, "Samsø/Tranebjerg", 2611311) },
            { 4592, new CityWithId(4592, "Sejerø", 2614122) },
            { 8600, new CityWithId(8600, "Silkeborg", 2614030) },
            { 9870, new CityWithId(9870, "Sindal", 2614011) },
            { 4583, new CityWithId(4583, "Sjællands Odde", 2609929) },
            { 6093, new CityWithId(6093, "Sjølund", 2613970) },
            { 9990, new CityWithId(9990, "Skagen", 2613939) },
            { 8832, new CityWithId(8832, "Skals", 2613896) },
            { 5485, new CityWithId(5485, "Skamby", 2613891) },
            { 8660, new CityWithId(8660, "Skanderborg", 2613887) },
            { 4050, new CityWithId(4050, "Skibby", 2613766) },
            { 7800, new CityWithId(7800, "Skive", 2613731) },
            { 6900, new CityWithId(6900, "Skjern", 2613715) },
            { 2942, new CityWithId(2942, "Skodsborg", 2613685) },
            { 2740, new CityWithId(2740, "Skovlunde", 2613588) },
            { 5881, new CityWithId(5881, "Skårup Fyn", 2613845) },
            { 4230, new CityWithId(4230, "Skælskør", 2613694) },
            { 6780, new CityWithId(6780, "Skærbæk", 2613539) },
            { 3320, new CityWithId(3320, "Skævinge", 2613471) },
            { 8541, new CityWithId(8541, "Skødstrup", 2613675) },
            { 9520, new CityWithId(9520, "Skørping", 2613672) },
            { 4200, new CityWithId(4200, "Slagelse", 2613460) },
            { 3550, new CityWithId(3550, "Slangerup", 2613451) },
            { 2765, new CityWithId(2765, "Smørum", 2613345) },
            { 7752, new CityWithId(7752, "Snedsted", 2613327) },
            { 3070, new CityWithId(3070, "Snekkersten", 2613319) },
            { 4460, new CityWithId(4460, "Snertinge", 2613313) },
            { 2680, new CityWithId(2680, "Solrød Strand", 2613233) },
            { 6560, new CityWithId(6560, "Sommersted", 2613224) },
            { 8641, new CityWithId(8641, "Sorring", 2612861) },
            { 4180, new CityWithId(4180, "Sorø", 2612862) },
            { 8981, new CityWithId(8981, "Spentrup", 2612823) },
            { 6971, new CityWithId(6971, "Spjald", 2612815) },
            { 8472, new CityWithId(8472, "Sporup", 2620835) },
            { 7270, new CityWithId(7270, "Stakroge", 2612757) },
            { 4780, new CityWithId(4780, "Stege", 2612689) },
            { 8781, new CityWithId(8781, "Stenderup", 2612664) },
            { 4295, new CityWithId(4295, "Stenlille", 2612633) },
            { 3660, new CityWithId(3660, "Stenløse", 2612630) },
            { 5771, new CityWithId(5771, "Stenstrup", 2612595) },
            { 4773, new CityWithId(4773, "Stensved", 2612588) },
            { 7850, new CityWithId(7850, "Stoholm Jyll.", 2612529) },
            { 4952, new CityWithId(4952, "Stokkemarke", 2612519) },
            { 4480, new CityWithId(4480, "Store Fuglede", 2612427) },
            { 4660, new CityWithId(4660, "Store Heddinge", 2612411) },
            { 4370, new CityWithId(4370, "Store Merløse", 2612381) },
            { 9280, new CityWithId(9280, "Storvorde", 2612301) },
            { 7140, new CityWithId(7140, "Stouby", 2612300) },
            { 9970, new CityWithId(9970, "Strandby Vends.", 2612274) },
            { 7600, new CityWithId(7600, "Struer", 2612204) },
            { 4671, new CityWithId(4671, "Strøby", 2612213) },
            { 4850, new CityWithId(4850, "Stubbekøbing", 2612192) },
            { 9530, new CityWithId(9530, "Støvring", 2612501) },
            { 9541, new CityWithId(9541, "Suldrup", 2612133) },
            { 7451, new CityWithId(7451, "Sunds", 2612107) },
            { 3740, new CityWithId(3740, "Svaneke", 2612078) },
            { 4470, new CityWithId(4470, "Svebølle", 2612057) },
            { 5700, new CityWithId(5700, "Svendborg", 2612045) },
            { 9230, new CityWithId(9230, "Svenstrup J", 2612026) },
            { 4520, new CityWithId(4520, "Svinninge", 2611999) },
            { 6470, new CityWithId(6470, "Sydals", 2611971) },
            { 9300, new CityWithId(9300, "Sæby", 2614175) },
            { 2860, new CityWithId(2860, "Søborg", 2613205) },
            { 5985, new CityWithId(5985, "Søby Ærø", 2613196) },
            { 4920, new CityWithId(4920, "Søllested", 2613136) },
            { 7280, new CityWithId(7280, "Sønder Felding", 2613064) },
            { 7260, new CityWithId(7260, "Sønder Omme", 2612996) },
            { 6092, new CityWithId(6092, "Sønder Stenderup", 2612951) },
            { 6400, new CityWithId(6400, "Sønderborg", 2613102) },
            { 5471, new CityWithId(5471, "Søndersø", 2612955) },
            { 7550, new CityWithId(7550, "Sørvad", 2612894) },
            { 2630, new CityWithId(2630, "Taastrup", 2611828) },
            { 4733, new CityWithId(4733, "Tappernøje", 2611872) },
            { 6880, new CityWithId(6880, "Tarm", 2611865) },
            { 9575, new CityWithId(9575, "Terndrup", 2611783) },
            { 8653, new CityWithId(8653, "Them", 2611761) },
            { 7700, new CityWithId(7700, "Thisted", 2611755) },
            { 8881, new CityWithId(8881, "Thorsø", 2611747) },
            { 7680, new CityWithId(7680, "Thyborøn", 2611738) },
            { 3080, new CityWithId(3080, "Tikøb", 2611723) },
            { 8381, new CityWithId(8381, "Tilst", 2611720) },
            { 6980, new CityWithId(6980, "Tim", 2611718) },
            { 6360, new CityWithId(6360, "Tinglev", 2611684) },
            { 6862, new CityWithId(6862, "Tistrup", 2611653) },
            { 3220, new CityWithId(3220, "Tisvildeleje", 2611649) },
            { 6731, new CityWithId(6731, "Tjæreborg", 2611610) },
            { 6520, new CityWithId(6520, "Toftlund", 2611565) },
            { 5690, new CityWithId(5690, "Tommerup", 2611520) },
            { 4891, new CityWithId(4891, "Toreby L", 2611452) },
            { 4943, new CityWithId(4943, "Torrig L", 2611404) },
            { 5953, new CityWithId(5953, "Tranekær", 2611307) },
            { 8570, new CityWithId(8570, "Trustrup", 2611171) },
            { 4030, new CityWithId(4030, "Tune", 2611132) },
            { 4682, new CityWithId(4682, "Tureby", 2611116) },
            { 4340, new CityWithId(4340, "Tølløse", 2611509) },
            { 6270, new CityWithId(6270, "Tønder", 2611497) },
            { 7160, new CityWithId(7160, "Tørring", 2611486) },
            { 9830, new CityWithId(9830, "Tårs", 2611854) },
            { 4350, new CityWithId(4350, "Ugerløse", 2610989) },
            { 7171, new CityWithId(7171, "Uldum", 2610960) },
            { 6990, new CityWithId(6990, "Ulfborg", 2610959) },
            { 5540, new CityWithId(5540, "Ullerslev", 2610943) },
            { 8860, new CityWithId(8860, "Ulstrup", 2610913) },
            { 9430, new CityWithId(9430, "Vadum", 2610824) },
            { 2500, new CityWithId(2500, "Valby", 2610802) },
            { 2625, new CityWithId(2625, "Vallensbæk", 2610789) },
            { 2665, new CityWithId(2665, "Vallensbæk Strand", 2610789) },
            { 6580, new CityWithId(6580, "Vamdrup", 2610760) },
            { 7184, new CityWithId(7184, "Vandel", 2610755) },
            { 2720, new CityWithId(2720, "Vanløse", 2610735) },
            { 6800, new CityWithId(6800, "Varde", 2610726) },
            { 2950, new CityWithId(2950, "Vedbæk", 2610680) },
            { 5474, new CityWithId(5474, "Veflinge", 2610649) },
            { 3210, new CityWithId(3210, "Vejby", 2610640) },
            { 6600, new CityWithId(6600, "Vejen", 2610634) },
            { 6853, new CityWithId(6853, "Vejers Strand", 2610625) },
            { 7100, new CityWithId(7100, "Vejle", 2610613) },
            { 5882, new CityWithId(5882, "Vejstrup", 2610572) },
            { 3670, new CityWithId(3670, "Veksø Sjælland", 2610566) },
            { 7570, new CityWithId(7570, "Vemb", 2610543) },
            { 4241, new CityWithId(4241, "Vemmelev", 2610541) },
            { 7742, new CityWithId(7742, "Vesløs", 2610503) },
            { 9380, new CityWithId(9380, "Vestbjerg", 2610499) },
            { 5762, new CityWithId(5762, "Vester Skerninge", 2610380) },
            { 4953, new CityWithId(4953, "Vesterborg", 2610479) },
            { 7770, new CityWithId(7770, "Vestervig", 2610348) },
            { 8800, new CityWithId(8800, "Viborg", 2610319) },
            { 8260, new CityWithId(8260, "Viby J", 2610310) },
            { 4130, new CityWithId(4130, "Viby Sjælland", 2610311) },
            { 6920, new CityWithId(6920, "Videbæk", 2610307) },
            { 4560, new CityWithId(4560, "Vig St.", 2610284) },
            { 7480, new CityWithId(7480, "Vildbjerg", 2610259) },
            { 7980, new CityWithId(7980, "Vils", 2610227) },
            { 7830, new CityWithId(7830, "Vinderup", 2610197) },
            { 4390, new CityWithId(4390, "Vipperød", 2610162) },
            { 2830, new CityWithId(2830, "Virum", 2610155) },
            { 5492, new CityWithId(5492, "Vissenbjerg", 2610140) },
            { 6052, new CityWithId(6052, "Viuf", 2610118) },
            { 9310, new CityWithId(9310, "Vodskov", 2610095) },
            { 6500, new CityWithId(6500, "Vojens", 2610075) },
            { 7173, new CityWithId(7173, "Vonge", 2610037) },
            { 6623, new CityWithId(6623, "Vorbasse", 2610025) },
            { 4760, new CityWithId(4760, "Vordingborg", 2610021) },
            { 9760, new CityWithId(9760, "Vrå", 2609994) },
            { 4873, new CityWithId(4873, "Væggerløse", 2610328) },
            { 3500, new CityWithId(3500, "Værløse", 2609956) },
            { 5970, new CityWithId(5970, "Ærøskøbing", 2625001) },
            { 6870, new CityWithId(6870, "Ølgod", 2615749) },
            { 3310, new CityWithId(3310, "Ølsted", 2615737) },
            { 3650, new CityWithId(3650, "Ølstykke", 2615730) },
            { 5853, new CityWithId(5853, "Ørbæk", 2615718) },
            { 6973, new CityWithId(6973, "Ørnhøj", 2615684) },
            { 8950, new CityWithId(8950, "Ørsted", 2615651) },
            { 8586, new CityWithId(8586, "Ørum Djursland", 2615645) },
            { 8752, new CityWithId(8752, "Østbirk", 2615626) },
            { 4894, new CityWithId(4894, "Øster Ulslev", 2615459) },
            { 7990, new CityWithId(7990, "Øster-Assels", 2615613) },
            { 3751, new CityWithId(3751, "Østermarie", 2615506) },
            { 9750, new CityWithId(9750, "Østervrå", 2615448) },
            { 6200, new CityWithId(6200, "Aabenraa", 2625070) },
            { 9440, new CityWithId(9440, "Aabybro", 2625037) },
            { 8230, new CityWithId(8230, "Åbyhøj", 2625033) },
            { 3720, new CityWithId(3720, "Aakirkeby", 2624929) },
            { 9000, new CityWithId(9000, "Aalborg", 2624886) },
            { 9982, new CityWithId(9982, "Ålbæk", 2624900) },
            { 9620, new CityWithId(9620, "Aalestrup", 2624864) },
            { 3140, new CityWithId(3140, "Ålsgårde", 2624784) },
            { 8000, new CityWithId(8000, "Aarhus", 2624652) },
            { 9600, new CityWithId(9600, "Aars", 2624602) },
            { 5792, new CityWithId(5792, "Årslev", 2624596) },
            { 5560, new CityWithId(5560, "Aarup", 2624587) },
        };

        #endregion
    }
}
