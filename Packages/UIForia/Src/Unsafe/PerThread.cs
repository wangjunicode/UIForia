using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public interface IPerThreadCompatible : IDisposable {

        void InitializeForThread(Allocator allocator);

        bool IsInitialized { get; }

    }

    public interface IPerThread<T> : IDisposable where T : unmanaged, IPerThreadCompatible {

        ref T GetForThread(int threadIndex);

        int GetUsedThreadCount();

        unsafe int GatherUsedThreadData(T* gatheredOutput);

    }

    public unsafe struct PerThreadAdapter<T> : IPerThread<T> where T : unmanaged, IPerThreadCompatible {

        private T* target;

        public PerThreadAdapter(T* target) {
            this.target = target;
        }

        public ref T GetForThread(int threadIndex) {
            return ref *target;
        }

        public int GetUsedThreadCount() {
            return 1;
        }

        public int GatherUsedThreadData(T* gatheredOutput) {
            *gatheredOutput = *target;
            return 1;
        }

        public void Dispose() { }

    }

    public unsafe struct PerThread<T> : IPerThread<T>, IDisposable where T : unmanaged, IPerThreadCompatible {

        public readonly Allocator allocator;

        private readonly int itemCount;
        [NativeDisableUnsafePtrRestriction] private T* perThreadData;

        private PerThread(int count, Allocator allocator) {
            this.allocator = allocator;
            this.itemCount = count;
            // want to save the data always but re-alloc with allocator inside this
            this.perThreadData = TypedUnsafe.MallocDefault<T>(count, Allocator.Persistent);
        }

        private PerThread(T* perThreadData, int count, Allocator allocator) {
            this.allocator = allocator;
            this.itemCount = count;
            this.perThreadData = perThreadData;
        }

        public PerThread(Allocator allocator) : this(JobsUtility.MaxJobThreadCount, allocator) { }

        public ref T GetForThread(int threadIndex) {

            threadIndex = threadIndex >= itemCount ? itemCount - 1 : threadIndex;

            ref T val = ref perThreadData[threadIndex];

            if (!val.IsInitialized) {
                val.InitializeForThread(allocator);
            }

            return ref val;

        }

        public void Dispose() {
            if (perThreadData == null) return;
            for (int i = 0; i < itemCount; i++) {
                if (perThreadData[i].IsInitialized) {
                    perThreadData[i].Dispose();
                }
            }

            perThreadData->Dispose();
            TypedUnsafe.Dispose(perThreadData, Allocator.Persistent);
        }

        public void Clear() {
            if (perThreadData == null) return;
            for (int i = 0; i < itemCount; i++) {
                if (perThreadData[i].IsInitialized) {
                    perThreadData[i].Dispose();
                }
            }

            TypedUnsafe.MemClear(perThreadData, itemCount);
        }

        public int GetUsedThreadCount() {
            if (perThreadData == null) return 0;
            int count = 0;
            for (int i = 0; i < itemCount; i++) {
                if (perThreadData[i].IsInitialized) {
                    count++;
                }
            }

            return count;
        }

        public int GatherUsedThreadData(T* gatheredOutput) {
            if (perThreadData == null) return 0;
            int idx = 0;
            for (int i = 0; i < itemCount; i++) {
                if (perThreadData[i].IsInitialized) {
                    gatheredOutput[idx++] = perThreadData[i];
                }
            }

            return idx;
        }

        public static PerThread<T> Single(Allocator allocator) {
            return new PerThread<T>(1, allocator);
        }

        public static PerThread<T> Single(T* ptr, Allocator allocator) {
            return new PerThread<T>(ptr, 1, allocator);
        }

        public static PerThread<T> Single(ref T ptr, Allocator allocator) {
            fixed (T* data = &ptr) {
                return new PerThread<T>(data, 1, allocator);
            }
        }

        public static PerThread<T> Adapter(T* ptr, int itemCount, Allocator allocator) {
            return new PerThread<T>(ptr, itemCount, allocator);
        }

    }

}