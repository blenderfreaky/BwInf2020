namespace Urlaubsfahrt
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Urlaubsfahrt
    {
        /// <summary>
        /// Finds the shortest and cheapest path through a set of stations. Prioritizes shortness over cheapness.
        /// </summary>
        /// <param name="allStations">All the stations on the track. May be unordered.</param>
        public static Track FindBestTrack(
            IEnumerable<GasStation> allStations,
            Car car,
            decimal destinationPosition)
        {
            // Der erste Weg der existiert ist ein Weg ohne Stopps
            List<Track> optimalSubTracks = new List<Track> { Track.Empty };

            // Kopiere die übergebenen Stationen in eine eigene, sortierte Liste
            var actualStations = allStations
                .OrderBy(x => x.Position)
                .ToList();

            // Füge das Ziel als eine Tankstelle mit einem Preis von null an
            actualStations.Add(new GasStation(destinationPosition, 0));

            // Für jede Tankstelle wird der ideale Weg ermittelt
            foreach (GasStation station in actualStations)
            {
                optimalSubTracks.RemoveAll(x =>
                    // Zuerst werden alle Wege entfernt, dessen letzte Station weiter von der aktuellen Station weg ist, als das Auto Reichweite hat
                    station.Position - x.LastStop.Position > car.TankDistance);

                if (optimalSubTracks.Count == 0)
                {
                    // Wenn man zu eiem Punkt nicht kommen kann, kann man auch nicht zu den Punkten dahinter oder zum Ziel kommen
                    throw new InvalidOperationException("No solutions found");
                }

                optimalSubTracks.AddRange(optimalSubTracks
                    // An jeden alten Weg wird die aktuelle Station angehangen
                    .Select(x => x.With(station))
                    // Es wird für jeden Weg der Preis gebildet
                    .Select(x => (Track: x, Price: x.GetCheapestPriceTo(station, car)))
                    // Es werden die nicht möglichen entfernt
                    .Where(x => x.Price.HasValue)
                    // Es werden die Wege gewählt, die am wenigsten Stopps brauchen
                    .AllMinsBy(x => x.Track.Stops.Count)
                    // Es werden die günstigsten gwählt
                    .AllMinsBy(x => x.Price!.Value)
                    .Select(x => x.Track));
            }

            return optimalSubTracks
                .Select(x => (Track: x, Price: x.GetCheapestPriceTo(destinationPosition, car)))
                .Where(x => x.Price.HasValue)
                .AllMinsBy(x => x.Track.Stops.Count)
                .AllMinsBy(x => x.Price!.Value)
                .Select(x => x.Track)
                // Wenn es mehrere gleich-günstige Wege gibt wird der letzte gewählt
                .Last();
        }
    }
}