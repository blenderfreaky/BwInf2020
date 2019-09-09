
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
            float TrackLength = 0;
            float MaxFuel = 0;
            float DasEnglischeWortFürVerbrauch = 0;
            float StartFuel = 0;
            Console.WriteLine("Insert path:");
            string path = Console.ReadLine();
            string[] FileValues = File.ReadAllText(path).Split('\n');
            DasEnglischeWortFürVerbrauch = int.Parse(FileValues[0]);
            MaxFuel = int.Parse(FileValues[1]);
            StartFuel = int.Parse(FileValues[2]);
            TrackLength = int.Parse(FileValues[3]);
            float FuelLength = MaxFuel / DasEnglischeWortFürVerbrauch * 100; //Will be removed later, but its faster to use it right now
            AllStations = new List<GasStation> {Track.EmtyTrack};
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
                if(FuelLength >= AllStations[i].Position)
                try
                {
                    Track t = new Track(PosssibleParts
                    .Where(x => AllStations[i].Position - x.Stops.Last().Position > FuelLength)
                    .AllMins(x => x.Stops.Count)
                    .Min(x => x.GetPriceTo(AllStations[i]), AllStations[i]));
                    PosssibleParts.Add(t);
                }
                catch
                {
                    Console.WriteLine("No possible route! Just stay at home!"); goto NOWAY;
                }
            }
            Track BestWay = PosssibleParts.AllMins(x => x.Stops.Count).Min(x => x.GetPriceTo(TrackLength));
            Console.WriteLine("The most efficent way is to stop at:");
            foreach(GasStation s in BestWay.Stops)
            {
                Console.WriteLine($"- {s.Position.ToString()}");
            }
            NOWAY:
            Console.ReadLine();
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

        public static Track EmtyTrack = new Track();

        float GetPrice()
        {
            float value = 0;
            for(int i = 0; i < Stops.Count - 1; i++)
            {
                value += (Stops[i+1].Position - Stops[i].Position) * Stops[i].PricePerVolumeInEuroPerLiter;
            }
            return value;
        }

        float GetPriceTo(int pos)
        {
            float value = GetPrice();
            value += (pos - Stops.Last().Position) * Stops.Last().PricePerVolumeInEuroPerLiter;
        }

        float GetPriceTo(GasStation s)
        {
            float value = GetPrice();
            value += GetPriceTo(s.Position);
        }
        public Track(GasStation s) => Stops = new List<GasStation> { s };
        public Track (Track t, GasStation s)
        {
            Stops = new List<GasStation>();
            foreach (GasStation ss in t.Stops) Stops.Add(ss);
            Stops.Add(s);
        }

        private Track() => Stops = new List<GasStation>();
    }

}

namespace Extensions
{
    public static class Extensions
    {
        IList<TSource> AllMins<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, int capacity = 1)
    where TKey : IComparable<TKey>
{
    var enumerator = source.GetEnumerator();

    if (!enumerator.MoveNext()) throw new ArgumentException(nameof(source) + " cant be emty");

    TSource firstElement = enumerator.Current;
    TKey min = selector(firstElement);
    List<TSource> minima = new List<TSource>(capacity) { firstElement };

    while (enumerator.MoveNext())
    {
        TSource element = enumerator.Current;
        TKey key = selector(element);

        if (key.CompareTo(min) < 0)
        {
            min = key;
            minima.Clear();
        }

        minima.Add(element);
    }
}
    }
}