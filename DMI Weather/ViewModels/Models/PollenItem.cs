using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

namespace DMI_Weather.ViewModels.Models
{
    public class PollenItem
    {
        public string City
        {
            get;
            set;
        }

        public string Data
        {
            get;
            set;
        }

        public string Forecast
        {
            get;
            set;
        }
    }
}
