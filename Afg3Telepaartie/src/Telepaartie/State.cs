namespace Telepaartie
{
    #nullable enable 
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class State : IEquatable<State>
    {
        public int Iterations => Parent == null ? 0 : (Parent.Iterations + 1);
        public State? Parent { get; }
        public IReadOnlyList<int> Buckets { get; }
        private readonly int _hashCode;

        public State(IEnumerable<int> unsortedBuckets, State? parent = null)
        {
            if (unsortedBuckets.Any(x => x < 0)) throw new ArgumentException(nameof(unsortedBuckets));

            Buckets = unsortedBuckets.OrderBy(x => x).ToList();
            Parent = parent;
            _hashCode = CalculateHashCode();
        }

        private State(List<int> sortedBuckets, State? parent = null)
        {
            Buckets = sortedBuckets;
            Parent = parent;
            _hashCode = CalculateHashCode();
        }

        private State ReverseTeelepartie(int first, int second)
        {
            List<int> temp = new List<int>(Buckets);

            temp[first] /= 2;               //die Anzahl der Biber im ersten Behälter halbieren
            temp[second] += temp[first];    //und die Biber im anderen Behälter hinzufügen
            temp.Sort();

            return new State(temp, this);
        }

        public IEnumerable<State> GetNextGen()
        {
            for (int i = 0; i < Buckets.Count; i++)
            {
                for (int u = 0; u < Buckets.Count; u++) //Finden jeder Kombination
                {
                    if (Buckets[i] % 2 == 0 && Buckets[i] > 0) //Zulässige Werte rausfiltern
                    {
                        yield return ReverseTeelepartie(i, u);  //und die bearbeitete Version zurückgeben
                    }
                }
            }
        }

        private int CalculateHashCode() =>
            Buckets.Aggregate(168560841, (x, y) => (x * -1521134295) + y);

        #region Overrides and Interface Implementations

        public override bool Equals(object? obj) => obj is State state && Equals(state);

        public bool Equals(State state)
        {
            if (state == null) return false;
            if (state.Buckets.Count != Buckets.Count) return false;

            for (int i = 0; i < Buckets.Count; i++)
            {
                if (state.Buckets[i] != Buckets[i]) return false;
            }

            return true;
        }

        public override int GetHashCode() => _hashCode;

        public override string ToString() => "State (Iter:" + Iterations + ") {" + string.Join(';', Buckets) + "}";

        #endregion Overrides and Interface Implementations
    }
}