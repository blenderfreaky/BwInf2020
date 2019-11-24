namespace Nummernmerker
{
    using System;

    public static class ArraySegmentExtensions
    {
        public static T ElementAtUnchecked<T>(this ArraySegment<T> arraySegment, int index) => arraySegment.Array[arraySegment.Offset + index];
    }
}