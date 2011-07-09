using System;
using System.Net;
using System.Collections.Generic;
using System.Device.Location;
using DMI.Model;

namespace DMI.ViewModel
{
    public class BeachWeatherViewModel
    {
        public BeachWeatherViewModel()
        {
            this.Center = new GeoCoordinate(56.183782, 10.395813);
        }

        public GeoCoordinate Center
        {
            get;
            private set;
        }

        public IEnumerable<Beach> Beaches
        {
            get
            {
                return DanishBeaches;
            }
        }

        #region Beaches

        private static List<Beach> DanishBeaches = new List<Beach>() 
        {
            new Beach { Latitude=57.735684, Longitude=10.583954, ID = 9033, Name = "Skagen" }, 
            new Beach { Latitude=57.33764, Longitude=10.513916, ID = 9030, Name = "Sæby" }, 
            new Beach { Latitude=57.279785, Longitude=10.991135, ID = 9019, Name = "Læsø", HasBlueFlag = false }, 
            new Beach { Latitude=57.373938, Longitude=9.720154, ID = 9021, Name = "Løkken" }, 
            new Beach { Latitude=57.255652, Longitude=9.584541, ID = 9031, Name = "Blokhus" }, 
            new Beach { Latitude=56.950966, Longitude=8.388062, ID = 9018, Name = "Vorupør" }, 
            new Beach { Latitude=56.701301, Longitude=8.211594, ID = 9036, Name = "Thyborøn" }, 
            new Beach { Latitude=56.024483, Longitude=8.12439, ID = 9012, Name = "Hvide Sande" }, 
            new Beach { Latitude=55.737017, Longitude=8.17606, ID = 9009, Name = "Henne" }, 
            new Beach { Latitude=55.420831, Longitude=8.415527, ID = 9006, Name = "Fanø" }, 
            new Beach { Latitude=55.145526, Longitude=8.510284, ID = 9029, Name = "Rømø" }, 
            new Beach { Latitude=56.79948, Longitude=10.278203, ID = 9043, Name = "Øster Hurup" }, 
            new Beach { Latitude=56.512154, Longitude=10.49675, ID = 9044, Name = "Rygaard" }, 
            new Beach { Latitude=56.412382, Longitude=10.894318, ID = 9008, Name = "Grenå" }, 
            new Beach { Latitude=56.088799, Longitude=10.248184, ID = 9028, Name = "Moesgaard" }, 
            new Beach { Latitude=55.710093, Longitude=9.998245, ID = 9013, Name = "Juelsminde" }, 
            new Beach { Latitude=55.511527, Longitude=9.899368, ID = 9004, Name = "Båring Vig" }, 
            new Beach { Latitude=54.896453, Longitude=9.715519, ID = 9039, Name = "Vemmingbund" }, 
            new Beach { Latitude=55.567087, Longitude=10.093689, ID = 9003, Name = "Bogense", HasBlueFlag = false }, 
            new Beach { Latitude=55.621018, Longitude=10.296249, ID = 9007, Name = "Flyvesandet", HasBlueFlag = false }, 
            new Beach { Latitude=55.27051, Longitude=9.901428, ID = 9045, Name = "Assens" }, 
            new Beach { Latitude=55.088291, Longitude=10.255737, ID = 9005, Name = "Faaborg/Klinten" }, 
            new Beach { Latitude=54.856059, Longitude=10.509109, ID = 9023, Name = "Marstal", HasBlueFlag = false }, 
            new Beach { Latitude=55.042483, Longitude=10.706778, ID = 9034, Name = "Smørmosen" }, 
            new Beach { Latitude=55.133752, Longitude=10.910025, ID = 9020, Name = "Lohals" }, 
            new Beach { Latitude=55.324457, Longitude=10.798874, ID = 9025, Name = "Nyborg" }, 
            new Beach { Latitude=55.453941, Longitude=10.653992, ID = 9016, Name = "Kerteminde" }, 
            new Beach { Latitude=55.257012, Longitude=11.251373, ID = 9017, Name = "Kobæk" }, 
            new Beach { Latitude=55.184356, Longitude=11.646881, ID = 9015, Name = "Karrebæksminde" }, 
            new Beach { Latitude=55.030974, Longitude=12.297134, ID = 9037, Name = "Ulvshale" }, 
            new Beach { Latitude=54.689858, Longitude=11.969776, ID = 9022, Name = "Marielyst", HasBlueFlag = false }, 
            new Beach { Latitude=54.632406, Longitude=11.40928, ID = 9027, Name = "Østersøbadet" }, 
            new Beach { Latitude=54.656159, Longitude=11.350079, ID = 9047, Name = "Rødbyhavn" }, 
            new Beach { Latitude=54.833918, Longitude=11.096878, ID = 9010, Name = "Hestehovedet" }, 
            new Beach { Latitude=55.533488, Longitude=12.22126, ID = 9035, Name = "Solrød", HasBlueFlag = false }, 
            new Beach { Latitude=55.680682, Longitude=11.072845, ID = 9014, Name = "Kalundborg" }, 
            new Beach { Latitude=55.830601, Longitude=11.393509, ID = 9040, Name = "Sejerøbugten" }, 
            new Beach { Latitude=55.945163, Longitude=11.756058, ID = 9026, Name = "Rørvig", HasBlueFlag = false }, 
            new Beach { Latitude=55.678504, Longitude=12.07818, ID = 9038, Name = "Veddelev", HasBlueFlag = false }, 
            new Beach { Latitude=56.059578, Longitude=12.071228, ID = 9049, Name = "Tisvilde" }, 
            new Beach { Latitude=56.0933, Longitude=12.455406, ID = 9011, Name = "Hornbæk" }, 
            new Beach { Latitude=55.931414, Longitude=12.519264, ID = 9024, Name = "Nivå", HasBlueFlag = false }, 
            new Beach { Latitude=55.756003, Longitude=12.580547, ID = 9046, Name = "Charlottenlund", HasBlueFlag = false }, 
            new Beach { Latitude=55.655897, Longitude=12.642517, ID = 9041, Name = "Amager Strandpark" }, 
            new Beach { Latitude=55.25689, Longitude=14.819269, ID = 9032, Name = "Sandvig", HasBlueFlag = false }, 
            new Beach { Latitude=55.084459, Longitude=14.706659, ID = 9048, Name = "Antoinette", HasBlueFlag = false }, 
            new Beach { Latitude=55.006924, Longitude=15.103884, ID = 9001, Name = "Dueodde" },  
        };

        #endregion
    }
}
