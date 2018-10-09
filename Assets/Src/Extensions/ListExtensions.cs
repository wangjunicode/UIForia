using System.Collections.Generic;

namespace Src.Extensions {

    public static class ListExtensions {

        public static void UnstableRemove<T>(this List<T> list, int index) {
            T value = list[list.Count - 1];
            list[index] = value;
            list.RemoveAt(list.Count - 1);
        } 

    }

}