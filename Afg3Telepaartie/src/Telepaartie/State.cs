namespace Telepaartie
{
        using System;
        using System.Collections.Generic;
        using System.Linq;

        public class State : IEquatable<State>
        {
        public int Depth => Parent == null ? 0 : (Parent.Depth + 1);

        public State? Parent { get; }

        public int[] Buckets { get; }

        private readonly int _hashCode;

        public State(IEnumerable<int> unsortedBuckets, State? parent = null)
        {
            if (unsortedBuckets.Any(x => x < 0)) throw new ArgumentException(nameof(unsortedBuckets));

            Buckets = unsortedBuckets.ToArray();
            Array.Sort(Buckets);

            Parent = parent;
            _hashCode = CalculateHashCode();
        }

        private State(int[] sortedBuckets, State? parent = null)
        {
            Buckets = sortedBuckets;
            Parent = parent;
            _hashCode = CalculateHashCode();
        }

        private State ReverseTeelepartie(int originalTarget, int originalSource)
        {
            int[] temp = new int[Buckets.Length];
            Buckets.CopyTo(temp, 0);

            temp[originalTarget] /= 2;
            temp[originalSource] += temp[originalTarget];
            Array.Sort(temp);

            return new State(temp, this);
        }

        public IEnumerable<State> Origins()
        {
            // Finden jeder Kombination
            for (int i = 0; i < Buckets.Length; i++)
            {
                for (int u = 0; u <= i; u++)
                {
                    // Zulässige Werte rausfiltern
                    if (Buckets[i] % 2 == 0 && Buckets[i] > 0)
                    {
                        // und die bearbeitete Version zurückgeben
                        yield return ReverseTeelepartie(i, u);
                    }

                    if (Buckets[u] % 2 == 0 && Buckets[u] > 0)
                    {
                        yield return ReverseTeelepartie(u, i);
                    }
                }
            }
        }

        private int CalculateHashCode()
        {
            int hashCode = 168560841;

            for (int i = 0; i < Buckets.Length; i++)
            {
                hashCode = (hashCode * -1521134295) + Buckets[i].GetHashCode();
            }

            return hashCode;
        }

        public static IEnumerable<List<int>> AllEndingStates(int numberOfCups, int numberOfItems)
        {
            foreach (var state in AllPossibleStates(numberOfCups - 1, numberOfItems, numberOfItems))
            {
                state.Add(0);
                yield return state;
            }
        }

        public static IEnumerable<List<int>> AllPossibleStates(int numberOfCups, int numberOfItems, int previousMax)
        {
            if (numberOfCups < 1) yield break;
            if (numberOfCups == 1) yield return new List<int> { numberOfItems };

            // Die Elementanzahl die mindestens dem aktuellen Behälter hinzugefügt werden muss
            int min = ((numberOfItems - 1) / numberOfCups) + 1;

            // Die Elementanzahl die maximal dem aktuellen Behälter hinzugefügt werden kann
            int max = Math.Min(previousMax + 1, numberOfItems);

            for (int i = min; i < max; i++)
            {
                // Finden aller Möglichen Kombinationen für den Rest der Biber und der Behälteranzahl -1
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
            if (Buckets.Length != state.Buckets.Length) return false;
            if (_hashCode != state._hashCode) return false;

            for (int i = 0; i < Buckets.Length; i++)
            {
                if (Buckets[i] != state.Buckets[i]) return false;
            }

            return true;
        }

        public override int GetHashCode() => _hashCode;

        public override string ToString() => "State (Depth:" + Depth + ") {" + string.Join(';', Buckets) + "}";

        #endregion Overrides and Interface Implementations
    }
}