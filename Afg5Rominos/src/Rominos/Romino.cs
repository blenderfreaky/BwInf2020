﻿namespace Rominos
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
            new Romino(new[] { new Vector2Int(0, 0), new Vector2Int(1, 1) }, new Vector2Int(0, 0),
                // These are hardcoded in by hand, because this list is only populated lazily by appending, rather than computed once. 
                // As this first romino can not be computed like other rominos, this won't be populated using normal methods.
                new[] { new Vector2Int(-1, -1), new Vector2Int(0, -1), new Vector2Int(1, -1),
                        new Vector2Int(-1, 0),                                                new Vector2Int(2, 0),
                        new Vector2Int(-1, 1),                                                new Vector2Int(2, 1),
                                                new Vector2Int(0, 2),  new Vector2Int(1, 2),  new Vector2Int(2, 2), });

        public readonly Vector2Int[] Blocks;
        public Vector2Int DiagonalRoot;

        public readonly Vector2Int[] PossibleExtensions;

        public Vector2Int[] DiagonalRootBlockade => new[]
        {
            DiagonalRoot + new Vector2Int(0, 0),
            DiagonalRoot + new Vector2Int(0, 1),
            DiagonalRoot + new Vector2Int(1, 0),
            DiagonalRoot + new Vector2Int(1, 1)
        };

        private BitBuffer512 _uniqueCode;

        public Romino(Vector2Int[] blocks, Vector2Int diagonalRoot, Vector2Int[] possibleExtensions)
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
                    var newPossibleExtensions = possibleExtensions.WhereF(x => x != newBlock).Union(extensionsFromNewBlock).ToArray();

                    var romino = new Romino(AppendOne(blocks, newBlock), diagonalRoot, newPossibleExtensions);
                    //romino.Orient();

                    var opt = Maps.SelectF(x => (romino, x));
                    opt.ForEach(x => x.romino.ProjectVoxels(x.x.BlockMap, x.x.DiagonalRootMap));
                    opt.ForEach(x => x.romino.FixDisplacement());
                    return opt.Min(x => x.romino);
                });
        }

        //private static T[] AppendOne<T>(T[] arr, T elem)
        //{
        //    int length = arr.Length;

        //    T[] newArr = new T[length + 1];

        //    Array.Copy(arr, 0, newArr, 0, arr.Length);

        //    newArr[arr.Length] = elem;

        //    return newArr;
        //}

        private static T[] AppendOne<T>(T[] arr, T elem)
        {
            int length = arr.Length;

            T[] newArr = new T[length + 1];

            for (int i = 0; i < length; i++)
            {
                newArr[i] = arr[i];
            }

            newArr[arr.Length] = elem;

            return newArr;
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

        private static BitBuffer512 CalculateUniqueCode(Vector2Int[] blocks, Func<Vector2Int, Vector2Int> map)
        {
            static int GetWeight(int x, int y, int size) => (y * size) + x;

            var bits = new BitBuffer512();

            int length = blocks.Length;

            foreach (var block in blocks)
            {
                var mapped = map(block); // TODO: Fix displacement
                bits[GetWeight(mapped.X, mapped.Y, length)] = true;
            }

            return bits;
        }

        // Map  Step
        // +x+y +x-y
        // +x-y -x-y
        // -x+y +x-y
        // -x-y -y-x
        // +y+x +x-y
        // +y-x -x-y
        // -y+x +x-y
        // -y-x -y-x

        private static readonly (Func<Vector2Int, Vector2Int> BlockMap, Func<Vector2Int, Vector2Int> DiagonalRootMap)[] Steps = new (Func<Vector2Int, Vector2Int> BlockMap, Func<Vector2Int, Vector2Int> DiagonalRootMap)[]
        {
            (x => new Vector2Int(+x.X, -x.Y), x => new Vector2Int(+x.X, ~x.Y)),
            (x => new Vector2Int(-x.X, -x.Y), x => new Vector2Int(~x.X, ~x.Y)),
            (x => new Vector2Int(+x.X, -x.Y), x => new Vector2Int(+x.X, ~x.Y)),
            (x => new Vector2Int(-x.Y, -x.X), x => new Vector2Int(~x.Y, ~x.X)),
            (x => new Vector2Int(+x.X, -x.Y), x => new Vector2Int(+x.X, ~x.Y)),
            (x => new Vector2Int(-x.X, -x.Y), x => new Vector2Int(~x.X, ~x.Y)),
            (x => new Vector2Int(+x.X, -x.Y), x => new Vector2Int(+x.X, ~x.Y)),
            (x => new Vector2Int(-x.Y, -x.X), x => new Vector2Int(~x.Y, ~x.X)),
        };

        private static readonly (Func<Vector2Int, Vector2Int> BlockMap, Func<Vector2Int, Vector2Int> DiagonalRootMap)[] Maps = new (Func<Vector2Int, Vector2Int> BlockMap, Func<Vector2Int, Vector2Int> DiagonalRootMap)[]
        {
            (x => new Vector2Int(+x.X, -x.Y), x => new Vector2Int(+x.X, ~x.Y)),
            (x => new Vector2Int(-x.X, +x.Y), x => new Vector2Int(~x.X, +x.Y)),
            (x => new Vector2Int(-x.X, -x.Y), x => new Vector2Int(~x.X, ~x.Y)),
            (x => new Vector2Int(+x.Y, +x.X), x => new Vector2Int(+x.Y, +x.X)),
            (x => new Vector2Int(+x.Y, -x.X), x => new Vector2Int(+x.Y, ~x.X)),
            (x => new Vector2Int(-x.Y, +x.X), x => new Vector2Int(~x.Y, +x.X)),
            (x => new Vector2Int(-x.Y, -x.X), x => new Vector2Int(~x.Y, ~x.X)),
            (x => new Vector2Int(+x.X, +x.Y), x => new Vector2Int(+x.X, +x.Y)),
        };

        public void Orient()
        {
            int minIndex = 7;
            BitBuffer512 min = _uniqueCode;

            for (int i = 0; i < Steps.Length; i++)
            {
                ProjectVoxels(Steps[i].BlockMap, Steps[i].DiagonalRootMap);
                FixDisplacement();
                var uniqueCode = CalculateUniqueCode(Blocks);
                if (min > uniqueCode)
                {
                    minIndex = i;
                    min = uniqueCode;
                }
            }

            ProjectVoxels(Maps[minIndex].BlockMap, Maps[minIndex].DiagonalRootMap);
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
