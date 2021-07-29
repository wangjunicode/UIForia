using System;
using System.Runtime.InteropServices;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    internal unsafe struct TempBumpAllocatorPool : IDisposable {

        private BumpAllocatorPool allocatorPool;

        public TempBumpAllocatorPool(int basePageSizeInBytes) {
            this.allocatorPool = new BumpAllocatorPool(basePageSizeInBytes);
        }

        public BumpAllocator* Get() {
            BumpAllocator* allocator = allocatorPool.Get();
            allocator->Clear();
            return allocator;
        }

        public DisposableBumpAllocator GetTempAllocator(out BumpAllocator* allocator) {
            allocator = Get();
            return new DisposableBumpAllocator((BumpAllocatorPool*) UnsafeUtility.AddressOf(ref this), allocator);
        }

        public void Release(BumpAllocator* allocator) {
            allocatorPool.Release(allocator);
        }

        public void Dispose() {
            allocatorPool.Dispose();
            this = default;
        }

        public void Consolidate() {
            allocatorPool.Consolidate();
        }

    }

    /// <summary>
    /// Should only be used as a pointer!!!
    /// </summary>
    [StructLayout(LayoutKind.Explicit)]
    internal unsafe struct LockedBumpAllocator : IDisposable {

        [FieldOffset(0)] private int lockRef;
        [FieldOffset(0)] private fixed byte padding[64]; // avoid false sharing
        [FieldOffset(4)] private BumpAllocator allocator;

        public LockedBumpAllocator(int bytesPerPage, Allocator allocatorLabel) {
            allocator = new BumpAllocator(bytesPerPage, allocatorLabel);
            lockRef = 0;
        }

        public T* Allocate<T>(in T item) where T : unmanaged {
            SpinLock.Lock(ref lockRef);
            T* retn = allocator.Allocate(item);
            SpinLock.Unlock(ref lockRef);
            return retn;
        }

        public T* Allocate<T>(int count) where T : unmanaged {
            SpinLock.Lock(ref lockRef);
            T* retn = allocator.Allocate<T>(count);
            SpinLock.Unlock(ref lockRef);
            return retn;
        }

        public T* Allocate<T>(T* items, int count) where T : unmanaged {
            SpinLock.Lock(ref lockRef);
            T* retn = allocator.Allocate(items, count);
            SpinLock.Unlock(ref lockRef);
            return retn;
        }

        public T* AllocateCleared<T>(int count) where T : unmanaged {
            SpinLock.Lock(ref lockRef);
            T* retn = allocator.AllocateCleared<T>(count);
            SpinLock.Unlock(ref lockRef);
            return retn;
        }

        public BumpList<T> AllocateList<T>(int count) where T : unmanaged {
            SpinLock.Lock(ref lockRef);
            BumpList<T> retn = allocator.AllocateList<T>(count);
            SpinLock.Unlock(ref lockRef);
            return retn;
        }

        public BumpList<T> AllocateListCleared<T>(int count) where T : unmanaged {
            SpinLock.Lock(ref lockRef);
            BumpList<T> retn = allocator.AllocateListCleared<T>(count);
            SpinLock.Unlock(ref lockRef);
            return retn;
        }

        public void Clear() {
            // no lock since we're fucked anyway if we clear at the wrong time
            allocator.Clear();
        }

        public void ClearAndConsolidate() {
            // no lock since we're fucked anyway if we clear at the wrong time
            allocator.ClearAndConsolidate();
        }

        public void Dispose() {
            allocator.Dispose();
            this = default;
        }

        public CheckedArray<T> AllocateCheckedArray<T>(int count) where T : unmanaged {
            return new CheckedArray<T>(Allocate<T>(count), count);
        }
        
        public CheckedArray<T> AllocateCheckedArrayCleared<T>(int count) where T : unmanaged {
            return new CheckedArray<T>(AllocateCleared<T>(count), count);
        }
    }

    internal unsafe struct DisposableBumpAllocator : IDisposable {

        private BumpAllocatorPool* pool;
        private BumpAllocator* allocator;

        public DisposableBumpAllocator(BumpAllocatorPool* pool, BumpAllocator* allocator) {
            this.pool = pool;
            this.allocator = allocator;
        }

        public BumpAllocator* GetPtr() {
            return allocator;
        }

        public void Dispose() {
            pool->Release(allocator);
            this = default;
        }

    }

    internal unsafe struct BumpAllocatorPool : IDisposable {

        private int lockFlag;
        private int basePageSizeInBytes;
        private int allocatorSize;
        private int allocatorCapacity;
        [NativeDisableUnsafePtrRestriction] private AllocatorStatus* allocatorPool;

        public BumpAllocatorPool(int basePageSizeInBytes) {
            this.lockFlag = 0;
            this.allocatorPool = TypedUnsafe.Malloc<AllocatorStatus>(4, Allocator.Persistent);
            this.allocatorSize = 0;
            this.allocatorCapacity = 4;
            this.basePageSizeInBytes = basePageSizeInBytes;
        }

        public DisposableBumpAllocator GetTempAllocator(out BumpAllocator* allocator) {
            allocator = Get();
            return new DisposableBumpAllocator((BumpAllocatorPool*) UnsafeUtility.AddressOf(ref this), allocator);
        }

        public BumpAllocator* Get() {
            SpinLock.Lock(ref lockFlag);
            try {
                for (int i = 0; i < allocatorSize; i++) {
                    if (!allocatorPool[i].isActive) {
                        allocatorPool[i].isActive = true;
                        return allocatorPool[i].allocator;
                    }
                }

                TypedUnsafe.AddToList(ref allocatorPool, ref allocatorSize, ref allocatorCapacity, new AllocatorStatus() {
                    isActive = true,
                    allocator = TypedUnsafe.Malloc(new BumpAllocator(basePageSizeInBytes, Allocator.Persistent), Allocator.Persistent)
                });

                return allocatorPool[allocatorSize - 1].allocator;
            }
            finally {
                SpinLock.Unlock(ref lockFlag);
            }
        }

        public void Release(BumpAllocator* allocator) {
            SpinLock.Lock(ref lockFlag);

            try {
                for (int i = 0; i < allocatorSize; i++) {
                    BumpAllocator* ptr = allocatorPool[i].allocator;
                    if (ptr == allocator) {
                        allocator->Clear();
                        allocatorPool[i].isActive = false;
                        return;
                    }
                }

                throw new Exception("Allocator was not found in pool!");
            }
            finally {
                SpinLock.Unlock(ref lockFlag);
            }
        }

        public void Consolidate() {
            for (int i = 0; i < allocatorSize; i++) {
                if (allocatorPool[i].isActive) {
                    throw new Exception("Allocator was not released from pool!");
                }

                allocatorPool[i].allocator->ClearAndConsolidate();
            }
        }

        public void Dispose() {
            for (int i = 0; i < allocatorSize; i++) {
                allocatorPool[i].allocator->Dispose();
            }

            TypedUnsafe.Dispose(allocatorPool, Allocator.Persistent);
            this = default;
        }

        private struct AllocatorStatus {

            public bool isActive;
            public BumpAllocator* allocator;

        }

    }

}