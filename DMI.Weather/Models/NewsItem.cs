// 
// NewsItem.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System;

namespace DMI.Models
{
    public class NewsItem
    {
        public string Title
        {
            get;
            set;
        }

        public string Description
        {
            get;
            set;
        }

        public Uri Link
        {
            get;
            set;
        }
    }
}
