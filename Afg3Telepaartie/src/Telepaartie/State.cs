namespace Telepaartie
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class State : IEquatable<State>
    {
        public int Iterations { get => Daddy == null ? 0 : (Daddy.Iterations + 1); }
        public State Daddy { get; }
        public List<int> Buckets { get; }
        private int _hashCode;

        public State(List<int> end)
        {
            Buckets = end.ToList();
            Buckets.Sort();
            Daddy = null;
            UpdateHashCode();
        }

        private void UpdateHashCode() =>
            _hashCode = Buckets.Aggregate(168560841, (x, y) => (x * -1521134295) + y);

        private State(State origin, (int, int) mover)
        {
            Buckets = origin.Buckets.ToList();
            ApplyOperation(mover.Item1, mover.Item2);

            Daddy = origin;
            UpdateHashCode();
        }

        public IEnumerable<State> GetNextGen()
        {
            for (int i = 0; i < Buckets.Count; i++)
            {
                for (int u = 0; u < Buckets.Count; u++)
                {
                    if (Buckets[i] % 2 == 0 && Buckets[i] > 0 && i != u)
                    {
                        yield return new State(this, (i, u));
                    }
                }
            }
        }

        public bool Equals(State state)
        {
            if (state == null) return false;
            if (state.Buckets.Count != Buckets.Count) throw new ArgumentException();

            for (int i = 0; i < Buckets.Count; i++)
            {
                if (state.Buckets[i] != Buckets[i]) return false;
            }
            return true;
        }

        private void ApplyOperation(int first, int second)
        {
            Buckets[first] /= 2;
            Buckets[second] += Buckets[first];
            Buckets.Sort();
        }

        public override bool Equals(object obj) => obj is State state && Equals(state);

        public override int GetHashCode() => _hashCode;

        public override string ToString() => "State (Iter:" + Iterations + ") {" + string.Join(';', Buckets) + "}";
    }
}