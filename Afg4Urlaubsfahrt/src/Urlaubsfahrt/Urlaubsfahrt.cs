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
            }

            Debug.Assert(optimalSubTracks
                    .AllMinsBy(x => x.Stops.Count)
                    .Select(x => (Track: x, Price: x.GetCheapestPriceTo(allStations.Last(), startFuelLength, tankLength)))
                    .Where(x => x.Price.HasValue)
                    .AllMinsBy(x => x.Price!.Value)
                    .Select(x => x.Track)
                    .Last()
                    == optimalSubTracks.Last(),
                    "Misordered paths");

            return optimalSubTracks
                    .Last();
        }
    }
}