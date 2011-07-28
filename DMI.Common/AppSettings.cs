using System;
using System.IO.IsolatedStorage;

namespace DMI.Common
{
    /// <summary>
    /// Application Settings
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// Periodic Task Name.
        /// </summary>
        public const string PeriodicTaskName = "DMI Byvejr";

        /// <summary>
        /// Favorites IsolatedStorage Key.
        /// </summary>
        public const string FavoritesKey = "favorites";

        /// <summary>
        /// PivotItem IsolatedStorage Key.
        /// </summary>
        public const string PivotItemKey = "pivotitem";

        /// <summary>
        /// ToggleGPS IsolatedStorage Key.
        /// </summary>
        public const string ToggleGPSKey = "togglegps";

        /// <summary>
        /// IsFirstStart IsolatedStorage Key.
        /// </summary>
        public const string IsFirstStartKey = "firststart";

        public static string SupportPageAdress = "/View/SupportPage.xaml";

        public static string ImagePageAddress = "/View/ImagePage.xaml?ImageSource={0}";

        public static string ChooseCityPageAddress = "/View/ChooseCityPage.xaml";

        public static string MainPageAddress = "/View/MainPage.xaml?PostalCode={0}&Country={1}";

        public static string MainPageWithTileAddress = "/View/MainPage.xaml?PostalCode={0}&Country={1}&TileType={2}";

        public static string RadarPageAddress = "/View/RadarPage.xaml";

        public static string BeachWeatherPageAddress = "/View/BeachWeatherPage.xaml";

        public static string AddTilePageAddress = "/View/AddTilePage.xaml?PostalCode={0}&Country={1}";

        public static string TileTypeUrlSegment = "TileType={0}";

        /// <summary>
        /// Gets a value indicating whether the GPS is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this the GPS is enabled; otherwise, <c>false</c>.
        /// </value>
        public static bool IsGPSEnabled
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(AppSettings.ToggleGPSKey))
                    return (bool)IsolatedStorageSettings.ApplicationSettings[AppSettings.ToggleGPSKey];
                else
                    return false;
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this instance is the first start.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if this instance is the first start; otherwise, <c>false</c>.
        /// </value>
        public static bool IsFirstStart
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(AppSettings.IsFirstStartKey))
                    return (bool)IsolatedStorageSettings.ApplicationSettings[AppSettings.IsFirstStartKey];
                else
                    return true;
            }
            set
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(AppSettings.IsFirstStartKey))
                    IsolatedStorageSettings.ApplicationSettings[AppSettings.IsFirstStartKey] = value;
                else
                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.IsFirstStartKey, value);

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }
    }
}
