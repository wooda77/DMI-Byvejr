// 
// CityGroup.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System.Collections.Generic;

namespace DMI.Models
{
    public class CityGroup : List<City>
    {
        public CityGroup(string category)
        {
            Key = category;
        }

        public string Key
        {
            get;
            set;
        }

        public bool HasItems
        {
            get
            {
                return Count > 0;
            }
        }
    }
}
