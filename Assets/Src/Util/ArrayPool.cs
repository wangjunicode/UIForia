using System;
using System.Collections.Generic;
using UnityEngine;

namespace Src.Util {

    public static class ArrayPool<T> {

        private static readonly List<T[]> s_ArrayPool = new List<T[]>();

        public static int MaxPoolSize = 16;

        public static T[] GetMinSize(int minSize) {
            minSize = Mathf.Max(0, minSize);
            for (int i = 0; i < s_ArrayPool.Count; i++) {
                if (s_ArrayPool[i].Length >= minSize) {
                    T[] retn = s_ArrayPool[i];
                    s_ArrayPool.RemoveAt(i);
                    return retn;
                }
            }

            return new T[minSize];
        }

        public static T[] GetExactSize(int size) {
            size = Mathf.Max(0, size);
            for (int i = 0; i < s_ArrayPool.Count; i++) {
                if (s_ArrayPool[i].Length == size) {
                    T[] retn = s_ArrayPool[i];
                    s_ArrayPool.RemoveAt(i);
                    return retn;
                }
            }

            return new T[size];
        }

        public static void Resize(ref T[] array, int minSize) {
            minSize = Mathf.Max(0, minSize);
            for (int i = 0; i < s_ArrayPool.Count; i++) {
                if (s_ArrayPool[i].Length >= minSize) {
                    T[] retn = s_ArrayPool[i];
                    s_ArrayPool[i] = array;
                    Array.Clear(array, 0, array.Length);
                    array = retn;
                    return;
                }
            }

            Array.Resize(ref array, minSize);
        }

        public static void Release(T[] array) {
            if (array == null) return;
            Array.Clear(array, 0, array.Length);
            if (s_ArrayPool.Count == MaxPoolSize) {
                int minCount = int.MaxValue;
                int minIndex = 0;
                for (int i = 0; i < s_ArrayPool.Count; i++) {
                    if (s_ArrayPool[i].Length < minCount) {
                        minCount = s_ArrayPool[i].Length;
                        minIndex = i;
                    }
                }

                if (array.Length > minCount) {
                    s_ArrayPool[minIndex] = array;
                }
            }
            else {
                s_ArrayPool.Add(array);
            }
        }

        public static T[] CopyFromList(IList<T> source) {
            T[] retn = GetMinSize(source.Count);
            for (int i = 0; i < source.Count; i++) {
                retn[i] = source[i];
            }
            return retn;
        }

    }

}