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
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace DMI
{
    public partial class App : Application
    {
        public const string Favorites = "favorites";
        public const string PivotItem = "pivotitem";
        public const string ToggleGPS = "togglegps";
        public const string FirstStart = "firststart";
        public const string LastCity = "lastcity";
        public const string ImageFolder = "dmiimagefolder";
        
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame
        {
            get;
            private set;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="T:App"/> class.
        /// </summary>
        public App()
        {
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();
            
            // Phone-specific initialization
            InitializePhoneApplication();
        }

        public static bool IsGPSEnabled
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(App.ToggleGPS))
                {
                    return (bool)IsolatedStorageSettings.ApplicationSettings[App.ToggleGPS];
                }
                else
                {
                    return false;
                }
            }
        }

        public static bool IsFirstStart
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(App.FirstStart))
                {
                    return (bool)IsolatedStorageSettings.ApplicationSettings[App.FirstStart];
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(App.FirstStart))
                {
                    IsolatedStorageSettings.ApplicationSettings[App.FirstStart] = value;
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings.Add(App.FirstStart, value);
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        #region Custom Utility

        public static ThemeBackground CurrentThemeBackground
        {
            get
            {
                var currentColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];

                if (currentColor == Colors.Black)
                {
                    return ThemeBackground.ThemeBackgroundDark;
                }
                else
                {
                    return ThemeBackground.ThemeBackgroundLight;
                }
            }
        }

        public enum ThemeBackground
        {
            ThemeBackgroundDark,
            ThemeBackgroundLight
        }

        public static bool Navigate(Uri source)
        {
            return (App.Current.RootVisual as PhoneApplicationFrame).Navigate(source);
        }

        #endregion

        #region Debugging Handlers

        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
            {
                Debugger.Break();
            }
        }

        #endregion

        #region Phone Application Initialization

        private bool phoneApplicationInitialized = false;

        private void InitializePhoneApplication()
        {
            // Set the UI Culture to the System Culture, to support more than WP7s six languages.
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            if (phoneApplicationInitialized)
            {
                return;
            }

            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            phoneApplicationInitialized = true;
        }

        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            if (RootVisual != RootFrame)
            {
                RootVisual = RootFrame;
            }

            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
    }
}