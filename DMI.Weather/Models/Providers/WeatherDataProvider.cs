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
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace DMI.Models
{
    using DMI.Properties;

    public static class WeatherDataProvider
    {
        /// <summary>
        /// Gets all the items from the DMI pollen feed.
        /// </summary>
        /// <param name="callback"></param>
        public static void GetPollenData(Action<IEnumerable<PollenItem>, Exception> callback)
        {
            var client = new WebClient()
            {
                Encoding = Encoding.GetEncoding("iso-8859-1")
            };

            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    callback(Enumerable.Empty<PollenItem>(), e.Error);
                }
                else
                {
                    var allItems = XElement.Parse(e.Result)
                        .Elements("channel")
                        .Elements("item")
                        .ToArray();

                    var items = allItems.Take(4).Chunks(2).ToArray();

                    var pollenItems = items.Select(x => new PollenItem()
                    {
                        City = x[0].Element("title").Value,
                        Data = ParsePollenData(x[0].Element("description").Value),
                        Forecast = x[1].Element("description").Value
                    });

                    callback(pollenItems, e.Error);
                }
            };

            client.DownloadStringAsync(new Uri(AppResources.PollenFeed));
        }

        /// <summary>
        /// Gets the regional weather text.
        /// </summary>
        /// <param name="callback"></param>
        public static void GetRegionData(string regionUrl, Action<Region, Exception> callback)
        {
            var client = new WebClient()
            {
                Encoding = Encoding.GetEncoding("iso-8859-1")
            };

            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    callback(new Region(), e.Error);
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

                    var region = new Region();
                    
                    if (nameMatches.Count >= 1)
                    {
                        region.Name = nameMatches[0].Groups["text"].Value;
                    }
                    
                    if (textMatches.Count >= 3)
                    {
                        region.Text = textMatches[2].Groups["text"].Value;
                    }

                    callback(region, e.Error);
                }
            };

            client.DownloadStringAsync(new Uri(regionUrl));
        }

        /// <summary>
        /// Get all the country-wide weather items from DMI.
        /// </summary>
        /// <param name="callback"></param>
        public static void GetWeatherData(Action<IEnumerable<WeatherItem>, Exception> callback)
        {
            var client = new WebClient()
            {
                Encoding = Encoding.GetEncoding("iso-8859-1")
            };

            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    callback(Enumerable.Empty<WeatherItem>(), e.Error);
                }
                else
                {
                    var input = HttpUtility.HtmlDecode(e.Result);

                    var pattern  = @"<td class=""mellemrubrik"">(?<title>.*?)</td>";
                        pattern += @"(.*?)<td class=""broedtekst"">(?<description>.*?)</td>";

                    var regex = new Regex(pattern, RegexOptions.Singleline);
                    var matches = regex.Matches(input);

                    var items = new List<WeatherItem>();

                    foreach (var match in matches.Cast<Match>())
                    {
                        var title = match.Groups["title"].Value;
                        title = title.Trim();
                        title = title.Replace(":", "");

                        var description = match.Groups["description"].Value;
                        description = description.Trim();

                        if (string.IsNullOrEmpty(title) == false)
                        {
                            items.Add(new WeatherItem()
                            {
                                Title = title,
                                Description = description
                            });
                        }
                    }

                    callback(items, e.Error);
                }
            };

            client.DownloadStringAsync(new Uri(AppResources.WeatherFeed));
        }

        /// <summary>
        /// Gets all the items from the DMI news feed.
        /// </summary>
        /// <param name="callback"></param>
        public static void GetNewsItems(Action<IEnumerable<NewsItem>, Exception> callback)
        {
            var client = new WebClient()
            {
                Encoding = Encoding.GetEncoding("iso-8859-1")
            };

            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    callback(Enumerable.Empty<NewsItem>(), e.Error);
                }
                else
                {
                    var items = XElement.Parse(e.Result)
                        .Elements("channel")
                        .Elements("item")
                        .Select(item =>
                            new NewsItem()
                            {
                                Title = item.Element("title").Value,
                                Description = item.Element("description").Value,
                                Link = new Uri(item.Element("link").Value)
                            });

                    callback(items, e.Error);
                }
            };

            client.DownloadStringAsync(new Uri(AppResources.RssFeed));
        }

        private static string ParsePollenData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                throw new ArgumentException("data");
            }

            string input = data.Replace("\n", "");
            input = input.Replace(" ", "");

            var result = new StringBuilder();

            string[] parts = input.Split(new char[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var part in parts)
            {
                var partValues = part.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);

                if ((partValues.Length == 2) && (partValues[1] != "-"))
                {
                    result.AppendFormat("{0}: {1} , ", partValues[0], partValues[1]);
                }
            }

            string output = result.ToString();

            if (string.IsNullOrEmpty(output) == false &&
                output.Length >= 4)
            {
                output = output.Substring(0, output.Length - 3);
            }

            return output;
        }
    }
}
