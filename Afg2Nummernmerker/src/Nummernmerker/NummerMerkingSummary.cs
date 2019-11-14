namespace Nummernmerker
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Represents a solution to a Nummermerking problem.
    /// </summary>
    public readonly struct NummerMerkingSummary : IEquatable<NummerMerkingSummary>
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
        /// Initializes a new instance of the <see cref="NummerMerkingSummary"/> structure with the given values.
        /// </summary>
        /// <param name="distribution">The final calculated distribution.</param>
        /// <param name="leadingZerosHit">The amount of leading zeros that the distribution hits.</param>
        public NummerMerkingSummary(int[] distribution, int leadingZerosHit)
        {
            Distribution = distribution;
            LeadingZerosHit = leadingZerosHit;
        }

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
        public readonly override bool Equals(object obj) => obj is NummerMerkingSummary other && Equals(other);

        /// <inheritdoc/>
        public readonly bool Equals(NummerMerkingSummary other) =>
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
        public static bool operator ==(NummerMerkingSummary left, NummerMerkingSummary right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(NummerMerkingSummary left, NummerMerkingSummary right) => !(left == right);
        #endregion
    }
}
