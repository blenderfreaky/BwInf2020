﻿namespace Telepaartie
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    public static class Teelepartie
    {
        private struct StateEqualityComparer : IEqualityComparer<State>
        {
            public static readonly StateEqualityComparer Default = new StateEqualityComparer();

            public readonly bool Equals(State l, State r) => l.Equals(r);
            public readonly int GetHashCode(State s) => s.GetHashCode();
        }

        public static int LLL(int NumberOfCups = 3, int NumberOfItems = 15, Action<int> handleProgress = null)
        {
            List<State> NewDads = GetEndings(NumberOfCups, NumberOfItems).Select(x => new State(x)).ToList();
            List<State> AllOlds = NewDads.ToList();
            for (int i = 0; ; i++)
            {
                handleProgress?.Invoke(i);
                List<State> NewChildos = NewDads.SelectMany(x => x.GetNextGen()).ToList();
                Debug.Write(NewChildos.Count.ToString());

                //NewChildos = NewChildos.DistinctBy<State, int[]>(x => x.Bucks.ToArray()).ToList();
                NewChildos = NewChildos.Distinct(StateEqualityComparer.Default).ToList();
                Debug.Write(NewChildos.Count.ToString());

                //NewChildos = NewChildos.ExceptBy(AllOlds, x => x.Bucks).ToList();
                NewChildos = NewChildos.Except(AllOlds, StateEqualityComparer.Default).ToList();
                Debug.Print(NewChildos.Count.ToString());

                if (NewChildos.Count == 0) return i;

                NewDads = NewChildos;
                AllOlds.AddRange(NewChildos);
            }
        }

        public static List<List<int>> GetGoals(int numberOfCups, int numberOfItems, int max = -1)
        {
            if (max == -1) max = numberOfItems;
            if (numberOfCups < 1) throw new ArgumentException();
            if (numberOfCups == 1) return Enumerable.Range(numberOfItems, 1).Select(x => new List<int> { x }).ToList();

            int min = (int)Math.Ceiling(numberOfItems / (decimal)numberOfCups);
            return Enumerable.Range(min, Math.Min(max - min + 1, numberOfItems - min))
                .SelectMany(i =>
                {
                    List<List<int>> possibilites = GetGoals(numberOfCups - 1, numberOfItems - i, i);
                    foreach (var p in possibilites) p.Add(i);
                    return possibilites;
                })
                .ToList();

            throw new ArgumentException();
        }

        private static List<List<int>> GetEndings(int NumberOfCups, int NumberOfItems) =>
            GetGoals(NumberOfCups - 1, NumberOfItems).Select(x => {; x.Insert(0, 0); return x; }).ToList();
    }
}