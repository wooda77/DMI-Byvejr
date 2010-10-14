//
// ChooseCityPage.xaml.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using Microsoft.Phone.Controls;

namespace DMI.Views
{
    using ViewModels;

    public partial class ChooseCityPage : PhoneApplicationPage
    {
        public ChooseCityPage()
        {
            InitializeComponent();
        }

        private void SearchTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            (DataContext as ChooseCityViewModel).FilterItems(SearchTextBox.Text);
        }
    }
}