
using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public unsafe struct UnsafeList<T> : IDisposable where T : unmanaged {

        public int size;
        public T* array;
        public readonly Allocator allocator;
        private int capacity;

        private const int k_MinCapacity = 4;
        
        public UnsafeList(int initialCapacity, Allocator allocator) {
            this.allocator = allocator;
            this.size = 0;
            this.array = default;
            this.capacity = 0;

            if (initialCapacity > 0) {
                this.capacity = CeilPow2(initialCapacity > k_MinCapacity ? initialCapacity : k_MinCapacity);
                this.array = (T*) UnsafeUtility.Malloc(UnsafeUtility.SizeOf<T>() * capacity, UnsafeUtility.AlignOf<T>(), allocator);
            }
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

        public void EnsureCapacity(int desiredCapacity) {
            if (capacity <= desiredCapacity) {
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