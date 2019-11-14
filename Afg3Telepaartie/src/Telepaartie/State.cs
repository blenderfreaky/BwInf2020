namespace Telepaartie
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class State : IEquatable<State>
    {
        public int Iterations { get => Daddy == null ? 0 : (Daddy.Iterations + 1); }
        public State Daddy { get; }
        public IReadOnlyList<int> Buckets { get; }
        private readonly int _hashCode;

        public State(List<int> end, State daddy = null)
        {
            List<int> temp = end.ToList();
            temp.Sort();
            Buckets = temp;
            Daddy = daddy;
            _hashCode = CalculateHashCode();
        }

        private static State ReverseTeelepartie(State daddy, int first, int second)
        {
            List<int> temp = daddy.Buckets.ToList();

            temp[first] /= 2;
            temp[second] += temp[first];
            temp.Sort();

            return new State(temp, daddy);
        }

        public IEnumerable<State> GetNextGen()
        {
            for (int i = 0; i < Buckets.Count; i++)
            {
                for (int u = 0; u < Buckets.Count; u++)
                {
                    if (Buckets[i] % 2 == 0 && Buckets[i] > 0 && i != u)
                    {
                        yield return ReverseTeelepartie(this, i, u);
                    }
                }
            }
        }

        private int CalculateHashCode() =>
            Buckets.Aggregate(168560841, (x, y) => (x * -1521134295) + y);

        #region Overrides and Interface Implementations
        public override bool Equals(object obj) => obj is State state && Equals(state);

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

        public override int GetHashCode() => _hashCode;

        public override string ToString() => "State (Iter:" + Iterations + ") {" + string.Join(';', Buckets) + "}";
        #endregion
    }
}