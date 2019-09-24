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
            /*Debug.Assert(Allstations is sorted after position);*/ //TODO Implement
            List<Track> trackParts = new List<Track> { Track.EmptyTrack };

            for (int i = 0; i < allStations.Count; i++) //TODO use foreach for clarity (and since not arr maybe perf)
            {
                //Todo: Add Emty-Check and adder! Code won't work without! It's just one line, why am I writing this,
                //I could have done it, in the time, I'm writing this! YEET
                trackParts.RemoveAll(x => allStations[i].Position- x.Stops.Last().Position < fuelLength);

                List<Track> possibleParts = trackParts
                    .ToList();

                if (possibleParts.Count == 0 && allStations[i].Position > fuelLength)
                {
                    //If there is no way to get to the destination
                    //I dunno what exeption I should use, plz help me!
                    throw new Exception();
                }
                if (fuelLength >= allStations[i].Position)
                {
                    Track t = new Track(possibleParts
                        .AllMins(x => x.Stops.Count)
                        .MinBy(x => x.GetPriceTo(allStations[i])), allStations[i]); //compiler error
                    trackParts.Add(t);
                }
                else
                {
                    //Come on dude, it's ugly af!
                    Console.WriteLine("No possible route! Just stay at home!");
                }
            }
            Track BestWay = trackParts.AllMins(x => x.Stops.Count).MinBy(x => x.GetPriceTo(trackLength));
            return BestWay;
        }
    }
}