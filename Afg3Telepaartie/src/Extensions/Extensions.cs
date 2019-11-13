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
            HashSet<string> knownKeys = new HashSet<string>();
            foreach (TSource element in source)
            {
                if (knownKeys.Add(string.Join(',', keySelector(element))))
                {
                    yield return element;
                }
            }
        }

        public static IEnumerable<TSource> ExceptBy<TSource, TKey>
            (this IEnumerable<TSource> source, IEnumerable<TSource> except, Func<TSource, TKey> keySelector)
        {
            HashSet<string> knownKeys = new HashSet<string>();
            foreach (TSource element in except)
            {
                knownKeys.Add(string.Join(',',keySelector(element)));
            }
            foreach (TSource element in source)
            {
                if (!knownKeys.Contains(string.Join(',', keySelector(element))))
                {
                    yield return element;
                }
            }
        }
    }
}
