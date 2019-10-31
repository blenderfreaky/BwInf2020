namespace Nummernmerker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public struct NummerMerkingSummary : IEquatable<NummerMerkingSummary>
    {
        public int[] Distribution;
        public int LeadingZerosHit;

        public NummerMerkingSummary(int[] distribution, int leadingZerosHit)
        {
            Distribution = distribution;
            LeadingZerosHit = leadingZerosHit;
        }

        public override bool Equals(object obj) => obj is NummerMerkingSummary other && Equals(other);

        public bool Equals(NummerMerkingSummary other) =>
            Distribution.SequenceEqual(other.Distribution)
            && LeadingZerosHit == other.LeadingZerosHit;

        public override int GetHashCode()
        {
            var hashCode = -1828418316;
            hashCode = (hashCode * -1521134295) + EqualityComparer<int[]>.Default.GetHashCode(Distribution);
            hashCode = (hashCode * -1521134295) + LeadingZerosHit.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(NummerMerkingSummary left, NummerMerkingSummary right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(NummerMerkingSummary left, NummerMerkingSummary right)
        {
            return !(left == right);
        }

        public IEnumerable<string> Apply(string text)
        {
            int position = 0;

            for (int i = 0; i < Distribution.Length; i++)
            {
                yield return text.Substring(position, Distribution[i]);
                position += Distribution[i];
            }
        }
    }
}
