namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Track
    {
        public static Track Empty => new Track();

        public List<GasStation> Stops { get; }

        private Track() => Stops = new List<GasStation>();

        public Track(GasStation station) => Stops.Add(station);

        private Track(Track t, GasStation s)
        {
            Stops.AddRange(t.Stops);
            Stops.Add(s);
        }

        public Track Append(GasStation s) => new Track(this, s);

        public float? GetPriceTo(GasStation s) => GetPriceTo(s.Position);

        public float? GetPriceTo(float pos)
        {
            List<(GasStation Station, float Distance)> path = new List<(GasStation Station, float Distance)>();
            List<GasStation> allStations = Stops.OrderBy(x => x.PricePerTank).ToList();

            // TODO: Don't use float, use GasStation references

            List<Range> fullBois = new List<Range>();
            List<float> possiblePaths = new List<float> { pos, Urlaubsfahrt.StartFuelLength };

            fullBois.Add(new Range(0, possiblePaths.Min()));

            foreach (GasStation s in allStations)
            {
                //TODO: If all covered => break
                if (!fullBois.TryIndexMinWhere(x => x.End > s.Position, out Range underBorder, out int underBorderIndex))
                {
                    Range part = new Range(
                            s.Position,
                            Math.Min(s.Position + Urlaubsfahrt.TankDistance, pos));

                    fullBois.Add(part);
                    path.Add((s, part.End - part.Start));

                    continue;
                }

                if (underBorder.Start <= s.Position && underBorder.End >= s.Position + Urlaubsfahrt.TankDistance)
                {
                    continue;
                }

                if (underBorder.Start > s.Position)
                {
                    fullBois.RemoveAt(underBorderIndex);

                    fullBois.Add(
                        new Range(s.Position,
                        underBorder.End));
                    path.Add((s, underBorder.Start - s.Position));
                }
                else
                {
                    fullBois.RemoveAt(underBorderIndex);

                    if (fullBois.TryIndexMinWhere(x => x.Start > underBorder.End, out Range upperBorder, out int upperBorderIndex))
                    {
                        if (upperBorder.Start <= s.Position + Urlaubsfahrt.TankDistance)
                        {
                            fullBois.RemoveAt(upperBorderIndex);
                            fullBois.Add(underBorder);
                            path.Add((s, upperBorder.Start - underBorder.End));
                        }

                        continue;
                    }

                    float end = Math.Min(s.Position + Urlaubsfahrt.TankDistance, pos);
                    fullBois.Add(
                        new Range(
                            underBorder.Start,
                            end));
                    path.Add((s, end - underBorder.End));
                }

                if (fullBois[0].Start == 0 && fullBois[0].End == pos)
                {
                    return path.Select(x => x.Station.PricePerTank * x.Distance).Sum();
                }
            }

            return null;
        }
    }
}