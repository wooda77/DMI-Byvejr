﻿#region License
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

namespace System.Collections.Generic
{
    using System.Linq;

    public static class CollectionExtensions
    {
        public static TValue TryGetValue<TKey, TValue>(this Dictionary<TKey, TValue> source, TKey key)
        {
            TValue result = default(TValue);
            if (source.TryGetValue(key, out result))
                return result;
            else
                return default(TValue);
        }

        public static IEnumerable<T> FilterNull<T>(this IEnumerable<T> source)
        {
            return source.Where(x => x != null);
        }

        public static IEnumerable<T[]> Chunks<T>(this IEnumerable<T> self, int size)
        {
            var chunk = new T[size];

            int index = 0;

            foreach (var item in self)
            {
                chunk[index++] = item;

                if (index >= size)
                {
                    yield return chunk;

                    index = 0;
                    chunk = new T[size];
                }
            }
        }
    }
}

namespace System.Xml.Linq
{
    using System.Diagnostics.CodeAnalysis;

    public static class LinqToXmlExtensions
    {        
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0",
            Justification = "Result is dependant on the argument 'element' being null or not.")]
        public static string TryGetValue(this XElement element, string defaultValue = "")
        {
            if (element != null)
                return element.Value;
            else
                return defaultValue;
        }
    }
}