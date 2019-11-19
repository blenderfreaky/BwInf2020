namespace Urlaubsfahrt.CLI
{
    using CommandLine;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;

    public class Options
    {
        [Option('f', "file", Required = false, HelpText = "The input file to evaluate.")]
        public string File { get; set; }
    }

    public static class Program
    {
        public static void Main(string[] args)
        {
            Parser.Default.ParseArguments<Options>(args)
                .WithParsed(RunWithOptions);
        }

        public static void RunWithOptions(Options o)
        {
            string[] lines = File.ReadAllLines(o.File);
            double usage = int.Parse(lines[0]);
            double maxFuel = int.Parse(lines[1]);
            double startFuel = int.Parse(lines[2]);
            double trackLength = int.Parse(lines[3]);
            double fuelLength = maxFuel / usage * 100;

            List<GasStation> allStations = new List<GasStation>();
            for (int i = 5; i < lines.Length; i++)
            {
                double[] values = lines[i].Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Select(x => double.Parse(x)).ToArray(); //compiler error
                allStations.Add(new GasStation(values[0], values[1]));
            }

            Track track = Urlaubsfahrt.FindBestTrack(allStations, startFuel / usage * 100, fuelLength);

            Console.WriteLine(track.ToString());
        }
    }
}