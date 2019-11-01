namespace Rominos
{
    using JM.LinqFaster;
    using JM.LinqFaster.Parallel;
    using MoreLinq;
    using System;
    using System.Buffers;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    public readonly struct Romino : IEquatable<Romino>, IComparable<Romino>
    {
        public static Romino One =
            new Romino(new[] { new Vector2Int(0, 0), new Vector2Int(1, 1) }, new Vector2Int(0, 0), false);

        public readonly Vector2Int[] Blocks;
        public readonly Vector2Int DiagonalRoot;

        private static readonly ArrayPool<Vector2Int> ArrayPool = ArrayPool<Vector2Int>.Shared; // TODO: Use this

        public IEnumerable<Vector2Int> DiagonalRootBlockade => new[]
        {
            DiagonalRoot + new Vector2Int(0, 0),
            DiagonalRoot + new Vector2Int(0, 1),
            DiagonalRoot + new Vector2Int(1, 0),
            DiagonalRoot + new Vector2Int(1, 1)
        };

        private readonly BitBuffer512 _uniqueCode;

        public Romino(Vector2Int[] blocks, Vector2Int diagonalRoot, bool check = true)
        {
            if (!check)
            {
                DiagonalRoot = diagonalRoot;
                Blocks = blocks;
                _uniqueCode = CalculateUniqueCode(blocks);
                return;
            }

            Vector2Int offset = -new Vector2Int(blocks.MinF(x => x.X), blocks.MinF(x => x.Y));

            Blocks = blocks;
            Blocks.SelectInPlaceF(x => x + offset);
            DiagonalRoot = diagonalRoot + offset;

            _uniqueCode = CalculateUniqueCode(blocks);
        }

        private static BitBuffer512 CalculateUniqueCode(Vector2Int[] blocks)
        {
            static int GetWeight(int x, int y, int size) => (y * size) + x;

            var bits = new BitBuffer512();

            int length = blocks.Length;

            foreach (var block in blocks)
            {
                bits[GetWeight(block.X, block.Y, length)] = true;
            }

            return bits;
        }

        public readonly Romino Orient() => GetPermutations().MinBy(x => x._uniqueCode).First();

        public readonly Romino[] GetPermutations() => new[]
        {
            this,
            ProjectVoxels(x => new Vector2Int(-x.X, x.Y), x => new Vector2Int(-1 - x.X, x.Y)),
            ProjectVoxels(x => new Vector2Int(x.X, -x.Y), x => new Vector2Int(x.X, -1 - x.Y)),
            ProjectVoxels(x => new Vector2Int(-x.X, -x.Y), x => new Vector2Int(-1 - x.X, -1 - x.Y)),

            ProjectVoxels(x => new Vector2Int(x.Y, x.X), x => new Vector2Int(x.Y, x.X)),
            ProjectVoxels(x => new Vector2Int(-x.Y, x.X), x => new Vector2Int(-1 - x.Y, x.X)),
            ProjectVoxels(x => new Vector2Int(x.Y, -x.X), x => new Vector2Int(x.Y, -1 - x.X)),
            ProjectVoxels(x => new Vector2Int(-x.Y, -x.X), x => new Vector2Int(-1 - x.Y, -1 - x.X)),
        };

        private readonly Romino ProjectVoxels(Func<Vector2Int, Vector2Int> func, Func<Vector2Int, Vector2Int> diagonalRootFunc) =>
            new Romino(Blocks.SelectF(func), diagonalRootFunc(DiagonalRoot));

        public readonly IEnumerable<Romino> AddOneNotUnique()
        {
            var corners = Blocks.SelectManyF(x => new[]
            {
                new Vector2Int(x.X, x.Y-1),
                new Vector2Int(x.X, x.Y+1),
                new Vector2Int(x.X+1, x.Y),
                new Vector2Int(x.X+1, x.Y-1),
                new Vector2Int(x.X+1, x.Y+1),
                new Vector2Int(x.X-1, x.Y),
                new Vector2Int(x.X-1, x.Y-1),
                new Vector2Int(x.X-1, x.Y+1),
            });

            // Copy these to locals for use in lambdas
            var blocks = Blocks;
            var diagonalRoot = DiagonalRoot;

            // Remove duplicates and already occupied positions, as well as exclude positions blocked by the diagonal
            return corners
                .Distinct().Except(blocks.AsParallel()).Except(DiagonalRootBlockade.AsParallel())
                .Select(newBlock => new Romino(AppendOne(blocks, newBlock), diagonalRoot).Orient());
        }

        private readonly bool[,] GetBlock2DArray()
        {
            var blocks = new bool[Blocks.MaxF(x => x.X) + 1, Blocks.MaxF(x => x.Y) + 1];

            Parallel.ForEach(Blocks, block => blocks[block.X, block.Y] = true);
            return blocks;
        }

        private static T[] AppendOne<T>(T[] arr, T elem)
        {
            int length = arr.Length;

            T[] newArr = new T[length + 1];

            Array.Copy(arr, 0, newArr, 0, arr.Length);

            newArr[arr.Length] = elem;

            return newArr;
        }

        public override readonly bool Equals(object obj) => obj is Romino romino && Equals(romino);

        public override readonly int GetHashCode() => _uniqueCode.GetHashCode();

        public readonly bool Equals(Romino romino) => _uniqueCode == romino._uniqueCode;

        public int CompareTo(Romino other) => _uniqueCode.CompareTo(other._uniqueCode);

        public static bool operator ==(Romino left, Romino right) => left.Equals(right);

        public static bool operator !=(Romino left, Romino right) => !(left == right);

        public IEnumerable<string> ToAsciiArt(bool highlightDiagonalBlockade = false)
        {
            bool[,] blocks = GetBlock2DArray();

            var buffer = new StringBuilder();

            for (int i = 0; i < blocks.GetLength(0); i++)
            {
                for (int j = 0; j < blocks.GetLength(1); j++)
                {
                    bool diagonalBlock = highlightDiagonalBlockade && DiagonalRootBlockade.Contains(new Vector2Int(i, j));
                    buffer.Append(blocks[i, j] ? (diagonalBlock ? '▓' : '█') : (diagonalBlock ? '·' : ' '));
                }

                yield return buffer.ToString();

                buffer.Clear();
            }
        }

        public static IEnumerable<(int Size, Romino[] Rominos)> GetRominosUntilSize(int size)
        {
            if (size < 2) throw new ArgumentOutOfRangeException(nameof(size));

            return GetRominosUntilSizeInternal();

            IEnumerable<(int Size, Romino[] Rominos)> GetRominosUntilSizeInternal()
            {
                var lastRominos = new List<Romino> { One };
                var newRominos = new List<Romino>();

                yield return (2, lastRominos.ToArray());

                for (int i = 3; i <= size; i++)
                {
                    newRominos.Clear();
                    foreach (var romino in lastRominos) newRominos.AddRange(romino.AddOneNotUnique());
                    newRominos.DistinctInPlaceF();
                    yield return (i, newRominos.ToArray());
                    (newRominos, lastRominos) = (lastRominos, newRominos);
                }
            }
        }
    }
}
