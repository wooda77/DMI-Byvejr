//using a selector inspired by http://blogs.msdn.com/b/delay/archive/2010/03/17/confessions-of-a-listbox-groupie-using-ivalueconverter-to-create-a-grouped-list-of-items-simply-and-flexibly.aspx
using System;


namespace Clarity.Phone.Controls
{
    public interface IQuickJumpGridSelector
    {
        /// <summary>
        /// Function that returns the group selector.
        /// </summary>
        /// <returns>Key to use for grouping.</returns>
        Func<object, IComparable> GetGroupBySelector();

        Func<object, string> GetOrderByKeySelector();

        Func<object, string> GetThenByKeySelector();
    }
}
