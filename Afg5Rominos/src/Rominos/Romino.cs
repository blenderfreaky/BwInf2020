namespace Rominos
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public readonly struct Romino : IEquatable<Romino>, IComparable<Romino>
    {
        /// <summary>
        /// <para>
        /// All different combinations of rotating and mirroring an arbitrary romino.
        /// </para>
        /// <para>
        /// BlockMap represents the functor mapping a block coordinate from the origin romino
        ///   to the rotated/mirrored romino.
        /// </para>
        /// <para>
        /// DiagonalRootMap represents the functor mapping the DiagonalRoot from the origin romino
        ///   to the rotated/mirrored romino.
        ///   Different from BlockMap because the DiagonalRoot is always the upper left of a square
        ///   of 4 coords; 
        /// </para>
        /// <para>    e.g. when mirroring along the y-Axis (x => (-x.X, x.Y)):
        /// 
        ///     Before  After
        ///       |       |
        ///     --D░--  -D▓--
        ///       ░▓     ▓░
        ///       |       |
        /// </para>
        /// </summary>
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

        /// <summary>
        /// The smallest Romino possible
        /// </summary>
        public static Romino One =
            new Romino(blocks: new[] { new Vector2Int(0, 0), new Vector2Int(1, 1) },
                possibleExtensions:
                // These are hardcoded in by hand, because this list is only populated lazily by appending, rather than computed once.
                // As this first romino can not be computed like other rominos, this won't be populated using normal methods.
                new[] { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                        new Vector2Int(-1, 0),                                                new Vector2Int(2, 0),
                        new Vector2Int(-1, 1),                                                new Vector2Int(2, 1),
                                                new Vector2Int(0, 2),  new Vector2Int(1, 2),  new Vector2Int(2, 2), }
                    .ToList(),
                diagonalRoot: new Vector2Int(0, 0),
                max: new Vector2Int(1, 1));

        /// <summary>
        /// All the Blocks composing the Romino.
        /// </summary>
        public readonly Vector2Int[] Blocks;

        /// <summary>
        /// All possible positions for adding new blocks.
        /// </summary>
        /// <remarks>
        /// This is a list, yet the length is fixed.
        /// Reason for this is, that at the point of creation, the size of this is not known,
        /// and converting to an array after the size is known adds unnecessary overhead.
        /// </remarks>
        public readonly List<Vector2Int> PossibleExtensions;

        /// <summary>
        /// The upper left (lowest x, y) corner of the protected diagonal.
        /// </summary>
        public readonly Vector2Int DiagonalRoot;

        /// <summary>
        /// The highest x and y coordinates of any block inside the romino.
        /// </summary>
        public readonly Vector2Int Max;

        /// <summary>
        /// The unique code assigned to this romino.
        /// </summary>
        private readonly BitBuffer512 _uniqueCode;

        /// <summary>
        /// Gets all the blocks blocked by the protected diagonal.
        /// </summary>
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

        /// <summary>
        /// Gets this romino as ASCII-art.
        /// For debugging.
        /// </summary>
        public string AsciiArt => string.Join(Environment.NewLine, ToAsciiArt(true, true));

        /// <summary>
        /// Initializes and orients a new instance of the <see cref="Romino"/> structure.
        /// </summary>
        /// <param name="blocks">All the Blocks composing the Romino.</param>
        /// <param name="possibleExtensions">All possible positions for adding new blocks.</param>
        /// <param name="diagonalRoot">The upper left (lowest x, y) corner of the protected diagonal.</param>
        /// <param name="max">The highest x and y coordinates of any block inside the romino.</param>
        public Romino(Vector2Int[] blocks, List<Vector2Int> possibleExtensions, Vector2Int diagonalRoot, Vector2Int max)
        {
            Blocks = blocks;
            DiagonalRoot = diagonalRoot;
            PossibleExtensions = possibleExtensions;
            Max = max;

            _uniqueCode = default; // Needs to be assigned in order to call methods, including CalculateUniqueCode.
            _uniqueCode = CalculateUniqueCode();

            // Find highest unique Code.
            // Start of with asserting the current permutation to be the one with the highest unique code.
            int maxIndex = 0;
            BitBuffer512 maxCode = _uniqueCode;

            // Check against all other permutations, skipping 1, as thats already been calculated.
            for (int i = 1; i < Maps.Length; i++)
            {
                var uniqueCode = CalculateUniqueCode(Maps[i].BlockMap);
                if (maxCode < uniqueCode)
                {
                    maxIndex = i;
                    maxCode = uniqueCode;
                }
            }

            // Only make changes if the highest unique Code isn't the initial state
            // (Maps[0] = (x => x, x => x))
            if (maxIndex != 0)
            {
                (Func<Vector2Int, Vector2Int> blockMap, Func<Vector2Int, Vector2Int> diagonalRootMap) = Maps[maxIndex];

                var offset = CalculateOffset(blockMap);

                for (int i = 0; i < Blocks.Length; i++) Blocks[i] = blockMap(Blocks[i]) + offset;
                for (int i = 0; i < PossibleExtensions.Count; i++) PossibleExtensions[i] = blockMap(PossibleExtensions[i]) + offset;

                DiagonalRoot = diagonalRootMap(DiagonalRoot) + offset;

                // Don't add offset to max, it might end up with x or y equal to 0.
                var mappedMax = blockMap(Max);
                // Take the absolute of both components, we only care about swapping of x and y, not inversion.
                Max = new Vector2Int(Math.Abs(mappedMax.X), Math.Abs(mappedMax.Y));

                // Recalculate the unique code, as the currently saved one is for Maps[0].
                _uniqueCode = CalculateUniqueCode();
            }
        }

        public static IEnumerable<(int Size, List<Romino> Rominos)> GetRominosUntilSize(int size)
        {
            // Validate arguments outside of iterator block, to prevent the exception being thrown lazily.
            if (size < 2) throw new ArgumentOutOfRangeException(nameof(size));

            return GetRominosUntilSizeInternal();

            IEnumerable<(int Size, List<Romino> Rominos)> GetRominosUntilSizeInternal()
            {
                // Start out with the smalles romino
                List<Romino> lastRominos = new List<Romino> { One };

                // The size of the smallest Romino is 2 blocks; yield it as such.
                yield return (2, lastRominos);

                for (int i = 3; i <= size; i++)
                {
                    var newRominos = lastRominos
                        // Enable parallelization using PLINQ.
                        .AsParallel()
                        // Map every romino to all rominos generated by adding one block to it.
                        .SelectMany(x => x.AddOneNotUnique())
                        // Remove duplicates, rominos are already oriented here.
                        .Distinct()
                        // Execute Query by iterating into a list. Cheaper than .ToArray()
                        .ToList();

                    // We don't need last generations rominos anymore. Replace them with the new generation.
                    lastRominos = newRominos;
                    // Yield this generations rominos with their size.
                    yield return (i, newRominos);
                }
            }
        }

        // Generate IEnumerable<T> instead of allocing a new array
        /// <summary>
        /// Gets all direct neighbours of a given block, not including the block itself.
        /// </summary>
        /// <param name="block">The block to get the neighbours of</param>
        /// <returns>An <see cref="IEnumerable{Vector2Int}"/> yielding all neighbours</returns>
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

        /// <summary>
        /// Returns all rominos generated by adding one block from <see cref="PossibleExtensions"/>
        /// </summary>
        /// <remarks>Does not remove duplicates, but orients results.</remarks>
        /// <returns>All, non-unique rominos generated by adding one block from <see cref="PossibleExtensions"/>.</returns>
        public readonly IEnumerable<Romino> AddOneNotUnique()
        {
            foreach (var newBlock in PossibleExtensions)
            {
                // If the new block has x or y smaller than 0, move the entire romino such that
                // the lowest x and y are 0.
                // This offset will need to be applied to anything inside the romino.
                var offset = new Vector2Int(Math.Max(-newBlock.X, 0), Math.Max(-newBlock.Y, 0));

                // If the new block is outside of the old rominos bounds, i.e. has bigger x or y coords than Max,
                // increase size.
                var newSize = new Vector2Int(Math.Max(newBlock.X, Max.X), Math.Max(newBlock.Y, Max.Y))
                    // or if the new block has coordinates x or y smaller than 0, increase size.
                    + offset;

                HashSet<Vector2Int> newPossibleExtensions =
                    // Get the direct neighbours, i.e. the blocks that will be possible spots
                    // for adding blocks after newBlock has been added
                    new HashSet<Vector2Int>(GetDirectNeighbours(newBlock + offset));

                // Remove already occupied positions
                newPossibleExtensions.ExceptWith(Blocks.Select(x => x + offset));
                // Exclude positions blocked by the protected diagonal
                newPossibleExtensions.ExceptWith(DiagonalRootBlockade.Select(x => x + offset));

                // Re-use old extension spots.
                newPossibleExtensions.UnionWith(PossibleExtensions.Select(x => x + offset));

                // Remove the newly added block.
                newPossibleExtensions.Remove(newBlock + offset);

                // Allocate a new array for the new romino, with one more space then right now
                // to store the new block in.
                Vector2Int[] newBlocks = new Vector2Int[Blocks.Length + 1];

                for (int i = 0; i < Blocks.Length; i++)
                {
                    // Copy elements from current romino and apply offset.
                    newBlocks[i] = Blocks[i] + offset;
                }

                // Insert the new block, also, with offset.
                newBlocks[Blocks.Length] = newBlock + offset;

                yield return new Romino(
                    newBlocks,
                    new List<Vector2Int>(newPossibleExtensions),
                    // Apply offset to the diagonal root as well.
                    DiagonalRoot + offset,
                    newSize);
            }
        }

        private readonly BitBuffer512 CalculateUniqueCode()
        {
            var bits = new BitBuffer512();

            // "Definitely very useful caching"
            int length = Blocks.Length;

            for (int i = 0; i < Blocks.Length; i++)
            {
                // Assign the relevant bit (2^((y * len) + x) = 1 << ((y * len) + x))
                bits[(Blocks[i].Y * length) + Blocks[i].X] = true;
            }

            return bits;
        }

        private readonly BitBuffer512 CalculateUniqueCode(Func<Vector2Int, Vector2Int> func)
        {
            var bits = new BitBuffer512();

            // "Definitely very useful caching"
            int length = Blocks.Length;

            // Calculate the offset to be applied.
            var offset = CalculateOffset(func);

            for (int i = 0; i < Blocks.Length; i++)
            {
                // Map the block and apply the offset.
                var mapped = func(Blocks[i]) + offset;

                // Assign the relevant bit (2^((y * len) + x) = 1 << ((y * len) + x))
                bits[(mapped.Y * length) + mapped.X] = true;
            }

            return bits;
        }

        /// <summary>
        /// Calculates the offset by which blocks inside the romino need to be moved after applying a given function
        /// in order to still have the lowest x and y be equal to 0.
        /// </summary>
        /// <remarks>The function <paramref name="map"/> may not apply any translations, only 
        /// scaling and rotation around the origin (0, 0) is handled.</remarks>
        /// <param name="map">The function to calculate the offset for.</param>
        /// <returns>The offset that needs to be applied to set the minimum x and y coordinates after applying <paramref name="map"/> back to 0.</returns>
        private readonly Vector2Int CalculateOffset(Func<Vector2Int, Vector2Int> map)
        {
            var mappedSize = map(Max);
            // We only need to offset if the blocks are being moved into the negative,
            // as translations from map are forbidden, and such the min will only change by
            // mirroring around an axis or rotating.
            return new Vector2Int(Math.Max(-mappedSize.X, 0), Math.Max(-mappedSize.Y, 0));
        }

        #region Visualization
        private readonly bool[,] GetBlock2DArray()
        {
            var blocks = new bool[Max.X + 1, Max.Y + 1];

            foreach (var block in Blocks.Take(Blocks.Length)) blocks[block.X, block.Y] = true;
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


        private static readonly Dictionary<(bool isBlock, bool isDiagonalBlockade, bool isPossibleExtensions), char> LatexChars = new Dictionary<(bool isBlock, bool isDiagonalBlockade, bool isPossibleExtensions), char>
        {
            [(false, false, false)] = 'w',
            [(false, false, true)] = 'c',
            [(false, true, false)] = 'd',
            [(true, false, false)] = 'b',
            [(true, true, false)] = 'C',
        };

        public readonly IEnumerable<string> ToLatex(bool highlightDiagonalBlockade = false, bool highlightPossibleExtensions = false)
        {
            bool[,] blocks = GetBlock2DArray();

            var buffer = new StringBuilder();

            buffer.AppendLine("\\printmatrix{}{{");

            for (int i = -1; i <= blocks.GetLength(0); i++)
            {
                buffer.Append("    {");

                for (int j = -1; j <= blocks.GetLength(1); j++)
                {
                    Vector2Int vec = new Vector2Int(i, j);

                    bool block = i >= 0 && j >= 0 && i < blocks.GetLength(0) && j < blocks.GetLength(1) && blocks[i, j];

                    bool diagonalBlockade = highlightDiagonalBlockade && DiagonalRootBlockade.Contains(vec);
                    bool possibleExtension = highlightPossibleExtensions && PossibleExtensions.Contains(vec);

                    if (j >= 0) buffer.Append(',');
                    buffer.Append(LatexChars[(block, diagonalBlockade, possibleExtension)]);
                }

                buffer.Append('}');
                buffer.Append(i < blocks.GetLength(0) ? "," :( "%" + Environment.NewLine + "}}"));

                yield return buffer.ToString();

                buffer.Clear();
            }
        }
        #endregion

        #region Overrides and Interface Implementations
        /// <inheritdoc/>
        /// <remarks>Returns invalid results for comparisons between rominos of different sizes</remarks>
        public override readonly bool Equals(object obj) => obj is Romino romino && Equals(romino);

        /// <inheritdoc/>
        public override readonly int GetHashCode() => _uniqueCode.GetHashCode();

        /// <inheritdoc/>
        /// <remarks>Returns invalid results for comparisons between rominos of different sizes</remarks>
        public readonly bool Equals(Romino romino) => _uniqueCode == romino._uniqueCode;

        /// <inheritdoc/>
        /// <remarks>Returns invalid results for comparisons between rominos of different sizes</remarks>
        public readonly int CompareTo(Romino other) => _uniqueCode.CompareTo(other._uniqueCode);

        /// <inheritdoc/>
        public static bool operator ==(Romino left, Romino right) => left.Equals(right);

        /// <inheritdoc/>
        public static bool operator !=(Romino left, Romino right) => !(left == right);

        /// <inheritdoc/>
        public static bool operator <(Romino left, Romino right) => left.CompareTo(right) < 0;

        /// <inheritdoc/>
        public static bool operator <=(Romino left, Romino right) => left.CompareTo(right) <= 0;

        /// <inheritdoc/>
        public static bool operator >(Romino left, Romino right) => left.CompareTo(right) > 0;

        /// <inheritdoc/>
        public static bool operator >=(Romino left, Romino right) => left.CompareTo(right) >= 0;
        #endregion
    }
}