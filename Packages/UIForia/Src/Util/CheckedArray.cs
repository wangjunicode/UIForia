using System.Diagnostics;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia {

    [NoAlias]
    [DebuggerDisplay("size = {" + nameof(size) + "}")]
    [DebuggerTypeProxy(typeof(CheckedArrayDebugView<>))]
    public unsafe struct CheckedArray<T> where T : unmanaged {

        [NativeDisableUnsafePtrRestriction] public readonly T* array;
        public readonly int size;

        [DebuggerStepThrough]
        public CheckedArray(T* array, int size) {
            this.array = array;
            this.size = size;
        }

        [DebuggerStepThrough]
        public CheckedArray(DataList<T> buffer, RangeInt range) {
            size = range.length;
            array = buffer.GetArrayPointer() + range.start;
        }

        [DebuggerStepThrough]
        public CheckedArray(T* buffer, RangeInt range) {
            size = range.length;
            array = buffer + range.start;
        }

        [DebuggerStepThrough]
        public CheckedArray(DataList<T> list) {
            size = list.size;
            array = list.GetArrayPointer();
        }

        [DebuggerStepThrough]
        public ref T Get(int idx) {
            TypedUnsafe.CheckRange(idx, size);
            return ref array[idx];
        }

        [DebuggerStepThrough]
        public void Set(int idx, in T value) {
            TypedUnsafe.CheckRange(idx, size);
            array[idx] = value;
        }

        public T this[int idx] {
            [DebuggerStepThrough]
            get {
                TypedUnsafe.CheckRange(idx, size);
                return array[idx];
            }
            [DebuggerStepThrough]
            set {
                TypedUnsafe.CheckRange(idx, size);
                array[idx] = value;
            }
        }

        public T this[uint idx] {
            [DebuggerStepThrough]
            get {
                TypedUnsafe.CheckRange(idx, size);
                return array[idx];
            }
            [DebuggerStepThrough]
            set {
                TypedUnsafe.CheckRange(idx, size);
                array[idx] = value;
            }
        }

        [DebuggerStepThrough]
        public T[] ToArray() {
            T[] dst = new T[size];
            for (int i = 0; i < size; i++) {
                dst[i] = array[i];
            }

            return dst;
        }

        [DebuggerStepThrough]
        public T* GetArrayPointer() {
            return array;
        }

        [DebuggerStepThrough]
        public CheckedArray<T> Slice(int start, int count) {
            TypedUnsafe.CheckRange(start, size);
            TypedUnsafe.CheckRange(start + count - 1, size);
            return new CheckedArray<T>(array + start, count);
        }

        [DebuggerStepThrough]
        public CheckedArray<T> Slice(RangeInt range) {
            return Slice(range.start, range.length);
        }

        [DebuggerStepThrough]
        public T* GetPointer(int offset) {
            TypedUnsafe.CheckRange(offset, size);
            return array + offset;
        }

        public void MemClear() {
            TypedUnsafe.MemClear(array, size);
        }

        public static CheckedArray<T> TypeCast(DataList<byte> values) {
            return new CheckedArray<T>((T*)values.array, values.size / sizeof(T));
        }

    }

    internal sealed class CheckedArrayDebugView<T> where T : unmanaged {

        private CheckedArray<T> m_Array;

        public CheckedArrayDebugView(CheckedArray<T> array) => this.m_Array = array;

        public T[] Items => this.m_Array.ToArray();

    }

    [NoAlias]
    [DebuggerDisplay("size = {" + nameof(size) + "}")]
    [DebuggerTypeProxy(typeof(CheckedArrayDebugView<>))]
    public unsafe struct ElementCheckedArray<T> where T : unmanaged {

        [NativeDisableUnsafePtrRestriction] public readonly T* array;
        public readonly int size;

        [DebuggerStepThrough]
        public ElementCheckedArray(T* array, int size) {
            this.array = array;
            this.size = size;
        }

        [DebuggerStepThrough]
        public ElementCheckedArray(DataList<T> buffer, RangeInt range) {
            size = range.length;
            array = buffer.GetArrayPointer() + range.start;
        }

        [DebuggerStepThrough]
        public ElementCheckedArray(T* buffer, RangeInt range) {
            size = range.length;
            array = buffer + range.start;
        }

        [DebuggerStepThrough]
        public ElementCheckedArray(DataList<T> list) {
            size = list.size;
            array = list.GetArrayPointer();
        }

        [DebuggerStepThrough]
        public ref T Get(ElementId idx) {
            TypedUnsafe.CheckRange(idx.index, size);
            return ref array[idx.index];
        }

        [DebuggerStepThrough]
        public void Set(int idx, in T value) {
            TypedUnsafe.CheckRange(idx, size);
            array[idx] = value;
        }

        public T this[int idx] {
            [DebuggerStepThrough]
            get {
                TypedUnsafe.CheckRange(idx, size);
                return array[idx];
            }
            [DebuggerStepThrough]
            set {
                TypedUnsafe.CheckRange(idx, size);
                array[idx] = value;
            }
        }

        public T this[uint idx] {
            [DebuggerStepThrough]
            get {
                TypedUnsafe.CheckRange(idx, size);
                return array[idx];
            }
            [DebuggerStepThrough]
            set {
                TypedUnsafe.CheckRange(idx, size);
                array[idx] = value;
            }
        }

        [DebuggerStepThrough]
        public T[] ToArray() {
            T[] dst = new T[size];
            for (int i = 0; i < size; i++) {
                dst[i] = array[i];
            }

            return dst;
        }

        [DebuggerStepThrough]
        public T* GetArrayPointer() {
            return array;
        }

        [DebuggerStepThrough]
        public CheckedArray<T> Slice(int start, int count) {
            TypedUnsafe.CheckRange(start, size);
            TypedUnsafe.CheckRange(start + count - 1, size);
            return new CheckedArray<T>(array + start, count);
        }

        [DebuggerStepThrough]
        public CheckedArray<T> Slice(RangeInt range) {
            return Slice(range.start, range.length);
        }

        [DebuggerStepThrough]
        public T* GetPointer(int offset) {
            TypedUnsafe.CheckRange(offset, size);
            return array + offset;
        }

        public void MemClear() {
            TypedUnsafe.MemClear(array, size);
        }

    }

}