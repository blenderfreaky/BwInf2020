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
                optimalSubTracks.RemoveAll(x =>
                    station.Position - x.LastStop.Position < tankLength);

                if (optimalSubTracks.Count == 0)
                {
                    throw new InvalidOperationException("No solutions found");
                }

                optimalSubTracks.AddRange(optimalSubTracks
                    .AllMinsBy(x => x.Stops.Count)
                    .Select(x => (Track: x, Price: x.GetCheapestPriceTo(station, startFuelLength, tankLength)))
                    .Where(x => x.Price != null)
                    .Select(x => x.Track.With(station)));
            }

            return optimalSubTracks.Last();
        }
    }
}