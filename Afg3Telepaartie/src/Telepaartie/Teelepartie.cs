namespace Telepaartie
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class Teelepartie
    {
        public static int LLL(
            int numberOfCups = 3,
            int numberOfItems = 15,
            Action<string>? handleProgress = null,
            IEnumerable<int>? Goal = null)
        {
            State? goal = null;
            if(Goal != null)
            {
                goal = new State(Goal);
                Goal = Goal.OrderBy(x => x);
                numberOfCups = Goal.Count();
                numberOfItems = Goal.Sum();
            }
            #region Funcs & Meth
            List<List<int>> GetEndings(int NumberOfCups, int NumberOfItems) =>
                GetGoals(NumberOfCups - 1, NumberOfItems).Select(x => {; x.Insert(0, 0); return x; }).ToList();

            List<List<int>> GetGoals(int numberOfCups, int numberOfItems, int max = -1)
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
            #endregion

            #region Fields
            List<State> newDads = GetEndings(numberOfCups, numberOfItems)
                .Select(x => new State(x))
                .ToList();

            List<State> allOlds = newDads
                .ToList();
            #endregion

            for (int i = 0; ; i++)
            {
                handleProgress?.Invoke($"\rStarting iteration {i}");

                List<State> newChildos = newDads
                    .AsParallel()
                    .SelectMany(x => x.GetNextGen())
                    .Distinct()
                    .Except(allOlds.AsParallel()).ToList();
                if(goal != null)
                {
                    if (newChildos.Contains(goal)) return i + 1;
                }
                else if (newChildos.Count == 0)
                {
                    handleProgress?.Invoke(Environment.NewLine);
                    foreach (var oldestChild in newDads
                        .AsParallel()
                        .SelectMany(x => x.GetNextGen())
                        .Distinct())
                    {
                        handleProgress?.Invoke(Environment.NewLine + "--------------" + Environment.NewLine + Environment.NewLine);

                        for (var current = oldestChild; current != null; current = current.Daddy)
                        {
                            handleProgress?.Invoke(current.ToString() + Environment.NewLine);    
                        }
                    }
                    handleProgress?.Invoke(Environment.NewLine + "--------------" + Environment.NewLine + Environment.NewLine) ;

                    return i + 1;
                }

                newDads = newChildos;
                allOlds.AddRange(newChildos);
            }
        }
    }
}
