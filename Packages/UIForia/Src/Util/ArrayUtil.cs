using System.Collections.Generic;

namespace UIForia.Util {

    public static class ArrayUtil {

        public static void ReverseInPlace<T>(IList<T> list) {
            for (int i = 0; i < list.Count / 2; i++) {
                T temp = list[i];
                list[i] = list[list.Count - i - 1];
                list[list.Count - i - 1] = temp;
            }
        } 


    }

}