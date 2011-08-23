using System;
using ImageTools.IO;
using ImageTools.IO.Gif;
using DMI.Service;
using GalaSoft.MvvmLight;

namespace DMI.ViewModels
{
    public class RadarPageViewModel : ViewModelBase
    {
        public RadarPageViewModel()
        {
            if (IsInDesignMode == false)
            {
                Decoders.AddDecoder<GifDecoder>();

                this.ImageSource = new Uri(AppSettings.RadarAnimation, UriKind.Absolute);            
            }
        }

        public Uri ImageSource
        {
            get;
            private set;
        }
    }
}
