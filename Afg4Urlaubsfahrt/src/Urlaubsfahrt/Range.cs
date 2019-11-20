namespace Urlaubsfahrt
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public readonly struct Range : IEquatable<Range>
    {
        /// <summary>
        /// Represents not a range. This field is constant.
        /// </summary>
        public static readonly Range NaR = new Range(double.NaN, double.NaN);

        /// <summary>
        /// The inclusive starting point.
        /// </summary>
        public readonly double Start;

        /// <summary>
        /// The inclusive ending point.
        /// </summary>
        public readonly double End;

        /// <summary>
        /// Gets the length.
        /// </summary>
        public readonly double Length => End - Start;

        public Range(double start, double end)
        {
            Start = start;
            End = end;
        }

        /// <summary>
        /// Checks whether a given <see cref="double"/> is inside the range. <see cref="Start"/> and <see cref="End"/> are inclusive.
        /// </summary>
        /// <param name="position">The position to check containment for.</param>
        /// <returns>Whether <paramref name="position"/> is contained by the range.</returns>
        public readonly bool Contains(double position) =>
            position >= Start && position <= End;

        public override readonly bool Equals(object? obj) => obj is Range range && Equals(range);

        public readonly bool Equals([AllowNull] Range other) =>
            Start == other.Start && End == other.End;

        public static bool IsNaR(Range range) => double.IsNaN(range.Start) && double.IsNaN(range.End);

        public override readonly int GetHashCode() => HashCode.Combine(Start, End);

        public static bool operator ==(in Range left, in Range right) => left.Equals(right);

        public static bool operator !=(in Range left, in Range right) => !(left == right);

        public override readonly string ToString() => $"[{Start}..{End}]";
    }
}