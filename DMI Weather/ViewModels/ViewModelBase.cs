//
// ViewModelBase.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;
using System.ComponentModel;

namespace DMI_Weather.ViewModels
{
    public class ViewModelBase : IViewModel
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged = (o, e) =>
        {
        };

        protected virtual void OnPropertyChanged(string propertyName)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged(this, e);
        }

        #endregion
    }
}
