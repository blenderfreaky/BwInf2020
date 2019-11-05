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

    public struct Romino : IEquatable<Romino>, IComparable<Romino>
    {
        public static Romino One =
            new Romino(new[] { new Vector2Int(0, 0), new Vector2Int(1, 1) }, new Vector2Int(0, 0),
                // These are hardcoded in by hand, because this array is only populated lazily by appending, rather than computed once. 
                // As this first romino can not be computed like other rominos, this wont be populated using normal methods.
                new[] { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                        new Vector2Int(-1, 0),                                                new Vector2Int(2, 0),
                        new Vector2Int(-1, 1),                                                new Vector2Int(2, 1),
                                                new Vector2Int(0, 2),  new Vector2Int(1, 2),  new Vector2Int(2, 2), });

        public readonly List<Vector2Int> Blocks;
        public Vector2Int DiagonalRoot;

        public readonly List<Vector2Int> PossibleExtensions;

        private static readonly ArrayPool<Vector2Int> ArrayPool = ArrayPool<Vector2Int>.Shared; // TODO: Use this

        public Vector2Int[] DiagonalRootBlockade => new[]
        {
            DiagonalRoot + new Vector2Int(0, 0),
            DiagonalRoot + new Vector2Int(0, 1),
            DiagonalRoot + new Vector2Int(1, 0),
            DiagonalRoot + new Vector2Int(1, 1)
        };

        private readonly BitBuffer512 _uniqueCode;

        public Romino(List<Vector2Int> blocks, Vector2Int diagonalRoot, List<Vector2Int> possibleExtensions)
        {
            Vector2Int offset = -new Vector2Int(blocks.MinF(x => x.X), blocks.MinF(x => x.Y));

            Blocks = blocks;
            Blocks.SelectInPlaceF(x => x + offset);
            DiagonalRoot = diagonalRoot + offset;
            PossibleExtensions = possibleExtensions;
            PossibleExtensions.SelectInPlaceF(x => x + offset);

            _uniqueCode = CalculateUniqueCode(blocks);
        }

        private static BitBuffer512 CalculateUniqueCode(List<Vector2Int> blocks)
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

        public readonly Romino Orient()
        {
            this,
            ProjectVoxels(x => new Vector2Int(-x.X, x.Y), x => new Vector2Int(-1 - x.X, x.Y)),
            ProjectVoxels(x => new Vector2Int(x.X, -x.Y), x => new Vector2Int(x.X, -1 - x.Y)),
            ProjectVoxels(x => new Vector2Int(-x.X, -x.Y), x => new Vector2Int(-1 - x.X, -1 - x.Y)),

            ProjectVoxels(x => new Vector2Int(x.Y, x.X), x => new Vector2Int(x.Y, x.X)),
            ProjectVoxels(x => new Vector2Int(-x.Y, x.X), x => new Vector2Int(-1 - x.Y, x.X)),
            ProjectVoxels(x => new Vector2Int(x.Y, -x.X), x => new Vector2Int(x.Y, -1 - x.X)),
            ProjectVoxels(x => new Vector2Int(-x.Y, -x.X), x => new Vector2Int(-1 - x.Y, -1 - x.X)),
        }

        private Romino ProjectVoxels(Func<Vector2Int, Vector2Int> func, Func<Vector2Int, Vector2Int> diagonalRootFunc)
        {
            Blocks.SelectInPlaceF(func);
            DiagonalRoot = diagonalRootFunc(DiagonalRoot);
            PossibleExtensions.SelectInPlaceF(func));
        }

        public readonly IEnumerable<Romino> AddOneNotUnique()
        {
            // Copy these to locals for use in lambdas
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
                    .Except(blocks).Except(diagonalRootBlockade);

                    // Remove the added block and add the new, now appendable positions
                    var newPossibleExtensions = possibleExtensions.WhereF(x => x != newBlock).Union(extensionsFromNewBlock).ToArray();

                    return new Romino(AppendOne(blocks, newBlock), diagonalRoot, newPossibleExtensions).Orient();
                });
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

        private static readonly Dictionary<(bool isBlock, bool isDiagonalBlockade, bool isPossibleExtensions), char> AsciiArtChars = new Dictionary<(bool isBlock, bool isDiagonalBlockade, bool isPossibleExtensions), char>
        {
            [(false, false, false)] = ' ',
            [(false, false, true)] = '·',
            [(false, true, false)] = '░',
            [(true, false, false)] = '█',
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
                        .SelectMany(x => x.AddOneNotUnique()).Distinct().ToArray();
                    lastRominos = newRominos;
                    yield return (i, newRominos);
                }
            }
        }
    }
}
