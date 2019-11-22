namespace Telepaartie
{
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

            temp[first] /= 2;
            temp[second] += temp[first];
            temp.Sort();

            return new State(temp, this);
        }

        public IEnumerable<State> GetNextGen()
        {
            for (int i = 0; i < Buckets.Count; i++)
            {
                for (int u = 0; u < Buckets.Count; u++)
                {
                    if (Buckets[i] % 2 == 0 && Buckets[i] > 0)
                    {
                        yield return ReverseTeelepartie(i, u);
                    }
                }
            }
        }

        private int CalculateHashCode() =>
            Buckets.Aggregate(168560841, (x, y) => (x * -1521134295) + y);

        public static IEnumerable<List<int>> AllEndingStates(int numberOfCups, int numberOfItems)
        {
            foreach (var state in AllPossibleStates(numberOfCups - 1, numberOfItems, numberOfItems))
            {
                state.Add(0);
                yield return state;
            }
        }

        public static IEnumerable<List<int>> AllPossibleStates(int numberOfCups, int numberOfItems, int max)
        {
            if (numberOfCups < 1) yield break;
            if (numberOfCups == 1) yield return new List<int> { numberOfItems };

            int min = ((numberOfItems - 1) / numberOfCups) + 1;

            for (int i = min; i < Math.Min(max + 1, numberOfItems); i++)
            {
                foreach (var state in AllPossibleStates(numberOfCups - 1, numberOfItems - i, i))
                {
                    state.Add(i);
                    yield return state;
                }
            }
        }

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