using System;
using System.Runtime.InteropServices;

namespace Rominos
{
    [StructLayout(LayoutKind.Auto, Pack = 1)]
    public readonly struct Vector2Int : IEquatable<Vector2Int>
    {
        public readonly int X;
        public readonly int Y;

        public Vector2Int(int x, int y)
        {
            X = x;
            Y = y;
        }

        public readonly bool Equals(Vector2Int vector) => X == vector.X && Y == vector.Y;
        public override readonly bool Equals(object obj) => obj is Vector2Int vector && Equals(vector);

        public override readonly int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = (hashCode * -1521134295) + X.GetHashCode();
            hashCode = (hashCode * -1521134295) + Y.GetHashCode();
            return hashCode;
        }

        public static bool operator ==(Vector2Int left, Vector2Int right) => left.Equals(right);

        public static bool operator !=(Vector2Int left, Vector2Int right) => !(left == right);

        public static Vector2Int operator +(Vector2Int lhs, Vector2Int rhs)
            => new Vector2Int(lhs.X + rhs.X, lhs.Y + rhs.Y);

        public static Vector2Int operator -(Vector2Int lhs, Vector2Int rhs)
            => new Vector2Int(lhs.X - rhs.X, lhs.Y - rhs.Y);

        public static Vector2Int operator -(Vector2Int vec)
            => new Vector2Int(-vec.X, -vec.Y);

        public override readonly string ToString() => $"X:{X}, Y:{Y}";
    }
}
