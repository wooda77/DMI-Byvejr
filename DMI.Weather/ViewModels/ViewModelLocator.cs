//
// ViewModelLocator.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//

namespace DMI.ViewModels
{
    public class ViewModelLocator
    {
        private static MainViewModel mainViewModel;

        public static MainViewModel MainViewModel
        {
            get
            {
                if (mainViewModel == null)
                {
                    mainViewModel = new MainViewModel();
                }

                return mainViewModel;
            }
        }

        private static ImageViewModel imageViewModel;

        public static ImageViewModel ImageViewModel
        {
            get
            {
                if (imageViewModel == null)
                {
                    imageViewModel = new ImageViewModel();
                }

                return imageViewModel;
            }
        }

        private static ChooseCityViewModel chooseCityViewModel;

        public static ChooseCityViewModel ChooseCityViewModel
        {
            get
            {
                if (chooseCityViewModel == null)
                {
                    chooseCityViewModel = new ChooseCityViewModel();
                }

                return chooseCityViewModel;
            }
        }

        private static SupportViewModel supportViewModel;

        public static SupportViewModel SupportViewModel
        {
            get
            {
                if (supportViewModel == null)
                {
                    supportViewModel = new SupportViewModel();
                }

                return supportViewModel;
            }
        }
    }
}
