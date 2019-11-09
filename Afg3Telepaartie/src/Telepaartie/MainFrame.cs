using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace Telepaartie
{
    class MainFrame
    {
        private List<int> Goal { get; set; }
        private List<List<int>> PossibleEndings { get; set; }

        public MainFrame(List<int> goal, List<List<int>> possibleEndings)
        {
            Goal = goal;
            PossibleEndings = possibleEndings;
        }

        public int LLL()
        {
            //Todo: all PossibleEndings need one father
            List<State> PossibleEndingStates = PossibleEndings.Select(x => new State(x)).ToList();
            for(int i = 0; true; i++)
            {
                List<State> NewDads = PossibleEndingStates
                    .SelectMany(x => x.GetChildosDeeper(i))
                    .ToList();

                List<State> NewChildos = NewDads.SelectMany(x => x.GetNextGen()).ToList();
            }
        }
    }
}
