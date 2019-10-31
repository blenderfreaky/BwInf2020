namespace Nummernmerker
{
    using MoreLinq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class Nummernmerker
    {
        private static readonly Dictionary<MerkedNummer, NummerMerkingSummary> MerkedNummers =
            new Dictionary<MerkedNummer, NummerMerkingSummary>();

        public static IEnumerable<string> MerkNummernToText(long number, int minSequenceLength, int maxSequenceLength) =>
            MerkNummernToText(number.ToString(), minSequenceLength, maxSequenceLength);

        public static IEnumerable<string> MerkNummernToText(string text, int minSequenceLength, int maxSequenceLength) =>
            MerkNummern(text.Select(x => x == '0').ToArray(), minSequenceLength, maxSequenceLength).Apply(text);

        public static NummerMerkingSummary MerkNummern(long number, int minSequenceLength, int maxSequenceLength) =>
            MerkNummern(number.ToString(), minSequenceLength, maxSequenceLength);

        public static NummerMerkingSummary MerkNummern(string text, int minSequenceLength, int maxSequenceLength) =>
            MerkNummern(text.Select(x => x == '0').ToArray(), minSequenceLength, maxSequenceLength);

        public static NummerMerkingSummary MerkNummern(bool[] zeros, int minSequenceLength, int maxSequenceLength) =>
            MerkNummern(new MerkedNummer(zeros, minSequenceLength, maxSequenceLength));

        private static NummerMerkingSummary MerkNummern(MerkedNummer merkedNummer)
        {
            if (MerkedNummers.TryGetValue(merkedNummer, out var optimalDistribution)) return optimalDistribution;

            int nextGenerationSize = Math.Min(merkedNummer.Zeros.Length, merkedNummer.MaxSequenceLength) - merkedNummer.MinSequenceLength + 1;

            if (merkedNummer.Zeros.Length < merkedNummer.MinSequenceLength)
            {
                return MerkedNummers[merkedNummer] = new NummerMerkingSummary(Array.Empty<int>(), int.MaxValue - 1);
            }

            if (nextGenerationSize == 0)
            {
                return MerkedNummers[merkedNummer] = new NummerMerkingSummary(Array.Empty<int>(), 0);
            }

            var nextGeneration = new NummerMerkingSummary[nextGenerationSize];

            for (int i = 0; i < nextGenerationSize; i++)
            {
                var summary =
                    MerkNummern(merkedNummer.Zeros.Skip(i + merkedNummer.MinSequenceLength).ToArray(), merkedNummer.MinSequenceLength, merkedNummer.MaxSequenceLength);
                nextGeneration[i] =
                    new NummerMerkingSummary(summary.Distribution.PrecedeOne(i + merkedNummer.MinSequenceLength), summary.LeadingZerosHit + (merkedNummer.Zeros[0] ? 1 : 0));
            }

            var elements = nextGeneration
                .Where(x => x.LeadingZerosHit < int.MaxValue - 1).ToList();

            if (elements.Count == 0)
            {
                return MerkedNummers[merkedNummer] = new NummerMerkingSummary(Array.Empty<int>(), int.MaxValue - 1);
            }

            return MerkedNummers[merkedNummer] = elements.MinBy(x => x.LeadingZerosHit).First();
        }

        private static T[] PrecedeOne<T>(this T[] source, T newStart)
        {
            int length = source.Length;

            var target = new T[length + 1];

            Array.Copy(source, 0, target, 1, length);

            target[0] = newStart;

            return target;
        }
    }
}
