// Enable this to disable debugger stepthrough
//#define DEBUG_BitBuffer512

// Add these to support bigger Rominos. Slightly affects performance.
//#define _8ULong // 22 size rominos
#define _4ULong // 16 size rominos
// Else         // 11 size rominos

// Here be dragons
namespace Rominos
{
    using System;

#if !DEBUG_BitBuffer512

    using System.Diagnostics;
    using System.Runtime.CompilerServices;

#endif

    using System.Runtime.InteropServices;

    /// <summary>
    /// Represents a binary number able to hold up to 512 bits of data.
    /// Implements methods for assigning and reading certain bits, and
    /// overloads operators for comparison.
    /// </summary>
    //    [StructLayout(LayoutKind.Explicit, Pack = 1, Size =
    //#if _8ULong
    //        64
    //#elif _4ULong
    //        32
    //#else
    //        16
    //#endif
    //    )]
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
#if !DEBUG_BitBuffer512
    [DebuggerStepThrough]
#endif
    public struct BitBuffer512 : IEquatable<BitBuffer512>, IComparable<BitBuffer512>
    {
        /// <summary>
        /// Represents the smallest possible value of <see cref="BitBuffer512"/>. This field is constant.
        /// </summary>
        public static readonly BitBuffer512 Min = new BitBuffer512();

        /// <summary>
        /// Represents the largest possible value of <see cref="BitBuffer512"/>. This field is constant.
        /// </summary>
        public static readonly BitBuffer512 Max = new BitBuffer512()
        {
#if _8ULong
            _h = ulong.MaxValue,
            _g = ulong.MaxValue,
            _f = ulong.MaxValue,
            _e = ulong.MaxValue,
#endif
#if _4ULong
            _d = ulong.MaxValue,
            _c = ulong.MaxValue,
#endif
            _b = ulong.MaxValue,
            _a = ulong.MaxValue,
        };

        #region Fields

        // Fields are stored descendingly; _h is the greates digit, whereas _a is the least.

#pragma warning disable RCS1169 // Make field read-only.
#pragma warning disable IDE0044 // Add readonly modifier
#if _8ULong
#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        /* [FieldOffset(64)] */ private ulong _h;

#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        /* [FieldOffset(56)] */ private ulong _g;

#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        /* [FieldOffset(48)] */ private ulong _f;

#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        /* [FieldOffset(32)] */ private ulong _e;
#endif

#if _4ULong
#if !DEBUG_BitBuffer512

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif

        /* [FieldOffset(24)] */
        private ulong _d;

#if !DEBUG_BitBuffer512

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        /* [FieldOffset(16)] */
        private ulong _c;

#endif

#if !DEBUG_BitBuffer512

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        /* [FieldOffset(8)] */
        private ulong _b;

#if !DEBUG_BitBuffer512

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        /* [FieldOffset(0)] */
        private ulong _a;

#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore RCS1169 // Make field read-only.

        #endregion Fields

        /// <summary>
        /// Gets or sets the bit at index <paramref name="bitIndex"/>.
        /// </summary>
        /// <param name="bitIndex">The 0 based index of the bit.</param>
        /// <returns>The value of the bit.</returns>
        public bool this[int bitIndex]
        {
            get => GetBit(ref this, bitIndex);
            set => SetBit(ref this, bitIndex, value);
        }

        #region Bits

        /// <summary>
        /// Gets the bit with index <paramref name="bitIndex"/> of <paramref name="bitBuffer"/>.
        /// </summary>
        /// <param name="bitBuffer">The <see cref="BitBuffer512"/> to set the bit of.</param>
        /// <param name="bitIndex">The 0 based index of the bit.</param>
        /// <returns>The bit with index <paramref name="bitIndex"/> of <paramref name="bitBuffer"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetBit(ref BitBuffer512 bitBuffer, int bitIndex) => GetBit(GetulongContainingBit(ref bitBuffer, bitIndex), bitIndex % 64);

        /// <summary>
        /// Sets the bit with index <paramref name="bitIndex"/> of <paramref name="bitBuffer"/>
        /// to the value <paramref name="bit"/>.
        /// </summary>
        /// <param name="bitBuffer">The <see cref="BitBuffer512"/> to set the bit of.</param>
        /// <param name="bitIndex">The 0 based index of the bit.</param>
        /// <param name="bit">The value to set for the bit.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetBit(ref BitBuffer512 bitBuffer, int bitIndex, bool bit) => SetBit(ref GetulongContainingBit(ref bitBuffer, bitIndex), bitIndex % 64, bit);

        // Static method due to defensive copies etc. (see https://stackoverflow.com/questions/50490143/why-cant-a-c-sharp-struct-method-return-a-reference-to-a-field-but-a-non-membe)
        /// <summary>
        /// Gets a reference to the <see cref="ulong"/> containing the bit with index <paramref name="bitIndex"/> of <paramref name="bitBuffer"/>.
        /// </summary>
        /// <param name="bitBuffer">The <see cref="BitBuffer512"/> to get the reference for the <see cref="ulong"/> of.</param>
        /// <param name="bitIndex">The index of the bit to find.</param>
        /// <returns>A reference to the <see cref="ulong"/> containing the bit with index <paramref name="bitIndex"/> of <paramref name="bitBuffer"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static ref ulong GetulongContainingBit(ref BitBuffer512 bitBuffer, int bitIndex)
        {
            if (bitIndex < 0) throw new ArgumentException(nameof(bitIndex));

            if (bitIndex < 64 * 1) return ref bitBuffer._a;
            if (bitIndex < 64 * 2) return ref bitBuffer._b;
#if _4ULong
            if (bitIndex < 64 * 3) return ref bitBuffer._c;
            if (bitIndex < 64 * 4) return ref bitBuffer._d;
#endif
#if _8ULong
            if (bitIndex < 64 * 5) return ref bitBuffer._e;
            if (bitIndex < 64 * 6) return ref bitBuffer._f;
            if (bitIndex < 64 * 7) return ref bitBuffer._g;
            if (bitIndex < 64 * 8) return ref bitBuffer._h;
#endif

            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        }

        /// <summary>
        /// Gets the bit with index <paramref name="bitIndex"/> of <paramref name="val"/>.
        /// </summary>
        /// <param name="val">The <see cref="ulong"/> to set the bit of.</param>
        /// <param name="bitIndex">The 0 based index of the bit.</param>
        /// <returns>The bit with index <paramref name="bitIndex"/> of <paramref name="val"/>.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool GetBit(ulong val, int bitIndex) => (val & (1ul << bitIndex)) != 0;

        /// <summary>
        /// Sets the bit with index <paramref name="bitIndex"/> of <paramref name="val"/>
        /// to the value <paramref name="bit"/>.
        /// </summary>
        /// <param name="val">The <see cref="ulong"/> to set the bit of.</param>
        /// <param name="bitIndex">The 0 based index of the bit.</param>
        /// <param name="bit">The value to set for the bit.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void SetBit(ref ulong val, int bitIndex, bool bit)
        {
            if (!bit) val &= ~(1ul << bitIndex);
            else val |= (1ul << bitIndex);
        }

        #endregion Bits

        #region Overrides and Interface Implementations

        /// <inheritdoc/>
        public override string ToString() =>
            (
#if _8ULong
            _h.ToString("X16")
            + _g.ToString("X16")
            + _f.ToString("X16")
            + _e.ToString("X16")
            +
#endif
#if _4ULong
            _d.ToString("X16")
            + _c.ToString("X16")
            +
#endif
            _b.ToString("X16")
            + _a.ToString("X16"))
            .TrimStart('0');

        /// <inheritdoc/>
        public override readonly int GetHashCode()
        {
            var hashCode = -553793028;
#if _8ULong
            hashCode = (hashCode * -1521134295) + _h.GetHashCode();
            hashCode = (hashCode * -1521134295) + _g.GetHashCode();
            hashCode = (hashCode * -1521134295) + _f.GetHashCode();
            hashCode = (hashCode * -1521134295) + _e.GetHashCode();
#endif
#if _4ULong
            hashCode = (hashCode * -1521134295) + _d.GetHashCode();
            hashCode = (hashCode * -1521134295) + _c.GetHashCode();
#endif
            hashCode = (hashCode * -1521134295) + _b.GetHashCode();
            hashCode = (hashCode * -1521134295) + _a.GetHashCode();
            return hashCode;
        }

        /// <inheritdoc/>
        public override readonly bool Equals(object? obj) => obj is BitBuffer512 buffer && Equals(buffer);

        /// <inheritdoc/>
        public readonly bool Equals(BitBuffer512 other) =>
#if _8ULong
            _h == other._h
            && _g == other._g
            && _f == other._f
            && _e == other._e
            &&
#endif
#if _4ULong
            _d == other._d
            && _c == other._c
            &&
#endif
            _b == other._b
            && _a == other._a;

        /// <inheritdoc/>
        public readonly int CompareTo(BitBuffer512 other)
        {
#if _8ULong
            if (_h < other._h) return -1;
            if (_h > other._h) return +1;
            if (_g < other._g) return -1;
            if (_g > other._g) return +1;
            if (_f < other._f) return -1;
            if (_f > other._f) return +1;
            if (_e < other._e) return -1;
            if (_e > other._e) return +1;
#endif
#if _4ULong
            if (_d < other._d) return -1;
            if (_d > other._d) return +1;
            if (_c < other._c) return -1;
            if (_c > other._c) return +1;
#endif
            if (_b < other._b) return -1;
            if (_b > other._b) return +1;
            if (_a < other._a) return -1;
            if (_a > other._a) return +1;

            return 0;

            //return this < other ? -1 : this > other ? 1 : 0;
        }

        #endregion Overrides and Interface Implementations

        #region Operators

        public static bool operator <(BitBuffer512 lhs, BitBuffer512 rhs)
        {
#if _8ULong
            if (lhs._h < rhs._h) return true;
            if (lhs._h > rhs._h) return false;
            if (lhs._g < rhs._g) return true;
            if (lhs._g > rhs._g) return false;
            if (lhs._f < rhs._f) return true;
            if (lhs._f > rhs._f) return false;
            if (lhs._e < rhs._e) return true;
            if (lhs._e > rhs._e) return false;
#endif
#if _4ULong
            if (lhs._d < rhs._d) return true;
            if (lhs._d > rhs._d) return false;
            if (lhs._c < rhs._c) return true;
            if (lhs._c > rhs._c) return false;
#endif
            if (lhs._b < rhs._b) return true;
            if (lhs._b > rhs._b) return false;
            if (lhs._a < rhs._a) return true;
            if (lhs._a > rhs._a) return false;

            return false;
        }

        public static bool operator >(BitBuffer512 lhs, BitBuffer512 rhs)
        {
#if _8ULong
            if (lhs._h > rhs._h) return true;
            if (lhs._h < rhs._h) return false;
            if (lhs._g > rhs._g) return true;
            if (lhs._g < rhs._g) return false;
            if (lhs._f > rhs._f) return true;
            if (lhs._f < rhs._f) return false;
            if (lhs._e > rhs._e) return true;
            if (lhs._e < rhs._e) return false;
#endif
#if _4ULong
            if (lhs._d > rhs._d) return true;
            if (lhs._d < rhs._d) return false;
            if (lhs._c > rhs._c) return true;
            if (lhs._c < rhs._c) return false;
#endif
            if (lhs._b > rhs._b) return true;
            if (lhs._b < rhs._b) return false;
            if (lhs._a > rhs._a) return true;
            if (lhs._a < rhs._a) return false;

            return false;
        }

        public static bool operator <=(BitBuffer512 lhs, BitBuffer512 rhs) => lhs < rhs || lhs == rhs;

        public static bool operator >=(BitBuffer512 lhs, BitBuffer512 rhs) => lhs > rhs || lhs == rhs;

        public static bool operator ==(BitBuffer512 lhs, BitBuffer512 rhs) => lhs.Equals(rhs);

        public static bool operator !=(BitBuffer512 lhs, BitBuffer512 rhs) => !(lhs == rhs);

        #endregion Operators
    }
}