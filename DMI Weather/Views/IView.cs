//
// IView.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//

namespace DMI_Weather.Views
{
    using ViewModels;

    public interface IView
    {
        IViewModel ViewModel
        {
            get;
        }
    }
}
