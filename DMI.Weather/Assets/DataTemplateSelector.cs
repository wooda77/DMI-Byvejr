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

namespace DMI.Assets
{
    public class DataTemplateSelector : ContentControl
    {
        /// <summary>
        /// Selects the template.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="container">The container.</param>
        /// <returns></returns>
        public virtual DataTemplate SelectTemplate(object item, DependencyObject container)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called when the value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property changes.
        /// </summary>
        /// <param name="oldContent">
        ///     The old value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property.
        /// </param>
        /// <param name="newContent">
        ///     The new value of the <see cref="P:System.Windows.Controls.ContentControl.Content"/> property.
        /// </param>
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            ContentTemplate = SelectTemplate(newContent, this);
        }
    }
}
