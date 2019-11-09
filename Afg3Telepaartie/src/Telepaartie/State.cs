using System.Security.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;

namespace Telepaartie
{
    public class State
    {
        public int iteration{get;}
        public List<int> Bucks {get;}

        public State(int cupCount)
        {
            Bucks = new List<int>();
            for (int i = 0; i < cupCount; i++)
                Bucks.Add(0);
        }

        private State(State origin, Tuple<int, int> mover)
        {
            iteration = origin.iteration+1;
            Bucks = origin.Bucks.ToList();
            ApplyOperation(mover);
        }

        public List<State> GetNextGen()
        {
            List<State> NextGen = new List<State>();
            for(int i = 0; i < Bucks.Count; i++)
                for(int u = 0; u < Bucks.Count; u++)
                    if (u != i && i <= u) NextGen.Add(new State(this, new Tuple<int, int>(i, u)));
            return NextGen;
        }

        private void ApplyOperation(Tuple<int, int> mover)
        {
            int val = Bucks[mover.Item1];
            Bucks[mover.Item1] *= 2;
            Bucks[mover.Item2] -= val;
        }
    }
}