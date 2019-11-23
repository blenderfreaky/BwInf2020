namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;

    public static class IEnumerableExtensions
    {
        public static IEnumerable<TElem> AllMinsBy<TElem, TKey>(this IEnumerable<TElem> enumerable, Func<TElem, TKey> keySelector)
        {
            var enumerator = enumerable.GetEnumerator();

            if (!enumerator.MoveNext()) return Array.Empty<TElem>();

            TElem first = enumerator.Current;
            var mins = new List<TElem> { first };
            TKey min = keySelector(first);

            while (enumerator.MoveNext())
            {
                var current = enumerator.Current;
                var currentKey = keySelector(current);

                int comparisonResult = Comparer<TKey>.Default.Compare(currentKey, min);

                if (comparisonResult > 0) continue;

                if (comparisonResult < 0)
                {
                    mins.Clear();
                    min = currentKey;
                }

                mins.Add(current);
            }

            return mins;
        }
    }
}