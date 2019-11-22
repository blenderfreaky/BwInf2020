namespace Telepaartie
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    public static class Teelepartie
    {
        private const string Separator = "--------------";

        public static int LLL(
            IEnumerable<int> goalBuckets,
            Action<string>? writeLine = null)
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
            Action<string>? writeLine = null)
        {
            return LLLCore(numberOfCups, numberOfItems, null, writeLine);
        }

        private static int LLLCore(
            int numberOfCups,
            int numberOfItems,
            State? goal,
            Action<string>? writeLine)
        {
            List<State> lastGen = State.AllEndingStates(numberOfCups, numberOfItems)  //Alle Endzustände bilden die nullte Generation
                .Select(x => new State(x))
                .ToList();

            List<State> allStates = lastGen
                .ToList();

            for (int i = 0; ; i++)
            {
                writeLine?.Invoke($"\rStarting iteration {i}");

                List<State> nextGen = lastGen
                    .AsParallel()
                    .SelectMany(x => x.GetNextGen())    //Erschaffe aus jedem Element die Kinder
                    .Distinct()                         //Töte die doppelten Kinder
                    .Except(allStates.AsParallel())       //Töte die Kinder, die schon in den Alten vorhanden sind
                    .ToList();

                if (goal != null)                                   //Falls die Operationsanzahl für nur 1 Zustand festgestellt werden soll
                {
                    if (nextGen.Contains(goal)) return i + 1;    //Wenn das Element in den neuen Kindern vorhanden ist, gebe die Operationsanzahl zurück zurück
                }
                else if (nextGen.Count == 0)                 //Wenn keine neuen Kinder gefunden worden sind
                {
                    writeLine?.Invoke(Environment.NewLine);
                    foreach (var oldestChild in lastGen)
                    {
                        writeLine?.Invoke(Environment.NewLine + Separator + Environment.NewLine + Environment.NewLine);

                        for (State? current = oldestChild; current != null; current = current.Parent)
                        {
                            writeLine?.Invoke(current.ToString() + Environment.NewLine);
                        }
                    }

                    writeLine?.Invoke(Environment.NewLine + Separator + Environment.NewLine + Environment.NewLine);

                    return i + 1;                               //Gebe die Operationsanzahl zurück
                }

                lastGen = nextGen;
                allStates.AddRange(nextGen);
            }
        }
    }
}