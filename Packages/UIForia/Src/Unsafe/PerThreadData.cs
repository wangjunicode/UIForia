using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public unsafe struct PerThreadData<T> : IDisposable where T : unmanaged {

        public void** perThreadData;
        private readonly Allocator allocator;

        public PerThreadData(Allocator allocator) {
            this.allocator = allocator;
            int size = sizeof(void**) * JobsUtility.MaxJobThreadCount;
            this.perThreadData = (void**) UnsafeUtility.Malloc(size, UnsafeUtility.AlignOf<T>(), allocator);
            UnsafeUtility.MemClear(perThreadData, size);
        }

        public bool TryGetPointerForThread(int threadIndex, out T* pointer) {
            if (perThreadData[threadIndex] == null) {
                pointer = null;
                return false;
            }

            pointer = (T*) perThreadData[threadIndex];
            return true;
        }

        public T* GetPointerForThread(int threadIndex) {
            return (T*) perThreadData[threadIndex];
        }

        public void SetPointerForThread(T* pointer, int threadIndex) {
            perThreadData[threadIndex] = (long*) pointer;
        }

        public void Dispose() {
            if (perThreadData == null) return;
            UnsafeUtility.Free(perThreadData, allocator);
            perThreadData = null;
        }

    }

}