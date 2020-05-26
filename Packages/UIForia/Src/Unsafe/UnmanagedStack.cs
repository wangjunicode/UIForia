using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Util.Unsafe {

    public unsafe struct UnmanagedStack<T> : IDisposable where T : unmanaged {

        public int size;
        [NativeDisableUnsafePtrRestriction] public T* array;
        public int capacity;
        public Allocator allocator { get; internal set; }
        private const int k_MinCapacity = 4;

        public UnmanagedStack(int initialCapacity, Allocator allocator) {
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

        public void PushUnchecked(in T item) {
            array[size++] = item;
        }
        
        public void Push(in T item) {
            EnsureAdditionalCapacity(1);
            array[size++] = item;
        }
        
        public T Pop() {
            T retn = array[--size];
            return retn;
        }
        
        public ref T PopRef() {
            return ref array[--size];
        }

        [BurstDiscard]
        private static void AssertSize() {
            if (!BitUtil.IsPowerOfTwo((uint) sizeof(T))) {
                Debug.Log("Cannot use " + typeof(T) + " in " + nameof(BufferList<T>) + " because it is not power of 2 aligned (size was " + sizeof(T) + ")");
            }
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

    }

}