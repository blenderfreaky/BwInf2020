using System;

namespace Extensions
{
    public static class Extensions
    {
        public static Tuple<T, P, R> DeepCopy<T, P, R>(this Tuple<T, P, R> origin) => 
            new Tuple<T, P, R>(origin.Item1, origin.Item2, origin.Item3);
    }
}
