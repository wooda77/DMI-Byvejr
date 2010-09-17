//
// PollenFeed.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Text;
using System.Xml.Linq;
using System.Diagnostics;

namespace DMI_Weather.Models
{
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

    public class PollenFeed : INotifyPropertyChanged
    {
        private ObservableCollection<PollenItem> pollen
            = new ObservableCollection<PollenItem>();

        public ObservableCollection<PollenItem> PollenData
        {
            get
            {
                return pollen;
            }
        }

        public void Update()
        {
            var client = new WebClient()
            {
                Encoding = Encoding.GetEncoding("iso-8859-1")
            };
            client.DownloadStringCompleted += DownloadStringCompleted;
            client.DownloadStringAsync(new Uri("http://www.dmi.dk/dmi/pollen-feed.xml"));
        }

        private void DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null)
            {
                return;
            }

            try
            {
                var items = XElement.Parse(e.Result).
                    Elements("channel").
                    Elements("item").ToList();

                pollen.Clear();

                for (int i = 0; i < 4; i += 2)
                {
                    pollen.Add(new PollenItem()
                    {
                        City = items[i].Element("title").Value,
                        Data = ParsePollenData(items[i].Element("description").Value),
                        Forecast = items[i + 1].Element("description").Value
                    });

                    OnPropertyChanged("PollenData");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }

        private string ParsePollenData(string data)
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
