namespace Telepaartie
    {
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Telepaartie
    {
        private const string _separator = "--------------";

        public static int L(
            IEnumerable<int> goalBuckets,
            Action<string>? writeLine = null)   //Zum finden der minimalen Anzahl an Operationen für einen Zustand
        {
            if (goalBuckets == null) throw new ArgumentNullException(nameof(goalBuckets));

            var goal = new State(goalBuckets);

            var numberOfCups = goalBuckets.Count();
            var numberOfItems = goalBuckets.Sum();

            return LLLCore(numberOfCups, numberOfItems, goal, writeLine);
        }

        public static int LLL(
            int numberOfCups = 3,
            int numberOfItems = 15,
            Action<string>? writeLine = null)   //Zum finden der maximalen Anzahl der minimalen Anzahlen an Operationen für eine Anzahl
        {
            return LLLCore(numberOfCups, numberOfItems, null, writeLine);
        }

        private static int LLLCore(
            int numberOfCups,
            int numberOfItems,
            State? goal,
            Action<string>? writeLine)
        {
            var lastGen =
                // Alle Endzustände bilden die nullte Generation
                State.AllEndingStates(numberOfCups, numberOfItems)
                .Select(x => new State(x))
                .ToList();

            var allStates = lastGen.ToList();

            for (int i = 0; ; i++)
            {
                writeLine?.Invoke($"\rStarting iteration {i + 1}");

                var nextGen = lastGen
                    // Aktiviere Parallelisierung mit PLINQ
                    .AsParallel()
                    // Ermittle alle Ursprungzustände
                    .SelectMany(x => x.Origins())
                    .Distinct()
                    // Entferne Zustände die schon in vorherigen Generationen vorhanden sind
                    .Except(allStates.AsParallel())
                    .ToList();

                // Falls die Operationsanzahl für nur einen Zustand festgestellt werden soll
                if (goal != null)
                {
                    // Wenn das Element in der neuen Generation vorhanden ist, gebe den Generationsindex, also die Anzahl zum lösen benötigter Telepaartien zurück
                    if (nextGen.Contains(goal)) return i + 1;
                }
                // Wenn die neue Generation die leere Menge ist
                else if (nextGen.Count == 0)
                {
                    // Output
                    if (writeLine != null)
                    {
                        writeLine(Environment.NewLine);
                        foreach (var oldestChild in lastGen)
                        {
                            writeLine(Environment.NewLine + _separator + Environment.NewLine + Environment.NewLine);

                            for (State? current = oldestChild; current != null; current = current.Parent)
                            {
                                writeLine(current.ToString() + Environment.NewLine);
                            }
                        }

                        writeLine(Environment.NewLine + _separator + Environment.NewLine + Environment.NewLine);
                    }

                    // Gebe den Generationsindex, also die Anzahl zum lösen benötigter Telepaartien zurück
                    return i + 1;
                }

                // Die letzte Generation durch die jetzige ersetzen, um die nächste korrekt ausrechnen zu lassen
                lastGen = nextGen;
                // Zur Sammlung aller bisher entdeckten Zustände die jetzige Generation hinzufügen.
                allStates.AddRange(nextGen);
            }
        }
    }
}