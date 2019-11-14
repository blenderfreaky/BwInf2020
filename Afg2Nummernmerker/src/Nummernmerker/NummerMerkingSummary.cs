namespace Nummernmerker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a solution to a Nummermerking problem.
    /// </summary>
    public readonly struct NummerMerkingSolution : IEquatable<NummerMerkingSolution>
    {
        /// <summary>
        /// The final calculated distribution.
        /// </summary>
        /// <remarks>
        /// Lists the lengths of the segments in the distribution.
        /// <code>
        /// Distribution = new int[] { 2, 4, 3 };
        /// 
        /// ApplyDistribution("123456789")
        /// > 12 3456 789
        /// </code>
        /// </remarks>
        public readonly int[] Distribution;

        /// <summary>
        /// The amount of leading zeros that the distribution hits.
        /// </summary>
        /// <remarks>
        /// <code>
        /// Distribution = new int[] { 2, 4, 3 };
        /// 
        /// ApplyDistribution("12023103")
        /// > 12 0231 03
        /// 
        /// LeadingZerosHit
        /// > 2
        /// </code>
        /// </remarks>
        public readonly int LeadingZerosHit;

        /// <summary>
        /// Whether the Nummermerking problem could be solved.
        /// </summary>
        public readonly bool IsSuccessful;

        /// <summary>
        /// Initializes a new instance of the <see cref="NummerMerkingSolution"/> structure with the given values.
        /// </summary>
        /// <param name="distribution">The final calculated distribution.</param>
        /// <param name="leadingZerosHit">The amount of leading zeros that the distribution hits.</param>
        /// <param name="isSuccessful">Whether the Nummermerking problem could be solved.</param>
        private NummerMerkingSolution(int[] distribution, int leadingZerosHit, bool isSuccessful
            )
        {
            Distribution = distribution;
            LeadingZerosHit = leadingZerosHit;
            IsSuccessful = isSuccessful;
        }

        public static NummerMerkingSolution Success(int[] distribution, int leadingZerosHit) => new NummerMerkingSolution(distribution, leadingZerosHit, true);

        private static readonly NummerMerkingSolution _failure = new NummerMerkingSolution(default, default, false);
        public static NummerMerkingSolution Failure() => _failure;

        private static readonly NummerMerkingSolution _empty = new NummerMerkingSolution(Array.Empty<int>(), 0, true);
        public static NummerMerkingSolution Empty() => _empty;

        /// <summary>
        /// Applies the distribution to the given text.
        /// </summary>
        /// <param name="text">The text to split up.</param>
        /// <returns>An <see cref="IEnumerable{string}"/> yielding the segments of the distribution sequentially.</returns>
        /// <remarks>
        /// <code>
        /// Distribution = new int[] { 2, 4, 3 };
        /// 
        /// ApplyDistribution("123456789").ToArray();
        /// > string[] { "12", "3456", "789" }
        /// </code>
        /// </remarks>
        public readonly IEnumerable<string> ApplyDistribution(string text)
        {
            int position = 0;

            for (int i = 0; i < Distribution.Length; i++)
            {
                yield return text.Substring(position, Distribution[i]);
                position += Distribution[i];
            }
        }

        #region Overrides and Interface Implementations
        /// <inheritdoc/>
        public readonly override bool Equals(object obj) => obj is NummerMerkingSolution other && Equals(other);

        /// <inheritdoc/>
        public readonly bool Equals(NummerMerkingSolution other) =>
            Distribution.SequenceEqual(other.Distribution)
            && LeadingZerosHit == other.LeadingZerosHit;

        /// <inheritdoc/>
        public readonly override int GetHashCode()
        {
            var hashCode = -1828418316;
            hashCode = (hashCode * -1521134295) + EqualityComparer<int[]>.Default.GetHashCode(Distribution);
            hashCode = (hashCode * -1521134295) + LeadingZerosHit.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(NummerMerkingSolution left, NummerMerkingSolution right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(NummerMerkingSolution left, NummerMerkingSolution right) => !(left == right);
        #endregion
    }
}
