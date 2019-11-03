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
            new Romino(2, new[] { new Vector2Int(0, 0), new Vector2Int(1, 1) }, new Vector2Int(0, 0),
                // These are hardcoded in by hand, because this array is only populated lazily by appending, rather than computed once. 
                // As this first romino can not be computed like other rominos, this wont be populated using normal methods.
                new[] { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                        new Vector2Int(-1, 0),                                                new Vector2Int(2, 0),
                        new Vector2Int(-1, 1),                                                new Vector2Int(2, 1),
                                                new Vector2Int(0, 2),  new Vector2Int(1, 2),  new Vector2Int(2, 2), });

        public readonly int BlockCount;
        public readonly Vector2Int[] Blocks;
        public readonly Vector2Int DiagonalRoot;

        public readonly Vector2Int[] PossibleExtensions;

        internal static readonly ArrayPool<Vector2Int> ArrayPool = ArrayPool<Vector2Int>.Shared;

        public Vector2Int[] DiagonalRootBlockade => new[]
        {
            DiagonalRoot + new Vector2Int(0, 0),
            DiagonalRoot + new Vector2Int(0, 1),
            DiagonalRoot + new Vector2Int(1, 0),
            DiagonalRoot + new Vector2Int(1, 1)
        };

        private readonly BitBuffer512 _uniqueCode;

        public Romino(int blockCount, Vector2Int[] blocks, Vector2Int diagonalRoot, Vector2Int[] possibleExtensions)
        {
            int minX = int.MaxValue, minY = int.MaxValue;
            for (int i = 0; i < blockCount; i++)
            {
                if (blocks[i].X < minX) minX = blocks[i].X;
                if (blocks[i].Y < minY) minY = blocks[i].Y;
            }

            Vector2Int offset = -new Vector2Int(minX, minY);

            BlockCount = blockCount;
            DiagonalRoot = diagonalRoot + offset;

            Blocks = blocks;
            PossibleExtensions = possibleExtensions;
            for (int i = 0; i < blockCount; i++) // LINQ would enumerate longer than blockCount if the array was longer, so do it manually
            {
                Blocks[i] += offset;
                PossibleExtensions[i] += offset;
            }

            _uniqueCode = CalculateUniqueCode(BlockCount, Blocks);
        }

        private static BitBuffer512 CalculateUniqueCode(int blockCount, Vector2Int[] blocks)
        {
            var bits = new BitBuffer512();

            for (int i = 0; i < blockCount; i++)
            {
                bits[blocks[i].X + (blocks[i].Y * blockCount)] = true;
            }

            return bits;
        }

        public readonly Romino Orient()
        {
            Romino max = this;

            foreach (var permutation in Permutations)
            {
                if (permutation._uniqueCode > max._uniqueCode)
                {
                    ArrayPool.Return(max.Blocks);
                    max = permutation;
                }
                else
                {
                    ArrayPool.Return(permutation.Blocks);
                }
            }

            return max;
        }

        public readonly Romino[] Permutations => new[]
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
            new Romino(BlockCount, Blocks.Select(BlockCount, func), diagonalRootFunc(DiagonalRoot), PossibleExtensions.SelectF(func));

        public readonly IEnumerable<Romino> AddOneNotUnique()
        {
            // Copy these to locals for use in lambdas
            var blockCount = BlockCount;
            var blocks = Blocks;
            var diagonalRoot = DiagonalRoot;
            var diagonalRootBlockade = DiagonalRootBlockade;
            var possibleExtensions = PossibleExtensions;

            return PossibleExtensions
                .SelectF(newBlock =>
                {
                    var extensionsFromNewBlock = (new[]
                    {
                        newBlock + new Vector2Int(0, -1),
                        newBlock + new Vector2Int(0, 1),
                        newBlock + new Vector2Int(1, 0),
                        newBlock + new Vector2Int(1, -1),
                        newBlock + new Vector2Int(1, 1),
                        newBlock + new Vector2Int(-1, 0),
                        newBlock + new Vector2Int(-1, -1),
                        newBlock + new Vector2Int(-1, 1),
                    })
                    // Remove already occupied positions, as well as exclude positions blocked by the diagonal
                    .Except(blocks.Concat(diagonalRootBlockade));

                    // Remove the added block and add the new, now appendable positions
                    var newPossibleExtensions = possibleExtensions.WhereF(x => x != newBlock).Union(extensionsFromNewBlock).ToArray();

                    return new Romino(blockCount + 1, AppendOne(blocks, newBlock), diagonalRoot, newPossibleExtensions).Orient();
                });
        }

        private readonly bool[,] GetBlock2DArray()
        {
            int maxX = 0, maxY = 0;
            for (int i = 0; i < BlockCount; i++)
            {
                if (Blocks[i].X > maxX) maxX = Blocks[i].X;
                if (Blocks[i].Y > maxY) maxY = Blocks[i].Y;
            }

            var blocks = new bool[maxX + 1, maxY + 1];

            for (int i = 0; i < BlockCount; i++) blocks[Blocks[i].X, Blocks[i].Y] = true;

            return blocks;
        }

        private static Vector2Int[] AppendOne(Vector2Int[] arr, Vector2Int elem)
        {
            int length = arr.Length;

            Vector2Int[] newArr = ArrayPool.Rent(length + 1);

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

        private static readonly Dictionary<(bool isBlock, bool isDiagonalBlockade, bool isPossibleExtensions), char> AsciiArtChars = new Dictionary<(bool isBlock, bool isDiagonalBlockade, bool isPossibleExtensions), char>
        {
            [(false, false, false)] = ' ',
            [(false, false, true)] = '·',
            [(false, true, false)] = '░',
            [(false, true, true)] = 'e', // Common error, output instead of crash
            [(true, false, false)] = '█',
            [(true, false, true)] = 'E', // Common error, output instead of crash
            [(true, true, false)] = '▓',
        };

        public IEnumerable<string> ToAsciiArt(bool highlightDiagonalBlockade = false, bool highlightPossibleExtensions = false)
        {
            bool[,] blocks = GetBlock2DArray();

            var buffer = new StringBuilder();

            for (int i = -1; i <= blocks.GetLength(0); i++)
            {
                for (int j = -1; j <= blocks.GetLength(1); j++)
                {
                    Vector2Int vec = new Vector2Int(i, j);

                    bool block = i >= 0 && j >= 0 && i < blocks.GetLength(0) && j < blocks.GetLength(1) && blocks[i, j];

                    bool diagonalBlockade = highlightDiagonalBlockade && DiagonalRootBlockade.Contains(vec);
                    bool possibleExtension = highlightPossibleExtensions && PossibleExtensions.Contains(vec);

                    buffer.Append(AsciiArtChars[(block, diagonalBlockade, possibleExtension)]);
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
                        .SelectMany(x => x.AddOneNotUnique())
                        .OrderBy(x => x._uniqueCode).ToList();

                    Romino currentRomino = newRominos[0];
                    int pos = 1;
                    for (int j = 1; j < newRominos.Count; j++)
                    {
                        var newRomino = newRominos[pos] = newRominos[j];

                        if (currentRomino == newRomino)
                        {
                            ArrayPool.Return(newRomino.Blocks);
                            pos++;
                        }
                        else
                        {
                            currentRomino = newRomino;
                        }
                    }
                    newRominos.RemoveRange(pos, newRominos.Count - pos);

                    var newRominosArray = newRominos.ToArray();
                    lastRominos = newRominosArray;
                    yield return (i, newRominosArray);
                }
            }
        }
    }

    internal static class Linq
    {
        public static Vector2Int[] Select(this Vector2Int[] source, int length, Func<Vector2Int, Vector2Int> func)
        {
            var target = Romino.ArrayPool.Rent(length);
            for (int i = 0; i < length; i++)
            {
                target[i] = func(source[i]);
            }
            return target;
        }
    }
}
