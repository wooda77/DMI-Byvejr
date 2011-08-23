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
using DMI.Data;
using DMI.Service;
using DMI.Service.Properties;
using HtmlAgilityPack;
using Newtonsoft.Json;

namespace DMI.Service
{
    public static class UVIndexProvider
    {
        public static void GetUVIndex(Action<IList<UVIndex>> callback)
        {
            var client = HttpWebRequest.Create(Resources.UVIndexFeed);
            client.DownloadStringAsync(html =>
            {
                var document = new HtmlDocument();
                document.LoadHtml(html);

                var framebody = document.DocumentNode
                    .Descendants("div")
                    .FirstOrDefault(x => x.Id == "framebody");
                
                var script = framebody.Element("script").InnerText;
                var lines = script.Split(new [] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                
                var header = lines[0].Substring(12, lines[0].Length - 15);
                var uvList = lines[1].Substring(11, lines[1].Length - 12);
                var symbolsList = lines[2].Substring(15, lines[2].Length - 16);

                var indices = JsonConvert.DeserializeObject<List<string>>(uvList);
                var symbols = JsonConvert.DeserializeObject<List<string>>(symbolsList);

                var result = new List<UVIndex>();

                for (int i = 0; i < indices.Count; i++)
                {
                    result.Add(new UVIndex() 
                    {
                        Text = indices[i],
                        Image = new Uri("http://www.dmi.dk/dmi/" + symbols[i], UriKind.Absolute)
                    });                    
                }

                callback(result);
            });
        }                
    }    
}
