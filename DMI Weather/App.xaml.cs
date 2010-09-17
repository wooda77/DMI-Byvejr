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

using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace DMI_Weather
{
    using ViewModels;
    using System.IO.IsolatedStorage;

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

        /// <summary>
        /// ServiceLocator container.
        /// </summary>
        private static readonly IDictionary<Type, object> services 
            = new Dictionary<Type, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="T:App"/> class.
        /// </summary>
        public App()
        {
            UnhandledException += Application_UnhandledException;

            InitializeServices();
            InitializeComponent();
            InitializePhoneApplication();
        }

        #region Service Locator

        private void InitializeServices()
        {
            App.Register<MainViewModel>(new MainViewModel());
        }

        public static void Register<TService>(TService service)
        {
            services[typeof(TService)] = service;
        }

        public static TService Resolve<TService>()
        {
            return (TService)services[typeof(TService)];
        }

        #endregion

        #region Persistance Helpers

        private void SaveApplicationSettings()
        {
            var store = IsolatedStorageSettings.ApplicationSettings;

            if (store.Contains("MainViewModel"))
            {
                store["MainViewModel"] = App.Resolve<MainViewModel>();
            }
            else
            {
                store.Add("MainViewModel", App.Resolve<MainViewModel>());
            }

            store.Save();
        }

        private void RestoreApplicationSettings()
        {
            var store = IsolatedStorageSettings.ApplicationSettings;

            if (store.Contains("MainViewModel"))
            {
                App.Register<MainViewModel>(
                    (MainViewModel)store["MainViewModel"]);
            }
        }

        #endregion

        #region Phone Application Service Events

        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            RestoreApplicationSettings();
        }

        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            RestoreApplicationSettings();
        }

        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            SaveApplicationSettings();
        }

        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            SaveApplicationSettings();
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