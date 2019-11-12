using System.Security.AccessControl;
using System;
using System.Collections.Generic;
using System.Linq;
using Extensions;

namespace Telepaartie
{
    public class State : IEquatable<State>
    {
        public int Iterations { get => ((Daddy == null) ? (0) : (Daddy.Iterations + 1)); }
        public State Daddy { get; private set; }
        public List<int> Bucks {get;}


        public State(List<int> end)
        {
            Bucks = end.ToList();
            Bucks.Sort();
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
            List<State> Childos = new List<State>();
            for(int i = 0; i < Bucks.Count; i++)
                for(int u = 0; u < Bucks.Count; u++)
                    if (Bucks[i]%2 == 0 && Bucks[i] > 0 && i != u) Childos.Add(new State(this, new Tuple<int, int>(i, u)));
            return Childos;
        }

        public bool IsEqual(List<int> test)
        {
            if(test.Count != Bucks.Count) throw new ArgumentException();
            for(int i = 0; i < test.Count; i++)
            {
                if(test[i] != Bucks[i]) return false;
            }
            return true;
        }

        public bool Equals(State Test)
        {
            if(Test == null) return false;
            if(Test.Bucks.Count != Bucks.Count) throw new ArgumentException();
            for(int i = 0; i < Bucks.Count; i++)
            {
                if(Test.Bucks[i] != Bucks[i]) return false;
            }
            return true;
        }

        public override bool Equals(Object Test) => Equals(Test as State);

        private void ApplyOperation(Tuple<int, int> mover)
        {
            Bucks[mover.Item1] /= 2;
            Bucks[mover.Item2] += Bucks[mover.Item1];
            Bucks.Sort();
        }
    }
}