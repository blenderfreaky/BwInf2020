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
            List<Track> optimalSubTracks = new List<Track> { Track.Empty }; //Der erste Weg der existiert ist ein Weg ohne Stopps

            var actualStations = allStations
                .OrderBy(x => x.Position)
                .ToList();

            actualStations.Add(new GasStation(destinationPosition, 0));

            foreach (GasStation station in allStations) //Für jede Tankstelle wird der ideale Weg ermittelt
            {
                optimalSubTracks.RemoveAll(x =>
                    station.Position - x.LastStop.Position > car.TankDistance); //Zuerst werden alle Wege entfernt, dessen letzte Station weiter von der aktuellen Station weg ist, als das Auto Reichweite hat

                if (optimalSubTracks.Count == 0)
                {
                    throw new InvalidOperationException("No solutions found");  //Wenn man zu eiem Punkt nicht kommen kann, kann man auch nicht zu den Punkten dahinter oder zum Ziel kommen
                }

                optimalSubTracks.AddRange(optimalSubTracks
                    .Select(x => x.With(station))   //an jeden alten Weg wird die aktuelle Station angehangen
                    .Select(x => (Track: x, Price: x.GetCheapestPriceTo(station, car))) //Es wird für jeden Weg der Preis gebildet
                    .Where(x => x.Price.HasValue)   //Es werden die nicht möglichen entfernt
                    .AllMinsBy(x => x.Track.Stops.Count)    //Es werden die Wege gewählt, die am wenigsten Stopps brauchen
                    .AllMinsBy(x => x.Price!.Value)    //Es werden die günstigsten gwählt
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
                .Last(); //wenn es mehrere gleich-günstige gibt, wird der letzte gewählt
        }
    }
}