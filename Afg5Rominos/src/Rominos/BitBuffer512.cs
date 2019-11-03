//#define DEBUG_BitBuffer512

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using System.Text;

namespace Rominos
{
    [StructLayout(LayoutKind.Explicit)]//, Pack = 1, Size = 512)]
    internal struct BitBuffer512 : IEquatable<BitBuffer512>, IComparable<BitBuffer512>
    {
        #region Fields
#pragma warning disable RCS1169 // Make field read-only.
#pragma warning disable IDE0044 // Add readonly modifier
#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(0)] private ulong _h;

#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(8)] private ulong _g;

#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(16)] private ulong _f;

#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(32)] private ulong _e;

#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(64)] private ulong _d;

#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(128)] private ulong _c;

#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(256)] private ulong _b;

#if !DEBUG_BitBuffer512
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
#endif
        [FieldOffset(512)] private ulong _a;
#pragma warning restore IDE0044 // Add readonly modifier
#pragma warning restore RCS1169 // Make field read-only.
#endregion

        public bool this[int bitIndex]
        {
            get => GetBit(ref this, bitIndex);
            set => SetBit(ref this, bitIndex, value);
        }

        #region Bits
        private static bool GetBit(ref BitBuffer512 bitBuffer, int bitIndex) => GetBit(GetUlongContainingBit(ref bitBuffer, bitIndex), bitIndex % 64);
        private static void SetBit(ref BitBuffer512 bitBuffer, int bitIndex, bool bit) => SetBit(ref GetUlongContainingBit(ref bitBuffer, bitIndex), bitIndex % 64, bit);

        // Extension method due to defensive copies etc. (see https://stackoverflow.com/questions/50490143/why-cant-a-c-sharp-struct-method-return-a-reference-to-a-field-but-a-non-membe)
        private static ref ulong GetUlongContainingBit(ref BitBuffer512 bitBuffer, int bitIndex)
        {
            if (bitIndex < 0) throw new ArgumentException(nameof(bitIndex));

            if (bitIndex < 64 * 1) return ref bitBuffer._a;
            if (bitIndex < 64 * 2) return ref bitBuffer._b;
            if (bitIndex < 64 * 3) return ref bitBuffer._c;
            if (bitIndex < 64 * 4) return ref bitBuffer._d;
            if (bitIndex < 64 * 5) return ref bitBuffer._e;
            if (bitIndex < 64 * 6) return ref bitBuffer._f;
            if (bitIndex < 64 * 7) return ref bitBuffer._g;
            if (bitIndex < 64 * 8) return ref bitBuffer._h;

            throw new ArgumentOutOfRangeException(nameof(bitIndex));
        }

        private static bool GetBit(ulong val, int bitIndex) => (val & (1ul << bitIndex)) != 0;

        private static void SetBit(ref ulong val, int bitIndex, bool bit)
        {
            if (!bit) val &= ~(1ul << bitIndex);
            else val |= (1ul << bitIndex);
        }
        #endregion

        #region Overrides and Interface Implementations
        public override readonly int GetHashCode()
        {
            var hashCode = -553793028;
            hashCode = (hashCode * -1521134295) + _h.GetHashCode();
            hashCode = (hashCode * -1521134295) + _g.GetHashCode();
            hashCode = (hashCode * -1521134295) + _f.GetHashCode();
            hashCode = (hashCode * -1521134295) + _e.GetHashCode();
            hashCode = (hashCode * -1521134295) + _d.GetHashCode();
            hashCode = (hashCode * -1521134295) + _c.GetHashCode();
            hashCode = (hashCode * -1521134295) + _b.GetHashCode();
            hashCode = (hashCode * -1521134295) + _a.GetHashCode();
            return hashCode;
        }

        public override readonly bool Equals(object? obj) => obj is BitBuffer512 buffer && Equals(buffer);

        public readonly bool Equals(BitBuffer512 other) =>
            _h == other._h
            && _g == other._g
            && _f == other._f
            && _e == other._e
            && _d == other._d
            && _c == other._c
            && _b == other._b
            && _a == other._a;

        public readonly int CompareTo(BitBuffer512 other) => this < other ? -1 : this > other ? 1 : 0;
        #endregion

        #region Operators
        public static bool operator <(BitBuffer512 lhs, BitBuffer512 rhs)
        {
            if (lhs._h < rhs._h) return true;
            if (lhs._g < rhs._g) return true;
            if (lhs._f < rhs._f) return true;
            if (lhs._e < rhs._e) return true;
            if (lhs._d < rhs._d) return true;
            if (lhs._c < rhs._c) return true;
            if (lhs._b < rhs._b) return true;
            if (lhs._a < rhs._a) return true;

            return false;
        }

        public static bool operator >(BitBuffer512 lhs, BitBuffer512 rhs)
        {
            if (lhs._h > rhs._h) return true;
            if (lhs._g > rhs._g) return true;
            if (lhs._f > rhs._f) return true;
            if (lhs._e > rhs._e) return true;
            if (lhs._d > rhs._d) return true;
            if (lhs._c > rhs._c) return true;
            if (lhs._b > rhs._b) return true;
            if (lhs._a > rhs._a) return true;

            return false;
        }

        public static bool operator <=(BitBuffer512 lhs, BitBuffer512 rhs) => lhs < rhs || lhs == rhs;

        public static bool operator >=(BitBuffer512 lhs, BitBuffer512 rhs) => lhs > rhs || lhs == rhs;

        public static bool operator ==(BitBuffer512 lhs, BitBuffer512 rhs) => lhs.Equals(rhs);

        public static bool operator !=(BitBuffer512 lhs, BitBuffer512 rhs) => !(lhs == rhs);
        #endregion
    }
}
