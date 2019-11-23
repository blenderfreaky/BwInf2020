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

        public readonly decimal? GetCheapestPriceTo(GasStation destination, Car car) =>
            GetCheapestPriceTo(destination.Position, car);

        public readonly decimal? GetCheapestPriceTo(decimal destination, Car car) =>
            GetCheapestPathTo(destination, car)?.PriceFor(car);

        public readonly DrivingPlan? GetCheapestPathTo(decimal destination, Car car)
        {
            DrivingPlan drivingPlan = DrivingPlan.Empty;

            //Wenn wir mit dem vorhandenen Tank bis zum Ziel fahren können, müssen wir nicht tanken
            if (destination < car.StartingFuelDistance)
            {
                drivingPlan.Add(GasStation.Home, destination);
                return drivingPlan;
            }

            //Das gesamte Startbenzin nutzen
            drivingPlan.Add(GasStation.Home, car.StartingFuelDistance);

            HashSet<Range> coveredRanges = new HashSet<Range>
            {
                new Range(0, car.StartingFuelDistance),
                new Range(destination, destination + car.TankDistance)
            };

            foreach (GasStation station in Stops.Where(x => x.Price > 0).OrderBy(x => x.Price)) //Die Tankstellen dem Preis nach überprüfen
            {
                Range newRange = new Range(station.Position, station.Position + car.TankDistance);

                decimal distance = car.TankDistance;

                bool startHit = false, endHit = false;

                foreach (var coveredRange in coveredRanges.ToList())
                {
                    //Überprüfen ob der Start- oder Endpunkt bereits überdeckt sind                                                                                                                                                t collide with the given range.
                    bool containsStart = coveredRange.Contains(newRange.Start);
                    bool containsEnd = coveredRange.Contains(newRange.End);

                    //Wenn der gesamte Bereich überdeckt ist muss nicht getankt werden
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

                //Wenn nur eine Range existiert, die den gesamten Bereich abdeckt, dann wird an den übrigen Stationen nicht getankt
                if (newRange.Start == 0 && newRange.End >= destination)
                {
                    return drivingPlan;
                }
            }

            return null;    //Wenn der Weg nicht vollständig Abgedeckt ist, ist dieser Track nicht möglich
        }

        public override readonly bool Equals(object? obj) => obj is Track track && Equals(track);

        public readonly bool Equals([AllowNull] Track other) => EqualityComparer<ImmutableList<GasStation>>.Default.Equals(Stops, other.Stops);

        public override readonly int GetHashCode() => HashCode.Combine(Stops);

        public static bool operator ==(Track left, Track right) => left.Equals(right);

        public static bool operator !=(Track left, Track right) => !(left == right);

        public override readonly string ToString() => string.Join(Environment.NewLine, Stops.Select(x => "  " + x));
    }
}