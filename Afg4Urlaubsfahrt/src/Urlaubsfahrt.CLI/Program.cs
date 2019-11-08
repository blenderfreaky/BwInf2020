namespace Urlaubsfahrt.CLI
{
    using CommandLine;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
    }

    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Insert path:");
            string path = Console.ReadLine();

            string[] FileValues = File.ReadAllText(path).Split('\n');
            float Usage = int.Parse(FileValues[0]);
            float MaxFuel = int.Parse(FileValues[1]);
            float StartFuel = int.Parse(FileValues[2]);
            float TrackLength = int.Parse(FileValues[3]);
            float FuelLength = MaxFuel / Usage * 100;
            List<GasStation> AllStations = new List<GasStation>();
            for (int i = 5; i < FileValues.Length; i++)
            {
                float[] values = FileValues[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray(); //compiler error
                AllStations.Add(new GasStation(values[0], values[1]));
            }

            Track Way = Urlaubsfahrt.Program.GetTrack(TrackLength, StartFuel / Usage * 100, FuelLength, AllStations);
            Console.WriteLine(Way.ToString());
        }
    }
}