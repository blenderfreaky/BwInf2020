namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Track
    {
#warning Global state is really bad
        public static float FuelLength { get; set; } 
#warning Global state is really bad
        public static float StartFuelLength { get; set; }

        public List<GasStation> Stops { get; }

        public static Track EmptyTrack => new Track();

        public float? GetPriceTo(float pos)
        {
            List<Tuple<GasStation, float>> Wai = new List<Tuple<GasStation, float>>();
            List<GasStation> allStations = Stops.OrderBy(x => x.PricePerVolumeInEuroPerLiter).ToList();

            List<Tuple<float, float>> FullBois = new List<Tuple<float, float>>();
            List<float> possslos = new List<float> { pos, StartFuelLength };
            FullBois.Add(new Tuple<float, float>(0, possslos.Min()));
            foreach (GasStation s in allStations)
            {
                //TODO: If all covered => break
                (Tuple<float, float> Element, int Index)? temp0 = FullBois.IndexMinWhere(x => x.Item2 > s.Position);
                if (!temp0.HasValue)
                {
                    List<float> posslos = new List<float>() { s.Position + FuelLength, pos };
                    Tuple<float, float> part = new Tuple<float, float>(
                            s.Position,
                            posslos.Min());

                    FullBois.Add(part);
                    Wai.Add(new Tuple<GasStation, float>(s, part.Item2 - part.Item1));
                }
                (Tuple<float, float> Element, int Index) UnderBorderTupleIndexTuple
                    = temp0.Value;

                Tuple<float, float> UnderBorderTuple = UnderBorderTupleIndexTuple.Item1;
                if (UnderBorderTuple.Item1 <= s.Position && UnderBorderTuple.Item2 >= s.Position + FuelLength)
                {
                    continue;
                }
                if (UnderBorderTuple.Item1 > s.Position)
                {
                    FullBois.RemoveAt(UnderBorderTupleIndexTuple.Item2);

                    FullBois.Add(
                        new Tuple<float, float>(s.Position,
                        UnderBorderTuple.Item2));
                    Wai.Add(new Tuple<GasStation, float>(s, UnderBorderTuple.Item1 - s.Position));
                }
                else
                {
                    FullBois.RemoveAt(UnderBorderTupleIndexTuple.Item2);
                    (Tuple<float, float> Element, int Index)? temp1 = FullBois.IndexMinWhere(x => x.Item1 > UnderBorderTuple.Item2);
                    if (!temp1.HasValue) goto Next;
                    (Tuple<float, float> Element, int Index) UpperBorderTupleIndexTuple
                        = temp1.Value;

                    if (UpperBorderTupleIndexTuple.Element.Item1 <= s.Position + FuelLength)
                    {
                        FullBois.RemoveAt(UpperBorderTupleIndexTuple.Index);
                        FullBois.Add(
                            new Tuple<float, float>(
                                UnderBorderTuple.Item1,
                                UnderBorderTupleIndexTuple.Item1.Item2));
                        Wai.Add(new Tuple<GasStation, float>(s, UpperBorderTupleIndexTuple.Element.Item1 - UnderBorderTuple.Item2));
                    }
                    continue;
                    Next:

                    List<float> posslos = new List<float>() { s.Position + FuelLength, pos };
                    FullBois.Add(
                        new Tuple<float, float>(
                            UnderBorderTuple.Item1,
                            posslos.Min()));
                    Wai.Add(new Tuple<GasStation, float>(s, posslos.Min() - UnderBorderTuple.Item2));
                }
                if (FullBois[0].Item1 == 0 && FullBois[0].Item2 == pos)
                {
                    return Wai.Select(x => x.Item1.PricePerVolumeInEuroPerLiter * x.Item2).Sum();
                }
            }
            return null;
        }

        public float? GetPriceTo(GasStation s) => GetPriceTo(s.Position);

        public Track(GasStation s) => Stops = new List<GasStation> { s };

        public Track(Track t, GasStation s)
        {
            Stops.AddRange(t.Stops);
            Stops.Add(s);
        }

        private Track() => Stops = new List<GasStation>();
    }
}