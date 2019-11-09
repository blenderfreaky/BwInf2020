using System.Security.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;

namespace Telepaartie
{
    public class State
    {
        public int Iterations { get => ((Daddy == null) ? (0) : (Daddy.Iterations + 1)); }

        public State Daddy { get; private set; }
        public List<State> Childos { get; private set; } = null;
        public List<int> Bucks {get;}

        public State(int cupCount)
        {
            Bucks = new List<int>();
            for (int i = 0; i < cupCount; i++)
                Bucks.Add(0);

            Daddy = null;
        }

        private State(State origin, Tuple<int, int> mover)
        {
            Bucks = origin.Bucks.ToList();
            ApplyOperation(mover);

            Daddy = origin;
        }

        public List<State> GetNextGen()
        {
            if (Childos != null) throw new Exception("I am a father, I don't want more kids");
            Childos = new List<State>();
            for(int i = 0; i < Bucks.Count; i++)
                for(int u = 0; u < Bucks.Count; u++)
                    if (u != i && i <= u) Childos.Add(new State(this, new Tuple<int, int>(i, u)));
            return Childos;
        }

        private void ApplyOperation(Tuple<int, int> mover)
        {
            int val = Bucks[mover.Item1];
            Bucks[mover.Item1] *= 2;
            Bucks[mover.Item2] -= val;
        }

        private void HelpMeIDontWannaGetAdopted(State NewDaddy)
        {
            if (NewDaddy.Iterations >= Daddy.Iterations) throw new Exception("My daddy is far better");
            Daddy.Childos.Remove(this);
            Daddy = NewDaddy;
        }
    }
}