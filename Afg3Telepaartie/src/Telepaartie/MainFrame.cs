using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Extensions;

namespace Telepaartiee
{
    public static class MainFrame
    {
        public static int LLL(int NumberOfCups = 3, int NumberOfItems = 15)
        {
            List<List<int>> Goal = GetGoals(NumberOfCups, NumberOfItems);

            List<State> AllOlds = new List<State>();
            List<State> NewDads = GetEndings(NumberOfCups, NumberOfItems).Select(x => new State(x)).ToList();
            for(int i = 0; true; i++)
            {
                List<State> NewChildos = NewDads.SelectMany(x => x.GetNextGen()).Distinct().Except(AllOlds).ToList();
                foreach(State s in NewChildos)
                {
                    Goal.RemoveAll(x => s.IsEqual(x));
                }
                if(Goal.Count == 0) return i;
                NewDads = NewChildos;
                AllOlds.AddRange(NewChildos);
            }
        }

        public static List<List<int>> GetGoals(int NumberOfCups, int NumberOfItems)
        {
            if(NumberOfItems < 0 || NumberOfCups < 1) throw new ArgumentException();
            if(NumberOfCups == 1) return new List<List<int>>{new List<int>{NumberOfItems}};
            List<List<int>> Goals = new List<List<int>>();
            for(int i = NumberOfItems; i >= Math.Ceiling((double)NumberOfItems/NumberOfCups); i--)
            {
                Goals.AddRange(GetGoals(NumberOfCups - 1, NumberOfItems - i).Select(x => {x.Insert(0, i); return x;}));
            }
            Goals.ForEach(x => {x.Sort(); x.Reverse();});
            Goals.Distinct();
            return Goals;
        }

        private static List<List<int>> GetEndings(int NumberOfCups, int NumberOfItems)
        {
            List<List<int>> Endings = new List<List<int>>();
            return Endings;
        }
    }
}
