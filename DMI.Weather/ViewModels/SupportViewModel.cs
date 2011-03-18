using System;
using System.IO.IsolatedStorage;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DMI.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Tasks;

namespace DMI.ViewModels
{
    public class SupportViewModel : ViewModelBase
    {
        private readonly ICommand openMailClientCommand;
        private readonly ICommand okCommand;

        /// <summary>
        /// Initializes a new instance of the <see cref="T:SupportViewModel"/> class.
        /// </summary>
        public SupportViewModel()
        {
            this.openMailClientCommand = new RelayCommand(OpenMailClientExecute);
            this.okCommand = new RelayCommand(OKExecute);
        }

        public string Version
        {
            get
            {
                return "1.0.0.0";
            }
        }

        public bool AllowGPS
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
            set
            {
                if (!IsolatedStorageSettings.ApplicationSettings.Contains(App.ToggleGPS))
                {
                    IsolatedStorageSettings.ApplicationSettings.Add(App.ToggleGPS, value);
                }
                else
                {
                    IsolatedStorageSettings.ApplicationSettings[App.ToggleGPS] = value;
                }

                IsolatedStorageSettings.ApplicationSettings.Save();
            }
        }

        public ICommand OpenMailClient
        {
            get
            {
                return openMailClientCommand;
            }
        }

        public ICommand OK
        {
            get
            {
                return okCommand;
            }
        }

        private void OKExecute()
        {
            App.Navigate(new Uri("/Views/MainPage.xaml", UriKind.Relative));
        }

        private void OpenMailClientExecute()
        {
            var emailTask = new EmailComposeTask()
            {
                To = "10229@iha.dk",
                Subject = AppResources.AppTitleSmall + " Support"
            };

            emailTask.Show();
        }
    }
}
