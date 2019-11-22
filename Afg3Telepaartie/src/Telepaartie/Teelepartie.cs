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
            List<State> newDads = GetEndingStates(numberOfCups, numberOfItems)  //Alle Endzustände bilden die nullte Generation
                .Select(x => new State(x))
                .ToList();

            List<State> allOlds = newDads
                .ToList();

            for (int i = 0; ; i++)
            {
                writeLine?.Invoke($"\rStarting iteration {i}");

                List<State> newChildos = newDads
                    .AsParallel()
                    .SelectMany(x => x.GetNextGen())    //Erschaffe aus jedem Element die Kinder
                    .Distinct()                         //Töte die doppelten Kinder
                    .Except(allOlds.AsParallel())       //Töte die Kinder, die schon in den Alten vorhanden sind
                    .ToList();

                if (goal != null)                                   //Falls die Operationsanzahl für nur 1 Zustand festgestellt werden soll
                {
                    if (newChildos.Contains(goal)) return i + 1;    //Wenn das Element in den neuen Kindern vorhanden ist, gebe die Operationsanzahl zurück zurück
                }
                else if (newChildos.Count == 0)                 //Wenn keine neuen Kinder gefunden worden sind
                {
                    writeLine?.Invoke(Environment.NewLine);
                    foreach (var oldestChild in newDads
                        .AsParallel()
                        .SelectMany(x => x.GetNextGen())
                        .Distinct())
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

                newDads = newChildos;
                allOlds.AddRange(newChildos);
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