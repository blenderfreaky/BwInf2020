namespace Nummernmerker
{
    using JM.LinqFaster;
    using System;
    using System.Collections.Generic;

    public static class Nummernmerker
    {
        private static readonly Dictionary<MerkedNummer, NummerMerkingSolution> MerkedNummers =
            new Dictionary<MerkedNummer, NummerMerkingSolution>();

        public static void ClearCache() => MerkedNummers.Clear();

        #region Overloads
        public static IEnumerable<string> MerkNummernToText(long number, int minSequenceLength, int maxSequenceLength) =>
            MerkNummernToText(number.ToString(), minSequenceLength, maxSequenceLength);

        public static IEnumerable<string> MerkNummernToText(string text, int minSequenceLength, int maxSequenceLength) =>
            MerkNummern(text, minSequenceLength, maxSequenceLength)
            .ApplyDistribution(text);

        public static NummerMerkingSolution MerkNummern(long number, int minSequenceLength, int maxSequenceLength) =>
            MerkNummern(number.ToString(), minSequenceLength, maxSequenceLength);

        public static NummerMerkingSolution MerkNummern(string text, int minSequenceLength, int maxSequenceLength) =>
            MerkNummern(new ArraySegment<bool>(text.ToCharArray().SelectF(x => x == '0')), minSequenceLength, maxSequenceLength);

        public static NummerMerkingSolution MerkNummern(in ArraySegment<bool> zeros, int minSequenceLength, int maxSequenceLength)
        {
            MerkedNummer merkedNummer = new MerkedNummer(zeros, minSequenceLength, maxSequenceLength);
            return MerkNummern(merkedNummer);
        }
        #endregion

        private static NummerMerkingSolution MerkNummern(in MerkedNummer merkedNummer)
        {
            // If the input has already been processed once, return previous result.
            if (MerkedNummers.TryGetValue(merkedNummer, out var optimalDistribution)) return optimalDistribution;

            // Not enough digits => Fail.
            if (merkedNummer.Zeros.Count < merkedNummer.MinSequenceLength)
            {
                return MerkedNummers[merkedNummer] = NummerMerkingSolution.Failure();
            }

            int nextGenerationSize =
                // Calculate the length of the longest segment possible.
                Math.Min(
                    // Either the number of digits left,
                    merkedNummer.Zeros.Count,
                    // or the max length of digits
                    merkedNummer.MaxSequenceLength)
                - merkedNummer.MinSequenceLength;

            if (nextGenerationSize <= 0)
            {
                return MerkedNummers[merkedNummer] = NummerMerkingSolution.Empty();
            }

            var nextGeneration = new NummerMerkingSolution[nextGenerationSize];

            for (int i = 0; i < nextGenerationSize; i++)
            {
                int length = i + merkedNummer.MinSequenceLength;

                var subSolution =
                    MerkNummern(
                        new ArraySegment<bool>(merkedNummer.Zeros.Array, merkedNummer.Zeros.Offset + length, merkedNummer.Zeros.Count - length),
                        merkedNummer.MinSequenceLength,
                        merkedNummer.MaxSequenceLength);

                nextGeneration[i] = !subSolution.IsSuccessful
                    ? NummerMerkingSolution.Failure()
                    : NummerMerkingSolution.Success(
                        subSolution.Distribution.PrecedeOne(length),
                        subSolution.LeadingZerosHit + (merkedNummer.Zeros.ElementAtUnchecked(0) ? 1 : 0));
            }

            var elements = nextGeneration.WhereF(x => x.IsSuccessful);

            if (elements.Length == 0)
            {
                return MerkedNummers[merkedNummer] = NummerMerkingSolution.Failure();
            }

            if (elements.Length == 1)
            {
                return MerkedNummers[merkedNummer] = elements[0];
            }

            NummerMerkingSolution bestSolution = elements.AggregateF((x, y) => x.LeadingZerosHit < y.LeadingZerosHit ? x : y);
            return MerkedNummers[merkedNummer] = bestSolution;
        }

        private static T[] PrecedeOne<T>(this T[] source, T newStart)
        {
            int length = source.Length;

            var target = new T[length + 1];

            //for (int i = 0; i < length; i++)
            //{
            //    target[i+1] = source[i];
            //}
            source.CopyTo(target, 1);

            target[0] = newStart;

            return target;
        }
    }
}
