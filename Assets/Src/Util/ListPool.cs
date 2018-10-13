using System.Collections.Generic;

namespace Src.Util {

    public static class ListPool<T> {

        private static readonly ObjectPool<List<T>> s_ListPool = new ObjectPool<List<T>>(null, l => l.Clear());
        public static readonly IReadOnlyList<T> Empty = new List<T>(0);

        public static List<T> Get() {
            return s_ListPool.Get();
        }

        public static void Release(ref List<T> toRelease) {
            if (Equals(toRelease, Empty)) {
                s_ListPool.Release(toRelease);
            }
            toRelease = null;
        }

    }

}