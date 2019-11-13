using System.Collections.Immutable;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Extensions;

namespace Telepaartiee
{
    public static class MainFrame
    {
        public static int LLL(int NumberOfCups = 3, int NumberOfItems = 15, Action<int> stamp = null)
        {
            List<State> AllOlds = new List<State>();
            List<State> NewDads = GetEndings(NumberOfCups, NumberOfItems).Select(x => new State(x)).ToList();
            for(int i = 0; true; i++)
            {
                if(stamp != null)stamp(i);
                List<State> NewChildos = NewDads.SelectMany(x => x.GetNextGen()).Distinct().Except(AllOlds).ToList();
                if(NewChildos.Count == 0) return i;
                NewDads = NewChildos;
                AllOlds.AddRange(NewChildos);
            }
        }

        public static List<List<int>> GetGoals(int numberOfCups, int numberOfItems, int max = -1)
        {
        if (max == -1) max = numberOfItems;
        if (numberOfCups < 1) throw new ArgumentException();
        if (numberOfCups == 1) return Enumerable.Range(numberOfItems, 1).Select(x => new List<int> { x }).ToList();
        
        int min = (int)Math.Ceiling(numberOfItems/(decimal)numberOfCups);
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
            GetGoals(NumberOfCups - 1, NumberOfItems).Select(x => {;x.Insert(0,0); return x;}).ToList();
    }
}
