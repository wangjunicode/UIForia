using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIForia.Util {

    public class LightList<T> : IReadOnlyList<T>, IList<T> {

        private int size;
        private T[] array;

        public LightList(int size = 8) {
            this.array = ArrayPool<T>.GetMinSize(size);
            this.size = 0;
        }

        public T[] Array => array;
        public int Count => size;
        
        public bool IsReadOnly => false;

        public int Capacity => array.Length;

        public void Add(T item) {
            if (size + 1 > array.Length) {
                ArrayPool<T>.Resize(ref array, (size + 1) * 2);
            }

            array[size] = item;
            size++;
        }

        public void AddRange(IEnumerable<T> collection) {
            if (collection == null || Equals(collection, this)) {
                return;
            }

            foreach (var item in collection) {
                Add(item);
            }
        }

        public void AddUnchecked(T item) {
            array[size++] = item;
        }

        public void Clear() {
            System.Array.Clear(array, 0, array.Length);
            size = 0;
        }

        public void ResetSize() {
            size = 0;
        }

        public bool Contains(T item) {
            for (int i = 0; i < size; i++) {
                if (array[i].Equals(item)) return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            for (int i = 0; i < size; i++) {
                array[arrayIndex + i] = this.array[i];
            }
        }

        public bool Remove(T item) {
            for (int i = 0; i < size; i++) {
                if (array[i].Equals(item)) {
                    for (int j = i; j < size - 1; j++) {
                        array[j] = array[j + 1];
                    }

                    array[size - 1] = default(T);
                    size--;
                    return true;
                }
            }

            return false;
        }

        public int IndexOf(T item) {
            for (int i = 0; i < size; i++) {
                if (array[i].Equals(item)) return i;
            }

            return -1;
        }

        public void Insert(int index, T item) {
            if (size + 1 >= array.Length) {
                ArrayPool<T>.Resize(ref array, (size + 1) * 2);
            }
            size++;
            index = Mathf.Clamp(index, 0, size - 1);
            for (int i = index; i < size; i++) {
                array[i + 1] = array[i];
            }

            array[index] = item;
        }

        public void InsertRange(int index, IEnumerable<T> collection) {
            if (collection == null) {
                return;
            }

            if ((uint) index > (uint) size) {
                throw new IndexOutOfRangeException();
            }

            if (collection is ICollection<T> objs) {
                int count = objs.Count;
                if (count > 0) {
                    this.EnsureCapacity(size + count);
                    if (index < size)
                        System.Array.Copy(array, index, array, index + count, size - index);
                    if (Equals(this, objs)) {
                        System.Array.Copy(array, 0, array, index, index);
                        System.Array.Copy(array, index + count, array, index * 2, size - index);
                    }
                    else {
                        T[] array = ArrayPool<T>.GetExactSize(count);
                        objs.CopyTo(array, 0);
                        array.CopyTo(this.array, index);
                        ArrayPool<T>.Release(ref array);
                    }

                    size += count;
                }
            }
            else {
                foreach (T obj in collection) {
                    this.Insert(index++, obj);
                }
            }
        }

        public T RemoveLast() {
            T retn = array[size - 1];
            array[size - 1] = default;
            size--;
            return retn;
        }

        public void RemoveAt(int index) {
            if ((uint) index >= (uint) size) return;
            if (index == size - 1) {
                array[--size] = default;
            }
            else {
                for (int j = index; j < size - 1; j++) {
                    array[j] = array[j + 1];
                }

                array[--size] = default(T);
            }
        }

        public int FindIndex(Predicate<T> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(array[i])) {
                    return i;
                }
            }

            return -1;
        }

        public int FindIndex<U>(U closureArg, Func<T, U, bool> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(array[i], closureArg)) {
                    return i;
                }
            }

            return -1;
        }

        public T Find(Predicate<T> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(array[i])) {
                    return array[i];
                }
            }

            return default(T);
        }

        public T Find<U>(U closureArg, Func<T, U, bool> fn) {
            for (int i = 0; i < size; i++) {
                if (fn(array[i], closureArg)) {
                    return array[i];
                }
            }

            return default(T);
        }

        public T this[int index] {
            get { return array[index]; }
            set { array[index] = value; }
        }

        public void EnsureCapacity(int capacity) {
            if (array.Length < capacity) {
                ArrayPool<T>.Resize(ref array, capacity * 2);
            }
        }

        public void EnsureAdditionalCapacity(int capacity) {
            if (array.Length < size + capacity) {
                ArrayPool<T>.Resize(ref array, (size + capacity) * 2);
            }
        }

        public void Sort(int start, int end, IComparer<T> comparison) {
            System.Array.Sort(array, start, end, comparison);
        }

        public void Sort(int start, int end, Comparison<T> comparison) {
            s_Compare.comparison = comparison;
            System.Array.Sort(array, start, end, s_Compare);
            s_Compare.comparison = null;
        }

        public void Sort(Comparison<T> comparison) {
            s_Compare.comparison = comparison;
            System.Array.Sort(array, 0, size, s_Compare);
            s_Compare.comparison = null;
        }

        public void Sort(IComparer<T> comparison) {
            System.Array.Sort(array, 0, size, comparison);
        }

        public int BinarySearch(T value, IComparer<T> comparer) {
            return InternalBinarySearch(array, 0, size, value, comparer);
        }

        public int BinarySearch(T value) {
            return InternalBinarySearch(array, 0, size, value, Comparer<T>.Default);
        }

        private static int InternalBinarySearch(T[] array, int index, int length, T value, IComparer<T> comparer) {
            int num1 = index;
            int num2 = index + length - 1;
            while (num1 <= num2) {
                int index1 = num1 + (num2 - num1 >> 1);
                int num3 = comparer.Compare(array[index1], value);

                if (num3 == 0) {
                    return index1;
                }

                if (num3 < 0) {
                    num1 = index1 + 1;
                }
                else {
                    num2 = index1 - 1;
                }
            }

            return ~num1;
        }

        private static readonly FunctorComparer s_Compare = new FunctorComparer();

        private sealed class FunctorComparer : IComparer<T> {

            public Comparison<T> comparison;

            public int Compare(T x, T y) {
                return this.comparison(x, y);
            }

        }

        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this);
        }

        public class Enumerator : IEnumerator<T> {

            private int index;
            private T current;
            private readonly LightList<T> list;

            internal Enumerator(LightList<T> list) {
                this.list = list;
                this.index = 0;
                this.current = default(T);
            }

            public void Dispose() { }

            public bool MoveNext() {
                if ((uint) index >= (uint) list.size) {
                    index = list.size + 1;
                    current = default(T);
                    return false;
                }

                current = list.array[index];
                ++index;
                return true;
            }

            public T Current => current;

            object IEnumerator.Current => Current;

            void IEnumerator.Reset() {
                index = 0;
                current = default(T);
            }

        }

    }

}