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
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace DMI
{
    public partial class App : Application
    {
        private bool phoneApplicationInitialized = false;

        public const string Favorites = "favorites";
        public const string PivotItem = "pivotitem";
        public const string ToggleGPS = "togglegps";
        public const string FirstStart = "firststart";
        public const string LastCity = "lastcity";
        public const string ImageFolder = "dmiimagefolder";

        public App()
        {
            UnhandledException += Application_UnhandledException;

            InitializeComponent();
            InitializePhoneApplication();
        }

        public PhoneApplicationFrame RootFrame
        {
            get;
            private set;
        }

        public static bool IsGPSEnabled
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(App.ToggleGPS))
                    return (bool)IsolatedStorageSettings.ApplicationSettings[App.ToggleGPS];
                else
                    return false;
            }
        }

        public static bool IsFirstStart
        {
            get
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(App.FirstStart))
                    return (bool)IsolatedStorageSettings.ApplicationSettings[App.FirstStart];
                else
                    return true;
            }
            set
            {
                if (IsolatedStorageSettings.ApplicationSettings.Contains(App.FirstStart))
                    IsolatedStorageSettings.ApplicationSettings[App.FirstStart] = value;
                else
                    IsolatedStorageSettings.ApplicationSettings.Add(App.FirstStart, value);

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        public static ThemeBackground CurrentThemeBackground
        {
            get
            {
                var currentColor = (Color)Application.Current.Resources["PhoneBackgroundColor"];

                if (currentColor == Colors.Black)
                    return ThemeBackground.ThemeBackgroundDark;
                else
                    return ThemeBackground.ThemeBackgroundLight;
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

        public static void GoBack()
        {
            (App.Current.RootVisual as PhoneApplicationFrame).GoBack();
        }

        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }

        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (Debugger.IsAttached)
                Debugger.Break();
        }

        private void InitializePhoneApplication()
        {
            Thread.CurrentThread.CurrentUICulture = Thread.CurrentThread.CurrentCulture;

            if (phoneApplicationInitialized)
                return;

            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            SmartDispatcher.Initialize(Deployment.Current.Dispatcher);

            phoneApplicationInitialized = true;
        }

        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }
    }
}