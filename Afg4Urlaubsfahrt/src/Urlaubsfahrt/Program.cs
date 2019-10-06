using System.Data;
//TODO: Make this a static utility class
namespace Urlaubsfahrt
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using Extensions;

    public class Program
    {
        public static Track GetTrack(
            float trackLength,
            float maxFuel,
            float startFuel,
            float fuelLength,
            List<GasStation> allStations)
        {
            List<Track> trackParts = new List<Track> { Track.EmptyTrack };

            foreach(GasStation station in allStations)
            {
                trackParts.RemoveAll(x => station.Position- x.Stops.Last().Position < fuelLength && x != Track.EmptyTrack);

                List<Track> possibleParts = trackParts.Select(x => x).ToList();

                if (possibleParts.Count == 0)
                    Console.WriteLine("No possible route! Just stay at home!");
                else
                {
                    Track t = new Track(possibleParts
                        .AllMins(x => x.Stops.Count)
                        .MinBy(x => x.GetPriceTo(station)), station);
                    trackParts.Add(t);
                }
            }
            Track BestWay = trackParts.AllMins(x => x.Stops.Count).MinBy(x => x.GetPriceTo(trackLength));
            return BestWay;
        }
    }
}