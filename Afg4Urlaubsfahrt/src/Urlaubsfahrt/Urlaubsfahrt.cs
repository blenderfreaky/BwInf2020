namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
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
                foreach (var track in optimalSubTracks)
                    Console.WriteLine(string.Join(' ', track.Stops));

                Console.WriteLine();

                optimalSubTracks.RemoveAll(x =>
                    station.Position - x.LastStop.Position > tankLength);

                if (optimalSubTracks.Count == 0)
                {
                    throw new InvalidOperationException("No solutions found");
                }

                optimalSubTracks.AddRange(optimalSubTracks
                    .AllMinsBy(x => x.Stops.Count)
                    .Select(x => x.With(station))
                    .Select(x => (Track: x, Price: x.GetCheapestPriceTo(station, startFuelLength, tankLength)))
                    .Where(x => x.Price != null)
                    .AllMinsBy(x => x.Price)
                    .Select(x => x.Track));
            }

            return optimalSubTracks.Last();
        }
    }
}