using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UIForia.Util {

    public class LightList<T> : IReadOnlyList<T>, IList<T> {

        private int size;
        private T[] list;

        public LightList(int size = 8) {
            this.list = ArrayPool<T>.GetMinSize(size);
            this.size = 0;
        }

        public T[] List => list;
        public int Count => size;
        
        public bool IsReadOnly => false;

        public int Capacity => list.Length;

        public void Add(T item) {
            if (size + 1 > list.Length) {
                ArrayPool<T>.Resize(ref list, (size + 1) * 2);
            }

            list[size] = item;
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
            list[size++] = item;
        }

        public void Clear() {
            Array.Clear(list, 0, list.Length);
            size = 0;
        }

        public void ResetSize() {
            size = 0;
        }

        public bool Contains(T item) {
            for (int i = 0; i < size; i++) {
                if (list[i].Equals(item)) return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex) {
            for (int i = 0; i < size; i++) {
                array[arrayIndex + i] = list[i];
            }
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
            if (size + 1 >= list.Length) {
                ArrayPool<T>.Resize(ref list, (size + 1) * 2);
            }
            size++;
            index = Mathf.Clamp(index, 0, size - 1);
            for (int i = index; i < size; i++) {
                list[i + 1] = list[i];
            }

            list[index] = item;
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
                        Array.Copy(list, index, list, index + count, size - index);
                    if (Equals(this, objs)) {
                        Array.Copy(list, 0, list, index, index);
                        Array.Copy(list, index + count, list, index * 2, size - index);
                    }
                    else {
                        T[] array = ArrayPool<T>.GetExactSize(count);
                        objs.CopyTo(array, 0);
                        array.CopyTo(list, index);
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
            T retn = list[size - 1];
            list[size - 1] = default;
            size--;
            return retn;
        }

        public void RemoveAt(int index) {
            if ((uint) index >= (uint) size) return;
            if (index == size - 1) {
                list[--size] = default;
            }
            else {
                for (int j = index; j < size - 1; j++) {
                    list[j] = list[j + 1];
                }

                list[--size] = default(T);
            }
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

        public int BinarySearch(T value, IComparer<T> comparer) {
            return InternalBinarySearch(list, 0, size, value, comparer);
        }

        public int BinarySearch(T value) {
            return InternalBinarySearch(list, 0, size, value, Comparer<T>.Default);
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

                current = list.list[index];
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