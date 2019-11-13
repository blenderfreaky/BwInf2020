//#define DEBUG_BitBuffer512
//#define _8ULong
//#define _4ULong

namespace Rominos
{
    using System;
    using System.Globalization;
    using System.Runtime.InteropServices;

    [StructLayout(LayoutKind.Explicit, Pack = 1, Size =
#if _8ULong
        64
#elif _4ULong
        32
#else
        16
#endif
    )]
    public struct BitBuffer512 : IEquatable<BitBuffer512>, IComparable<BitBuffer512>
    {
        public static readonly BitBuffer512 Min = new BitBuffer512();

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

#pragma warning disable RCS1169 // Make field read-only.
#pragma warning disable IDE0044 // Add readonly modifier
#if _8ULong
#if DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(64)] private ulong _h;

#if DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(56)] private ulong _g;

#if DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(48)] private ulong _f;

#if DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(32)] private ulong _e;
#endif

#if DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif

#if _4ULong
        [FieldOffset(24)] private ulong _d;

#if DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(16)] private ulong _c;
#endif

#if DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(8)] private ulong _b;

#if DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(0)] private ulong _a;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore RCS1169 // Make field read-only.

#endregion Fields

        public bool this[int bitIndex]
        {
            get => GetBit(ref this, bitIndex);
            set => SetBit(ref this, bitIndex, value);
        }

#region Bits

        private static bool GetBit(ref BitBuffer512 bitBuffer, int bitIndex) => GetBit(GetulongContainingBit(ref bitBuffer, bitIndex), bitIndex % 64);

        private static void SetBit(ref BitBuffer512 bitBuffer, int bitIndex, bool bit) => SetBit(ref GetulongContainingBit(ref bitBuffer, bitIndex), bitIndex % 64, bit);

        // Extension method due to defensive copies etc. (see https://stackoverflow.com/questions/50490143/why-cant-a-c-sharp-struct-method-return-a-reference-to-a-field-but-a-non-membe)
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

        private static bool GetBit(ulong val, int bitIndex) => (val & (1ul << bitIndex)) != 0;

        private static void SetBit(ref ulong val, int bitIndex, bool bit)
        {
            if (!bit) val &= ~(1ul << bitIndex);
            else val |= (1ul << bitIndex);
        }

#endregion Bits

#region Overrides and Interface Implementations

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

        public override readonly bool Equals(object? obj) => obj is BitBuffer512 buffer && Equals(buffer);

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

        public readonly int CompareTo(BitBuffer512 other) => this < other ? -1 : this > other ? 1 : 0;

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