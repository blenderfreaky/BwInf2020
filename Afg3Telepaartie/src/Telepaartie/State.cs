using System.Security.AccessControl;
using System;
using System.Linq;
using Extensions;

namespace Telepaartie
{
    public class State
    {
        public int iteration{get;}
        public Tuple<int, int, int> Bucks {get;}

        public State(State origin, Tuple<int, int> mover)
        {
            iteration = origin.iteration+1;
            Bucks
        }

    }
}