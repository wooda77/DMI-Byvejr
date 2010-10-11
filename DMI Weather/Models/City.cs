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
    public class City : IEquatable<City>
    {
        public string Name
        {
            get;
            set;
        }

        public int PostalCode
        {
            get;
            set;
        }

        #region IEquatable<City> Members

        public bool Equals(City other)
        {
            return this.PostalCode == other.PostalCode;
        }

        #endregion
    }
}
