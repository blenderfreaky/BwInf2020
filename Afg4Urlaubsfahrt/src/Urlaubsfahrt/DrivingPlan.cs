using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Urlaubsfahrt
{
    public readonly struct DrivingPlan : IEquatable<DrivingPlan>
    {
        public readonly List<(GasStation Station, decimal Distance)> Stops;

        public static DrivingPlan Empty => new DrivingPlan(new List<(GasStation Station, decimal Distance)>());

        public DrivingPlan(List<(GasStation Station, decimal Distance)> stops) => Stops = stops;

        public readonly decimal PriceFor(Car car) => Stops.Sum(x => x.Distance * car.GetPriceForDistanceAt(x.Station));

        public readonly void Add(GasStation station, decimal distance) => Stops.Add((station, distance));

        public readonly void Sort() => Stops.Sort((x, y) => x.Station.Position.CompareTo(y.Station.Position));

        public override bool Equals(object? obj) => obj is DrivingPlan plan && Equals(plan);

        public bool Equals([AllowNull] DrivingPlan other) => Stops.SequenceEqual(other.Stops);

        public override int GetHashCode() => HashCode.Combine(Stops);

        public static bool operator ==(DrivingPlan left, DrivingPlan right) => left.Equals(right);

        public static bool operator !=(DrivingPlan left, DrivingPlan right) => !(left == right);

        public override readonly string ToString() => $"Track ({Stops.Count}) {{ {string.Join(", ", Stops)} }}";

        public readonly string ToString(Car car) =>
            "  Drive for " + Stops[0].Distance + "m on the starting fuel"
            + Environment.NewLine
            + string.Join(Environment.NewLine, Stops
                .Skip(1) // Skip the home "station"
                .Select(x => $"  Tank {x.Distance * car.FuelUsage}l at {x.Station}. (Cost: {car.GetPriceForDistanceAt(x.Station) * x.Distance}EUR)"));
    }
}