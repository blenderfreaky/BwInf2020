namespace Nummernmerker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal struct MerkedNummer : IEquatable<MerkedNummer>
    {
        public bool[] Zeros;
        public int MinSequenceLength;
        public int MaxSequenceLength;

        public MerkedNummer(bool[] zeros, int minSequenceLength, int maxSequenceLength)
        {
            Zeros = zeros;
            MinSequenceLength = minSequenceLength;
            MaxSequenceLength = maxSequenceLength;
        }

        public override bool Equals(object obj) => obj is MerkedNummer other && Equals(other);
        public bool Equals(MerkedNummer other) =>
            Zeros.SequenceEqual(other.Zeros)
            && MinSequenceLength == other.MinSequenceLength
            && MaxSequenceLength == other.MaxSequenceLength;

        public override int GetHashCode()
        {
            var hashCode = 1685608418;
            hashCode = (hashCode * -1521134295) + EqualityComparer<bool[]>.Default.GetHashCode(Zeros);
            hashCode = (hashCode * -1521134295) + MinSequenceLength.GetHashCode();
            hashCode = (hashCode * -1521134295) + MaxSequenceLength.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(MerkedNummer left, MerkedNummer right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(MerkedNummer left, MerkedNummer right)
        {
            return !(left == right);
        }
    }
}
