//
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

                var items = XElement.Parse(e.Result)
                    .Elements("channel")
                    .Elements("item")
                    .Take(4)
                    .Chunks(2);

                var pollenItems = items.Select(x => new PollenItem()
                {
                    City = x[0].Element("title").Value,
                    Data = ParsePollenData(x[0].Element("description").Value),
                    Forecast = x[1].Element("description").Value
                });

                callback(pollenItems, e.Error);
            };

            client.DownloadStringAsync(new Uri(AppResources.PollenFeed));
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
