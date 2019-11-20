using System;
using System.Diagnostics.CodeAnalysis;

namespace Urlaubsfahrt
{
    public readonly struct GasStation : IEquatable<GasStation>
    {
        /// <summary>
        /// Represents the starting point, with a position of 0 and a price of 0.
        /// </summary>
        public static readonly GasStation Home = new GasStation(0, 0);

        /// <summary>
        /// The price of gas at the station in €/km. Based on the car
        /// </summary>
        public readonly double Price;
        public readonly double Position;

        public GasStation(double position, double price)
        {
            Price = price;
            Position = position;
        }

        public override readonly bool Equals(object? obj) => obj is GasStation station && Equals(station);

        public readonly bool Equals([AllowNull] GasStation other) => Price == other.Price && Position == other.Position;

        public override readonly int GetHashCode() => HashCode.Combine(Price, Position);

        public static bool operator ==(GasStation left, GasStation right) => left.Equals(right);

        public static bool operator !=(GasStation left, GasStation right) => !(left == right);

        public override readonly string ToString() => $"Position: {Position} Price: {Price}";
    }
}