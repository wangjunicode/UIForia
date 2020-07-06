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

using UIForia.Text;

namespace UIForia.ListTypes {

    

    [DebuggerTypeProxy(typeof(DebugView_TextSymbol))]
    internal unsafe struct List_TextSymbol : IBasicList<TextSymbol> {

        public int size;
        private ushort capacityShiftBits;
        private AllocatorUShort allocator;

        [NativeDisableUnsafePtrRestriction] public TextSymbol* array;

        private const int k_MinCapacity = 4;

        public List_TextSymbol(int initialCapacity, Allocator allocator, bool clearMemory = false) {
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
            this.array = (TextSymbol*) UnsafeUtility.Malloc(sizeof(TextSymbol) * capacity, UnsafeUtility.AlignOf<TextSymbol>(), allocator);
            this.capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex((uint) capacity);
            if (clearMemory) {
                UnsafeUtility.MemClear(array, sizeof(TextSymbol) * capacity);
            }
        }

        public ref TextSymbol this[int index] {
            get => ref array[index];
        }

        public void SetSize(int size) {
            EnsureCapacity(size);
            this.size = size;
        }

        public void Add(in TextSymbol item) {
            if (size + 1 >= (1 << capacityShiftBits)) {
                EnsureCapacity(size + 1);
            }

            array[size++] = item;
        }

        public void AddUnchecked(in TextSymbol item) {
            array[size++] = item;
        }

        public void AddRange(TextSymbol* items, int itemCount) {
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

            long bytesToMalloc = sizeof(TextSymbol) * capacity;
            void* newPointer = UnsafeUtility.Malloc(bytesToMalloc, UnsafeUtility.AlignOf<TextSymbol>(), fullAllocator);

            if (clearMemory) {
                byte* bytePtr = (byte*) newPointer;
                UnsafeUtility.MemClear(bytePtr + (bytesToMalloc / 2), bytesToMalloc / 2);
            }

            if (array != default) {
                int bytesToCopy = size * sizeof(TextSymbol);
                UnsafeUtility.MemCpy(newPointer, array, bytesToCopy);
                UnsafeUtility.Free(array, fullAllocator);
            }

            capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex((uint) capacity);
            array = (TextSymbol*) newPointer;
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

        public ref TextSymbol Get(int index) {
            return ref array[index];
        }

        public void Set(in TextSymbol item, int index) {
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

        public TextSymbol* GetPointer(int index) {
            return array + index;
        }

        public void SwapRemove(int index) {
            array[index] = array[--size];
        }

        public TextSymbol GetLast() {
            return array[size - 1];
        }

        public void CopyFrom(TextSymbol* data, int count) {
            EnsureCapacity(count);
            size = count;
            TypedUnsafe.MemCpy(array, data, size);
        }

        public void CopyFrom(TextSymbol* data, int count, Allocator allocator) {
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

            [NativeDisableUnsafePtrRestriction] public List_TextSymbol* state;

            public Shared(int initialCapacity, Allocator allocator, bool clear = false) {
                this.state = TypedUnsafe.Malloc<List_TextSymbol>(1, allocator);
                *this.state = new List_TextSymbol(initialCapacity, allocator, clear);
            }

            public void Add(in TextSymbol item) {
                state->Add(item);
            }

            public void AddRange(TextSymbol* items, int itemCount) {
                state->AddRange(items, itemCount);
            }

            public void EnsureCapacity(int desiredCapacity) {
                state->EnsureCapacity(desiredCapacity);
            }

            public void EnsureAdditionalCapacity(int additional) {
                state->EnsureAdditionalCapacity(additional);
            }

            public ref TextSymbol GetReference(int index) {
                return ref state->Get(index);
            }

            public TextSymbol* GetPointer(int index) {
                return state->GetPointer(index);
            }

            public int size {
                get => state->size;
                set => state->size = value;
            }

            public int capacity {
                get => state->Capacity;
            }

            public ref TextSymbol this[int index] {
                get => ref state->Get(index);
            }

            public void Set(in TextSymbol item, int index) {
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

            public TextSymbol* GetArrayPointer() {
                return state->array;
            }

            public void AddUnchecked(in TextSymbol item) {
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

            public void FilterSwapRemove<TFilter>(TFilter filter) where TFilter : IListFilter<TextSymbol> {
                int itemCount = state->size;

                for (int i = 0; i < itemCount; i++) {
                    if (!filter.Filter(state->array[i])) {
                        state->array[i--] = state->array[--itemCount];
                    }
                }

                state->size = itemCount;

            }

        }

        public struct DebugView_TextSymbol {

            public int size;
            public int capacity;
            public TextSymbol[] data;

            public DebugView_TextSymbol(List_TextSymbol target) {
                this.size = target.size;
                this.capacity = target.Capacity;
                this.data = new TextSymbol[size];
                for (int i = 0; i < size; i++) {
                    data[i] = target[i];
                }
            }

            public DebugView_TextSymbol(List_TextSymbol.Shared target) {
                this.size = target.size;
                this.capacity = target.capacity;
                this.data = new TextSymbol[size];
                for (int i = 0; i < size; i++) {
                    data[i] = target[i];
                }
            }

        }

    }

}