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

namespace DMI_Weather.Models
{
    public class Resources
    {
        private static AppResources resources = new AppResources();

        public AppResources Localized
        {
            get
            {
                return resources;
            }
        }
    }
}
