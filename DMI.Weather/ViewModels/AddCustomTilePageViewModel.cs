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
                        return "1 time ";
                    else
                        return i + " timer";
                });

            this.Hours = Enumerable.Range(1, 23).Select(i =>
            {
                return string.Format("{0:d2}:00", i);
            });

            this.Types = new string[] { 
                "Forskudt (F.eks. vejret om 6 timer)",
                "Fikseret (F.eks. vejret kl. 8 hver dag)"
            };
        }

        public IEnumerable<string> Types
        {
            get;
            private set;
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
