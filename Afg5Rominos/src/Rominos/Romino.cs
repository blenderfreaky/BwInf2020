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
                                                new Vector2Int(0, 2),  new Vector2Int(1, 2),  new Vector2Int(2, 2), },
                new Vector2Int(1, 1));

        public readonly Vector2Int[] Blocks;
        public Vector2Int DiagonalRoot;
        public Vector2Int Max;

        public readonly Vector2Int[] PossibleExtensions;

        public IEnumerable<Vector2Int> DiagonalRootBlockade
        {
            get
            {
                yield return DiagonalRoot + new Vector2Int(0, 0);
                yield return DiagonalRoot + new Vector2Int(0, 1);
                yield return DiagonalRoot + new Vector2Int(1, 0);
                yield return DiagonalRoot + new Vector2Int(1, 1);
            }
        }

        private BitBuffer512 _uniqueCode;

        public Romino(Vector2Int[] blocks, Vector2Int diagonalRoot, Vector2Int[] possibleExtensions, Vector2Int max)
        {
            Blocks = blocks;
            DiagonalRoot = diagonalRoot;
            PossibleExtensions = possibleExtensions;
            Max = max;

            _uniqueCode = CalculateUniqueCode(x => x);
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
                    var newRominos = lastRominos//.AsParallel()
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

                    var offset = new Vector2Int(Math.Max(-newBlock.X, 0), Math.Max(-newBlock.Y, 0));
                    var newSize = new Vector2Int(Math.Max(newBlock.X, Max.X), Math.Max(newBlock.Y, Max.Y)) + offset;

                    var romino = new Romino(AppendOneAndSelectInPlace(blocks, newBlock, x => x + offset), diagonalRoot, newPossibleExtensions, newSize);
                    romino.Orient();
                    return romino;
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

        private BitBuffer512 CalculateUniqueCode()
        {
            static int GetWeight(int x, int y, int size) => (y * size) + x;

            var bits = new BitBuffer512();

            int length = Blocks.Length;//Math.Max(Max.X, Max.Y) + 1;

            foreach (var block in Blocks)
            {
                bits[GetWeight(block.X, block.Y, length)] = true;
            }

            return bits;
        }

        private BitBuffer512 CalculateUniqueCode(Func<Vector2Int, Vector2Int> map)
        {
            static int GetWeight(int x, int y, int size) => (y * size) + x;

            var bits = new BitBuffer512();

            int length = Blocks.Length;//Math.Max(Max.X, Max.Y) + 1;

            foreach (var block in Blocks)
            {
                var pma = map(block);
                var mapped = MapPositionWithinSize(block, map);
                bits[GetWeight(mapped.X, mapped.Y, length)] = true;
            }

            return bits;
        }

        private Vector2Int MapPositionWithinSize(Vector2Int position, Func<Vector2Int, Vector2Int> map)
        {
            var mappedSize = map(Max);

            return map(position) + new Vector2Int(Math.Max(-mappedSize.X, 0), Math.Max(-mappedSize.Y, 0));
        }

        //        |X               
        //        |                
        //        |                
        // -------DDDD----           
        //        DDDD              
        //        |                


        // Map  Step
        // +x+y +x-y
        // +x-y -x-y
        // -x+y +x-y
        // -x-y -y-x
        // +y+x +x-y
        // +y-x -x-y
        // -y+x +x-y
        // -y-x -y-x

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

        public void Orient()
        {
            int minIndex = 0;
            BitBuffer512 min = _uniqueCode;

            for (int i = 1; i < Maps.Length; i++)
            {
                var uniqueCode = CalculateUniqueCode(Maps[i].BlockMap);
                if (min > uniqueCode)
                {
                    minIndex = i;
                    min = uniqueCode;
                }
            }

            ProjectVoxels(Maps[minIndex].BlockMap, Maps[minIndex].DiagonalRootMap);

            _uniqueCode = CalculateUniqueCode();
        }

        private void ProjectVoxels(Func<Vector2Int, Vector2Int> func, Func<Vector2Int, Vector2Int> diagonalRootFunc)
        {
            Blocks.SelectInPlaceF(x => MapPositionWithinSize(x, func));
            PossibleExtensions.SelectInPlaceF(x => MapPositionWithinSize(x, func));
            DiagonalRoot = MapPositionWithinSize(DiagonalRoot, diagonalRootFunc);
            Max = MapPositionWithinSize(Max, func);
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
