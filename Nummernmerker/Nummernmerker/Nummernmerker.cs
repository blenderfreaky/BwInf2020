namespace Nummernmerker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Nummernmerker
    {
        public static byte[][] MerkNummern(byte[] digits, int minSequenceLength, int maxSequenceLength)
        {
            bool[] zeroes = digits.Select(x => x == 0).ToArray();
            var zeroCount = zeroes.SequenceCount().ToArray();
            int[] startingPosition = 
        }

        private static IEnumerable<(T Value, int Count, int StartingPosition)> SequenceCount<T>(this T[] values)
        {
            if (values.Length < 1) throw new ArgumentException("Sequence contains no elements", nameof(values));

            return SequenceCountInternal();

            IEnumerable<(T Value, int Count, int StartingPosition)> SequenceCountInternal()
            {
                T currentValue = values[0];
                int currentCount = 1;
                int currentStartingPosition = 0;

                for (int i = 1; i < values.Length; i++)
                {
                    T value = values[i];
                    if (value.Equals(currentValue))
                    {
                        currentCount++;
                    }
                    else
                    {
                        yield return (Value: currentValue, Count: currentCount, StartingPosition: currentStartingPosition);
                        currentValue = value;
                        currentCount = 1;
                        currentStartingPosition = i;
                    }
                }

                yield return (Value: currentValue, Count: currentCount, StartingPosition: currentStartingPosition);
            }
        }
    }
}
