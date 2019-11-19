﻿namespace Urlaubsfahrt
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
                trackParts.RemoveAll(x => station.Position - x.Stops.Last()
                .Position < fuelLength &&x != Track.Empty);

                List<Track> possibleParts = trackParts.ToList();

                if (!possibleParts.Count == 0)
                {
                    List<Tuple<Track, float?>> bestTrackTuples = new List<Tuple<Track, float?>>();
                    possibleParts
                        .AllMins(x => x.Stops.Count)
                        .ToList()
                        .ForEach(
                            x => bestTrackTuples
                            .Add(new Tuple<Track, float?>(x, x.GetPriceTo(station))));
                    bestTrackTuples.RemoveAll(x => x.Item2 == null);
                    bestTrackTuples.Select(x => x.Item1).ToList().ForEach(x => possibleParts.Add(x.Append(station)));
                }
                else throw new InvalidOperationException("No solutions found");
            }
            return trackParts.Last();
        }
    }
}