namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public readonly struct Track : IEquatable<Track>
    {
        public static readonly Track Empty = new Track(ImmutableList<GasStation>.Empty.Add(GasStation.Home));

        public readonly ImmutableList<GasStation> Stops;

        public readonly GasStation LastStop => Stops[Stops.Count - 1];

        private Track(ImmutableList<GasStation> stops) => Stops = stops;

        public readonly Track With(GasStation newEnd) => new Track(Stops.Add(newEnd));

        public readonly double? GetCheapestPriceTo(GasStation destination, Car car) =>
            GetCheapestPriceTo(destination.Position, car);

        public readonly double? GetCheapestPriceTo(double destination, Car car) =>
            GetCheapestPathTo(destination, car)?.PriceFor(car);

        public readonly DrivingPlan? GetCheapestPathTo(double destination, Car car)
        {
            DrivingPlan drivingPlan = DrivingPlan.Empty;

            // If we can get to the destination on our tank already, we don't need to check for other options; it's already free.
            if (destination < car.StartingFuelDistance)
            {
                drivingPlan.Add(GasStation.Home, destination);
                return drivingPlan;
            }

            // Use up the entirety of the starting fuel
            drivingPlan.Add(GasStation.Home, car.StartingFuelDistance);

            HashSet<Range> coveredRanges = new HashSet<Range>
            {
                new Range(0, car.StartingFuelDistance),
                new Range(destination, destination + car.TankDistance)
            };

            foreach (GasStation station in Stops.Where(x => x.Price > 0).OrderBy(x => x.Price))
            {
                Range newRange = new Range(station.Position, station.Position + car.TankDistance);

                double distance = car.TankDistance;

                bool startHit = false, endHit = false;

                foreach (var coveredRange in coveredRanges.ToList())
                {
                    // Check whether the start and end poin                                                                                                                                                  t collide with the given range.
                    bool containsStart = coveredRange.Contains(newRange.Start);
                    bool containsEnd = coveredRange.Contains(newRange.End);

                    // If the new range is fully contained by a cheaper path we can stop.
                    if (containsStart && containsEnd)
                    {
                        newRange = Range.NaR;
                        break;
                    }

                    if (!containsStart && !containsEnd) continue;

                    coveredRanges.Remove(coveredRange);

                    if (containsStart)
                    {
                        if (startHit) throw new InvalidOperationException();
                        startHit = true;

                        distance -= coveredRange.End - newRange.Start;
                        newRange = new Range(coveredRange.Start, newRange.End);
                    }
                    else if (containsEnd)
                    {
                        if (endHit) throw new InvalidOperationException();
                        endHit = true;

                        distance -= newRange.End - coveredRange.Start;
                        newRange = new Range(newRange.Start, coveredRange.End);
                    }

                    if (newRange.Length == 0 || distance == 0)
                    {
                        newRange = Range.NaR;
                        break;
                    }
                }

                if (Range.IsNaR(newRange)) continue;

                coveredRanges.Add(newRange);
                drivingPlan.Add(station, distance);

                // If range spans the entire path, then the track is covered.
                if (newRange.Start == 0 && newRange.End >= destination)
                {
                    return drivingPlan;
                }
            }

            return null;
        }

        public override readonly bool Equals(object? obj) => obj is Track track && Equals(track);

        public readonly bool Equals([AllowNull] Track other) => EqualityComparer<ImmutableList<GasStation>>.Default.Equals(Stops, other.Stops);

        public override readonly int GetHashCode() => HashCode.Combine(Stops);

        public static bool operator ==(Track left, Track right) => left.Equals(right);

        public static bool operator !=(Track left, Track right) => !(left == right);

        public override readonly string ToString() => string.Join(Environment.NewLine, Stops.Select(x => "  " + x));
    }
}