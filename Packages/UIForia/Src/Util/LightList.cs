using System;
using System.Collections.Generic;
using UnityEngine;

namespace UIForia.Util {

    public class LightList<T> {

        private int size;
        private T[] list;

        public LightList(int size = 8) {
            this.list = ArrayPool<T>.GetMinSize(size);
            this.size = 0;
        }

        public T[] List => list;
        public int Count => size;
        public int Length => size;
        public int Capacity => list.Length;

        public void Add(T item) {
            if (size + 1 > list.Length) {
                ArrayPool<T>.Resize(ref list, (size + 1) * 2);
            }

            list[size] = item;
            size++;
        }

        public void AddUnchecked(T item) {
            list[size] = item;
            size++;
        }

        public void Clear() {
            Array.Clear(list, 0, list.Length);
            size = 0;
        }
        
        public void DangerousClear() {
            size = 0;
        }

        public bool Contains(T item) {
            for (int i = 0; i < size; i++) {
                if (list[i].Equals(item)) return true;
            }

            return false;
        }

        public bool Remove(T item) {
            for (int i = 0; i < size; i++) {
                if (list[i].Equals(item)) {
                    for (int j = i; j < size - 1; j++) {
                        list[j] = list[j + 1];
                    }

                    list[size - 1] = default(T);
                    size--;
                    return true;
                }
            }

            return false;
        }


        public int IndexOf(T item) {
            for (int i = 0; i < size; i++) {
                if (list[i].Equals(item)) return i;
            }

            return -1;
        }

        public void Insert(int index, T item) {
            Add(item);
            index = Mathf.Clamp(index, 0, size);
            for (int i = index; i < size; i++) {
                list[i + 1] = list[i];
            }

            list[index] = list[size - 1];
        }

        public void RemoveAt(int index) {
            if ((uint) index >= (uint) size) return;
            for (int j = index; j < size; j++) {
                list[j] = list[j + 1];
            }

            list[size - 1] = default(T);
            size--;
        }

        public int FindIndex(Predicate<T> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(list[i])) {
                    return i;
                }
            }

            return -1;
        }
        
        public int FindIndex<U>(U closureArg, Func<T, U, bool> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(list[i], closureArg)) {
                    return i;
                }
            }

            return -1;
        }

        public T Find(Predicate<T> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(list[i])) {
                    return list[i];
                }
            }

            return default(T);
        }
        
        public T Find<U>(U closureArg, Func<T, U, bool> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(list[i], closureArg)) {
                    return list[i];
                }
            }

            return default(T);
        }

        
        public T this[int index] {
            get { return list[index]; }
            set { list[index] = value; }
        }

        public void EnsureCapacity(int capacity) {
            if (list.Length < capacity) {
                ArrayPool<T>.Resize(ref list, capacity * 2);
            }
        }

        public void EnsureAdditionalCapacity(int capacity) {
            if (list.Length < size + capacity) {
                ArrayPool<T>.Resize(ref list, (size + capacity) * 2);
            }
        }

        public void Sort(int start, int end, IComparer<T> comparison) {
            Array.Sort(list, start, end, comparison);
        }
        
        public void Sort(int start, int end, Comparison<T> comparison) {
            s_Compare.comparison = comparison;
            Array.Sort(list, start, end, s_Compare);
            s_Compare.comparison = null;
        }
        
        public void Sort(Comparison<T> comparison) {
            s_Compare.comparison = comparison;
            Array.Sort(list, 0, size, s_Compare);
            s_Compare.comparison = null;
        }
        
        public void Sort(IComparer<T> comparison) {
            Array.Sort(list, 0, size, comparison);
        }

        private static readonly FunctorComparer s_Compare = new FunctorComparer();

        internal sealed class FunctorComparer : IComparer<T> {

            public Comparison<T> comparison;

            public int Compare(T x, T y) {
                return this.comparison(x, y);
            }

        }


//        internal class ArraySortHelper<T> {
//
//            public void Sort(T[] keys, int index, int length, IComparer<T> comparer) {
//                if (comparer == null)
//                    comparer = (IComparer<T>) Comparer<T>.Default;
//                ArraySortHelper<T>.IntrospectiveSort(keys, index, length, comparer);
//            }
//
//            public int BinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer) {
//                try {
//                    if (comparer == null)
//                        comparer = (IComparer<T>) Comparer<T>.Default;
//                    return ArraySortHelper<T>.InternalBinarySearch(array, index, length, value, comparer);
//                }
//                catch (Exception ex) {
//                    throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_IComparerFailed"), ex);
//                }
//            }
//
//            internal static int InternalBinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer) {
//                int num1 = index;
//                int num2 = index + length - 1;
//                while (num1 <= num2) {
//                    int index1 = num1 + (num2 - num1 >> 1);
//                    int num3 = comparer.Compare(array[index1], value);
//                    if (num3 == 0)
//                        return index1;
//                    if (num3 < 0)
//                        num1 = index1 + 1;
//                    else
//                        num2 = index1 - 1;
//                }
//
//                return ~num1;
//            }
//
//            private static void SwapIfGreater(T[] keys, IComparer<T> comparer, int a, int b) {
//                if (a == b || comparer.Compare(keys[a], keys[b]) <= 0)
//                    return;
//                T key = keys[a];
//                keys[a] = keys[b];
//                keys[b] = key;
//            }
//
//            private static void Swap(T[] a, int i, int j) {
//                if (i == j)
//                    return;
//                T obj = a[i];
//                a[i] = a[j];
//                a[j] = obj;
//            }
//
//
//            internal static void IntrospectiveSort(T[] keys, int left, int length, IComparer<T> comparer) {
//                if (length < 2)
//                    return;
//                IntroSort(keys, left, length + left - 1, 2 * FloorLog2(keys.Length), comparer);
//            }
//
//            internal static int FloorLog2(int n) {
//                int num = 0;
//                while (n >= 1) {
//                    ++num;
//                    n /= 2;
//                }
//
//                return num;
//            }
//
//            private static void IntroSort(T[] keys, int lo, int hi, int depthLimit, IComparer<T> comparer) {
//                while (hi > lo) {
//                    int partitionSize = hi - lo + 1;
//                    if (partitionSize <= 16) {
//                        if (partitionSize == 1) {
//                            return;
//                        }
//
//                        if (partitionSize == 2) {
//                            SwapIfGreater(keys, comparer, lo, hi);
//                            return;
//                        }
//
//                        if (partitionSize == 3) {
//                            SwapIfGreater(keys, comparer, lo, hi - 1);
//                            SwapIfGreater(keys, comparer, lo, hi);
//                            SwapIfGreater(keys, comparer, hi - 1, hi);
//                            return;
//                        }
//
//                        InsertionSort(keys, lo, hi, comparer);
//                        return;
//                    }
//
//                    if (depthLimit == 0) {
//                        Heapsort(keys, lo, hi, comparer);
//                        return;
//                    }
//
//                    depthLimit--;
//
//                    int p = PickPivotAndPartition(keys, lo, hi, comparer);
//                    // Note we've already partitioned around the pivot and do not have to move the pivot again.
//                    IntroSort(keys, p + 1, hi, depthLimit, comparer);
//                    hi = p - 1;
//                }
//            }
//
//            private static int PickPivotAndPartition(T[] keys, int lo, int hi, IComparer<T> comparer) {
//                int index = lo + (hi - lo) / 2;
//                ArraySortHelper<T>.SwapIfGreater(keys, comparer, lo, index);
//                ArraySortHelper<T>.SwapIfGreater(keys, comparer, lo, hi);
//                ArraySortHelper<T>.SwapIfGreater(keys, comparer, index, hi);
//                T key = keys[index];
//                ArraySortHelper<T>.Swap(keys, index, hi - 1);
//                int i = lo;
//                int j = hi - 1;
//                while (i < j) {
//                    do
//                        ;
//                    while (comparer.Compare(keys[++i], key) < 0);
//                    do
//                        ;
//                    while (comparer.Compare(key, keys[--j]) < 0);
//                    if (i < j)
//                        ArraySortHelper<T>.Swap(keys, i, j);
//                    else
//                        break;
//                }
//
//                ArraySortHelper<T>.Swap(keys, i, hi - 1);
//                return i;
//            }
//
//            private static void Heapsort(T[] keys, int lo, int hi, IComparer<T> comparer) {
//                int n = hi - lo + 1;
//                for (int i = n / 2; i >= 1; --i)
//                    ArraySortHelper<T>.DownHeap(keys, i, n, lo, comparer);
//                for (int index = n; index > 1; --index) {
//                    ArraySortHelper<T>.Swap(keys, lo, lo + index - 1);
//                    ArraySortHelper<T>.DownHeap(keys, 1, index - 1, lo, comparer);
//                }
//            }
//
//            private static void DownHeap(T[] keys, int i, int n, int lo, IComparer<T> comparer) {
//                T key = keys[lo + i - 1];
//                int num;
//                for (; i <= n / 2; i = num) {
//                    num = 2 * i;
//                    if (num < n && comparer.Compare(keys[lo + num - 1], keys[lo + num]) < 0)
//                        ++num;
//                    if (comparer.Compare(key, keys[lo + num - 1]) < 0)
//                        keys[lo + i - 1] = keys[lo + num - 1];
//                    else
//                        break;
//                }
//
//                keys[lo + i - 1] = key;
//            }
//
//            private static void InsertionSort(T[] keys, int lo, int hi, IComparer<T> comparer) {
//                for (int index1 = lo; index1 < hi; ++index1) {
//                    int index2 = index1;
//                    T key;
//                    for (key = keys[index1 + 1]; index2 >= lo && comparer.Compare(key, keys[index2]) < 0; --index2)
//                        keys[index2 + 1] = keys[index2];
//                    keys[index2 + 1] = key;
//                }
//            }
//
//        }

    }

}