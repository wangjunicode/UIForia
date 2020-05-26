using System;
using System.Diagnostics;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public struct DataListDebugView<T> where T : unmanaged {

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

        public DataListDebugView(in DataList<T>.Shared target) {
            this.size = target.size;
            this.capacity = target.capacity;
            this.data = new T[size];
            for (int i = 0; i < size; i++) {
                data[i] = target[i];
            }
        }

    }

    public unsafe struct DisposedDataListDebugView<T> where T : unmanaged, IDisposable {

        public int size;
        public int capacity;
        public T[] data;

        public DisposedDataListDebugView(in DisposedDataList<T> target) {
            this.size = target.size;
            this.capacity = target.capacity;
            this.data = new T[size];
            for (int i = 0; i < size; i++) {
                data[i] = target[i];
            }
        }

        public DisposedDataListDebugView(in DisposedDataList<T>.Shared target) {
            this.size = target.size;
            this.capacity = target.capacity;
            this.data = new T[size];
            for (int i = 0; i < size; i++) {
                data[i] = target[i];
            }
        }

    }

    public unsafe struct DataListState {

        public int size;
        private ushort capacityShiftBits;
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

    }

    [AssertSize(16)]
    [DebuggerTypeProxy(typeof(DisposedDataListDebugView<>))]
    public unsafe struct DisposedDataList<T> where T : unmanaged, IDisposable {

        private DataListState state;

        public DisposedDataList(int initialCapacity, Allocator allocator, NativeArrayOptions clearMemory = NativeArrayOptions.UninitializedMemory) {
            state = DataListState.Create<T>(initialCapacity, allocator, clearMemory);
        }

        public void Add(in T item) {
            state.Add(item);
        }

        public void AddRange(T* items, int itemCount) {
            state.AddRange(items, itemCount);
        }

        public void EnsureCapacity(int desiredCapacity) {
            state.EnsureCapacity<T>(desiredCapacity);
        }

        public void EnsureAdditionalCapacity(int additional) {
            state.EnsureAdditionalCapacity<T>(additional);
        }

        public ref T GetReference(int index) {
            return ref state.Get<T>(index);
        }

        public T* GetPointer(int index) {
            return state.GetPointer<T>(index);
        }

        public ref T this[int index] {
            get => ref state.Get<T>(index);
        }

        public int capacity {
            get => state.Capacity;
        }

        public int size {
            get => state.size;
        }

        public void Dispose() {
            for (int i = 0; i < state.size; i++) {
                state.Get<T>(i).Dispose();
            }

            state.Dispose();
            this = default;
        }

        public void SetSize(int count, NativeArrayOptions clearMemory = NativeArrayOptions.UninitializedMemory) {
            state.EnsureCapacity<T>(count, clearMemory);
            state.size = count;
        }

        [DebuggerTypeProxy(typeof(DisposedDataListDebugView<>))]
        public struct Shared : IDisposable {

            [NativeDisableUnsafePtrRestriction] public DataListState* state;

            public Shared(int initialCapacity, Allocator allocator, NativeArrayOptions clear = NativeArrayOptions.UninitializedMemory) {
                this.state = TypedUnsafe.Malloc<DataListState>(1, allocator);
                *this.state = DataListState.Create<T>(initialCapacity, allocator, clear);
            }

            public void Add(in T item) {
                state->Add(item);
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
                return ref state->Get<T>(index);
            }

            public T* GetPointer(int index) {
                return state->GetPointer<T>(index);
            }

            public ref T this[int index] {
                get => ref state->Get<T>(index);
            }

            public void Set(in T item, int index) {
                state->Set<T>(item, index);
            }

            public int capacity {
                get => state->Capacity;
            }

            public int size {
                get => state->size;
            }

            public void Dispose() {
                if (state != null) {
                    for (int i = 0; i < state->size; i++) {
                        state->Get<T>(i).Dispose();
                    }

                    state->Dispose();
                }

                this = default;
            }

            public void SetSize(int count, NativeArrayOptions clearMemory = NativeArrayOptions.UninitializedMemory) {
                state->EnsureCapacity<T>(count, clearMemory);
                state->size = count;
            }

        }

    }

    [AssertSize(16)]
    [DebuggerTypeProxy(typeof(DataListDebugView<>))]
    public unsafe struct DataList<T> : IDisposable where T : unmanaged {

        private DataListState state;

        public DataList(int initialCapacity, Allocator allocator, NativeArrayOptions clearMemory = NativeArrayOptions.UninitializedMemory) {
            state = DataListState.Create<T>(initialCapacity, allocator, clearMemory);
        }

        public void Add(in T item) {
            state.Add(item);
        }

        public void AddUnchecked(in T item) {
            state.AddUnchecked(item);
        }

        public void AddRange(T* items, int itemCount) {
            state.AddRange(items, itemCount);
        }

        public void EnsureCapacity(int desiredCapacity) {
            state.EnsureCapacity<T>(desiredCapacity);
        }

        public void EnsureAdditionalCapacity(int additional) {
            state.EnsureAdditionalCapacity<T>(additional);
        }

        public void Dispose() {
            state.Dispose();
            this = default;
        }

        public ref T GetReference(int index) {
            return ref state.Get<T>(index);
        }

        public T* GetPointer(int index) {
            return state.GetPointer<T>(index);
        }

        public ref T this[int index] {
            get => ref state.Get<T>(index);
        }

        public void Set(in T item, int index) {
            state.Set<T>(item, index);
        }

        public int size {
            get => state.size;
            set => state.size = value;
        }

        public int capacity {
            get => state.Capacity;
        }

        public void EnsureCapacityCleared(int count) {
            state.EnsureCapacity<T>(count, NativeArrayOptions.ClearMemory);
        }

        public void SetSize(int count, NativeArrayOptions clearMemory = NativeArrayOptions.UninitializedMemory) {
            state.EnsureCapacity<T>(count, clearMemory);
            state.size = count;
        }

        public void SwapRemove(int index) {
            state.SwapRemove<T>(index);
        }

        public T* GetArrayPointer() {
            return (T*) state.array;
        }

        [DebuggerTypeProxy(typeof(DataListDebugView<>))]
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
                state->Add(item);
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
                return ref state->Get<T>(index);
            }

            public T* GetPointer(int index) {
                return state->GetPointer<T>(index);
            }

            public int size {
                get => state->size;
                set => state->size = value;
            }

            public int capacity {
                get => state->Capacity;
            }

            public ref T this[int index] {
                get => ref state->Get<T>(index);
            }

            public void Set(in T item, int index) {
                state->Set<T>(item, index);
            }

            public void Dispose() {
                if (state != null) {
                    state->Dispose();
                }

                this = default;
            }

            public void SetSize(int count, NativeArrayOptions clearMemory = NativeArrayOptions.UninitializedMemory) {
                state->EnsureCapacity<T>(count, clearMemory);
                state->size = count;
            }

            public unsafe T* GetArrayPointer() {
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

        }

    }

}