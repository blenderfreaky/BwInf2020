namespace Rominos
{
    using System.Collections.Generic;
    using System.Reflection;

    // https://stackoverflow.com/questions/6771917/why-cant-i-preallocate-a-hashsett
    public static class HashSetExtensions
    {
        private static class HashSetDelegateHolder<T>
        {
            private const BindingFlags Flags = BindingFlags.Instance | BindingFlags.NonPublic;
            public static MethodInfo InitializeMethod { get; } = typeof(HashSet<T>).GetMethod("Initialize", Flags);
        }

        public static void SetCapacity<T>(this HashSet<T> hs, int capacity)
        {
            HashSetDelegateHolder<T>.InitializeMethod.Invoke(hs, new object[] { capacity });
        }

#pragma warning disable RCS1224 // Make method an extension method.
        public static HashSet<T> GetHashSet<T>(int capacity)
#pragma warning restore RCS1224 // Make method an extension method.
        {
            var hashSet = new HashSet<T>();
            hashSet.SetCapacity(capacity);
            return hashSet;
        }
    }
}
