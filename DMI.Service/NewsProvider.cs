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
using System.Xml.Linq;
using DMI.Data;
using DMI.Service.Properties;
using Newtonsoft.Json;

namespace DMI.Service
{
    public static class NewsProvider
    {
        public static void GetNewsItems(Action<IEnumerable<NewsItem>, Exception> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var client = HttpWebRequest.Create(Resources.NewsFeed);
            client.DownloadStringAsync(xml =>
            {
                var items = XElement.Parse(xml)
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

                callback(items, null);
            });
        }

        public static void GetVideos(Action<List<WebTVItem>, Exception> callback)
        {
            if (callback == null)
                throw new ArgumentNullException("callback");

            var client = HttpWebRequest.Create(Resources.WebTVFeed);
            client.DownloadStringAsync(text =>
            {
                var json = HttpUtility.HtmlDecode(text);

                try
                {
                    var response = JsonConvert.DeserializeObject<WebTVResponse>(json);
                        
                    callback(response.Items.ToList(), null);
                }
                catch (JsonSerializationException exception)
                {
                    callback(new List<WebTVItem>(), exception);
                }
            });
        }
    }
}
