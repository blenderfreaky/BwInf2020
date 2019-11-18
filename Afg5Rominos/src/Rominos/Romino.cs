namespace Rominos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public readonly struct Romino : IEquatable<Romino>, IComparable<Romino>
    {
        private static readonly (Func<Vector2Int, Vector2Int> BlockMap, Func<Vector2Int, Vector2Int> DiagonalRootMap)[] Maps = new (Func<Vector2Int, Vector2Int> BlockMap, Func<Vector2Int, Vector2Int> DiagonalRootMap)[]
        {
            (x => new Vector2Int(+x.X, +x.Y), x => new Vector2Int(+x.X, +x.Y)),
            (x => new Vector2Int(+x.X, -x.Y), x => new Vector2Int(+x.X, ~x.Y)),
            (x => new Vector2Int(-x.X, +x.Y), x => new Vector2Int(~x.X, +x.Y)),
            (x => new Vector2Int(-x.X, -x.Y), x => new Vector2Int(~x.X, ~x.Y)),
            (x => new Vector2Int(+x.Y, +x.X), x => new Vector2Int(+x.Y, +x.X)),
            (x => new Vector2Int(+x.Y, -x.X), x => new Vector2Int(+x.Y, ~x.X)),
            (x => new Vector2Int(-x.Y, +x.X), x => new Vector2Int(~x.Y, +x.X)),
            (x => new Vector2Int(-x.Y, -x.X), x => new Vector2Int(~x.Y, ~x.X)),
        };

        public static Romino One =
            new Romino(2, new[] { new Vector2Int(0, 0), new Vector2Int(1, 1) },
                // These are hardcoded in by hand, because this list is only populated lazily by appending, rather than computed once.
                // As this first romino can not be computed like other rominos, this won't be populated using normal methods.
                new[] { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                        new Vector2Int(-1, 0),                                                new Vector2Int(2, 0),
                        new Vector2Int(-1, 1),                                                new Vector2Int(2, 1),
                                                new Vector2Int(0, 2),  new Vector2Int(1, 2),  new Vector2Int(2, 2), }.ToList(),
                new Vector2Int(0, 0),
                new Vector2Int(1, 1));

        public readonly Vector2Int[] Blocks;
        public readonly List<Vector2Int> PossibleExtensions;
        public readonly Vector2Int DiagonalRoot;
        public readonly Vector2Int Max;
        public readonly int BlockLength;

        private readonly BitBuffer512 _uniqueCode;

        public readonly IEnumerable<Vector2Int> DiagonalRootBlockade
        {
            get
            {
                yield return DiagonalRoot + new Vector2Int(0, 0);
                yield return DiagonalRoot + new Vector2Int(0, 1);
                yield return DiagonalRoot + new Vector2Int(1, 0);
                yield return DiagonalRoot + new Vector2Int(1, 1);
            }
        }

        public string AsciiArt => string.Join(Environment.NewLine, ToAsciiArt(true, true));

        public Romino(int blockLength, Vector2Int[] blocks, List<Vector2Int> possibleExtensions, Vector2Int diagonalRoot, Vector2Int max)
        {
            Blocks = blocks;
            DiagonalRoot = diagonalRoot;
            PossibleExtensions = possibleExtensions;
            Max = max;
            BlockLength = blockLength;

            _uniqueCode = default;

            {
                int maxIndex = 0;
                BitBuffer512 maxCode = BitBuffer512.Min;

                for (int i = 0; i < Maps.Length; i++)
                {
                    var uniqueCode = CalculateUniqueCode(Maps[i].BlockMap);
                    if (maxCode < uniqueCode)
                    {
                        maxIndex = i;
                        maxCode = uniqueCode;
                    }
                }

                if (maxIndex != 0)
                {
                    (Func<Vector2Int, Vector2Int> BlockMap, Func<Vector2Int, Vector2Int> DiagonalRootMap) = Maps[maxIndex];

                    var offset = CalculateOffset(BlockMap);

                    for (int i = 0; i < BlockLength; i++) Blocks[i] = BlockMap(Blocks[i]) + offset;
                    for (int i = 0; i < PossibleExtensions.Count; i++) PossibleExtensions[i] = BlockMap(PossibleExtensions[i]) + offset;

                    DiagonalRoot = DiagonalRootMap(DiagonalRoot) + offset;

                    // Don't add offset to max, it would might up with x or y equal to 0
                    var mappedMax = BlockMap(Max);
                    Max = new Vector2Int(Math.Abs(mappedMax.X), Math.Abs(mappedMax.Y));
                }

                _uniqueCode = CalculateUniqueCode();
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

        // Generate IEnumerable<T> instead of allocing a new array
        private static IEnumerable<Vector2Int> GetDirectNeighbours(Vector2Int block)
        {
            yield return block + new Vector2Int(0, -1);
            yield return block + new Vector2Int(0, 1);
            yield return block + new Vector2Int(1, 0);
            yield return block + new Vector2Int(1, -1);
            yield return block + new Vector2Int(1, 1);
            yield return block + new Vector2Int(-1, 0);
            yield return block + new Vector2Int(-1, -1);
            yield return block + new Vector2Int(-1, 1);
        }

        public readonly IEnumerable<Romino> AddOneNotUnique()
        {
            foreach (var newBlock in PossibleExtensions)
            {
                IEnumerable<Vector2Int> extensionsFromNewBlock =
                    GetDirectNeighbours(newBlock)
                    // Remove already occupied positions
                    .Except(Blocks.Take(BlockLength))
                    // Exclude positions blocked by the protected diagonal
                    .Except(DiagonalRootBlockade);

                var offset = new Vector2Int(Math.Max(-newBlock.X, 0), Math.Max(-newBlock.Y, 0));
                var newSize = new Vector2Int(Math.Max(newBlock.X, Max.X), Math.Max(newBlock.Y, Max.Y)) + offset;

                // Remove the added block and add the new, now appendable positions
                List<Vector2Int> newPossibleExtensions = PossibleExtensions.Where(x => x != newBlock).Union(extensionsFromNewBlock).Select(x => x + offset).ToList();

                yield return new Romino(
                    BlockLength + 1,
                    AppendOneAndSelectInPlace(Blocks, newBlock, offset, BlockLength),
                    newPossibleExtensions,
                    DiagonalRoot + offset,
                    newSize);
            }
        }

        private static Vector2Int[] AppendOneAndSelectInPlace(Vector2Int[] arr, Vector2Int elem, Vector2Int offset, int length)
        {
            //Vector2Int[] newArr = ArrayPool<Vector2Int>.Shared.Rent(length + 1);
            Vector2Int[] newArr = new Vector2Int[length + 1];

            for (int i = 0; i < length; i++)
            {
                newArr[i] = arr[i] + offset;
            }

            newArr[length] = elem + offset;

            return newArr;
        }

        private readonly BitBuffer512 CalculateUniqueCode()
        {
            static int GetWeight(int x, int y, int size) => (y * size) + x;

            var bits = new BitBuffer512();

            int length = BlockLength;

            for (int i = 0; i < BlockLength; i++)
            {
                bits[GetWeight(Blocks[i].X, Blocks[i].Y, length)] = true;
            }

            return bits;
        }

        private readonly BitBuffer512 CalculateUniqueCode(Func<Vector2Int, Vector2Int> func)
        {
            static int GetWeight(int x, int y, int size) => (y * size) + x;

            var bits = new BitBuffer512();

            int length = BlockLength;

            var offset = CalculateOffset(func);

            Vector2Int min = new Vector2Int(int.MaxValue, int.MaxValue);

            for (int i = 0; i < BlockLength; i++)
            {
                var mapped = func(Blocks[i]) + offset;

                if (mapped.X < min.X) min = new Vector2Int(mapped.X, min.Y);
                if (mapped.Y < min.Y) min = new Vector2Int(min.X, mapped.Y);

                bits[GetWeight(mapped.X, mapped.Y, length)] = true;
            }

            if (min != new Vector2Int(0, 0))
                throw new Exception("Invalid min " + min + " offset:" + offset + " Blocks: " + string.Join(";", Blocks));

            return bits;
        }

        private readonly Vector2Int CalculateOffset(Func<Vector2Int, Vector2Int> map)
        {
            var mappedSize = map(Max);
            return new Vector2Int(Math.Max(-mappedSize.X, 0), Math.Max(-mappedSize.Y, 0));
        }

        #region Visualization
        private readonly bool[,] GetBlock2DArray()
        {
            var blocks = new bool[Max.X + 1, Max.Y + 1];

            foreach (var block in Blocks.Take(BlockLength)) blocks[block.X, block.Y] = true;
            return blocks;
        }

        private static readonly Dictionary<(bool isBlock, bool isDiagonalBlockade, bool isPossibleExtensions), char> AsciiArtChars = new Dictionary<(bool isBlock, bool isDiagonalBlockade, bool isPossibleExtensions), char>
        {
            [(false, false, false)] = ' ',
            [(false, false, true)] = '·',
            [(false, true, false)] = '░',
            [(true, false, false)] = '█',
            [(true, true, false)] = '▓',
        };

        public readonly IEnumerable<string> ToAsciiArt(bool highlightDiagonalBlockade = false, bool highlightPossibleExtensions = false)
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
        #endregion

        #region Overrides and Interface Implementations
        // <inheritdoc/>
#pragma warning disable RCS1139 // Add summary element to documentation comment.
        /// <remarks>Returns invalid results for comparisons between rominos of different sizes</remarks>

        public override readonly bool Equals(object obj) => obj is Romino romino && Equals(romino);

#pragma warning restore RCS1139 // Add summary element to documentation comment.

        public override readonly int GetHashCode() => _uniqueCode.GetHashCode();

        // <inheritdoc/>
#pragma warning disable RCS1139 // Add summary element to documentation comment.
        /// <remarks>Returns invalid results for comparisons between rominos of different sizes</remarks>

        public readonly bool Equals(Romino romino) => _uniqueCode == romino._uniqueCode;

#pragma warning restore RCS1139 // Add summary element to documentation comment.

        public readonly int CompareTo(Romino other) => _uniqueCode.CompareTo(other._uniqueCode);

        public static bool operator ==(Romino left, Romino right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(Romino left, Romino right)
        {
            return !(left == right);
        }

        public static bool operator <(Romino left, Romino right)
        {
            return left.CompareTo(right) < 0;
        }

        public static bool operator <=(Romino left, Romino right)
        {
            return left.CompareTo(right) <= 0;
        }

        public static bool operator >(Romino left, Romino right)
        {
            return left.CompareTo(right) > 0;
        }

        public static bool operator >=(Romino left, Romino right)
        {
            return left.CompareTo(right) >= 0;
        }
        #endregion
    }
}