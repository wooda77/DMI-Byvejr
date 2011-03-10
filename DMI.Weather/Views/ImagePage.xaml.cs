//
// ImagePage.xaml.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System.Windows.Navigation;
using Microsoft.Phone.Controls;

namespace DMI.Views
{
    using ViewModels;

    public partial class ImagePage : PhoneApplicationPage
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:ImagePage"/> class.
        /// </summary>
        public ImagePage()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            string imageSource = "";
            if (NavigationContext.QueryString.TryGetValue("ImageSource", out imageSource))
            {
                (DataContext as ImageViewModel).LoadImage(imageSource);
            }
        }
    }
}