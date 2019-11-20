namespace Urlaubsfahrt
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;

    public static class Urlaubsfahrt
    {
        /// <summary>
        /// Finds the shortest and cheapest path through a set of stations. Prioritizes shortness over cheapness.
        /// </summary>
        /// <param name="allStations">All GasStations</param>
        /// <param name="startFuelLength"></param>
        /// <param name="tankLength"></param>
        /// <returns></returns>
        public static Track FindBestTrack(
            IEnumerable<GasStation> allStations,
            Car car,
            double destinationPosition)
        {
            // Start with the empty track as the only possibility
            List<Track> optimalSubTracks = new List<Track> { Track.Empty };

            var actualStations = allStations
                // We want to iterate from the lowest to the highest position
                //.OrderBy(x => x.Position) // Should already be sorted
                .ToList();

            actualStations.Add(new GasStation(destinationPosition, 0));

            foreach (GasStation station in allStations)
            {
                optimalSubTracks.RemoveAll(x =>
                    station.Position - x.LastStop.Position > car.TankDistance);

                if (optimalSubTracks.Count == 0)
                {
                    throw new InvalidOperationException("No solutions found");
                }

                optimalSubTracks.AddRange(optimalSubTracks
                    .Select(x => x.With(station))
                    .Select(x => (Track: x, Price: x.GetCheapestPriceTo(station, car)))
                    .Where(x => x.Price.HasValue)
                    .AllMinsBy(x => x.Track.Stops.Count)
                    .AllMinsBy(x => x.Price!.Value)
                    .Select(x => x.Track)
                    .ToList());
            }

            //Debug.Assert(optimalSubTracks
            //        .AllMinsBy(x => x.Stops.Count)
            //        .Select(x => (Track: x, Price: x.GetCheapestPriceTo(allStations.Last(), car)))
            //        .Where(x => x.Price.HasValue)
            //        .AllMinsBy(x => x.Price!.Value)
            //        .Select(x => x.Track)
            //        .Last()
            //        == optimalSubTracks.Last(),
            //        "Misordered paths");

            //return optimalSubTracks
            //        .Last();

            return optimalSubTracks
                .AllMinsBy(x => x.Stops.Count)
                .Select(x => (Track: x, Price: x.GetCheapestPriceTo(allStations.Last(), car)))
                .Where(x => x.Price.HasValue)
                .AllMinsBy(x => x.Price!.Value)
                .Select(x => x.Track)
                .Last();
        }
    }
}