namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static class Urlaubsfahrt
    {
        public static Track FindBestTrack(
            List<GasStation> allStations,
            double startFuelLength,
            double tankLength)
        {
            List<Track> optimalSubTracks = new List<Track> { Track.Empty };

            foreach (GasStation station in allStations)
            {
                Console.WriteLine(station);

                Console.WriteLine("--------------------------------");

                foreach (var track in optimalSubTracks)
                    Console.WriteLine(string.Join(' ', track.Stops));

                Console.WriteLine("");

                optimalSubTracks.RemoveAll(x =>
                    station.Position - x.LastStop.Position > tankLength);

                if (optimalSubTracks.Count == 0)
                {
                    throw new InvalidOperationException("No solutions found");
                }

                optimalSubTracks.AddRange(optimalSubTracks
                    .Select(x => x.With(station))
                    .Select(x => (Track: x, Price: x.GetCheapestPriceTo(station, startFuelLength, tankLength)))
                    .Where(x => x.Price.HasValue)
                    .AllMinsBy(x => x.Track.Stops.Count)
                    .AllMinsBy(x => x.Price!.Value)
                    .Select(x => x.Track)
                    .ToList());

                foreach (var track in optimalSubTracks)
                    Console.WriteLine(string.Join(' ', track.Stops));

                Console.WriteLine("");
                Console.WriteLine("");
            }

            return optimalSubTracks
                    .AllMinsBy(x => x.Stops.Count)
                    .Select(x => (Track: x, Price: x.GetCheapestPriceTo(allStations.Last(), startFuelLength, tankLength)))
                    .Where(x => x.Price.HasValue)
                    .AllMinsBy(x => x.Price!.Value)
                    .Select(x => x.Track)
                    .Last();
        }
    }
}