//
// City.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;

namespace DMI.Models
{
    public class City : IEquatable<City>, IComparable<City>
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

        #region IComparable<City> Members

        public int CompareTo(City other)
        {
            return this.Name.CompareTo(other.Name);
        }

        #endregion
    }
}
