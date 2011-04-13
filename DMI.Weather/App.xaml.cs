//
// App.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO.IsolatedStorage;
using System.Threading;
using System.Windows;
using System.Windows.Media;
using System.Windows.Navigation;
using DMI.Models;
using GalaSoft.MvvmLight.Messaging;
using Microsoft.Phone.Controls;

namespace DMI
{
    public partial class App : Application
    {
        public const string Favorites = "favorites";
        public const string PivotItem = "pivotitem";
        public const string ToggleGPS = "togglegps";
        public const string FirstStart = "firststart";

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
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
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