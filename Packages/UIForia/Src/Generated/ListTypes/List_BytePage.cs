////////////////////////////////////////////////////////////////////////////////
// This file is auto-generated.
// Do not hand modify this file.
// It will be overwritten next time the generator is run.
////////////////////////////////////////////////////////////////////////////////

using System;
using System.Diagnostics;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

using UIForia.Util.Unsafe;

namespace UIForia.ListTypes {

    

    [DebuggerTypeProxy(typeof(DebugView_BytePage))]
    internal unsafe struct List_BytePage : IBasicList<BytePage> {

        public int size;
        private ushort capacityShiftBits;
        private AllocatorUShort allocator;

        [NativeDisableUnsafePtrRestriction] public BytePage* array;

        private const int k_MinCapacity = 4;

        public List_BytePage(int initialCapacity, Allocator allocator, bool clearMemory = false) {
            this.allocator = TypedUnsafe.CompressAllocatorToUShort(allocator);
            this.size = 0;
            this.capacityShiftBits = 0;
            this.array = default;

            if (initialCapacity > 0) {
                Initialize(initialCapacity, allocator, clearMemory);
            }

        }

        private void Initialize(int capacity, Allocator allocator, bool clearMemory) {
            this.allocator = TypedUnsafe.CompressAllocatorToUShort(allocator);
            capacity = BitUtil.EnsurePowerOfTwo(capacity > k_MinCapacity ? capacity : k_MinCapacity);
            this.array = (BytePage*) UnsafeUtility.Malloc(sizeof(BytePage) * capacity, UnsafeUtility.AlignOf<BytePage>(), allocator);
            this.capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex((uint) capacity);
            if (clearMemory) {
                UnsafeUtility.MemClear(array, sizeof(BytePage) * capacity);
            }
        }

        public ref BytePage this[int index] {
            get => ref array[index];
        }

        public void SetSize(int size) {
            EnsureCapacity(size);
            this.size = size;
        }

        public void Add(in BytePage item) {
            if (size + 1 >= (1 << capacityShiftBits)) {
                EnsureCapacity(size + 1);
            }

            array[size++] = item;
        }

        public void AddUnchecked(in BytePage item) {
            array[size++] = item;
        }

        public void AddRange(BytePage* items, int itemCount) {
            EnsureAdditionalCapacity(itemCount);
            TypedUnsafe.MemCpy(array + size, items, itemCount);
            size += itemCount;
        }

        public void EnsureCapacity(int desiredCapacity, Allocator allocator, bool clearMemory = false) {
            if (array == null) {
                Initialize(desiredCapacity, allocator, clearMemory);
            }
            else {
                EnsureCapacity(desiredCapacity, clearMemory);
            }
        }

        public void EnsureCapacity(int desiredCapacity, bool clearMemory = false) {

            int capacity = 1 << capacityShiftBits;
            if (capacity >= desiredCapacity) {
                return;
            }

            capacity = BitUtil.EnsurePowerOfTwo(desiredCapacity < k_MinCapacity ? k_MinCapacity : desiredCapacity);
            Allocator fullAllocator = TypedUnsafe.ConvertCompressedAllocator(allocator);

            long bytesToMalloc = sizeof(BytePage) * capacity;
            void* newPointer = UnsafeUtility.Malloc(bytesToMalloc, UnsafeUtility.AlignOf<BytePage>(), fullAllocator);

            if (clearMemory) {
                byte* bytePtr = (byte*) newPointer;
                UnsafeUtility.MemClear(bytePtr + (bytesToMalloc / 2), bytesToMalloc / 2);
            }

            if (array != default) {
                int bytesToCopy = size * sizeof(BytePage);
                UnsafeUtility.MemCpy(newPointer, array, bytesToCopy);
                UnsafeUtility.Free(array, fullAllocator);
            }

            capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex((uint) capacity);
            array = (BytePage*) newPointer;
        }

        public void EnsureAdditionalCapacity(int additional) {
            EnsureCapacity(size + additional);
        }

        public void SetSize(int count, bool clearMemory = false) {
            EnsureCapacity(count, clearMemory);
            size = count;
        }

        public void SetSize(int count, Allocator allocator, bool clearMemory = false) {
            if (array == null) {
                Initialize(count, allocator, clearMemory);
            }
            else {
                EnsureCapacity(count, clearMemory);
            }

            size = count;
        }

        public ref BytePage Get(int index) {
            return ref array[index];
        }

        public void Set(in BytePage item, int index) {
            array[index] = item;
        }

        public int Capacity {
            get => capacityShiftBits == 0 ? 0 : 1 << capacityShiftBits;
        }

        public void Dispose() {
            if (array != default) {
                UnsafeUtility.Free(array, TypedUnsafe.ConvertCompressedAllocator(allocator));
            }

            this = default;
        }

        public BytePage* GetPointer(int index) {
            return array + index;
        }

        public void SwapRemove(int index) {
            array[index] = array[--size];
        }

        public BytePage GetLast() {
            return array[size - 1];
        }

        public void CopyFrom(BytePage* data, int count) {
            EnsureCapacity(count);
            size = count;
            TypedUnsafe.MemCpy(array, data, size);
        }

        public void CopyFrom(BytePage* data, int count, Allocator allocator) {
            if (array == null) {
                Initialize(count, allocator, false);
            }
            else {
                EnsureCapacity(count);
            }

            size = count;
            TypedUnsafe.MemCpy(array, data, size);
        }

        [DebuggerTypeProxy(typeof(DataListDebugView<>))]
        public struct Shared : IDisposable {

            [NativeDisableUnsafePtrRestriction] public List_BytePage* state;

            public Shared(int initialCapacity, Allocator allocator, bool clear = false) {
                this.state = TypedUnsafe.Malloc<List_BytePage>(1, allocator);
                *this.state = new List_BytePage(initialCapacity, allocator, clear);
            }

            public void Add(in BytePage item) {
                state->Add(item);
            }

            public void AddRange(BytePage* items, int itemCount) {
                state->AddRange(items, itemCount);
            }

            public void EnsureCapacity(int desiredCapacity) {
                state->EnsureCapacity(desiredCapacity);
            }

            public void EnsureAdditionalCapacity(int additional) {
                state->EnsureAdditionalCapacity(additional);
            }

            public ref BytePage GetReference(int index) {
                return ref state->Get(index);
            }

            public BytePage* GetPointer(int index) {
                return state->GetPointer(index);
            }

            public int size {
                get => state->size;
                set => state->size = value;
            }

            public int capacity {
                get => state->Capacity;
            }

            public ref BytePage this[int index] {
                get => ref state->Get(index);
            }

            public void Set(in BytePage item, int index) {
                state->Set(item, index);
            }

            public void Dispose() {
                if (state != null) {
                    Allocator allocator = TypedUnsafe.ConvertCompressedAllocator(state->allocator);
                    state->Dispose();
                    TypedUnsafe.Dispose(state, allocator);
                }

                this = default;
            }

            public void SetSize(int count, bool clearMemory = false) {
                state->EnsureCapacity(count, clearMemory);
                state->size = count;
            }

            public BytePage* GetArrayPointer() {
                return state->array;
            }

            public void AddUnchecked(in BytePage item) {
                state->AddUnchecked(item);
            }

            public int Reserve(int count = 1) {
                int retn = state->size;
                state->size += count;
                return retn;
            }

            public void SwapRemove(int i) {
                state->array[i] = state->array[--state->size];
            }

            public void FilterSwapRemove<TFilter>(TFilter filter) where TFilter : IListFilter<BytePage> {
                int itemCount = state->size;

                for (int i = 0; i < itemCount; i++) {
                    if (!filter.Filter(state->array[i])) {
                        state->array[i--] = state->array[--itemCount];
                    }
                }

                state->size = itemCount;

            }

        }

        public struct DebugView_BytePage {

            public int size;
            public int capacity;
            public BytePage[] data;

            public DebugView_BytePage(List_BytePage target) {
                this.size = target.size;
                this.capacity = target.Capacity;
                this.data = new BytePage[size];
                for (int i = 0; i < size; i++) {
                    data[i] = target[i];
                }
            }

            public DebugView_BytePage(List_BytePage.Shared target) {
                this.size = target.size;
                this.capacity = target.capacity;
                this.data = new BytePage[size];
                for (int i = 0; i < size; i++) {
                    data[i] = target[i];
                }
            }

        }

    }

}