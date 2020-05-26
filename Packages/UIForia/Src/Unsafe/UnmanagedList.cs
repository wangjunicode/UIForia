using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public interface IUnmanagedList<T> where T : unmanaged {

        void Add(in T item);

        unsafe void AddRange(T* items, int itemCount);

        void EnsureCapacity(int itemCount);

        void EnsureAdditionalCapacity(int additionalItemCount);

    }

    [DebuggerTypeProxy(typeof(UnmanagedListDebugView<>))]
    public unsafe struct UnmanagedList<T> : IDisposable, IUnmanagedList<T> where T : unmanaged {

        [NativeDisableUnsafePtrRestriction] private UnmanagedListState* state;

        public UnmanagedList(int initialSize, Allocator allocator) {
            state = UnmanagedListState.Create<T>(initialSize, allocator);
        }

        public UnmanagedList(Allocator allocator) {
            state = UnmanagedListState.Create<T>(0, allocator);
        }

        public int size {
            get => state->size;
            set => state->size = value;
        }

        public void Add(in T item) {
            state->Add<T>(item);
        }

        public void AddRange(T* items, int count) {
            state->AddRange<T>(items, count);
        }

        public T this[int idx] {
            get => UnsafeUtility.ReadArrayElement<T>(state->array, idx);
            set => UnsafeUtility.WriteArrayElement(state->array, idx, value);
        }

        public void Dispose() {
            if (state != null) {
                state->Dispose();
            }

            this = default;
        }

        public void EnsureCapacity(int capacity) {
            state->EnsureCapacity<T>(capacity);
        }

        public void EnsureAdditionalCapacity(int additional) {
            state->EnsureAdditionalCapacity<T>(additional);
        }

        public T* GetBuffer() {
            return (T*) state->array;
        }

        public T * GetPointer(int index) {
            T* typedStateArray = (T*) state->array;
            return typedStateArray + index;
        }

        public ref T GetReference(int index) {
            return ref state->GetReference<T>(index);
        }

        public UnmanagedListState* GetStatePointer() {
            return state;
        }

    }

    public unsafe struct UnmanagedListState {

        public void* array;
        public int size;
        public int capacity;
        public Allocator allocator;

        public T Get<T>(int idx) where T : unmanaged {
            return UnsafeUtility.ReadArrayElement<T>(array, idx);
        }

        public void Add<T>(in T item) where T : unmanaged {
            if (size + 1 >= capacity) {
                EnsureCapacity<T>(size + 1);
            }

            T* typedArray = (T*) array;
            typedArray[size++] = item;
        }

        public void AddRange<T>(T* items, int itemCount) where T : unmanaged {
            int desiredCapacity = size + itemCount;

            if (capacity <= desiredCapacity) {
                GrowToUnchecked<T>(size + itemCount);
            }

            T* typedArray = (T*) array;
            TypedUnsafe.MemCpy(typedArray + size, items, itemCount);
            size += itemCount;
        }

        public void EnsureCapacity<T>(int desiredCapacity) where T : unmanaged {
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

        public void SetCapacity<T>(int desiredCapacity) where T : unmanaged {
            if (capacity <= desiredCapacity) {
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

        public void EnsureAdditionalCapacity<T>(int additional) where T : unmanaged {
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

        public ref T GetReference<T>(int index) where T : unmanaged {
            T* typedArray = (T*) array;
            return ref typedArray[index];
        }

        private void GrowToUnchecked<T>(int desiredCapacity) where T : unmanaged {
            desiredCapacity = CeilPow2(desiredCapacity < 4 ? 4 : desiredCapacity);

            T* newptr = (T*) array;

            TypedUnsafe.Resize(ref newptr, capacity, desiredCapacity, allocator);
            capacity = desiredCapacity;
            array = newptr;

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

        public static UnmanagedListState* Create<T>(int initialSize, Allocator allocator) where T : unmanaged {
            UnmanagedListState* state = TypedUnsafe.Malloc<UnmanagedListState>(1, allocator);
            state->allocator = allocator;
            state->array = null;
            state->capacity = 0;
            state->size = 0;

            if (initialSize > 0) {
                state->capacity = CeilPow2(initialSize);
                state->array = TypedUnsafe.MallocDefault<T>(state->capacity, allocator);
            }

            return state;

        }

        public void Dispose() {
            if (array != null) {
                UnsafeUtility.Free(array, allocator);
            }

            this = default;
        }

    }

}