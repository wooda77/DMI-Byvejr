﻿//
// WeatherDataProvider.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;

using DMI.Properties;
using System.Text.RegularExpressions;

namespace DMI.Models
{
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
                    var html = e.Result;
                    html = html.Replace("&oslash;", "ø");
                    html = html.Replace("&aring;", "å");
                    html = html.Replace("&aelig;", "æ");

                    var textPattern = @"<td class=""broedtekst"">(?<text>.*?)</td>";
                    var textRegex = new Regex(textPattern, RegexOptions.Singleline);
                    var textMatches = textRegex.Matches(html);

                    var namePattern = @"<font class=""mellemrubrik"">(?<text>.*?)</font>";
                    var nameRegex = new Regex(namePattern, RegexOptions.Singleline);
                    var nameMatches = nameRegex.Matches(html);

                    var region = new Region();
                    region.Name = nameMatches[0].Groups["text"].Value;
                    region.Text = textMatches[2].Groups["text"].Value;

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
                    var html = e.Result;
                    html = html.Replace("&oslash;", "ø");
                    html = html.Replace("&aring;", "å");
                    html = html.Replace("&aelig;", "æ");

                    var pattern = @"<td class=""mellemrubrik"">(?<title>.*?)</td>(.*?)<td class=""broedtekst"">(?<description>.*?)</td>";

                    var regex = new Regex(pattern, RegexOptions.Singleline);
                    var matches = regex.Matches(html);

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

            if (output != string.Empty)
            {
                output = output.Substring(0, output.Length - 3);
            }

            return output;
        }
    }
}
