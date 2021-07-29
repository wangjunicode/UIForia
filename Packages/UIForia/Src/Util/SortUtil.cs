using System;

namespace UIForia.Util {

    public interface IComparableWithoutCopy<T> where T : unmanaged {

        int CompareWithoutCopy(in T other);

    }

    public interface IComparerWithoutCopy<T> where T : unmanaged {

        int CompareWithoutCopy(in T a, in T b);

    }

    public static unsafe class SortUtil {

        public static void InsertionSort<T>(T* array, int count) where T : unmanaged, IComparableWithoutCopy<T> {
            for (int i = 1; i <= count - 1; i++) {
                T temp = array[i];
                int j = i - 1;
                while (j >= 0 && array[j].CompareWithoutCopy(temp) > 0) {
                    array[j + 1] = array[j];
                    j--;
                }

                array[j + 1] = temp;
            }
        }

        public static void InsertionSort<T, U>(T* array, int count, U cmp) where T : unmanaged where U : IComparerWithoutCopy<T> {
            for (int i = 1; i <= count - 1; i++) {
                T temp = array[i];
                int j = i - 1;
                while (j >= 0 && cmp.CompareWithoutCopy(array[j], temp) > 0) {
                    array[j + 1] = array[j];
                    j--;
                }

                array[j + 1] = temp;
            }
        }

        public static void BubbleSort<T>(T* array, int count) where T : unmanaged, IComparableWithoutCopy<T> {
            int n = count;
            do {
                int sw = 0; // last swap index

                for (int i = 0; i < n - 1; i++) {

                    int cmpVal = array[i].CompareWithoutCopy(array[i + 1]);

                    if (cmpVal <= 0) {
                        continue;
                    }

                    T temp = array[i];
                    array[i] = array[i + 1];
                    array[i + 1] = temp;
                    // Save swap position
                    sw = i + 1;
                }

                // We do not need to visit all elements
                // we only need to go as far as the last swap
                n = sw;
            }

            //Once n = 1 then the whole list is sorted
            while (n > 1);

        }

    }

}