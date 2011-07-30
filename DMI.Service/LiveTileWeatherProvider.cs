using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using System.Globalization;
using DMI.Common;
using System.Windows;

namespace DMI.Service
{
    public class LiveTileWeatherProvider
    {
        public static void GetForecast(GeoLocationCity city, DateTime date, Action<IEnumerable<LiveTileWeatherResponse>, Exception> callback)
        {
            if (city == null)
                throw new ArgumentNullException("city");

            if (callback == null)
                throw new ArgumentNullException("callback");

            var client = new RestClient("http://www.dr.dk/tjenester/drvejret/");
            var request = new RestRequest();
            request.DateFormat = "yyyyMMddHHmmss";            
            request.Resource = "Forecast/{latitude}/{longitude}/{date}";
            request.AddUrlSegment("latitude", city.Location.Latitude.ToString(CultureInfo.InvariantCulture));
            request.AddUrlSegment("longitude", city.Location.Longitude.ToString(CultureInfo.InvariantCulture));
            request.AddUrlSegment("date", date.ToString("yyyyMMdd"));

            client.ExecuteAsync<List<LiveTileWeatherResponse>>(request,
                (response) =>
                {
                    callback(response.Data, response.ErrorException);
                });
        }

        public static void GetFakeForecast(GeoLocationCity city, DateTime date, Action<IEnumerable<LiveTileWeatherResponse>, Exception> callback)
        {
            var response = new List<LiveTileWeatherResponse>();

            response.Add(new LiveTileWeatherResponse()
            {
                Df = new DateTime(2011, 07, 29, 21, 00, 00),
                Dtt = "Aften",
                Prosa = "Skyet. Jævn vind, 6 m/s fra nordvest. Ingen nedbør",
                S = "1",
                T = "30",
            });

            response.Add(new LiveTileWeatherResponse()
            {
                Df = new DateTime(2011, 07, 30, 03, 00, 00),
                Dtt = "Nat",
                Prosa = "Skyet. Frisk vind, 8 m/s fra vest-nordvest. Ingen nedbør",
                S = "4",
                T = "14",
            });

            response.Add(new LiveTileWeatherResponse()
            {
                Df = new DateTime(2011, 07, 30, 09, 00, 00),
                Dtt = "Morgen",
                Prosa = "Skyet. Frisk vind, 8 m/s fra vest-nordvest. Ingen nedbør",
                S = "4",
                T = "15",
            });

            Deployment.Current.Dispatcher.BeginInvoke(() => callback(response, null));
        }
    }

    public class LiveTileWeatherResponse
    {
        public DateTime Df
        {
            get;
            set;
        }

        /// <summary>
        /// Time a day.
        /// </summary>
        public string Dtt
        {
            get;
            set;
        }

        public string T
        {
            get;
            set;
        }

        public LiveTileWeatherTime TimeADay
        {
            get
            {
                if (string.IsNullOrEmpty(Dtt) == false)
                    return (LiveTileWeatherTime)Enum.Parse(typeof(LiveTileWeatherTime), Dtt.Trim(), true);
                
                return LiveTileWeatherTime.Indeterminate;
            }
        }
        
        /// <summary>
        /// Description
        /// </summary>
        public string Prosa
        {
            get;
            set;
        }
        
        /// <summary>
        /// Image
        /// </summary>
        public string S
        {
            get;
            set;
        } 
    }

    public enum LiveTileWeatherTime
    {
        Nu,
        Morgen,
        Eftermiddag,
        Aften,
        Nat,
        Indeterminate
    }
}
