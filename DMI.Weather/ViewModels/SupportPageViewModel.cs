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
using System;
using System.IO.IsolatedStorage;
using System.Windows;
using System.Windows.Input;
using System.Xml.Linq;
using DMI.Common;
using DMI.Properties;
using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using Microsoft.Phone.Tasks;

namespace DMI.ViewModels
{
    public class SupportPageViewModel : ViewModelBase
    {
        public SupportPageViewModel()
        {
            AppSettings.IsFirstStart = false;
            
            this.SendEmail = new RelayCommand(() =>
            {
                var emailTask = new EmailComposeTask()
                {
                    To = Properties.Resources.Email,
                    Subject = Properties.Resources.AppSupportEmailHeader
                };

                emailTask.Show();
            });
        }

        public string Version
        {
            get
            {
                return XDocument.Load("WMAppManifest.xml").Root.Element("App").Attribute("Version").Value;
            }
        }

        public bool AllowGPS
        {
            get
            {
                return AppSettings.IsGPSEnabled;
            }
            set
            {
                AppSettings.IsGPSEnabled = value;
            }
        }

        public ICommand SendEmail
        {
            get;
            private set;
        }
    }
}
