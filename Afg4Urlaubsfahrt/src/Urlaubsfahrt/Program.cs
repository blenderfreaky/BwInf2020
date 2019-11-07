//TODO: Make this a static utility class
namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Program
    {
        public static Track GetTrack(
            float trackLength,
            float startFuelLength,
            float fuelLength,
            List<GasStation> allStations)
        {
            Track.FuelLength = fuelLength;
            Track.StartFuelLength = startFuelLength;

            List<Track> trackParts = new List<Track> { Track.EmptyTrack };

            foreach (GasStation station in allStations)
            {
                trackParts.RemoveAll(x => station.Position - x.Stops.Last().Position < fuelLength &&
                    x != Track.EmptyTrack);

                List<Track> possibleParts = trackParts.ToList();

                if (possibleParts.Count == 0) throw new InvalidOperationException("No solutions found");
                else
                {
#if false
                    Track t = new Track(possibleParts
                        .AllMins(x => x.Stops.Count)
                        .MinBy(x => x.GetPriceTo(station)), station);
                    trackParts.Add(t);
#else
                    List<Tuple<Track, float?>> BestTrackTuples = new System.Collections.Generic.List<Tuple<Track, float?>>();
                    possibleParts
                        .AllMins(x => x.Stops.Count)
                        .ToList()
                        .ForEach(
                            x => BestTrackTuples
                            .Add(new Tuple<Track, float?>(x, x.GetPriceTo(station))));
                    BestTrackTuples.RemoveAll(x => x.Item2 == null);
                    BestTrackTuples.Select(x => x.Item1).ToList().ForEach(x => possibleParts.Add(new Track(x, station)));
#endif
                }
            }
            return trackParts.Last();
        }
    }
}