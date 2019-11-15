namespace Urlaubsfahrt
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    public readonly struct Range : IEquatable<Range>
    {
        public readonly float Start, End;

        public Range(float start, float end)
        {
            Start = start;
            End = end;
        }

        public override bool Equals(object obj) => obj is Range range && Equals(range);

        public bool Equals([AllowNull] Range other) => Start == other.Start && End == other.End;

        public override int GetHashCode() => HashCode.Combine(Start, End);

        public static bool operator ==(Range left, Range right) => left.Equals(right);

        public static bool operator !=(Range left, Range right) => !(left == right);
    }
}