using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Extensions;

namespace Telepaartie
{
    class MainFrame
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
            //Todo: all PossibleEndings need one father

            List<State> PossibleEndingStates = PossibleEndings.Select(x => new State(x)).ToList();
            List<State> NewDads = PossibleEndingStates
                    .SelectMany(x => x.GetChildosDeeper(0))
                    .ToList();
            for(int i = 0; true; i++)
            {
                List<State> NewChildos = NewDads.SelectMany(x => x.GetNextGen()).ToList();
                List<State>ChildosWeWannaKill = NewChildos.GetDublicates(true, null);
                NewChildos = NewChildos.Except(ChildosWeWannaKill).ToList();
                ChildosWeWannaKill.ForEach(x => x.KillMe());
                foreach(State s in NewChildos)
                {
                    Goal.RemoveAll(x => s.IsEqual(x));
                }
                if(Goal.Count == 0) return i;
                NewDads = NewChildos;
            }
        }
    }
}
