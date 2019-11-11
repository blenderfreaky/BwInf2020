namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    public static class ExtensionsCollection
    {
        public static IList<TSource> AllMins<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> selector, int capacity = 1)
            where TKey : IComparable<TKey>
        {
            var enumerator = source.GetEnumerator();

            if (!enumerator.MoveNext()) throw new InvalidOperationException("Sequence contains no elements");

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

        public static TSource MinBy<TSource, TKey>
            (this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (selector == null) throw new ArgumentNullException(nameof(selector));
            comparer ??= Comparer<TKey>.Default;

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

        [Obsolete("Use " + nameof(TryIndexMinWhere) + " instead")]
        public static (T Element, int Index)? IndexMinWhere<T>
            (this IEnumerable<T> value, Func<T, bool> check)
        {
            int index = -1;
            int minIndex = -1;
            T min = default;
            var enumerator = value.GetEnumerator();
            while (enumerator.MoveNext())
            {
                index++;
                if (check(enumerator.Current) && Comparer<T>.Default.Compare(min, enumerator.Current) > 0)
                {
                    minIndex = index;
                    min = enumerator.Current;
                }
            }
            if (minIndex == -1) return null;
            return (min, index);
        }

        public static bool TryIndexMinWhere<T> // TODO: Better name
            (this IEnumerable<T> enumerable, Predicate<T> filter, [NotNullWhen(true)]out T element, [NotNullWhen(true)]out int index)
        {
            int i = -1;
            int minIndex = -1;
            T min = default;

            foreach (var elem in enumerable)
            {
                i++;
                if (filter(elem) && Comparer<T>.Default.Compare(min, elem) > 0)
                {
                    minIndex = i;
                    min = elem;
                }
            }

            if (minIndex == -1)
            {
                element = default;
                index = default;
                return false;
            }

            element = min;
            index = i;
            return true;
        }
    }
}