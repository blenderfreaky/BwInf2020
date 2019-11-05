using System.Data;
//TODO: Make this a static utility class
namespace Urlaubsfahrt
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;

    public class Program
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

            foreach(GasStation station in allStations)
            {
                trackParts.RemoveAll(x => station.Position - x.Stops.Last().Position < fuelLength && 
                    x != Track.EmptyTrack);

                List<Track> possibleParts = trackParts.ToList();

                if (possibleParts.Count == 0)
                    Console.WriteLine("No possible route! Just stay at home!");
                else
                {
                    #if false
                    Track t = new Track(possibleParts
                        .AllMins(x => x.Stops.Count)
                        .MinBy(x => x.GetPriceTo(station)), station);
                    trackParts.Add(t);
                    #else
                    List<Track> BestTracks = possibleParts
                        .AllMins(x => x.Stops.Count)
                        .AllMins(x => x.GetPriceTo(station))
                        .ToList();
                    BestTracks.ForEach(x => possibleParts.Add(new Track(x, station)));
                    #endif
                    
                }
            }
            return trackParts.Last();
        }
    }
}