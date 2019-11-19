namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Collections.Immutable;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    public readonly struct Track : IEquatable<Track>
    {
        public static readonly Track Empty = new Track(ImmutableList<GasStation>.Empty);

        public readonly ImmutableList<GasStation> Stops;

        public readonly GasStation LastStop => Stops[Stops.Count - 1];

        private Track(ImmutableList<GasStation> stops) => Stops = stops;

        public readonly Track With(GasStation newEnd) => new Track(Stops.Add(newEnd));

        public readonly double? GetCheapestPriceTo(GasStation destination, double startFuelLength, double tankLength) =>
            GetCheapestPriceTo(destination.Position, startFuelLength, tankLength);

        public readonly double? GetCheapestPriceTo(double destination, double startFuelLength, double tankLength) =>
            GetCheapestPathTo(destination, startFuelLength, tankLength)?.Price;

        public readonly DrivingPlan? GetCheapestPathTo(double destination, double startFuelLength, double tankLength)
        {
            DrivingPlan drivingPlan = new DrivingPlan();

            // If we can get to the destination on our tank already, we don't need to check for other options; it's already free.
            if (destination < startFuelLength)
            {
                drivingPlan.Add(GasStation.Home, destination);
                return drivingPlan;
            }

            // Use up the entirety of the starting fuel
            drivingPlan.Add(GasStation.Home, startFuelLength);

            HashSet<Range> coveredRanges = new HashSet<Range>
            {
                new Range(0, startFuelLength)
            };

            foreach (GasStation station in Stops.OrderBy(x => x.Price))
            {
                Range newRange = new Range(station.Position, station.Position + tankLength);

                foreach (var coveredRange in coveredRanges)
                {
                    // Check whether the start and end point collide with the given range.
                    bool containsStart = coveredRange.Contains(newRange.Start);
                    bool containsEnd = coveredRange.Contains(newRange.End);

                    // If the new range is fully contained by a cheaper path we can stop.
                    if (containsStart && containsEnd)
                    {
                        newRange = Range.NaR;
                        break;
                    }

                    coveredRanges.Remove(coveredRange);

                    if (containsStart)
                    {
                        newRange = new Range(coveredRange.Start, newRange.End);
                    }
                    else if (containsEnd)
                    {
                        newRange = new Range(newRange.Start, coveredRange.End);
                    }
                }

                if (newRange == Range.NaR) continue;

                coveredRanges.Add(newRange);
                drivingPlan.Add(station, newRange.Length);

                // If range spans the entire path, then the track is covered.
                if (newRange.Start == 0 && newRange.End == destination)
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
    }
}