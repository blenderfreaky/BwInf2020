
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

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
        List<GasStation> Stops {get;}
        float GetPrice()
        {
            float value = 0;
            for(int i = 0; i < Stops.Count - 1; i++)
            {
                value += (Stops[i+1].Position - Stops[i].Position) * Stops[i].PricePerVolumeInEuroPerLiter;
            }
            return value;
        }
    }

}