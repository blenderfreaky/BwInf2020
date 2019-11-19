namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public sealed class Track
    {
        public static Track Empty => new Track();

        public List<GasStation> Stops { get; }

        private Track() => Stops = new List<GasStation>();

        private Track(Track t, GasStation s)
        {
            Stops.AddRange(t.Stops);
            Stops.Add(s);
        }

        public Track With(GasStation newEnd) => new Track(this, newEnd);

        public double? GetCheapestPriceTo(GasStation destination) => GetCheapestPriceTo(destination.Position);

        public double? GetCheapestPriceTo(double destination) =>
            GetCheapestPathTo(destination).Sum(x => x.Distance * x.Station?.Price);

        public List<(GasStation Station, double Distance)> GetCheapestPathTo(double destination)
        {
            List<(GasStation Station, double Distance)> finalPath = new List<(GasStation Station, double Distance)>();

            // If we can get to the destination on our tank already, we don't need to check for other options; it's already free.
            if (destination < Urlaubsfahrt.StartFuelLength)
            {
                finalPath.Add((null, Urlaubsfahrt.StartFuelLength));
                return finalPath;
            }

            finalPath.Add((null, Urlaubsfahrt.StartFuelLength));

            HashSet<Range> coveredRanges = new HashSet<Range>
            {
                new Range(0, Urlaubsfahrt.StartFuelLength)
            };

            foreach (GasStation station in Stops.OrderBy(x => x.Price))
            {
                Range newRange = new Range(station.Position, station.Position + Urlaubsfahrt.TankDistance);

                foreach (var coveredRange in coveredRanges)
                {
                    // Check whether the start and end point collide with the given range
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

                finalPath.Add((station, newRange.Length));

                if (newRange.Start == 0 && newRange.End == destination) return finalPath;
            }

            return null;
        }
    }
}