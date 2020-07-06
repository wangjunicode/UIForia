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

using UnityEngine;

namespace UIForia.ListTypes {

    

    [DebuggerTypeProxy(typeof(DebugView_Color32))]
    internal unsafe struct List_Color32 : IBasicList<Color32> {

        public int size;
        private ushort capacityShiftBits;
        private AllocatorUShort allocator;

        [NativeDisableUnsafePtrRestriction] public Color32* array;

        private const int k_MinCapacity = 4;

        public List_Color32(int initialCapacity, Allocator allocator, bool clearMemory = false) {
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
            this.array = (Color32*) UnsafeUtility.Malloc(sizeof(Color32) * capacity, UnsafeUtility.AlignOf<Color32>(), allocator);
            this.capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex((uint) capacity);
            if (clearMemory) {
                UnsafeUtility.MemClear(array, sizeof(Color32) * capacity);
            }
        }

        public ref Color32 this[int index] {
            get => ref array[index];
        }

        public void SetSize(int size) {
            EnsureCapacity(size);
            this.size = size;
        }

        public void Add(in Color32 item) {
            if (size + 1 >= (1 << capacityShiftBits)) {
                EnsureCapacity(size + 1);
            }

            array[size++] = item;
        }

        public void AddUnchecked(in Color32 item) {
            array[size++] = item;
        }

        public void AddRange(Color32* items, int itemCount) {
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

            long bytesToMalloc = sizeof(Color32) * capacity;
            void* newPointer = UnsafeUtility.Malloc(bytesToMalloc, UnsafeUtility.AlignOf<Color32>(), fullAllocator);

            if (clearMemory) {
                byte* bytePtr = (byte*) newPointer;
                UnsafeUtility.MemClear(bytePtr + (bytesToMalloc / 2), bytesToMalloc / 2);
            }

            if (array != default) {
                int bytesToCopy = size * sizeof(Color32);
                UnsafeUtility.MemCpy(newPointer, array, bytesToCopy);
                UnsafeUtility.Free(array, fullAllocator);
            }

            capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex((uint) capacity);
            array = (Color32*) newPointer;
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

        public ref Color32 Get(int index) {
            return ref array[index];
        }

        public void Set(in Color32 item, int index) {
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

        public Color32* GetPointer(int index) {
            return array + index;
        }

        public void SwapRemove(int index) {
            array[index] = array[--size];
        }

        public Color32 GetLast() {
            return array[size - 1];
        }

        public void CopyFrom(Color32* data, int count) {
            EnsureCapacity(count);
            size = count;
            TypedUnsafe.MemCpy(array, data, size);
        }

        public void CopyFrom(Color32* data, int count, Allocator allocator) {
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

            [NativeDisableUnsafePtrRestriction] public List_Color32* state;

            public Shared(int initialCapacity, Allocator allocator, bool clear = false) {
                this.state = TypedUnsafe.Malloc<List_Color32>(1, allocator);
                *this.state = new List_Color32(initialCapacity, allocator, clear);
            }

            public void Add(in Color32 item) {
                state->Add(item);
            }

            public void AddRange(Color32* items, int itemCount) {
                state->AddRange(items, itemCount);
            }

            public void EnsureCapacity(int desiredCapacity) {
                state->EnsureCapacity(desiredCapacity);
            }

            public void EnsureAdditionalCapacity(int additional) {
                state->EnsureAdditionalCapacity(additional);
            }

            public ref Color32 GetReference(int index) {
                return ref state->Get(index);
            }

            public Color32* GetPointer(int index) {
                return state->GetPointer(index);
            }

            public int size {
                get => state->size;
                set => state->size = value;
            }

            public int capacity {
                get => state->Capacity;
            }

            public ref Color32 this[int index] {
                get => ref state->Get(index);
            }

            public void Set(in Color32 item, int index) {
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

            public Color32* GetArrayPointer() {
                return state->array;
            }

            public void AddUnchecked(in Color32 item) {
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

            public void FilterSwapRemove<TFilter>(TFilter filter) where TFilter : IListFilter<Color32> {
                int itemCount = state->size;

                for (int i = 0; i < itemCount; i++) {
                    if (!filter.Filter(state->array[i])) {
                        state->array[i--] = state->array[--itemCount];
                    }
                }

                state->size = itemCount;

            }

        }

        public struct DebugView_Color32 {

            public int size;
            public int capacity;
            public Color32[] data;

            public DebugView_Color32(List_Color32 target) {
                this.size = target.size;
                this.capacity = target.Capacity;
                this.data = new Color32[size];
                for (int i = 0; i < size; i++) {
                    data[i] = target[i];
                }
            }

            public DebugView_Color32(List_Color32.Shared target) {
                this.size = target.size;
                this.capacity = target.capacity;
                this.data = new Color32[size];
                for (int i = 0; i < size; i++) {
                    data[i] = target[i];
                }
            }

        }

    }

}