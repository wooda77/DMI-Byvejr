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
using DMI.Models;

namespace DMI.Assets
{
    public class PushpinTemplateSelector : DataTemplateSelector
    {
        public DataTemplate BlueFlag
        {
            get;
            set;
        }

        public DataTemplate NoFlag
        {
            get;
            set;
        }

        public override DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            var beach = item as Beach;
            if (beach != null)
            {
                System.Diagnostics.Debug.WriteLine(beach.Location);

                return beach.HasBlueFlag ? BlueFlag : NoFlag;
            }

            return base.SelectTemplate(item, container);
        }
    }
}
