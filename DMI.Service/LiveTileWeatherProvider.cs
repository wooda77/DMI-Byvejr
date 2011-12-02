using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using DMI.Data;
using RestSharp;

namespace DMI.Service
{
    public class LiveTileWeatherProvider
    {
        public static Task<List<LiveTileWeatherResponse>> GetForecast(GeoLocationCity city, DateTime date)
        {
            if (city == null)
                throw new ArgumentNullException("city");

            var client = new RestClient("http://www.dr.dk/tjenester/drvejret/");
            var request = new RestRequest();
            request.DateFormat = "yyyyMMddHHmm";            
            request.Resource = "Forecast/{latitude}/{longitude}/{date}";
            request.AddUrlSegment("latitude", city.Location.Latitude.ToString(CultureInfo.InvariantCulture));
            request.AddUrlSegment("longitude", city.Location.Longitude.ToString(CultureInfo.InvariantCulture));
            request.AddUrlSegment("date", date.ToString("yyyyMMdd"));

            return client.ExecuteTask<List<LiveTileWeatherResponse>>(request);
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
