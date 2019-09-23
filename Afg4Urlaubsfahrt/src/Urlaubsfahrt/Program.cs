using System.Data;
//TODO: Make this a static utility class
namespace Urlaubsfahrt
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using Urlaubsfahrt.Extensions;

    public class Program
    {
        public static void Main()
        {
            #region Values
            Tuple<float, float, float, float, float, List<GasStation>> DataStore = GetValues();
            float TrackLength = DataStore.Item1;
            float MaxFuel = DataStore.Item2;
            float DasEnglischeWortFürVerbrauch = DataStore.Item3;
            float StartFuel = DataStore.Item4;
            float FuelLength = DataStore.Item5;
            List<GasStation> AllStations = DataStore.Item6;
            /*Debug.Assert(Allstations is sorted after position);*/ //TODO Implement
            List<Track> TrackParts = new List<Track> { Track.EmptyTrack };
            #endregion

            #region Read data

            #endregion

            for (int i = 0; i < AllStations.Count; i++) //TODO use foreach for clarity (and since not arr maybe perf)
            {
                //Todo: Add Emty-Check and adder! Code won't work without! It's just one line, why am I writing this,
                //I could have done it, in the time, I'm writing this! YEET
                List<Track> PosssibleParts = TrackParts
                    .Where(x => AllStations[i].Position - x.Stops.Last().Position > FuelLength)
                    .ToList();

                if (PosssibleParts.Count == 0 && AllStations[i].Position > FuelLength)
                {
                    //If there is no way to get to the destination
                    //I dunno what exeption I should use, plz help me!
                    throw new Exception();
                }
                if (FuelLength >= AllStations[i].Position)
                {
                    Track t = new Track(PosssibleParts
                        .AllMins(x => x.Stops.Count)
                        .MinBy(x => x.GetPriceTo(AllStations[i])), AllStations[i]); //compiler error
                    TrackParts.Add(t);
                }
                else
                {
                    //Come on dude, it's ugly af!
                    Console.WriteLine("No possible route! Just stay at home!");
                }
            }
            Track BestWay = TrackParts.AllMins(x => x.Stops.Count).MinBy(x => x.GetPriceTo(TrackLength));
            Console.WriteLine("The most efficent way is to stop at:");
            foreach (GasStation s in BestWay.Stops)
            {
                Console.WriteLine($"- {s.Position.ToString()}");
            }
        }

        public static Tuple<float, float, float, float, float, List<GasStation>> GetValues()
        {
            Console.WriteLine("Insert path:");
            string path = Console.ReadLine();
                try 
                {
                    string[] FileValues = File.ReadAllText(path).Split('\n');
                    float DasEnglischeWortFürVerbrauch = int.Parse(FileValues[0]);
                    float MaxFuel = int.Parse(FileValues[1]);
                    float StartFuel = int.Parse(FileValues[2]);
                    float TrackLength = int.Parse(FileValues[3]);
                    float FuelLength = MaxFuel / DasEnglischeWortFürVerbrauch * 100; //Will be removed later, but its faster to use it right now

                    List<GasStation> AllStations = new List<GasStation>();
                    for (int i = 5; i < FileValues.Length; i++)
                    {
                        float[] values = FileValues[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray(); //compiler error
                        AllStations.Add(new GasStation(values[0], values[1]));
                    }
                    return new Tuple<float, float, float, float, float, List<GasStation>>(DasEnglischeWortFürVerbrauch,
                    MaxFuel,
                    StartFuel,
                    TrackLength,
                    FuelLength,
                    AllStations
                    );
                }
                catch
                {
                    throw new ArgumentException($"{path} doesn't exists or contains wrong data.");
                }
        }
    }
}