namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Urlaubsfahrt
    {
        [ThreadStatic]
        internal static double TankDistance;

        [ThreadStatic]
        internal static double StartFuelLength;

        public static Track FindBestTrack(
            double startFuelLength,
            double fuelLength,
            List<GasStation> allStations)
        {
            TankDistance = fuelLength;
            StartFuelLength = startFuelLength;

            List<Track> optimalSubTracks = new List<Track> { Track.Empty };

            foreach (GasStation station in allStations)
            {
                optimalSubTracks.RemoveAll(x =>
                    station.Position - x.Stops.Last().Position < fuelLength);

                if (optimalSubTracks.Count == 0)
                {
                    throw new InvalidOperationException("No solutions found");
                }

                optimalSubTracks.AddRange(optimalSubTracks
                    .AllMins(x => x.Stops.Count)
                    .Select(x => (Track: x, Price: x.GetCheapestPriceTo(station)))
                    .Where(x => x.Price != null)
                    .Select(x => x.Track.With(station)));
            }
            return optimalSubTracks.Last();
        }
    }
}