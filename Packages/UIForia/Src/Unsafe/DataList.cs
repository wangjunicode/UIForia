using System;
using System.Diagnostics;
using UIForia.Layout;
using UIForia.Text;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Util.Unsafe {

    public unsafe class DataListDebugView<T> where T : unmanaged {

        public int size;
        public int capacity;
        public T[] data;

        public DataListDebugView(DataList<T> target) {
            this.size = target.size;
            this.capacity = target.capacity;
            this.data = new T[size];
            for (int i = 0; i < size; i++) {
                data[i] = target[i];
            }
        }

        public DataListDebugView(DataList<T>.Shared target) {
            if (target.state == null) {
                return;
            }

            data = target.ToArray();
            size = target.size;
            capacity = target.capacity;
        }

    }

    public unsafe class DataListDebugViewShared<T> where T : unmanaged {

        public int size;
        public int capacity;
        public T[] data;

        public DataListDebugViewShared(DataList<T>.Shared target) {
            if (target.state == null) {
                return;
            }

            data = target.ToArray();
            size = target.size;
            capacity = target.capacity;
        }

    }

    public unsafe struct DataListState {

        public int size;
        internal ushort capacityShiftBits;
        private AllocatorUShort allocator;

        [NativeDisableUnsafePtrRestriction] public void* array;

        private const int k_MinCapacity = 4;

        public static DataListState Create<T>(int initialCapacity, Allocator allocator, NativeArrayOptions clearMemory) where T : unmanaged {
            DataListState retn = new DataListState {
                allocator = TypedUnsafe.CompressAllocatorToUShort(allocator),
                size = 0,
                capacityShiftBits = 0,
                array = default
            };

            if (initialCapacity > 0) {
                int capacity = BitUtil.EnsurePowerOfTwo(initialCapacity > k_MinCapacity ? initialCapacity : k_MinCapacity);
                retn.array = UnsafeUtility.Malloc(sizeof(T) * capacity, UnsafeUtility.AlignOf<T>(), allocator);
                retn.capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex((uint) capacity);
                if (clearMemory == NativeArrayOptions.ClearMemory) {
                    UnsafeUtility.MemClear(retn.array, sizeof(T) * capacity);
                }
            }

            return retn;
        }

        public Allocator GetAllocator() {
            return TypedUnsafe.ConvertCompressedAllocator(allocator);
        }

        public void Add<T>(in T item) where T : unmanaged {
            if (size + 1 >= (1 << capacityShiftBits)) {
                EnsureCapacity<T>(size + 1);
            }

            ((T*) array)[size++] = item;
        }

        public void AddUnchecked<T>(in T item) where T : unmanaged {
            ((T*) array)[size++] = item;
        }

        public void AddRange<T>(T* items, int itemCount) where T : unmanaged {
            EnsureAdditionalCapacity<T>(itemCount);
            TypedUnsafe.MemCpy(((T*) (array)) + size, items, itemCount);
            size += itemCount;
        }

        public void EnsureCapacity<T>(int desiredCapacity, NativeArrayOptions cleared = NativeArrayOptions.UninitializedMemory) where T : unmanaged {

            desiredCapacity = math.max(desiredCapacity, 8);
            
            int capacity = 1 << capacityShiftBits;
            if (capacity >= desiredCapacity) {
                return;
            }

            capacity = BitUtil.EnsurePowerOfTwo(desiredCapacity < 4 ? 4 : desiredCapacity);
            Allocator fullAllocator = TypedUnsafe.ConvertCompressedAllocator(allocator);

            long bytesToMalloc = sizeof(T) * capacity;
            void* newPointer = UnsafeUtility.Malloc(bytesToMalloc, UnsafeUtility.AlignOf<T>(), fullAllocator);

            if (cleared == NativeArrayOptions.ClearMemory) {
                byte* bytePtr = (byte*) newPointer;
                UnsafeUtility.MemClear(bytePtr + bytesToMalloc / 2, bytesToMalloc / 2);
            }

            if (array != default) {
                int bytesToCopy = size * sizeof(T);
                UnsafeUtility.MemCpy(newPointer, array, bytesToCopy);
                UnsafeUtility.Free(array, fullAllocator);
            }

            capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex((uint) capacity);
            array = newPointer;
        }

        public void EnsureAdditionalCapacity<T>(int additional) where T : unmanaged {
            EnsureCapacity<T>(size + additional);
        }

        public ref T Get<T>(int index) where T : unmanaged {
            return ref ((T*) array)[index];
        }

        public void Set<T>(in T item, int index) where T : unmanaged {
            ((T*) (array))[index] = item;
        }

        public int Capacity {
            get => 1 << capacityShiftBits;
        }

        public void Dispose() {
            if (array != default) {
                UnsafeUtility.Free(array, TypedUnsafe.ConvertCompressedAllocator(allocator));
            }

            this = default;
        }

        public T* GetPointer<T>(int index) where T : unmanaged {
            return ((T*) (array)) + index;
        }

        public void SwapRemove<T>(int index) where T : unmanaged {
            T* typedArray = (T*) array;
            typedArray[index] = typedArray[--size];
        }

        public T GetLast<T>() where T : unmanaged {
            return ((T*) array)[size - 1];
        }

        public void Clear<T>() where T : unmanaged {
            if (array != null) {
                UnsafeUtility.MemClear(array, Capacity * sizeof(T));
            }

            size = 0;
        }

    }

    [AssertSize(16)]
    [DebuggerTypeProxy(typeof(DataListDebugView<>))]
    public unsafe struct DataList<T> : IEquatable<DataList<T>>, IDisposable where T : unmanaged {

        public int size;
        internal ushort capacityShiftBits;
        private AllocatorUShort allocator;
        [NativeDisableUnsafePtrRestriction] public T* array;
        private const int k_MinCapacity = 4;

        public DataList(int initialCapacity, Allocator allocator, NativeArrayOptions clearMemory = NativeArrayOptions.UninitializedMemory) {
            this.allocator = TypedUnsafe.CompressAllocatorToUShort(allocator);
            this.size = 0;
            this.capacityShiftBits = 0;
            this.array = default;
            if (initialCapacity > 0) {
                int cap = BitUtil.EnsurePowerOfTwo(initialCapacity > k_MinCapacity ? initialCapacity : k_MinCapacity);
                array = (T*)UnsafeUtility.Malloc(sizeof(T) * cap, UnsafeUtility.AlignOf<T>(), allocator);
                capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex((uint) cap);
                if (clearMemory == NativeArrayOptions.ClearMemory) {
                    UnsafeUtility.MemClear(array, sizeof(T) * cap);
                }
            }

        }

        // public T* array => (T*)state.array;
        
        // public int size {
        //     [DebuggerStepThrough] get => state.size;
        //     [DebuggerStepThrough] set => state.size = value;
        // }
        
        [DebuggerStepThrough]
        public void Add(in T item) {
            if (size + 1 >= (1 << capacityShiftBits)) {
                EnsureCapacity(size + 1);
            }

            array[size++] = item;
        }

        [DebuggerStepThrough]
        public void AddUnchecked(in T item) {
            array[size++] = item;
        }

        [DebuggerStepThrough]
        public void AddRange(T* items, int itemCount) {
            EnsureAdditionalCapacity(itemCount);
            TypedUnsafe.MemCpy(array + size, items, itemCount);
            size += itemCount;
        }

        [DebuggerStepThrough]
        public void EnsureCapacity(int desiredCapacity, NativeArrayOptions cleared = NativeArrayOptions.UninitializedMemory) {
            if (desiredCapacity == 0) return;
            int currentCapacity = 1 << capacityShiftBits;
            if (currentCapacity >= desiredCapacity) {
                return;
            }

            currentCapacity = BitUtil.EnsurePowerOfTwo(desiredCapacity < 4 ? 4 : desiredCapacity);
            Allocator fullAllocator = TypedUnsafe.ConvertCompressedAllocator(allocator);

            long bytesToMalloc = sizeof(T) * currentCapacity;
            void* newPointer = UnsafeUtility.Malloc(bytesToMalloc, UnsafeUtility.AlignOf<T>(), fullAllocator);

            if (cleared == NativeArrayOptions.ClearMemory) {
                byte* bytePtr = (byte*) newPointer;
                UnsafeUtility.MemClear(bytePtr + bytesToMalloc / 2, bytesToMalloc / 2);
            }

            if (array != default) {
                int bytesToCopy = size * sizeof(T);
                UnsafeUtility.MemCpy(newPointer, array, bytesToCopy);
                UnsafeUtility.Free(array, fullAllocator);
            }

            capacityShiftBits = (ushort) BitUtil.GetPowerOfTwoBitIndex((uint) currentCapacity);
            array = (T*) newPointer;
        }

        [DebuggerStepThrough]
        public void EnsureAdditionalCapacity(int additional) {
            EnsureCapacity(size + additional);
        }

        [DebuggerStepThrough]
        public void Dispose() {
            if (array != default) {
                UnsafeUtility.Free(array, TypedUnsafe.ConvertCompressedAllocator(allocator));
            }

            this = default;
        }

        [DebuggerStepThrough]
        public T* GetPointer(int index) {
            return array + index;
        }

        public ref T this[int index] {
            [DebuggerStepThrough]
            get {
                CheckRange(index);
                return ref array[index];
            }
        }

        public void Set(in T item, int index) {
            CheckRange(index);
             array[index] = item;
        }

        [DebuggerStepThrough]
        public ref T Get(int index) {
            CheckRange(index);
            return ref  array[index];
        }


        public int capacity {
            get => 1 << capacityShiftBits;
        }

        public void EnsureCapacityCleared(int count) {
            EnsureCapacity(count, NativeArrayOptions.ClearMemory);
        }

        public void SetSize(int count, NativeArrayOptions clearMemory = NativeArrayOptions.UninitializedMemory) {
            EnsureCapacity(count, clearMemory);
            size = count;
        }

        public void SwapRemove(int index) {
            array[index] = array[--size];
        }

        public T* GetArrayPointer() {
            return  array;
        }

        public void CopyFrom(DataList<T> dataList) {
            SetSize(dataList.size);
            TypedUnsafe.MemCpy(array, dataList.array, dataList.size);
        }

        public void CopyFrom(Shared dataList) {
            SetSize(dataList.size);
            TypedUnsafe.MemCpy(array, dataList.GetArrayPointer(), dataList.size);
        }

        public void CopyFrom(T* items, int count) {
            SetSize(count);
            TypedUnsafe.MemCpy(array, items, count);
        }

   
        public ref T GetLast() {
            return ref Get(size - 1);
        }

        public void Reverse() {
            int max = size / 2;
            for (int i = 0; i < max; i++) {
                T tmp = this[i];
                this[i] = this[size - i - 1];
                this[size - i - 1] = tmp;
            }
        }

        public void Clear() {
            if (array != null) {
                UnsafeUtility.MemClear(array, capacity * sizeof(T));
            }

            size = 0;
        }

        [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
        private void CheckRange(int index) {
            if ((uint) index >= size) {
                throw new IndexOutOfRangeException(string.Format("Index {0} is out of range of '{1}' size.", (object) index, (object) size));
            }
        }

        public T Pop() {
            int index = size - 1;
            CheckRange(index);
            T retn = array[index];
            size--;
            return retn;
        }

        public CheckedArray<T> Slice(RangeInt range) {
            return new CheckedArray<T>(array, range);
        }
        
        public CheckedArray<T> Slice(int start, int length) { 
            return new CheckedArray<T>(array + start, length);
        }

        public CheckedArray<T> ToCheckedArray() {
            return new CheckedArray<T>(array, size);
        }

        public bool Equals(DataList<T> b) {
            return size == b.size && UnsafeUtility.MemCmp(array, b.array, sizeof(T) * size) == 0;
        }
        
        [DebuggerTypeProxy(typeof(DataListDebugViewShared<>))]
        public struct Shared : IDisposable {

            [NativeDisableUnsafePtrRestriction] public DataListState* state;

            public Shared(DataListState* state) {
                this.state = state;
            }

          
            public Shared(int initialCapacity, Allocator allocator, NativeArrayOptions clear = NativeArrayOptions.UninitializedMemory) {
                this.state = TypedUnsafe.Malloc<DataListState>(1, allocator);
                *this.state = DataListState.Create<T>(initialCapacity, allocator, clear);
            }

            public void Add(in T item) {
                if (state->size + 1 >= (1 << state->capacityShiftBits)) {
                    state->EnsureCapacity<T>(state->size + 1);
                }

                ((T*) state->array)[state->size++] = item;
            }

            public void AddRange(T* items, int itemCount) {
                state->AddRange(items, itemCount);
            }

            public void EnsureCapacity(int desiredCapacity) {
                state->EnsureCapacity<T>(desiredCapacity);
            }

            public void EnsureAdditionalCapacity(int additional) {
                state->EnsureAdditionalCapacity<T>(additional);
            }

            public ref T GetReference(int index) {
                return ref ((T*) state->array)[index];
            }

            public T* GetPointer(int index) {
                return ((T*) (state->array)) + index;
            }

            public int size {
                [DebuggerStepThrough] get => state->size;
                [DebuggerStepThrough] set => state->size = value;
            }

            public int capacity {
                get => state->Capacity;
            }

            public ref T this[int index] {
                [DebuggerStepThrough]
                get {
                    CheckRange(index);
                    return ref ((T*) state->array)[index];
                }
            }

            public void Set(in T item, int index) {
                state->Set<T>(item, index);
            }

            public void Dispose() {
                if (state != null) {
                    Allocator alloc = state->GetAllocator();
                    state->Dispose();
                    TypedUnsafe.Dispose(state, alloc);
                }

                this = default;
            }

            public void SetSize(int count, NativeArrayOptions clearMemory = NativeArrayOptions.UninitializedMemory) {
                state->EnsureCapacity<T>(count, clearMemory);
                state->size = count;
            }

            public T* GetArrayPointer() {
                return (T*) state->array;
            }

            public void AddUnchecked(in T item) {
                state->AddUnchecked(item);
            }

            public int Reserve(int count = 1) {
                int retn = state->size;
                state->size += count;
                return retn;
            }

            public void SwapRemove(int i) {
                T* array = (T*) state->array;
                array[i] = array[--state->size];
            }

            public void FilterSwapRemove<TFilter>(TFilter filter) where TFilter : IListFilter<T> {
                T* array = (T*) state->array;
                int itemCount = state->size;

                for (int i = 0; i < itemCount; i++) {
                    if (!filter.Filter(array[i])) {
                        array[i--] = array[--itemCount];
                    }
                }

                state->size = itemCount;

            }

            public void CopyFrom(DataList<T> dataList) {
                SetSize(dataList.size);
                TypedUnsafe.MemCpy((T*) state->array, dataList.GetArrayPointer(), dataList.size);
            }

            public void CopyFrom(T* item, int count) {
                SetSize(count);
                TypedUnsafe.MemCpy((T*) state->array, item, count);
            }

            public void ShiftRight(int startIndex, int count) {
                EnsureAdditionalCapacity(count);
                byte* array = (byte*) state->array;
                UnsafeUtility.MemMove(array + ((startIndex + count) * sizeof(T)), array + (startIndex * sizeof(T)), count * sizeof(T));
            }

            public void Reverse() {
                int max = size / 2;
                for (int i = 0; i < max; i++) {
                    T tmp = this[i];
                    this[i] = this[size - i - 1];
                    this[size - i - 1] = tmp;
                }
            }

            public void Clear() {
                state->Clear<T>();
            }

            public T[] ToArray() {
                T[] retn = new T[size];
                fixed (T* ptr = retn) {
                    TypedUnsafe.MemCpy(ptr, (T*) state->array, size);
                }

                return retn;
            }

            [Conditional("ENABLE_UNITY_COLLECTIONS_CHECKS")]
            private void CheckRange(int index) {
                if ((uint) index >= size) {
                    throw new IndexOutOfRangeException(string.Format("Index {0} is out of range of '{1}' size.", (object) index, (object) size));
                }
            }

            public ref T GetLast() {
                return ref state->Get<T>(state->size - 1);
            }

            public ref T Get(int idx) {
                return ref state->Get<T>(idx);
            }

        }

        public void RemoveAt(int index) {
            if (index == size - 1) {
                array[--size] = default;
                return;
            }

            --size;
            
            for (int i = index; i < size; i++) {
                array[i] = array[i + 1];
            }
            
            array[size] = default;

        }

        public void EnsureCapacity(int desiredCapacity, Allocator allocator) {
            if (array == null) {
                this = new DataList<T>(desiredCapacity, allocator);
            }
            else {
                EnsureCapacity(desiredCapacity);
            }
        }

    }

}