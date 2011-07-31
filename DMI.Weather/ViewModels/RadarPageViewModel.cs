using System;
using ImageTools.IO;
using ImageTools.IO.Gif;
using DMI.Common;

namespace DMI.ViewModels
{
    public class RadarPageViewModel
    {
        public RadarPageViewModel()
        {
            Decoders.AddDecoder<GifDecoder>();

            this.ImageSource = new Uri(AppSettings.RadarAnimation, UriKind.Absolute);            
        }

        public Uri ImageSource
        {
            get;
            private set;
        }
    }
}
