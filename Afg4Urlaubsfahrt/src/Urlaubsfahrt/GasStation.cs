namespace Urlaubsfahrt
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    public readonly struct GasStation : IEquatable<GasStation>
    {
        /// <summary>
        /// Der Startpunkt mit Position und Preis gleich 0.
        /// </summary>
        public static readonly GasStation Home = new GasStation(0, 0);

        /// <summary>
        /// Der Preis in Euro pro Liter.
        /// </summary>
        public readonly decimal Price;

        /// <summary>
        /// Die Position der Tankstelle
        /// </summary>
        public readonly decimal Position;

        public GasStation(decimal position, decimal price)
        {
            Price = price;
            Position = position;
        }

        public override readonly bool Equals(object? obj) => obj is GasStation station && Equals(station);

        public readonly bool Equals([AllowNull] GasStation other) => Price == other.Price && Position == other.Position;

        public override readonly int GetHashCode() => HashCode.Combine(Price, Position);

        public static bool operator ==(GasStation left, GasStation right) => left.Equals(right);

        public static bool operator !=(GasStation left, GasStation right) => !(left == right);

        public override readonly string ToString() => $"Gas station({Position}m {Price}EUR/l)";
    }
}