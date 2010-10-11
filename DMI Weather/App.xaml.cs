//
// App.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Navigation;
using System.Diagnostics;
using System.Globalization;
using System.Threading;

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DMI_Weather
{
    using ViewModels;
    using System.IO.IsolatedStorage;
    using System.Windows.Media;

    public partial class App : Application
    {
        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame
        {
            get;
            private set;
        }

        public const string Favorites = "favorites";

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

        /// <summary>
        /// Initializes a new instance of the <see cref="T:App"/> class.
        /// </summary>
        public App()
        {
            UnhandledException += Application_UnhandledException;

            Thread.CurrentThread.CurrentCulture = new CultureInfo("da-DK");
            Thread.CurrentThread.CurrentUICulture = new CultureInfo("da-DK");

            InitializeComponent();
            InitializePhoneApplication();
        }

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