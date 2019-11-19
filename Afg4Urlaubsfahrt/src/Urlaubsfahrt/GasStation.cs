using System;
using System.Diagnostics.CodeAnalysis;

namespace Urlaubsfahrt
{
    public readonly struct GasStation : IEquatable<GasStation>
    {
        public static readonly GasStation Home = new GasStation(0, 0);

        public readonly double Price;
        public readonly double Position;

        public GasStation(double pricePerVolumeInEuroPerLiter, double position)
        {
            Price = pricePerVolumeInEuroPerLiter;
            Position = position;
        }

        public override bool Equals(object? obj) => obj is GasStation station && Equals(station);
        public bool Equals([AllowNull] GasStation other) => Price == other.Price && Position == other.Position;

        public override int GetHashCode() => HashCode.Combine(Price, Position);

        public static bool operator ==(GasStation left, GasStation right) => left.Equals(right);

        public static bool operator !=(GasStation left, GasStation right) => !(left == right);
    }
}