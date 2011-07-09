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
using System.Xml.Linq;
using DMI.Service.Properties;
using Newtonsoft.Json;

namespace DMI.Service
{
    public class NewsProvider
    {
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
                        {
                            var title = item.Element("title");
                            var description = item.Element("description");
                            var link = item.Element("link");

                            return new NewsItem()
                            {
                                Title = title.TryGetValue(),
                                Description = description.TryGetValue(),
                                Link = link == null ? null : new Uri(link.Value)
                            };
                        });

                    callback(items, e.Error);
                }
            };

            client.DownloadStringAsync(new Uri(AppResources.NewsFeed));
        }

        public static void GetVideos(Action<List<WebTVItem>, Exception> callback)
        {
            var client = new WebClient()
            {
                Encoding = Encoding.GetEncoding("iso-8859-1")
            };

            client.DownloadStringCompleted += (sender, e) =>
            {
                if (e.Error != null)
                {
                    callback(new List<WebTVItem>(), e.Error);
                }
                else
                {
                    var json = HttpUtility.HtmlDecode(e.Result);

                    try
                    {
                        var response = JsonConvert.DeserializeObject<WebTVResponse>(json);
                        
                        callback(response.Items.ToList(), e.Error);
                    }
                    catch (JsonSerializationException exception)
                    {
                        callback(new List<WebTVItem>(), exception);
                    }
                }
            };

            client.DownloadStringAsync(new Uri(AppResources.WebTVFeed));
        }
    }
}
