namespace Telepaartie
{
    using System;
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
            List<State> lastGen = GetEndingStates(numberOfCups, numberOfItems)  //Alle Endzustände bilden die nullte Generation
                .Select(x => new State(x))
                .ToList();

            List<State> allStates = lastGen
                .ToList();

            for (int i = 0; ; i++)
            {
                writeLine?.Invoke($"\rStarting iteration {i + 1}");

                List<State> nextGen = lastGen
                    .AsParallel()
                    .SelectMany(x => x.GetNextGen())    //Erschaffe aus jedem Element die Kinder
                    .Distinct()                         //Entferne die doppelten Kinder
                    .Except(allStates.AsParallel())       //Entferne die Kinder, die schon in den Alten vorhanden sind
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

        private static List<List<int>> GetEndingStates(int NumberOfCups, int NumberOfItems)
        {
            List<List<int>> states = GetStates(NumberOfCups - 1, NumberOfItems);

            foreach (var state in states) state.Insert(0, 0);

            return states;
        }

        private static List<List<int>> GetStates(int numberOfCups, int numberOfItems, int max = -1)
        {
            if (max == -1) max = numberOfItems;
            if (numberOfCups < 1) throw new ArgumentException();
            if (numberOfCups == 1) return new List<List<int>> { new List<int> { numberOfItems } };

            int min = (int)Math.Ceiling(numberOfItems / (decimal)numberOfCups);
            return Enumerable.Range(min, Math.Min(max - min + 1, numberOfItems - min))
                .SelectMany(i =>
                {
                    List<List<int>> states = GetStates(numberOfCups - 1, numberOfItems - i, i);
                    foreach (var state in states) state.Add(i);
                    return states;
                })
                .ToList();

            throw new ArgumentException();
        }
    }
}