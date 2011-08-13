using System;
using System.Linq;
using System.Collections.Generic;
using GalaSoft.MvvmLight;

namespace DMI.ViewModels
{
    public class AddCustomTilePageViewModel : ViewModelBase
    {
        public AddCustomTilePageViewModel()
        {            
            this.Offsets = Enumerable.Range(1, 23).Select(i => 
                {
                    if (i == 1)
                        return "1 time";
                    else
                        return i + " timer";
                });

            this.Hours = Enumerable.Empty<string>();
        }

        public IEnumerable<string> Hours
        {
            get;
            private set;
        }

        public IEnumerable<string> Offsets
        {
            get;
            private set;
        }
    }
}
