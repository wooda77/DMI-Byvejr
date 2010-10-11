using System;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using Clarity.Phone.Controls;

namespace DMI_Weather.Models
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
