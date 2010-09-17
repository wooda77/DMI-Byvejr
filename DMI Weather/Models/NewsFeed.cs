// 
// NewsFeed.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;

namespace DMI_Weather.Models
{
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
    }

    public class NewsFeed : INotifyPropertyChanged
    {
        private ObservableCollection<NewsItem> newsItems
            = new ObservableCollection<NewsItem>();

        public ObservableCollection<NewsItem> NewsItems
        {
            get
            {
                return newsItems;
            }
        }

        public void Update()
        {
            var client = new WebClient();
            client.Encoding = Encoding.GetEncoding("iso-8859-1");
            client.DownloadStringCompleted += NewsWebClient_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri("http://www.dmi.dk/dmi/rss-nyheder"));
        }

        private void NewsWebClient_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }

            try
            {
                var items = XElement.Parse(e.Result).
                    Elements("channel").
                    Elements("item");

                newsItems.Clear();

                foreach (var item in items)
                {
                    newsItems.Add(new NewsItem()
                    {
                        Title = item.Element("title").Value,
                        Description = item.Element("description").Value,
                        Link = new Uri(item.Element("link").Value)
                    });

                    OnPropertyChanged("NewsItems");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = (o, e) =>
        {
        };

        protected virtual void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged(this, e);
        }

        #endregion
    }
}
