namespace Rominos
{
    using JM.LinqFaster;
    using JM.LinqFaster.Parallel;
    using MoreLinq;
    using System;
    using System.Buffers;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Numerics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading.Tasks;

    public struct Romino : IEquatable<Romino>, IComparable<Romino>
    {
        public static Romino One =
            new Romino(new[] { new Vector2Int(0, 0), new Vector2Int(1, 1) }, new Vector2Int(0, 0),
                // These are hardcoded in by hand, because this list is only populated lazily by appending, rather than computed once. 
                // As this first romino can not be computed like other rominos, this won't be populated using normal methods.
                new[] { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                        new Vector2Int(-1, 0),                                                new Vector2Int(2, 0),
                        new Vector2Int(-1, 1),                                                new Vector2Int(2, 1),
                                                new Vector2Int(0, 2),  new Vector2Int(1, 2),  new Vector2Int(2, 2), },
                new Vector2Int(1, 1));

        public readonly Vector2Int[] Blocks;
        public Vector2Int DiagonalRoot;
        public Vector2Int Max;

        public readonly Vector2Int[] PossibleExtensions;

        public readonly IEnumerable<Vector2Int> DiagonalRootBlockade
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                yield return DiagonalRoot + new Vector2Int(0, 0);
                yield return DiagonalRoot + new Vector2Int(0, 1);
                yield return DiagonalRoot + new Vector2Int(1, 0);
                yield return DiagonalRoot + new Vector2Int(1, 1);
            }
        }

        private BitBuffer512 _uniqueCode;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Romino(Vector2Int[] blocks, Vector2Int diagonalRoot, Vector2Int[] possibleExtensions, Vector2Int max)
        {
            Blocks = blocks;
            DiagonalRoot = diagonalRoot;
            PossibleExtensions = possibleExtensions;
            Max = max;

            _uniqueCode = default; // Needs to be assigned before instance methods can be called, so just assign default before calling the method computing it
            _uniqueCode = CalculateUniqueCode();
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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public readonly IEnumerable<Romino> AddOneNotUnique()
        {
            foreach (var newBlock in PossibleExtensions)
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
                .Except(Blocks).Except(DiagonalRootBlockade);

                var offset = new Vector2Int(Math.Max(-newBlock.X, 0), Math.Max(-newBlock.Y, 0));
                var newSize = new Vector2Int(Math.Max(newBlock.X, Max.X), Math.Max(newBlock.Y, Max.Y)) + offset;

                // Remove the added block and add the new, now appendable positions
                Vector2Int[] newPossibleExtensions = PossibleExtensions.WhereF(x => x != newBlock).Union(extensionsFromNewBlock).Select(x => x + offset).ToArray();

                var romino = new Romino(AppendOneAndSelectInPlace(Blocks, newBlock, x => x + offset), DiagonalRoot + offset, newPossibleExtensions, newSize);
                romino.Orient();
                yield return romino;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static T[] AppendOneAndSelectInPlace<T>(T[] arr, T elem, Func<T, T> func)
        {
            int length = arr.Length;

            T[] newArr = new T[length + 1];

            for (int i = 0; i < length; i++)
            {
                newArr[i] = func(arr[i]);
            }

            newArr[arr.Length] = func(elem);

            return newArr;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly BitBuffer512 CalculateUniqueCode()
        {
            static int GetWeight(int x, int y, int size) => (y * size) + x;

            var bits = new BitBuffer512();

            int length = Blocks.Length;

            foreach (var block in Blocks)
            {
                bits[GetWeight(block.X, block.Y, length)] = true;
            }

            return bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly BitBuffer512 CalculateUniqueCode(Func<Vector2Int, Vector2Int> func)
        {
            static int GetWeight(int x, int y, int size) => (y * size) + x;

            var bits = new BitBuffer512();

            int length = Blocks.Length;

            var offset = CalculateOffset(func);

            foreach (var block in Blocks)
            {
                var mapped = func(block) + offset;
                bits[GetWeight(mapped.X, mapped.Y, length)] = true;
            }

            return bits;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private readonly Vector2Int CalculateOffset(Func<Vector2Int, Vector2Int> map)
        {
            var mappedSize = map(Max);
            return new Vector2Int(Math.Max(-mappedSize.X, 0), Math.Max(-mappedSize.Y, 0));
        }

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

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Orient()
        {
            int minIndex = 0;
            BitBuffer512 min = _uniqueCode;

            BitBuffer512[] bitBuffers = new BitBuffer512[Maps.Length];
            bitBuffers[0] = _uniqueCode; // Maps[0] doesn't contain make any changes to its input

            for (int i = 1; i < Maps.Length; i++)
            {
                bitBuffers[i] = CalculateUniqueCode(Maps[i].BlockMap);
                if (min > bitBuffers[i])
                {
                    minIndex = i;
                    min = bitBuffers[i];
                }
            }

            ProjectVoxels(Maps[minIndex].BlockMap, Maps[minIndex].DiagonalRootMap);

            _uniqueCode = bitBuffers[minIndex];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ProjectVoxels(Func<Vector2Int, Vector2Int> func, Func<Vector2Int, Vector2Int> diagonalRootFunc)
        {
            var offset = CalculateOffset(func);

            for (int i = 0; i < Blocks.Length; i++) Blocks[i] = func(Blocks[i]) + offset;
            for (int i = 0; i < PossibleExtensions.Length; i++) PossibleExtensions[i] = func(PossibleExtensions[i]) + offset;

            DiagonalRoot = diagonalRootFunc(DiagonalRoot) + offset;

            var mappedMax = func(Max);
            Max = new Vector2Int(Math.Abs(mappedMax.X), Math.Abs(mappedMax.Y));
        }

        private readonly bool[,] GetBlock2DArray()
        {
            var blocks = new bool[Max.X + 1, Max.Y + 1];

            foreach (var block in Blocks) blocks[block.X, block.Y] = true;
            return blocks;
        }

        // <inheritdoc/>
#pragma warning disable RCS1139 // Add summary element to documentation comment.
        /// <remarks>Returns invalid results for comparisons between rominos of different sizes</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override readonly bool Equals(object obj) => obj is Romino romino && Equals(romino);
#pragma warning restore RCS1139 // Add summary element to documentation comment.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => _uniqueCode.GetHashCode();


        // <inheritdoc/>
#pragma warning disable RCS1139 // Add summary element to documentation comment.
        /// <remarks>Returns invalid results for comparisons between rominos of different sizes</remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Romino romino) => _uniqueCode == romino._uniqueCode;
#pragma warning restore RCS1139 // Add summary element to documentation comment.

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public int CompareTo(Romino other) => _uniqueCode.CompareTo(other._uniqueCode);


        private static readonly Dictionary<(bool isBlock, bool isDiagonalBlockade, bool isPossibleExtensions), char> AsciiArtChars = new Dictionary<(bool isBlock, bool isDiagonalBlockade, bool isPossibleExtensions), char>
        {
            [(false, false, false)] = ' ',
            [(false, false, true)] = '·',
            [(false, true, false)] = '░',
            [(false, true, true)] = 'E',
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
    }
}
