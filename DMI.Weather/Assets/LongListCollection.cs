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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Collections;

namespace DMI.Assets
{
    public class LongListCollection<T, TKey> : ObservableCollection<LongListItem<T, TKey>>
        where T : IComparable<T>
    {
        public LongListCollection()
        {
        }

        public LongListCollection(IEnumerable<T> items, Func<T, TKey> keySelector)            
        {
            if (items == null)
                throw new ArgumentException("items");

            var groups = new Dictionary<TKey, LongListItem<T, TKey>>();

            foreach (var item in items.OrderBy(x => x))
            {
                var key = keySelector(item);

                if (groups.ContainsKey(key) == false)
                    groups.Add(key, new LongListItem<T, TKey>(key));

                groups[key].Add(item);
            }

            foreach (var value in groups.Values)
                this.Add(value);
        }
    }

    public class LongListItem<T, TKey> : ObservableCollection<T>
    {
        public LongListItem()
        {
        }

        public LongListItem(TKey key)
        {
            this.Key = key;
        }

        public TKey Key
        {
            get;
            set;
        }

        public bool HasItems
        {
            get
            {
                return Count > 0;
            }
        }
    }
}
