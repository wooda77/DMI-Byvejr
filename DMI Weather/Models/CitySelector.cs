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
            return (p) => ((City)p).Name.FirstOrDefault();
        }

        public Func<object, string> GetOrderByKeySelector()
        {
            return (p) => ((City)p).Name;
        }

        public Func<object, string> GetThenByKeySelector()
        {
            return (p) => (string.Empty);
        }
    }
}
