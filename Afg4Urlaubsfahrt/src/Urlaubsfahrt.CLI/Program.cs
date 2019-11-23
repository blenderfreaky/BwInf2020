namespace Urlaubsfahrt.CLI
{
    using CommandLine;
    using System;
    using System.Collections.Generic;
    using System.IO;

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

            decimal trackLength = int.Parse(lines[3]); // km
            var car = new Car(
                int.Parse(lines[0]) / 100d /*l/100km to l/km*/,
                int.Parse(lines[1]),
                int.Parse(lines[2]));

            List<GasStation> allStations = new List<GasStation>();
            for (int i = 5; i < lines.Length; i++)
            {
                string[] values = lines[i]
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                allStations.Add(new GasStation(decimal.Parse(values[0]), decimal.Parse(values[1]) / 100d /*ct to €*/));
            }

            Track track = Urlaubsfahrt.FindBestTrack(allStations, car, trackLength);

            Console.WriteLine("Stops:");

            Console.WriteLine(track);

            var drivingPlan = track.GetCheapestPathTo(trackLength, car);

            Console.WriteLine();
            Console.WriteLine("Driving Plan:");

            Console.WriteLine(drivingPlan.Value.ToString(car));
            Console.WriteLine();
            Console.WriteLine("  Stops: " + drivingPlan.Value.Stops.Count);
            Console.WriteLine("  Price: " + drivingPlan.Value.PriceFor(car) + "EUR");
        }
    }
}