using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Newtonsoft.Json;
using System.Collections.Generic;
using DMI.Properties;
using System.Text;
using System.Linq;

namespace DMI.Models
{
    public class WebTVProvider
    {
        private const string WebTvFeed = "http://tv.dmi.dk/js/photos?raw";

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

                    } catch (JsonSerializationException exception)
                    {
                        callback(new List<WebTVItem>(), exception);
                    }
                }
            };

            client.DownloadStringAsync(new Uri(AppResources.WebTVFeed));
        }
    }

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
}
