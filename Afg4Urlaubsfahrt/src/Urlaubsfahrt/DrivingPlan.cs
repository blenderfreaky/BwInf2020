using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace Urlaubsfahrt
{
    public readonly struct DrivingPlan : IEquatable<DrivingPlan>
    {
        public readonly List<(GasStation Station, double Distance)> Stops;

        public static DrivingPlan Empty => new DrivingPlan(new List<(GasStation Station, double Distance)>());

        public DrivingPlan(List<(GasStation Station, double Distance)> stops) => Stops = stops;

        public readonly double PriceFor(Car car) => Stops.Sum(x => x.Distance * car.GetPriceForDistanceAt(x.Station));

        public readonly void Add(GasStation station, double distance) => Stops.Add((station, distance));

        public readonly void Sort() => Stops.Sort((x, y) => x.Station.Position.CompareTo(y.Station.Position));

        public override bool Equals(object? obj) => obj is DrivingPlan plan && Equals(plan);

        public bool Equals([AllowNull] DrivingPlan other) => Stops.SequenceEqual(other.Stops);

        public override int GetHashCode() => HashCode.Combine(Stops);

        public static bool operator ==(DrivingPlan left, DrivingPlan right) => left.Equals(right);

        public static bool operator !=(DrivingPlan left, DrivingPlan right) => !(left == right);
    }
}