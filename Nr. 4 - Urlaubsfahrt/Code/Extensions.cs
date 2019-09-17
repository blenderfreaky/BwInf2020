namespace Extensions
{
    public static class Extensions
    {
        IList<TSource> AllMins<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, int capacity = 1)
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
    }
}