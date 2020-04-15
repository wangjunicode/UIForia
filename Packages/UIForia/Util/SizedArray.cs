using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace UIForia.Util {

    public struct ReadOnlySizedArray<T> {

        public readonly int size;
        public readonly T[] array;

        public ReadOnlySizedArray(in SizedArray<T> source) {
            this.size = source.size;
            this.array = source.array;
        }

        public ReadOnlySizedArray(int size, T[] array) {
            this.size = size;
            this.array = array;
        }

        public static implicit operator ReadOnlySizedArray<T>(SizedArray<T> source) {
            return new ReadOnlySizedArray<T>(source);
        }

        public static ReadOnlySizedArray<T> CopyFrom(T[] source, int size = -1) {

            if (source == null || size == 0) {
                return default;
            }

            if (size < 0) {
                size = source.Length;
            }

            T[] array = new T[size];
            for (int i = 0; i < size; i++) {
                array[i] = source[i];
            }

            return new ReadOnlySizedArray<T>(size, array);
        }

    }

    public struct SizedArray<T> {

        public int size;
        public T[] array;

        public SizedArray(int capacity) {
            this.size = 0;
            if (capacity > 0) {
                this.array = new T[capacity];
            }
            else {
                this.array = default;
            }
        }

        public SizedArray(T[] data) {
            this.size = data.Length;
            this.array = data;
        }

        public T this[int idx] {
            get => array[idx];
            set => array[idx] = value;
        }

        public T Add(in T item) {
            if (array == null) {
                array = new T[4];
            }
            else if (size + 1 >= array.Length) {
                Array.Resize(ref array, size + array.Length * 2);
            }

            array[size] = item;
            size++;
            return item;
        }

        public SizedArray<T> Clone(int threshold = -1) {
            if (array == null) return default;
            if (threshold <= 0) {
                T[] a = new T[size];
                Array.Copy(a, 0, array, size, size);
                return new SizedArray<T>(a);
            }
            else {
                int diff = size - array.Length;
                T[] a;
                if (diff <= threshold) {
                    a = new T[array.Length + threshold];
                }
                else {
                    a = new T[array.Length];
                }
                Array.Copy(a, 0, array, size, size);
                return new SizedArray<T>() {
                    array = a,
                    size = size
                };
            }
        }

        public void AddRange(SizedArray<T> collection) {
            if (size + collection.size >= array.Length) {
                Array.Resize(ref array, size + collection.size * 2);
            }

            const int HandCopyThreshold = 5;
            if (collection.size < HandCopyThreshold) {
                int idx = size;
                for (int i = 0; i < collection.size; i++) {
                    array[idx++] = collection.array[i];
                }
            }
            else {
                Array.Copy(collection.array, 0, array, size, collection.size);
            }

            size += collection.size;
        }

        public void AddRange(ReadOnlySizedArray<T> collection) {
            if (size + collection.size >= array.Length) {
                Array.Resize(ref array, size + collection.size * 2);
            }

            const int HandCopyThreshold = 5;
            if (collection.size < HandCopyThreshold) {
                int idx = size;
                for (int i = 0; i < collection.size; i++) {
                    array[idx++] = collection.array[i];
                }
            }
            else {
                Array.Copy(collection.array, 0, array, size, collection.size);
            }

            size += collection.size;
        }

        public T[] CloneArray() {
            T[] retn = new T[size];
            for (int i = 0; i < size; i++) {
                retn[i] = array[i];
            }

            return retn;
        }

        public void Clear() {
            size = 0;
            if (array == null) {
                return;
            }
            Array.Clear(array, 0, array.Length);
        }

        public void EnsureCapacity(int count) {
            if (count > 0 && size + count >= array.Length) {
                Array.Resize(ref array, size + count * 2);
            }
        }

        public T[] ToArray() {
            T[] retn = new T[size];
            for (int i = 0; i < size; i++) {
                retn[i] = array[i];
            }

            return retn;
        }

    }

}