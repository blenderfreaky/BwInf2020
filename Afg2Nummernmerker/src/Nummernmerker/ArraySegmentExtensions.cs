using System;
using System.Collections.Generic;
using System.Text;

namespace Nummernmerker
{
    public static class ArraySegmentExtensions
    {
        public static T ElementAtUnchecked<T>(this ArraySegment<T> arraySegment, int index) => arraySegment.Array[arraySegment.Offset + index];
    }
}
