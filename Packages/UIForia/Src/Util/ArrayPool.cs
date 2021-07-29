using System;

namespace UIForia.Util {

    internal static class ArrayPool<T> {

        [ThreadStatic] private static T[] array0;
        [ThreadStatic] private static T[] array1;
        [ThreadStatic] private static T[] array2;
        [ThreadStatic] private static T[] array3;
        [ThreadStatic] private static T[] array4;
        [ThreadStatic] private static T[] array5;
        [ThreadStatic] private static T[] array6;
        [ThreadStatic] private static T[] array7;
        [ThreadStatic] private static T[] array8;

        public static T[] GetOrAllocate(int count) {
            if (count <= 8) return Get(count);
            return new T[count];
        }

        public static T[] Get(T t0) {
            T[] retn = Get(1);
            retn[0] = t0;
            return retn;
        }

        public static T[] Get(T t0, T t1) {
            T[] retn = Get(2);
            retn[0] = t0;
            retn[1] = t1;
            return retn;
        }

        public static T[] Get(T t0, T t1, T t2) {
            T[] retn = Get(3);
            retn[0] = t0;
            retn[1] = t1;
            retn[2] = t2;
            return retn;
        }

        public static T[] Get(T t0, T t1, T t2, T t3) {
            T[] retn = Get(4);
            retn[0] = t0;
            retn[1] = t1;
            retn[2] = t2;
            retn[3] = t3;
            return retn;
        }

        public static T[] Get(T t0, T t1, T t2, T t3, T t4) {
            T[] retn = Get(5);
            retn[0] = t0;
            retn[1] = t1;
            retn[2] = t2;
            retn[3] = t3;
            retn[4] = t4;
            return retn;
        }

        public static T[] Get(T t0, T t1, T t2, T t3, T t4, T t5) {
            T[] retn = Get(6);
            retn[0] = t0;
            retn[1] = t1;
            retn[2] = t2;
            retn[3] = t3;
            retn[4] = t4;
            retn[5] = t5;
            return retn;
        }

        public static T[] Get(T t0, T t1, T t2, T t3, T t4, T t5, T t6) {
            T[] retn = Get(7);
            retn[0] = t0;
            retn[1] = t1;
            retn[2] = t2;
            retn[3] = t3;
            retn[4] = t4;
            retn[5] = t5;
            retn[6] = t6;
            return retn;
        }

        public static T[] Get(T t0, T t1, T t2, T t3, T t4, T t5, T t6, T t7) {
            T[] retn = Get(7);
            retn[0] = t0;
            retn[1] = t1;
            retn[2] = t2;
            retn[3] = t3;
            retn[4] = t4;
            retn[5] = t5;
            retn[6] = t6;
            retn[7] = t7;
            return retn;
        }

        public static T[] Get(int count) {
            switch (count) {
                case 0:
                    array0 ??= new T[0];
                    return array0;

                case 1:
                    array1 ??= new T[1];
                    return array1;

                case 2:
                    array2 ??= new T[2];
                    return array2;

                case 3:
                    array3 ??= new T[3];
                    return array3;

                case 4:
                    array4 ??= new T[4];
                    return array4;

                case 5:
                    array5 ??= new T[5];
                    return array5;

                case 6:
                    array6 ??= new T[6];
                    return array6;

                case 7:
                    array7 ??= new T[7];
                    return array7;

                case 8:
                    array8 ??= new T[8];
                    return array8;

                default:
                    throw new ArgumentOutOfRangeException(nameof(count), "Array pool supports only up to 8 arguments");
            }
        }

    }

}