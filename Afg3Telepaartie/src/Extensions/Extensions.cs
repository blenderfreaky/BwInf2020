using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    public static class Extensions
    {
        public static Tuple<T, P, R> DeepCopy<T, P, R>(this Tuple<T, P, R> origin) => 
            new Tuple<T, P, R>(origin.Item1, origin.Item2, origin.Item3);

        public static List<T> GetDublicates<T>(this List<T> Input, bool ReturnDublicates = true, Func<T, bool> Test = null)
        {
            List<T> Singlos = new List<T>();
            List<T> Dubblos = new List<T>();
            return Dubblos;
        }
    }
}
