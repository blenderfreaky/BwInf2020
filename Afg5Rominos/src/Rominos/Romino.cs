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

    public class Romino : IEquatable<Romino>, IComparable<Romino>
    {
        public static Romino One =
            new Romino(new List<Vector2Int> { new Vector2Int(0, 0), new Vector2Int(1, 1) }, new Vector2Int(0, 0),
                // These are hardcoded in by hand, because this list is only populated lazily by appending, rather than computed once. 
                // As this first romino can not be computed like other rominos, this won't be populated using normal methods.
                new List<Vector2Int> { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                        new Vector2Int(-1, 0),                                                new Vector2Int(2, 0),
                        new Vector2Int(-1, 1),                                                new Vector2Int(2, 1),
                                                new Vector2Int(0, 2),  new Vector2Int(1, 2),  new Vector2Int(2, 2), });

        public readonly List<Vector2Int> Blocks;
        public Vector2Int DiagonalRoot;

        public readonly List<Vector2Int> PossibleExtensions;

        public Vector2Int[] DiagonalRootBlockade => new[]
        {
            DiagonalRoot + new Vector2Int(0, 0),
            DiagonalRoot + new Vector2Int(0, 1),
            DiagonalRoot + new Vector2Int(1, 0),
            DiagonalRoot + new Vector2Int(1, 1)
        };

        private BitBuffer512 _uniqueCode;

        public Romino(List<Vector2Int> blocks, Vector2Int diagonalRoot, List<Vector2Int> possibleExtensions)
        {
            Blocks = blocks;
            DiagonalRoot = diagonalRoot;
            PossibleExtensions = possibleExtensions;

            _uniqueCode = default;

            FixDisplacement();

            _uniqueCode = CalculateUniqueCode(Blocks);
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

        public IEnumerable<Romino> AddOneNotUnique()
        {
            // Copy these to locals for use in lambdas
            var blocks = Blocks;
            var diagonalRoot = DiagonalRoot;
            var diagonalRootBlockade = DiagonalRootBlockade;
            var possibleExtensions = PossibleExtensions;

            return PossibleExtensions
                .SelectF(newBlock =>
                {
                    IEnumerable<Vector2Int> extensionsFromNewBlock = (new[]
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
                    List<Vector2Int> newPossibleExtensions = possibleExtensions.WhereF(x => x != newBlock).Union(extensionsFromNewBlock).ToList();

                    List<Vector2Int> newBlocks = new List<Vector2Int>(blocks)
                    {
                        newBlock
                    };

                    var romino = new Romino(newBlocks, diagonalRoot, newPossibleExtensions);
                    romino.Orient();
                    return romino;
                });
        }

        private static BitBuffer512 CalculateUniqueCode(List<Vector2Int> blocks)
        {
            static int GetWeight(int x, int y, int size) => (y * size) + x;

            var bits = new BitBuffer512();

            int length = blocks.Count;

            foreach (var block in blocks)
            {
                bits[GetWeight(block.X, block.Y, length)] = true;
            }

            return bits;
        }

        private static readonly (Func<Vector2Int, Vector2Int> BlockMap, Func<Vector2Int, Vector2Int> DiagonalRootMap)[] Steps = new (Func<Vector2Int, Vector2Int> BlockMap, Func<Vector2Int, Vector2Int> DiagonalRootMap)[]
        {                                                                       // Map  Step
            (x => new Vector2Int(+x.X, -x.Y), x => new Vector2Int(+x.X, ~x.Y)), // +x+y +x-y
            (x => new Vector2Int(-x.X, -x.Y), x => new Vector2Int(~x.X, ~x.Y)), // +x-y -x-y
            (x => new Vector2Int(+x.X, -x.Y), x => new Vector2Int(+x.X, ~x.Y)), // -x+y +x-y
            (x => new Vector2Int(-x.Y, -x.X), x => new Vector2Int(~x.Y, ~x.X)), // -x-y -y-x
            (x => new Vector2Int(+x.X, -x.Y), x => new Vector2Int(+x.X, ~x.Y)), // +y+x +x-y
            (x => new Vector2Int(-x.X, -x.Y), x => new Vector2Int(~x.X, ~x.Y)), // +y-x -x-y
            (x => new Vector2Int(+x.X, -x.Y), x => new Vector2Int(+x.X, ~x.Y)), // -y+x +x-y
            (x => new Vector2Int(-x.X, -x.Y), x => new Vector2Int(~x.X, ~x.Y)), // -y-x -y-x
        };

        private static readonly (Func<Vector2Int, Vector2Int> BlockMap, Func<Vector2Int, Vector2Int> DiagonalRootMap)[] Maps = new (Func<Vector2Int, Vector2Int> BlockMap, Func<Vector2Int, Vector2Int> DiagonalRootMap)[]
        {                                                                       // Map  Step
            (x => new Vector2Int(+x.X, +x.Y), x => new Vector2Int(+x.X, +x.Y)), // +x+y +x-y
            (x => new Vector2Int(+x.X, -x.Y), x => new Vector2Int(+x.X, ~x.Y)), // +x-y -x-y
            (x => new Vector2Int(-x.X, +x.Y), x => new Vector2Int(~x.X, +x.Y)), // -x+y +x-y
            (x => new Vector2Int(-x.X, -x.Y), x => new Vector2Int(~x.X, ~x.Y)), // -x-y -y-x
            (x => new Vector2Int(+x.Y, +x.X), x => new Vector2Int(+x.Y, +x.X)), // +y+x +x-y
            (x => new Vector2Int(+x.Y, -x.X), x => new Vector2Int(+x.Y, ~x.X)), // +y-x -x-y
            (x => new Vector2Int(-x.Y, +x.X), x => new Vector2Int(~x.Y, +x.X)), // -y+x +x-y
            (x => new Vector2Int(-x.Y, -x.X), x => new Vector2Int(~x.Y, ~x.X)), // -y-x -y-x
        };

        public void Orient()
        {
            int minIndex = -1;
            BitBuffer512 min = default;

            for (int i = 0; i < Steps.Length; i++)
            {
                ProjectVoxels(Steps[i].BlockMap, Steps[i].DiagonalRootMap);
                FixDisplacement();
                _uniqueCode = CalculateUniqueCode(Blocks);
                if (_uniqueCode < min)
                {
                    minIndex = 1;
                    min = _uniqueCode;
                }
            }

            ProjectVoxels(Maps[(minIndex + 7) % 8].BlockMap, Maps[(minIndex + 7) % 8].DiagonalRootMap);
            FixDisplacement();
            _uniqueCode = CalculateUniqueCode(Blocks);
        }

        private void ProjectVoxels(Func<Vector2Int, Vector2Int> func, Func<Vector2Int, Vector2Int> diagonalRootFunc)
        {
            Blocks.SelectInPlaceF(func);
            PossibleExtensions.SelectInPlaceF(func);
            DiagonalRoot = diagonalRootFunc(DiagonalRoot);
        }

        private void FixDisplacement()
        {
            Vector2Int offset = -new Vector2Int(Blocks.MinF(x => x.X), Blocks.MinF(x => x.Y));

            Blocks.SelectInPlaceF(x => x + offset);
            DiagonalRoot += offset;
            PossibleExtensions.SelectInPlaceF(x => x + offset);
        }

        private bool[,] GetBlock2DArray()
        {
            var blocks = new bool[Blocks.MaxF(x => x.X) + 1, Blocks.MaxF(x => x.Y) + 1];

            Parallel.ForEach(Blocks, block => blocks[block.X, block.Y] = true);
            return blocks;
        }

        public override bool Equals(object obj) => obj is Romino romino && Equals(romino);

        public override int GetHashCode() => _uniqueCode.GetHashCode();

        public bool Equals(Romino romino) => _uniqueCode == romino._uniqueCode;

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

        public static bool operator <(Romino left, Romino right)
        {
            return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : left.CompareTo(right) < 0;
        }

        public static bool operator <=(Romino left, Romino right)
        {
            return ReferenceEquals(left, null) || left.CompareTo(right) <= 0;
        }

        public static bool operator >(Romino left, Romino right)
        {
            return !ReferenceEquals(left, null) && left.CompareTo(right) > 0;
        }

        public static bool operator >=(Romino left, Romino right)
        {
            return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.CompareTo(right) >= 0;
        }
    }
}
