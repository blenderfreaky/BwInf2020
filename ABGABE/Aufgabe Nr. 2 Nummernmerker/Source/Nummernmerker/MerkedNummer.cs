namespace Nummernmerker
{
    using System;
    using System.Linq;

    /// <summary>
    /// Represents the inputs to the Nummermerking problem, saving bools representing whether a digit is zero.
    /// </summary>
    internal readonly struct MerkedNummer : IEquatable<MerkedNummer>
    {
        /// <summary>
        /// The array indicating whether the n-th digit is zero. <langword>true</langword> for zero, <langword>false</langword> for not zero.
        /// </summary>
        public readonly ArraySegment<bool> Zeros;

        /// <summary>
        /// The lowest amount of digits that need to be in a segment of a distribution.
        /// </summary>
        public readonly int MinSequenceLength;

        /// <summary>
        /// The highest amount of digits that may be in a segment of a distribution.
        /// </summary>
        public readonly int MaxSequenceLength;

        /// <summary>
        /// Stores the hash code for fast access.
        /// </summary>
        private readonly int _hashCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="MerkedNummer"/> structure with the given values.
        /// </summary>
        /// <param name="zeros">The array indicating whether the n-th digit is zero. <langword>true</langword> for zero, <langword>false</langword> for not zero.</param>
        /// <param name="minSequenceLength">The lowest amount of digits that need to be in a segment of a distribution.</param>
        /// <param name="maxSequenceLength">The highest amount of digits that may be in a segment of a distribution.</param>
        public MerkedNummer(in ArraySegment<bool> zeros, int minSequenceLength, int maxSequenceLength)
        {
            Zeros = zeros;
            MinSequenceLength = minSequenceLength;
            MaxSequenceLength = maxSequenceLength;

            _hashCode = 1685608418;
            for (int i = 0; i < zeros.Count; i++)
            {
                _hashCode = (_hashCode * -1521134295) + zeros.ElementAtUnchecked(i).GetHashCode();
            }
            _hashCode = (_hashCode * -1521134295) + MinSequenceLength.GetHashCode();
            _hashCode = (_hashCode * -1521134295) + MaxSequenceLength.GetHashCode();
        }

        #region Overrides and Interface Implementations

        /// <inheritdoc/>
        public readonly override bool Equals(object obj) => obj is MerkedNummer other && Equals(other);

        /// <inheritdoc/>
        public readonly bool Equals(MerkedNummer other) =>
            Zeros.SequenceEqual(other.Zeros)
            && MinSequenceLength == other.MinSequenceLength
            && MaxSequenceLength == other.MaxSequenceLength;

        /// <inheritdoc/>
        public readonly override int GetHashCode() => _hashCode;

        /// <inheritdoc/>
        public static bool operator ==(MerkedNummer left, MerkedNummer right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(MerkedNummer left, MerkedNummer right) => !(left == right);

        #endregion Overrides and Interface Implementations
    }
}