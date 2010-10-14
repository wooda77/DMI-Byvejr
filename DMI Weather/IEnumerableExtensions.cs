//
// IEnumerableExtensions.cs.cs
//
// Authors:
//     Claus Jørgensen <10229@iha.dk>
//
using System.Collections.Generic;

namespace DMI
{
    public static class IEnumerableExtensions
    {
        public static IEnumerable<T[]> Chunks<T>(this IEnumerable<T> self, int size)
        {
            var chunk = new T[size];

            int index = 0;

            foreach (var item in self)
            {
                if (index == size)
                {
                    yield return chunk;
                    index = 0;
                    chunk = new T[size];
                }

                chunk[index++] = item;
            }
        }
    }
}
