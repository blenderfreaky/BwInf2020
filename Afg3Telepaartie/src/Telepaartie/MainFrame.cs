using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Extensions;

namespace Telepaartie
{
    public static class MainFrame
    {
        public static int LLL(int NumberOfCups = 3)
        {
            List<List<int>> Goal = GetGoals(NumberOfCups);

            List<State> AllOlds = new List<State>();
            List<State> NewDads = GetEndings(NumberOfCups).Select(x => new State(x)).ToList();
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

        private static List<List<int>> GetGoals(int NumberOfCups)
        {
            List<List<int>> Goals = new List<List<int>>();
            return Goals;
        }

        private static List<List<int>> GetEndings(int NumberOfCups)
        {
            List<List<int>> Endings = new List<List<int>>();
            return Endings;
        }
    }
}
