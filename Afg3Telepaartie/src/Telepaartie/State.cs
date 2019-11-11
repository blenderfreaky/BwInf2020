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
            if (Childos != null) throw new Exception("I am a father, I don't want more kids");
            Childos = new List<State>();
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

        public bool IsEqual(State test)
        {
            if(test.Bucks.Count != Bucks.Count) throw new ArgumentException();
            for(int i = 0; i < Bucks.Count; i++)
            {
                if(test.Bucks[i] != Bucks[i]) return false;
            }
            return true;
        }

        public List<State>GetChildosDeeper(int y)
        {
            if (y < 0) throw new Exception("Bruh u want me and my brothers, my parents or other stupid people");
            if (y == 0) return new List<State> {this};
            return Childos.SelectMany(x => x.GetChildosDeeper(y - 1)).ToList();
        }

        private void ApplyOperation(Tuple<int, int> mover)
        {
            Bucks[mover.Item1] /= 2;
            Bucks[mover.Item2] += Bucks[mover.Item1];
            Bucks.Sort();
        }

        private void KillMe() => Daddy.Childos.Remove(this);

        private void HelpMeIDontWannaGetAdopted(State NewDaddy)
        {
            if (NewDaddy.Iterations >= Daddy.Iterations) throw new Exception("My daddy is far better");
            Daddy.Childos.Remove(this);
            Daddy = NewDaddy;
        }
    }
}