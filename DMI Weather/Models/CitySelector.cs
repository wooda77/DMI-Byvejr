//
// CitySelector.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.Linq;

using Clarity.Phone.Controls;

namespace DMI.Models
{
    public class CitySelector : IQuickJumpGridSelector
    {
        public Func<object, IComparable> GetGroupBySelector()
        {
            return (p) => GetNamePropertyValue(p).FirstOrDefault();
        }

        public Func<object, string> GetOrderByKeySelector()
        {
            return (p) => GetNamePropertyValue(p);
        }

        public Func<object, string> GetThenByKeySelector()
        {
            return (p) => (string.Empty);
        }

        private string GetNamePropertyValue(object p)
        {
            if (p is City)
            {
                return (p as City).Name;
            }
            else
            {
                var type = p.GetType();
                var info = type.GetProperty("Name");

                return info.GetValue(p, null).ToString();
            }
        }
    }
}
