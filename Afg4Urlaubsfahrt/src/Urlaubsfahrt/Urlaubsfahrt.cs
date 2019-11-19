namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Urlaubsfahrt
    {
        [ThreadStatic]
        internal static float TankDistance;

        [ThreadStatic]
        internal static float StartFuelLength;

        public static Track GetTrack(
            float startFuelLength,
            float fuelLength,
            List<GasStation> allStations)
        {
            TankDistance = fuelLength;
            StartFuelLength = startFuelLength;

            List<Track> trackParts = new List<Track> { Track.Empty };

            foreach (GasStation station in allStations)
            {
                trackParts.RemoveAll(x => 
                    station.Position - x.Stops.Last().Position < fuelLength);

                if (trackParts.Count == 0)
                {
                    throw new InvalidOperationException("No solutions found");
                }

                trackParts.AddRange(trackParts
                    .AllMins(x => x.Stops.Count)
                    .Select(x => (Track: x, Price: x.GetPriceTo(station)))
                    .Where(x => x.Price != null)
                    .Select(x => x.Track.Append(station)));
            }
            return trackParts.Last();
        }
    }
}