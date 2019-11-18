namespace Rominos
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Represents a 2-dimensional vector of <see cref="int"/>
    /// </summary>
    [DebuggerStepThrough]
    [DebuggerDisplay("X:{X}, Y:{Y}")]
    public readonly struct Vector2Int : IEquatable<Vector2Int>
    {
        /// <summary>
        /// Represents the X-component of the <see cref="Vector2Int"/>.
        /// </summary>
        public readonly int X;

        /// <summary>
        /// Represents the Y-component of the <see cref="Vector2Int"/>.
        /// </summary>
        public readonly int Y;

        /// <summary>
        /// Initializes a new instance of the <see cref="Vector2Int"/> structure.
        /// </summary>
        /// <param name="x">The X-component of the <see cref="Vector2Int"/>.</param>
        /// <param name="y">The Y-component of the <see cref="Vector2Int"/>.</param>
        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <inheritdoc/>
        public readonly bool Equals(Vector2Int vector) => X == vector.X && Y == vector.Y;

        /// <inheritdoc/>
        public override readonly bool Equals(object obj) => obj is Vector2Int vector && Equals(vector);

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = (hashCode * -1521134295) + X.GetHashCode();
            hashCode = (hashCode * -1521134295) + Y.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public static bool operator ==(Vector2Int left, Vector2Int right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(Vector2Int left, Vector2Int right) => !(left == right);

        /// <inheritdoc/>
        public static Vector2Int operator +(Vector2Int lhs, Vector2Int rhs)
            => new Vector2Int(lhs.X + rhs.X, lhs.Y + rhs.Y);

        /// <inheritdoc/>
        public static Vector2Int operator -(Vector2Int lhs, Vector2Int rhs)
            => new Vector2Int(lhs.X - rhs.X, lhs.Y - rhs.Y);

        /// <inheritdoc/>
        public static Vector2Int operator -(Vector2Int vec)
            => new Vector2Int(-vec.X, -vec.Y);

        /// <inheritdoc/>
        public override readonly string ToString() => $"X:{X}, Y:{Y}";
    }
}