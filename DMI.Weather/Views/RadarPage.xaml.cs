using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using ImageTools.IO;
using ImageTools.IO.Gif;

namespace DMI_Weather
{
    public partial class RadarPage : PhoneApplicationPage
    {
        public RadarPage()
        {
            InitializeComponent();

            Decoders.AddDecoder<GifDecoder>();

            ImageSource = new Uri("http://www.dmi.dk/dmi/radaranim2.gif", UriKind.Absolute);
            DataContext = this;
        }

        public Uri ImageSource
        {
            get;
            private set;
        }
    }
}
