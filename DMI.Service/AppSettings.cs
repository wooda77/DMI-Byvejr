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
using System.IO.IsolatedStorage;

namespace DMI.Service
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
        /// ToggleGPS IsolatedStorage Key.
        /// </summary>
        public const string ToggleGPSKey = "togglegps";

        /// <summary>
        /// IsFirstStart IsolatedStorage Key.
        /// </summary>
        public const string IsFirstStartKey = "firststart";

        public static string SupportPageAdress = "/Views/SupportPage.xaml";

        public static string ImagePageAddress = "/Views/ImagePage.xaml?ImageSource={0}";

        public static string ChooseCityPageAddress = "/Views/ChooseCityPage.xaml";

        public static string MainPageBaseAddress = "/Views/MainPage.xaml";

        public static string MainPageAddress = "/Views/MainPage.xaml?PostalCode={0}&Country={1}";

        public static string MainPageWithTileAddress = "/Views/MainPage.xaml?PostalCode={0}&Country={1}&TileType={2}&Offset={3}";

        public static string RadarPageAddress = "/Views/RadarPage.xaml";

        public static string BeachWeatherPageAddress = "/Views/BeachWeatherPage.xaml";

        public static string AddTilePageAddress = "/Views/AddTilePage.xaml?PostalCode={0}&Country={1}";

        public static string BeachWeatherInfoPageAddress = "/Views/BeachWeatherInfoPage.xaml?ID={0}";

        public static string UVIndexPageAddress = "/Views/UVIndexPage.xaml";

        public static string TileTypeUrlSegment = "TileType={0}&Offset={1}";

        public static string TemperatureImageSource = "http://servlet.dmi.dk/byvejr/servlet/byvejr_dag1?by={0}&tabel=dag1&mode=long";

        public static string RadarAnimation = "http://www.dmi.dk/dmi/radaranim2.gif";

        public static string WavesImageSource = "http://servlet.dmi.dk/byvejr/servlet/byvejr?by={0}&tabel=dag1&param=bolger";

        /// <summary>
        /// Gets or sets a value indicating whether the GPS is enabled.
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
            set
            {
                if (!IsolatedStorageSettings.ApplicationSettings.Contains(AppSettings.ToggleGPSKey))
                {
                    IsolatedStorageSettings.ApplicationSettings.Add(AppSettings.ToggleGPSKey, value);
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings[AppSettings.ToggleGPSKey] = value;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
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
