using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    public unsafe struct HeapAllocated<T> : IDisposable where T : unmanaged {

        [NativeDisableUnsafePtrRestriction] public T* ptr;

        public HeapAllocated(T value) {
            ptr = (T*) UnsafeUtility.Malloc(sizeof(T), UnsafeUtility.AlignOf<T>(), Allocator.Persistent);
            *ptr = value;
        }

        public void Dispose() {
            if (ptr != null) {
                UnsafeUtility.Free(ptr, Allocator.Persistent);
            }
        }

        public static implicit operator T*(HeapAllocated<T> allocated) {
            return allocated.ptr;
        }

        public void Set(T value) {
            if (ptr != null) {
                *ptr = value;
            }
        }

    }

}