using System;
using System.Collections.Generic;
using System.Linq;

namespace Extensions
{
    public static class Extensions
    {
        public static Tuple<T, P, R> DeepCopy<T, P, R>(this Tuple<T, P, R> origin) => 
            new Tuple<T, P, R>(origin.Item1, origin.Item2, origin.Item3);

        public static List<T> GetDublicates<T>(this List<T> Input, bool ReturnDuplicates = true, Func<T, T, bool> Test = null)
            where T: IEquatable<T>
        {
            List<T> Singlos = new List<T>();
            List<T> Dubblos = new List<T>();
            foreach(T t in Input)
            {
                foreach(T tt in Singlos)
                {
                    if((Test != null)?(Test(t, t)):(t.Equals(tt)))
                    {
                        if(!(!ReturnDuplicates && Dubblos.Contains(t)))
                            Dubblos.Add(t);
                    }
                }
            }
            return Dubblos;
        }
    }
}
