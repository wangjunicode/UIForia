using System;
using System.Runtime.CompilerServices;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Util.Unsafe {


// if base list is resized this will break!
    public unsafe struct UnsafeSpan<T> where T : unmanaged {

        [NativeDisableUnsafePtrRestriction] public readonly T* array;
        public readonly int size;

        public UnsafeSpan(int rangeStart, int rangeEnd, T* array) {
            this.size = rangeEnd - rangeStart;
            this.array = array + rangeStart;
        }

    }

    public unsafe struct BufferList<T> : IDisposable, IUnmanagedList<T> where T : unmanaged {

        public int size;
        [NativeDisableUnsafePtrRestriction] public T* array;
        public int capacity;

        public Allocator allocator { get; internal set; }

        private const int k_MinCapacity = 4;

        public BufferList(int initialCapacity, Allocator allocator) {
            this.allocator = allocator;
            this.size = 0;
            this.capacity = 0;
            this.array = default;

            AssertSize();

            if (initialCapacity > 0) {
                this.capacity = CeilPow2(initialCapacity > k_MinCapacity ? initialCapacity : k_MinCapacity);
                this.array = (T*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * capacity, UnsafeUtility.AlignOf<T>(), allocator);
            }

        }

        public BufferList(Allocator allocator) {
            this.allocator = allocator;
            this.size = 0;
            this.capacity = 0;
            this.array = default;
            AssertSize();
        }

        public T this[int index] {
            get => array[index];
            set => array[index] = value;
        }

        public void Add(in T item) {
            if (size + 1 >= capacity) {
                EnsureCapacity(size + 1);
            }

            array[size++] = item;
        }

        public void AddRange(T* items, int itemCount) {
            EnsureAdditionalCapacity(itemCount);
            TypedUnsafe.MemCpy(array + size, items, itemCount);
            size += itemCount;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void AddUnchecked(in T item) {
            array[size++] = item;
        }

        public void SwapRemoveAt(int index) {
            array[index] = array[size - 1];
            array[size--] = default;
        }

        public void Clear() {
            if (array != default) {
                UnsafeUtility.MemClear(array, (long) capacity * sizeof(T));
            }

            size = 0;
        }

        public void QuickClear() {
            if (array != default) {
                UnsafeUtility.MemClear(array, (long) size * sizeof(T));
            }

            size = 0;
        }

        public void EnsureCapacity(int desiredCapacity) {
            if (capacity <= desiredCapacity) {
                AssertSize();
                capacity = CeilPow2(desiredCapacity < 4 ? 4 : desiredCapacity);
                int bytesToMalloc = sizeof(T) * capacity;
                void* newPointer = UnsafeUtility.Malloc(bytesToMalloc, 4, allocator);

                if (array != default) {
                    int bytesToCopy = size * sizeof(T);
                    UnsafeUtility.MemCpy(newPointer, array, bytesToCopy);
                    UnsafeUtility.Free(array, allocator);
                }

                array = (T*) newPointer;
            }
        }

        public void SetCapacity(int desiredCapacity) {
            if (capacity <= desiredCapacity) {
                AssertSize();
                capacity = desiredCapacity < 4 ? 4 : desiredCapacity;
                int bytesToMalloc = sizeof(T) * capacity;
                void* newPointer = UnsafeUtility.Malloc(bytesToMalloc, 4, allocator);

                if (array != default) {
                    int bytesToCopy = size * sizeof(T);
                    UnsafeUtility.MemCpy(newPointer, array, bytesToCopy);
                    UnsafeUtility.Free(array, allocator);
                }

                array = (T*) newPointer;
            }
        }

        public void EnsureAdditionalCapacity(int additional) {
            int desiredCapacity = size + additional;
            if (capacity <= desiredCapacity) {
                AssertSize();
                capacity = CeilPow2(desiredCapacity < 4 ? 4 : desiredCapacity);
                int bytesToMalloc = sizeof(T) * capacity;
                void* newPointer = UnsafeUtility.Malloc(bytesToMalloc, UnsafeUtility.AlignOf<T>(), allocator);

                if (array != default) {
                    int bytesToCopy = size * sizeof(T);
                    UnsafeUtility.MemCpy(newPointer, array, bytesToCopy);
                    UnsafeUtility.Free(array, allocator);
                }

                array = (T*) newPointer;
            }
        }

        public void Dispose() {
            size = 0;
            capacity = 0;
            if (array != default) {
                UnsafeUtility.Free(array, allocator);
            }

        }

        private static int CeilPow2(int i) {
            i -= 1;
            i |= i >> 1;
            i |= i >> 2;
            i |= i >> 4;
            i |= i >> 8;
            i |= i >> 16;
            return i + 1;
        }

        [BurstDiscard]
        private static void AssertSize() {
            if (BitUtil.CountSetBits((uint) sizeof(T)) != 1) {
                Debug.Log("Cannot use " + typeof(T) + " in " + nameof(BufferList<T>) + " because it is not power of 2 aligned (size was " + sizeof(T) + ")");
            }
        }

        public T* GetRawPointer() {
            return array;
        }

        public T* GetSlicePointer(int sliceSize) {
            EnsureAdditionalCapacity(sliceSize);
            T* value = array + size;
            size += sliceSize;
            return value;
        }

        public UnsafeSpan<T> GetSpan(int rangeStart, int rangeEnd) {
            return new UnsafeSpan<T>(rangeStart, rangeEnd, array);
        }

        public ref T GetChecked(int idx) {
            if (idx < 0 || idx >= size) {
                throw new IndexOutOfRangeException();
            }

            return ref array[idx];
        }

        public void CopyFrom(BufferList<T> other) {
            UnsafeUtility.MemCpy(array, other.array, sizeof(T) * other.size);
            size = other.size;
        }

        public void Sort() {
            throw new NotImplementedException();
        }

        public void AddRange(object ids) {
            throw new NotImplementedException();
        }

    }

}