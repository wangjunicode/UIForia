using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    [AssertSize(16)]
    public unsafe struct PointerListState {

        // in order keep the size of this struct at 16 bits, store just the number of bits to shift to get our actual capacity.
        // this trick works because capacity is always a power of two. 

        public int size;
        private ushort capacityShiftBits;
        private readonly AllocatorUShort allocator;

        [NativeDisableUnsafePtrRestriction] public void** array;

        private const int k_MinCapacity = 4;

        public PointerListState(int initialCapacity, Allocator allocator) {
            this.allocator = TypedUnsafe.CompressAllocatorToUShort(allocator);
            this.size = 0;
            this.capacityShiftBits = 0;
            this.array = default;

            if (initialCapacity > 0) {
                int capacity = BitUtil.EnsurePowerOfTwo(initialCapacity > k_MinCapacity ? initialCapacity : k_MinCapacity);
                this.array = (void**) UnsafeUtility.Malloc(sizeof(void**) * capacity, UnsafeUtility.AlignOf<long>(), allocator);
                this.capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex((uint) capacity);
            }
        }

        public void Add(in void* item) {
            if (size + 1 >= (1 << capacityShiftBits)) {
                EnsureCapacity(size + 1);
            }

            array[size++] = item;
        }

        public void AddRange<T>(T** items, int itemCount) where T : unmanaged {
            EnsureAdditionalCapacity(itemCount);
            UnsafeUtility.MemCpy(array + size, items, sizeof(void*) * itemCount);
            size += itemCount;
        }

        public void EnsureCapacity(int desiredCapacity) {

            int capacity = 1 << capacityShiftBits;
            if (capacity > desiredCapacity) {
                return;
            }

            capacity = BitUtil.EnsurePowerOfTwo(desiredCapacity < 4 ? 4 : desiredCapacity);
            int bytesToMalloc = sizeof(void**) * capacity;
            Allocator fullAllocator = TypedUnsafe.ConvertCompressedAllocator(allocator);
            void** newPointer = (void**) UnsafeUtility.Malloc(bytesToMalloc, UnsafeUtility.AlignOf<long>(), fullAllocator);

            if (array != default) {
                int bytesToCopy = size * sizeof(void**);
                UnsafeUtility.MemCpy(newPointer, array, bytesToCopy);
                UnsafeUtility.Free(array, fullAllocator);
            }

            capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex(capacityShiftBits);
            array = newPointer;
        }

        public void EnsureAdditionalCapacity(int additional) {
            int desiredCapacity = size + additional;
            int capacity = 1 << capacityShiftBits;
            if (capacity > desiredCapacity) {
                return;
            }

            Allocator fullAllocator = TypedUnsafe.ConvertCompressedAllocator(allocator);

            capacity = BitUtil.EnsurePowerOfTwo(desiredCapacity < 4 ? 4 : desiredCapacity);
            int bytesToMalloc = sizeof(void**) * capacity;
            void* newPointer = UnsafeUtility.Malloc(bytesToMalloc, UnsafeUtility.AlignOf<long>(), fullAllocator);

            if (array != default) {
                int bytesToCopy = size * sizeof(void**);
                UnsafeUtility.MemCpy(newPointer, array, bytesToCopy);
                UnsafeUtility.Free(array, fullAllocator);
            }

            capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex(capacityShiftBits);
            array = (void**) newPointer;
        }

        public void Dispose() {
            if (array != default) {
                UnsafeUtility.Free(array, TypedUnsafe.ConvertCompressedAllocator(allocator));
            }

            this = default;
        }
        

    }

    [AssertSize(16)]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct PointerList<T> : IDisposable where T : unmanaged {

        [AssertSize(8)]
        public struct Shared {

            private PointerListState* state;

            public Shared(PointerListState* state) {
                this.state = state;
            }

            public void Add(T* value) {
                state->Add(value);
            }

            public void AddRange(T** items, int itemCount) {
                state->AddRange(items, itemCount);
            }

            public void EnsureCapacity(int desiredCapacity) {
                state->EnsureCapacity(desiredCapacity);
            }

            public void EnsureAdditionalCapacity(int additional) {
                state->EnsureCapacity(additional);
            }

            public void Dispose() {
                if (state != null) {
                    state->Dispose();
                }

                this = default;
            }

            public T* this[int index] {
                // no need to type the array since always a pointer and all pointers are the same size
                get => (T*) state->array[index];
                set => state->array[index] = value;
            }

            public int size {
                get => state->size;
            }

        }

        private PointerListState state;

        public PointerList(int initialCapacity, Allocator allocator) {
            this.state = new PointerListState(initialCapacity, allocator);
        }

        public void Add(in T* item) {
            state.Add(item);
        }

        public void AddRange(T** items, int itemCount) {
            state.AddRange(items, itemCount);
        }

        public void EnsureCapacity(int desiredCapacity) {
            state.EnsureCapacity(desiredCapacity);
        }

        public void EnsureAdditionalCapacity(int additional) {
            state.EnsureAdditionalCapacity(additional);
        }

        public void Dispose() {
            state.Dispose();
            this = default;
        }

        public T* this[int index] {
            // no need to type the array since always a pointer
            get => (T*) state.array[index];
            set => state.array[index] = value;
        }

        public int size {
            get => state.size;
            set => state.size = value;
        }

    }

}