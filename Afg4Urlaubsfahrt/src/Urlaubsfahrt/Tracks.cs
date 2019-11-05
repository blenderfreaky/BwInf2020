using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System;
namespace Urlaubsfahrt
{
    using System.Collections.Generic;
    using System.Linq;

    public class Track
    {
        public static float FuelLength {get; set;}
        public static float TrackLength {get; set;}

        public List<GasStation> Stops { get; }

        public static Track EmptyTrack => new Track();

        public float GetPriceTo(float pos)
        {
            List<Tuple<GasStation, float>> Wai = new List<Tuple<GasStation, float>>();
            List<GasStation>allStations = Stops.OrderBy(x => x.PricePerVolumeInEuroPerLiter).ToList();

            List<Tuple<float, float>> FullBois = new List<Tuple<float, float>>();
            foreach(GasStation s in allStations)
            {
                Tuple<Tuple<float, float>, int> UnderBorderTupleIndexTuple = null;
                try
                {
                    UnderBorderTupleIndexTuple = FullBois.IndexMinWhere(x => x.Item2 > s.Position);
                }
                catch
                {
                    List<float> posslos = new List<float>() { s.Position + FuelLength, pos };
                    Tuple<float, float> part = new Tuple<float, float>(
                            s.Position, 
                            posslos.Min());

                    FullBois.Add(part);
                     Wai.Add(new Tuple<GasStation, float>(s, part.Item2 - part.Item1));
                }
                Tuple<float, float> UnderBorderTuple = UnderBorderTupleIndexTuple.Item1;
                if(UnderBorderTuple.Item1 <= s.Position && UnderBorderTuple.Item2 >= s.Position + FuelLength)
                {
                    continue;
                }
                if(UnderBorderTuple.Item1 > s.Position)
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
                    Tuple<Tuple<float, float>, int> UpperBorderTupleIndexTuple = null;
                    try
                    {
                        UpperBorderTupleIndexTuple = 
                            FullBois.IndexMinWhere(x => x.Item1 > UnderBorderTuple.Item2);
                    }
                    catch
                    {
                        goto Next;
                    }

                    if (UpperBorderTupleIndexTuple.Item1.Item1 <= s.Position + FuelLength)
                    {
                        FullBois.RemoveAt(UpperBorderTupleIndexTuple.Item2);
                        FullBois.Add(
                            new Tuple<float, float>(
                                UnderBorderTuple.Item1,
                                UnderBorderTupleIndexTuple.Item1.Item2));
                        Wai.Add(new Tuple<GasStation, float>(s, UpperBorderTupleIndexTuple.Item1.Item1 - UnderBorderTuple.Item2));
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
            } 
            return Wai.Select(x => x.Item1.PricePerVolumeInEuroPerLiter * x.Item2).Sum();
        }

        public float GetPriceTo(GasStation s)
        {
            return GetPriceTo(s.Position);
        }

        public Track(GasStation s) => Stops = new List<GasStation> { s };

        public Track(Track t, GasStation s)
        {
            Stops.AddRange(t.Stops);
            Stops.Add(s);
        }

        private Track() => Stops = new List<GasStation>();
    }
}