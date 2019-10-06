using System.Security.Cryptography.X509Certificates;
using System.Runtime.CompilerServices;
using System;
namespace Urlaubsfahrt
{
    using System.Collections.Generic;
    using System.Linq;

    public class Track
    {
        public List<GasStation> Stops { get; }

        public static Track EmptyTrack => new Track();

        public float GetPrice()
        {
            float value = 0;
            #if false
            for (int i = 0; i < Stops.Count - 1; i++)
            {
                value += (Stops[i + 1].Position - Stops[i].Position) * Stops[i].PricePerVolumeInEuroPerLiter;
            }
            #else
            List<Tuple<float, float>> Covered = new List<Tuple<float,float>>();
            List<GasStation> Sorted = Stops.Select(x => x).ToList();
            Sorted.Sort((x, y) => x.PricePerVolumeInEuroPerLiter.CompareTo(y.PricePerVolumeInEuroPerLiter));
            foreach(GasStation s in Sorted)
            {
                //if s.Position in Coverd: continue
                //float underborder
                //float upperborder
                //underborder = Coverd.First( x => x > s.pos).Min(x => x[1])
            }
            #endif
            return value;
        }

        public float GetPriceTo(float pos)
        {
            float value = GetPrice();
            value += (pos - Stops.Last().Position) * Stops.Last().PricePerVolumeInEuroPerLiter;
            return value;
        }

        public float GetPriceTo(GasStation s)
        {
            float value = GetPrice();
            value += GetPriceTo(s.Position);
            return value;
        }

        public Track(GasStation s) => Stops = new List<GasStation> { s };

        public Track(Track t, GasStation s) : this()
        {
            //Stops.AddAll(t.Stops); //TODO try this
            foreach (GasStation ss in t.Stops) Stops.Add(ss);
            Stops.Add(s);
        }

        private Track() => Stops = new List<GasStation>();
    }
}