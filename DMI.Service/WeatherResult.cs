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
using Newtonsoft.Json;

namespace DMI.Service
{
    public class WebTVResponse
    {
        [JsonProperty("status")]
        public string Status
        {
            get;
            set;
        }

        [JsonProperty("permisssion_level")]
        public string PermissionLevel
        {
            get;
            set;
        }

        [JsonProperty("photos")]
        public WebTVItem[] Items
        {
            get;
            set;
        }
    }

    public class WebTVItem
    {
        [JsonProperty("title")]
        public string Title
        {
            get;
            set;
        }

        [JsonProperty("video_medium_download")]
        public string Video
        {
            get;
            set;
        }

        [JsonProperty("publish_date_ansi")]
        public DateTime Published
        {
            get;
            set;
        }

        [JsonProperty("album_title")]
        public string Category
        {
            get;
            set;
        }

        [JsonProperty("medium_download")]
        public string Image
        {
            get;
            set;
        }
    }

    public class NewsItem
    {
        public string Title
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public Uri Link
        {
            get;
            set;
        }

        public WebTVItem WebTVItem
        {
            get;
            set;
        }
    }

    public class CityWeatherResult
    {
        public Uri CityWeatherThreeDaysImage
        {
            get;
            set;
        }

        public Uri CityWeatherSevenDaysImage
        {
            get;
            set;
        }
    }

    public class RegionalWeatherResult
    {
        public Uri Image
        {
            get;
            set;
        } 

        public string Name
        {
            get;
            set;
        }

        public string Content
        {
            get;
            set;
        }
    }

    public class CountryWeatherResult
    {
        public Uri Image
        {
            get;
            set;
        }

        public IEnumerable<CountryWeatherItem> Items
        {
            get;
            set;
        }
    }

    public class CountryWeatherItem
    {
        public string Title
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }
    }

    public class PollenResult
    {
        public Uri Image
        {
            get;
            set;
        }

        public IEnumerable<PollenItem> Items
        {
            get;
            set;
        }
    }

    public class PollenItem
    {
        public string City
        {
            get;
            set;
        }

        public string Data
        {
            get;
            set;
        }

        public string Forecast
        {
            get;
            set;
        }
    }
}
