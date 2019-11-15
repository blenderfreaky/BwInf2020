namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class Track
    {
        public List<GasStation> Stops { get; }

        public static Track Empty => new Track();

        public float? GetPriceTo(float pos)
        {
            List<(GasStation Station, float Distance)> path = new List<(GasStation Station, float Distance)>();
            List<GasStation> allStations = Stops.OrderBy(x => x.PricePerVolumeInEuroPerLiter).ToList();

            List<Range> fullBois = new List<Range>();
            List<float> possiblePaths = new List<float> { pos, Urlaubsfahrt.StartFuelLength };

            fullBois.Add(new Range(0, possiblePaths.Min()));

            foreach (GasStation s in allStations)
            {
                //TODO: If all covered => break
                (Range Element, int Index)? temp0 = fullBois.IndexMinWhere(x => x.End > s.Position);
                if (!temp0.HasValue)
                {
                    List<float> posslos = new List<float>() { s.Position + Urlaubsfahrt.FuelLength, pos };
                    Range part = new Range(
                            s.Position,
                            posslos.Min());

                    fullBois.Add(part);
                    path.Add((s, part.End - part.Start));
                }
#warning This will throw a null-ref exception if the !temp0.HasValue branch was visited, as it does not assign a new value
                (Range Element, int Index) underBorderIndexTuple
                    = temp0.Value;

                Range underBorder = underBorderIndexTuple.Element;
                if (underBorder.Start <= s.Position && underBorder.End >= s.Position + Urlaubsfahrt.FuelLength)
                {
                    continue;
                }
                if (underBorder.Start > s.Position)
                {
                    fullBois.RemoveAt(underBorderIndexTuple.Index);

                    fullBois.Add(
                        new Range(s.Position,
                        underBorder.End));
                    path.Add((s, underBorder.Start - s.Position));
                }
                else
                {
                    fullBois.RemoveAt(underBorderIndexTuple.Index);

                    (Range Element, int Index)? temp1 = fullBois.IndexMinWhere(x => x.Start > underBorder.End);

                    if (temp1.HasValue)
                    {
                        (Range Element, int Index) UpperBorderTupleIndexTuple
                            = temp1.Value;

                        if (UpperBorderTupleIndexTuple.Element.Start <= s.Position + Urlaubsfahrt.FuelLength)
                        {
                            fullBois.RemoveAt(UpperBorderTupleIndexTuple.Index);
                            fullBois.Add(
                                new Range(
                                    underBorder.Start,
                                    underBorderIndexTuple.Element.End));
                            path.Add((s, UpperBorderTupleIndexTuple.Element.Start - underBorder.End));
                        }
                        continue;
                    }

                    List<float> posslos = new List<float>() { s.Position + Urlaubsfahrt.FuelLength, pos };
                    fullBois.Add(
                        new Range(
                            underBorder.Start,
                            posslos.Min()));
                    path.Add((s, posslos.Min() - underBorder.End));
                }
                if (fullBois[0].Start == 0 && fullBois[0].End == pos)
                {
                    return path.Select(x => x.Station.PricePerVolumeInEuroPerLiter * x.Distance).Sum();
                }
            }
            return null;
        }

        public float? GetPriceTo(GasStation s) => GetPriceTo(s.Position);

        private Track() => Stops = new List<GasStation>();

        public Track(GasStation station) => Stops.Add(station);

        public Track Append(GasStation s) => new Track(this, s);

        private Track(Track t, GasStation s)
        {
            Stops.AddRange(t.Stops);
            Stops.Add(s);
        }
    }
}