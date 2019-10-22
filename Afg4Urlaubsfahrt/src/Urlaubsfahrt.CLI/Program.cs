namespace Urlaubsfahrt.CLI
{
    using CommandLine;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.IO;

    public class Options
    {
        [Option('v', "verbose", Required = false, HelpText = "Set output to verbose messages.")]
        public bool Verbose { get; set; }
    }

    public static class Program
    {
        public static void Main()
        {
            #if true
            Console.WriteLine("Insert path:");
            string path = Console.ReadLine();

            string[] FileValues = File.ReadAllText(path).Split('\n');
            float Usage = int.Parse(FileValues[0]);
            float MaxFuel = int.Parse(FileValues[1]);
            float StartFuel = int.Parse(FileValues[2]);
            float TrackLength = int.Parse(FileValues[3]);
            float FuelLength = MaxFuel / Usage * 100;
            List<Urlaubsfahrt.GasStation> AllStations = new List<Urlaubsfahrt.GasStation>();
            for (int i = 5; i < FileValues.Length; i++)
            {
                float[] values = FileValues[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => float.Parse(x)).ToArray(); //compiler error
                AllStations.Add(new Urlaubsfahrt.GasStation(values[0], values[1]));
            }

            Urlaubsfahrt.Track Way = Urlaubsfahrt.Program.GetTrack(TrackLength, MaxFuel, StartFuel, FuelLength, AllStations);
            Console.WriteLine(Way.ToString());
            #else
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(o =>
                {
                    if (o.Verbose)
                    {
                        Console.WriteLine($"Verbose output enabled. Current Arguments: -v {o.Verbose}");
                        Console.WriteLine("Quick Start Example! App is in Verbose mode!");
                    }
                    else
                    {
                        Console.WriteLine($"Current Arguments: -v {o.Verbose}");
                        Console.WriteLine("Quick Start Example!");
                    }
                });
            #endif
        }
    }
}
