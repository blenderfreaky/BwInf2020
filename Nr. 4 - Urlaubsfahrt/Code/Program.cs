
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Linq;

namespace p
{
    public class program
    {
        int FuelSize;
        float Usage;
        int StartFuel;
        float Trip;
        List<GasStation> AllStations;

        List<Track> TrackParts;

        void Main()
        {
            int FuelLength = 10; //Will be removed later, but its faster to use it right now
            Console.WriteLine("Insert path:");
            string path = Console.ReadLine();
            string[] FileValues = File.ReadAllText(path).Split('\n');
            AllStations = new List<GasStation>();
            for(int i = 5; i < FileValues.Length; i++)
            {
                float[] values = FileValues[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmtys).Select(x => (float)x);
                AllStations.Add(new GasStation(values[0], values[1]));
            }
            for(int i = 0; i < AllStations.Count; i ++)
            {
                List<Track> PosssibleParts = TrackParts.Where(x => AllStations[i].Position - x.Stops.Last().Position > FuelLength);
                if(PosssibleParts.Count == 0 && FuelLength < AllStations[i].Position)
                {
                    throw new Exception();
                }
                Track t = PosssibleParts.M(x => x.Stops.Count);
                TrackParts.Add(new Track(PosssibleParts.AllMins(x => x.Stops.Count).Min(x => x.GetPrice())));
            }
        }
    }

    public class GasStation
    {
        public GasStation(float pricePerVolumeInEuroPerLiter, float position)
        {
            PricePerVolumeInEuroPerLiter = pricePerVolumeInEuroPerLiter;
            Position = position;
        }

        public float PricePerVolumeInEuroPerLiter {get;}
        public float Position {get;}
    }

    public class Track
    {
        public List<GasStation> Stops {get;}
        float GetPrice(int pos)
        {
            float value = 0;
            for(int i = 0; i < Stops.Count - 1; i++)
            {
                value += (Stops[i+1].Position - Stops[i].Position) * Stops[i].PricePerVolumeInEuroPerLiter;
            }
            value += (pos - Stops.Last().Position) * Stops.Last().PricePerVolumeInEuroPerLiter;
            return value;
        }
        public Track(GasStation s)
        {
            Stops = new List<GasStation> { s };
        }
        public Track (Track t, GasStation s)
        {
            Stops = new List<GasStation>();
            foreach (GasStation ss in t.Stops) Stops.Add(ss);
            Stops.Add(s);
        }
    }

}

namespace Extensions
{
    public static class Extensions
    {
        IEnumerable<TSource> AllMins<TSource, TKey> (this IEnumerable<TSource> Source, Func<TSource, TKey> Comparer)
        {
            if (!typeof(IComparable).IsAssignableFrom(TKey)) throw new ArgumentException(nameof(TKey) + " must implement IComparable");

            List<TSource> Values = new List<TSource>() { Source[0]};
            TKey CurrentMin = Comparer(Source[0]);
            for(int i = 1; i < Source.Count(); i++)
            {
                TKey CurrentValue = Comparer(Source[i]);
                if (CurrentValue > CurrentMin) continue;
                if (CurrentValue == CurrentMin) { Values.Add(Source[i]); continue; }
                Values = new List<TSource>() { Source[i] };
                CurrentMin = CurrentValue;
            }
            return Values;
        }
    }
}