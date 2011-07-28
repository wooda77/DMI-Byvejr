using System;
using System.Collections.Generic;
using System.Linq;
using RestSharp;
using System.Globalization;
using DMI.Common;

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
