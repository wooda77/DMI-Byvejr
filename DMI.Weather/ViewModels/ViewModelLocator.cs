#region License
// Copyright (c) 2011 Claus Jørgensen <10229@iha.dk>
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
#endregion

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
