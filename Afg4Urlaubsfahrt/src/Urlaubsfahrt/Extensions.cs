namespace Urlaubsfahrt.Extensions
{
    using System;
    using System.Collections.Generic;

    public static class Extensions
    {
        public static IList<TSource> AllMins<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, int capacity = 1)
            where TKey : IComparable<TKey>
        {
            var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext()) throw new ArgumentException(nameof(source) + " cant be emty");

            TSource firstElement = enumerator.Current;
            TKey min = selector(firstElement);
            List<TSource> minima = new List<TSource>(capacity) { firstElement };

            while (enumerator.MoveNext())
            {
                TSource element = enumerator.Current;
                TKey key = selector(element);

                if (key.CompareTo(min) < 0)
                {
                    min = key;
                    minima.Clear();
                }

                minima.Add(element);
            }
            return minima;
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector)
        {
            return source.MinBy(selector, null);
        }

        public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source,
            Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException("source");
            if (selector == null) throw new ArgumentNullException("selector");
            comparer = comparer ?? Comparer<TKey>.Default;

            using var sourceIterator = source.GetEnumerator();
            if (!sourceIterator.MoveNext())
            {
                throw new InvalidOperationException("Sequence contains no elements");
            }
            var min = sourceIterator.Current;
            var minKey = selector(min);
            while (sourceIterator.MoveNext())
            {
                var candidate = sourceIterator.Current;
                var candidateProjected = selector(candidate);
                if (comparer.Compare(candidateProjected, minKey) < 0)
                {
                    min = candidate;
                    minKey = candidateProjected;
                }
            }
            return min;
        }
    }
}