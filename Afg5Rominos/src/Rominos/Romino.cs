namespace Rominos
{
    using MoreLinq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Numerics;
    using System.Text;
    using System.Threading.Tasks;

    public readonly struct Romino : IEquatable<Romino>
    {
        public static Romino One =
            new Romino(new[] { new Vector2Int(0, 0), new Vector2Int(1, 1) }, new Vector2Int(0, 0), false);

        public readonly Vector2Int[] Blocks;
        public readonly Vector2Int DiagonalRoot;

        public IEnumerable<Vector2Int> DiagonalRootBlockade => new[]
        {
            DiagonalRoot + new Vector2Int(0, 0),
            DiagonalRoot + new Vector2Int(0, 1),
            DiagonalRoot + new Vector2Int(1, 0),
            DiagonalRoot + new Vector2Int(1, 1)
        };

        public readonly BigInteger UniqueCode;

        public Romino(Vector2Int[] blocks, Vector2Int diagonalRoot, bool check = true)
        {
            DiagonalRoot = diagonalRoot;

            if (!check)
            {
                Blocks = blocks;
                UniqueCode = GetUniqueCode();
                return;
            }

            Vector2Int offset = -new Vector2Int(blocks.Min(x => x.X), blocks.Min(x => x.Y));

            Blocks = blocks.Select(x => x + offset).ToArray();

            UniqueCode = GetUniqueCode();
        }

        public Romino(IEnumerable<Vector2Int> blocks, Vector2Int diagonalRoot, bool check = true)
        {
            DiagonalRoot = diagonalRoot;

            if (!check)
            {
                Blocks = blocks.ToArray();
                UniqueCode = GetUniqueCode();
                return;
            }

            Vector2Int offset = -new Vector2Int(blocks.Min(x => x.X), blocks.Min(x => x.Y));

            Blocks = blocks.Select(x => x + offset).ToArray();

            UniqueCode = GetUniqueCode();
        }

        private static BigInteger GetWeight(int x, int y, int size) =>
            BigInteger.One << ((y * size) + x);

        private readonly BigInteger GetUniqueCode()
        {
            int blockCount = Blocks.Length;

            BigInteger accumulator = 0;

            foreach (var block in Blocks)
            {
                accumulator += GetWeight(block.X, block.Y, blockCount);
            }

            return accumulator;
        }

        public readonly Romino Orient() => GetPermutations().MinBy(x => x.UniqueCode).First();

        public readonly IEnumerable<Romino> GetRotations()
        {
            yield return Rotate(0);
            yield return Rotate(1);
            yield return Rotate(2);
            yield return Rotate(3);
        }

        public readonly IEnumerable<Romino> GetPermutations() => GetRotations()
            .SelectMany(x => new[]
            {
                x,
                x.Mirror(Axis.X),
                x.Mirror(Axis.X)
            });

        public readonly Romino Mirror(Axis axis) => axis switch
        {
            Axis.X => ProjectVoxels(x => new Vector2Int(-x.X, x.Y), x => new Vector2Int(-1 - x.X, x.Y)),
            Axis.Y => ProjectVoxels(x => new Vector2Int(x.X, -x.Y), x => new Vector2Int(x.X, -1 - x.Y)),
            _ => throw new ArgumentException(nameof(axis)),
        };

        //public Romino Rotate(int clockwiseTurns) => Rotate((uint)(clockwiseTurns < 0 ? 4 + (clockwiseTurns % 4) : clockwiseTurns));
        public readonly Romino Rotate(uint clockwiseTurns) => (clockwiseTurns % 4) switch
        {
            0 => this,
            1 => ProjectVoxels(x => new Vector2Int(-x.Y, x.X), x => new Vector2Int(-1 - x.Y, x.X)),
            2 => ProjectVoxels(x => new Vector2Int(-x.X, -x.Y), x => new Vector2Int(-1 - x.X, -1 - x.Y)),
            3 => ProjectVoxels(x => new Vector2Int(x.Y, -x.X), x => new Vector2Int(x.Y, -1 - x.X)),
            _ => throw new ArithmeticException("Modulo with 4 was equal or greater than 4"),
        };

        private readonly Romino ProjectVoxels(Func<Vector2Int, Vector2Int> func, Func<Vector2Int, Vector2Int> diagonalRootFunc) =>
            new Romino(Blocks.Select(func), diagonalRootFunc(DiagonalRoot));

        //public readonly IEnumerable<Romino> AddOne() => AddOneNotUnique().Distinct();

        public readonly IEnumerable<Romino> AddOneNotUnique()
        {
            var corners = Blocks.SelectMany(x => new[]
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
            var blocks = new bool[Blocks.Max(x => x.X) + 1, Blocks.Max(x => x.Y) + 1];

            Parallel.ForEach(Blocks, block => blocks[block.X, block.Y] = true);
            return blocks;
        }

        private static T[] AppendOne<T>(T[] arr, T elem)
        {
            T[] newArr = new T[arr.Length + 1];

            Array.Copy(arr, 0, newArr, 0, arr.Length);

            newArr[arr.Length] = elem;

            return newArr;
        }

        public override readonly bool Equals(object obj) => obj is Romino romino && Equals(romino);

        public override readonly int GetHashCode() => UniqueCode.GetHashCode();

        public readonly bool Equals(Romino romino) => UniqueCode == romino.UniqueCode;

        public static bool operator ==(Romino left, Romino right) => left.Equals(right);

        public static bool operator !=(Romino left, Romino right) => !(left == right);

        public IEnumerable<string> ToAsciiArt()
        {
            bool[,] blocks = GetBlock2DArray();

            var buffer = new StringBuilder();

            for (int i = 0; i < blocks.GetLength(0); i++)
            {
                for (int j = 0; j < blocks.GetLength(1); j++)
                {
                    buffer.Append(blocks[i, j] ? '█' : ' ');
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
                Romino[] lastRominos = new[] { One };

                yield return (2, lastRominos);

                for (int i = 3; i <= size; i++)
                {
                    var newRominos = lastRominos.AsParallel()
                        .SelectMany(x => x.AddOneNotUnique()).Distinct().ToArray();
                    lastRominos = newRominos;
                    yield return (i, newRominos);
                }
            }
        }
    }
}
