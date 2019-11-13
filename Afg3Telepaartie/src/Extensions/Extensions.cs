using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    public static class Extensions
    {
        public static IEnumerable<TSource> DistinctBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(keySelector(element)))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<TSource> ExceptBy<TSource, TKey>
            (this IEnumerable<TSource> source, IEnumerable<TSource> except, Func<TSource, TKey> keySelector)
        {
            HashSet<TKey> knownKeys = new HashSet<TKey>();
            foreach (TSource element in except)
            {
                knownKeys.Add(keySelector(element));
            }
            foreach (TSource element in source)
            {
                if (knownKeys.Contains(keySelector(element)))
                {
                    yield return element;
                }
            }
        }
    }
}
