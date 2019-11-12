using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Extensions;

namespace Telepaartie
{
    public class MainFrame
    {
        private List<List<int>> Goal { get; set; }
        private List<List<int>> PossibleEndings { get; set; }

        public MainFrame(List<List<int>> goal, List<List<int>> possibleEndings)
        {
            Goal = goal;
            PossibleEndings = possibleEndings;
        }

        public int LLL()
        {
            List<State> AllOlds = new List<State>();
            List<State> PossibleEndingStates = PossibleEndings.Select(x => new State(x)).ToList();
            List<State> NewDads = PossibleEndingStates.ToList();
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
    }
}
