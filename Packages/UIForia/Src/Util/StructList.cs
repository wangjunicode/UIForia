using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Parsing.Expression.Tokenizer;

namespace UIForia.Util {

    [DebuggerTypeProxy(typeof(StructList<>))]
    internal class StructListDebugView<T> where T : struct {

        private readonly StructList<T> structList;
        public T[] array;

        public StructListDebugView(StructList<T> structList) {
            this.structList = structList;
            array = structList.ToArray();
        }

    }

    [DebuggerDisplay("StructList Count = {" + nameof(size) + "} | capacity = {array.Length}")]
    [DebuggerTypeProxy(typeof(StructListDebugView<>))]
    public class StructList<T> where T : struct {

        public T[] array;
        public int size;
        private bool isInPool;

        public T[] Array => array;

        public StructList(int capacity = 8) {
            this.size = 0;
            this.array = new T[capacity];
        }

        public StructList(T[] array) {
            this.array = array;
            this.size = array.Length;
        }

        public int Count {
            get { return size; }
            set { size = value; }
        }

        public void Add(in T item) {
            if (size + 1 > array.Length) {
                System.Array.Resize(ref array, (size + 1) * 2);
            }

            array[size] = item;
            size++;
        }

        public void AddUnsafe(in T item) {
            array[size++] = item;
        }

        public void AddRange(T[] collection) {
            if (size + collection.Length >= array.Length) {
                System.Array.Resize(ref array, size + collection.Length * 2);
            }

            if (collection.Length < HandCopyThreshold) {
                int idx = size;
                for (int i = 0; i < collection.Length; i++) {
                    array[idx++] = collection[i];
                }
            }
            else {
                System.Array.Copy(collection, 0, array, size, collection.Length);
            }

            size += collection.Length;
        }

        public void AddRange(T[] collection, int start, int count) {
            if (size + count >= array.Length) {
                System.Array.Resize(ref array, size + count * 2);
            }

            if (count < HandCopyThreshold) {
                int idx = size;
                for (int i = start; i < count; i++) {
                    array[idx++] = collection[i];
                }
            }
            else {
                System.Array.Copy(collection, start, array, size, count);
            }

            size += count;
        }

        public void AddRange(StructList<T> collection) {
            if (size + collection.size >= array.Length) {
                System.Array.Resize(ref array, size + collection.size * 2);
            }

            if (collection.size < HandCopyThreshold) {
                T[] src = collection.array;
                int count = collection.size;
                for (int i = 0; i < count; i++) {
                    array[size + i] = src[i];
                }
            }
            else {
                System.Array.Copy(collection.array, 0, array, size, collection.size);
            }

            size += collection.size;
        }

        private const int HandCopyThreshold = 8;

        public void AddRange(StructList<T> collection, int start, int count) {
            if (size + count >= array.Length) {
                System.Array.Resize(ref array, size + count * 2);
            }

            if (collection.size < HandCopyThreshold) {
                T[] src = collection.array;
                int idx = size;
                for (int i = start; i < count; i++) {
                    array[idx++] = src[i];
                }
            }
            else {
                System.Array.Copy(collection.array, start, array, size, count);
            }

            size += count;
        }

        public void EnsureCapacity(int capacity) {
            if (array.Length < capacity) {
                System.Array.Resize(ref array, capacity * 2);
            }
        }

        public void EnsureAdditionalCapacity(int capacity) {
            if (array.Length < size + capacity) {
                System.Array.Resize(ref array, (size + capacity) * 2);
            }
        }

        public void QuickClear() {
            size = 0;
        }

        public void Clear() {
            size = 0;
            System.Array.Clear(array, 0, array.Length);
        }

        public T this[int idx] {
            get => array[idx];
            set => array[idx] = value;
        }

        public void SetFromRange(T[] source, int start, int count) {
            if (array.Length <= count) {
                System.Array.Resize(ref array, count * 2);
            }

            System.Array.Copy(source, start, array, 0, count);
            size = count;
        }

        public void ShiftRight(int startIndex, int count) {
            if (count <= 0) return;
            if (startIndex < 0) startIndex = 0;
            EnsureCapacity(startIndex + count + count); // I think this is too big
            System.Array.Copy(array, startIndex, array, startIndex + count, count);
            System.Array.Clear(array, startIndex, count);
            size += count;
        }

        public void ShiftLeft(int startIndex, int count) {
            if (count <= 0) return;
            if (startIndex < 0) startIndex = 0;
            System.Array.Copy(array, startIndex, array, startIndex - count, size - startIndex);
            System.Array.Clear(array, size - count, count);
            size -= count;
        }


        private void QuickSort(Comparison<T> comparison, int low, int high) {
            while (true) {
                if (low < high) {
                    int partition = Partition(comparison, low, high);
                    QuickSort(comparison, low, partition - 1);
                    low = partition + 1;
                    continue;
                }

                break;
            }
        }

        private void QuickSort(IComparer<T> comparison, int low, int high) {
            while (true) {
                if (low < high) {
                    int partition = Partition(comparison, low, high);
                    QuickSort(comparison, low, partition - 1);
                    low = partition + 1;
                    continue;
                }

                break;
            }
        }

        private int Partition(Comparison<T> comparison, int low, int high) {
            T temp;
            T pivot = array[high];

            int i = (low - 1);
            for (int j = low; j <= high - 1; j++) {
                if (comparison(array[j], pivot) <= 0) {
                    i++;

                    temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                }
            }

            temp = array[i + 1];
            array[i + 1] = array[high];
            array[high] = temp;

            return i + 1;
        }

        private int Partition(IComparer<T> comparison, int low, int high) {
            T temp;
            T pivot = array[high];

            int i = (low - 1);
            for (int j = low; j <= high - 1; j++) {
                if (comparison.Compare(array[j], pivot) <= 0) {
                    i++;

                    temp = array[i];
                    array[i] = array[j];
                    array[j] = temp;
                }
            }

            temp = array[i + 1];
            array[i + 1] = array[high];
            array[high] = temp;

            return i + 1;
        }

        public void Sort(Comparison<T> comparison) {
            if (size < 2) return;
            QuickSort(comparison, 0, size - 1);
        }

        public void Sort(Comparison<T> comparison, int start, int end) {
            if (size < 2) return;
            if (start < 0) start = 0;
            if (start >= size) start = size - 1;
            if (end >= size) end = size - 1;
            QuickSort(comparison, start, end);
        }

        public void Sort(IComparer<T> comparison, int start, int end) {
            if (size < 2) return;
            if (start < 0) start = 0;
            if (start >= size) start = size - 1;
            if (end >= size) end = size - 1;
            QuickSort(comparison, start, end);
        }

        public void Sort(IComparer<T> comparison) {
            if (size < 2) return;
            QuickSort(comparison, 0, size - 1);
        }

        public StructList<T> GetRange(int index, int count, StructList<T> retn = null) {
            if (retn == null) {
                retn = Get(count);
            }
            else {
                retn.EnsureCapacity(count);
            }

            System.Array.Copy(array, index, retn.array, 0, count);
            retn.size = count;
            return retn;
        }

        private static readonly LightList<StructList<T>> s_Pool = new LightList<StructList<T>>();

        public static implicit operator StructList<T>(T[] array) {
            return new StructList<T>(array);
        }

        public static StructList<T> Get() {
            StructList<T> retn = s_Pool.Count > 0 ? s_Pool.RemoveLast() : new StructList<T>();
            retn.isInPool = false;
            return retn;
        }

        public static StructList<T> Get(int minCapacity) {
            if (minCapacity < 1) minCapacity = 4;
            StructList<T> retn = s_Pool.Count > 0 ? s_Pool.RemoveLast() : new StructList<T>(minCapacity);
            retn.isInPool = false;

            if (retn.array.Length < minCapacity) {
                T[] array = retn.array;
                System.Array.Resize(ref array, minCapacity);
                retn.array = array;
            }

            return retn;
        }

        public static void Release(ref StructList<T> toPool) {
            toPool.Clear();
            if (toPool.isInPool) return;
            toPool.isInPool = true;
            s_Pool.Add(toPool);
        }

        public void Insert(int index, in T item) {
            if (size + 1 >= array.Length) {
                ArrayPool<T>.Resize(ref array, (size + 1) * 2);
            }

            size++;
            if (index < 0 || index > array.Length) {
                throw new IndexOutOfRangeException();
            }

            System.Array.Copy(array, index, array, index + 1, size - index);
            array[index] = item;
        }

        public T[] ToArray() {
            T[] retn = new T[size];
            System.Array.Copy(array, 0, retn, 0, size);
            return retn;
        }

    }

}