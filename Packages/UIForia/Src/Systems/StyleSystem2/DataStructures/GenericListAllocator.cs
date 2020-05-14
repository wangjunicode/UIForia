using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    public unsafe struct GenericListAllocator {

        private ListHandle allocators;
        private DataListState overflowAllocations;
        private int maxAllocators;
        private Allocator allocator;

        public void Dispose() {
            for (int i = 0; i < overflowAllocations.size; i++) {
                TypedUnsafe.Dispose((byte*) overflowAllocations.Get<long>(i), allocator);
            }

            overflowAllocations.Dispose();
            FixedBlockAllocator* allocatorList = (FixedBlockAllocator*) allocators.array;
            for (int i = 0; i < allocators.size; i++) {
                allocatorList[i].Dispose();
            }

            this = default;
        }

        public void Free(void* ptr, int oldCapacity) {

            int oldAllocatorIdx = GetAllocatorIndex(maxAllocators, (uint) oldCapacity);

            if (oldAllocatorIdx >= maxAllocators) {

                byte** overflow = (byte**) overflowAllocations.array;

                for (int i = 0; i < overflowAllocations.size; i++) {
                    if (overflow[i] == ptr) {
                        TypedUnsafe.Dispose(ptr, allocator);
                        overflow[i] = overflow[--overflowAllocations.size];
                        break;
                    }
                }
            }
            else {
                FixedBlockAllocator* allocatorList = (FixedBlockAllocator*) allocators.array;
                allocatorList[oldAllocatorIdx].Free(ptr);
            }
        }

        public void EnsureCapacity<T>(ref TypedListHandle<T> handle, int count) where T : unmanaged {
            if (handle.size + count <= handle.capacity) {
                return;
            }

            int newAllocatorIndex = GetAllocatorIndex(maxAllocators, (uint) (handle.size + count));

            FixedBlockAllocator* allocatorList = (FixedBlockAllocator*) allocators.array;

            if (newAllocatorIndex < maxAllocators) {
                T* newListData = TypedUnsafe.Malloc<T>(BitUtil.EnsurePowerOfTwo(handle.size + count), allocator);
                TypedUnsafe.MemCpy(newListData, handle.array, handle.size);
                Free(handle.array, handle.capacity);
                overflowAllocations.Add((long) newListData);
                handle.SetArray(newListData, 1 << newAllocatorIndex);
            }
            else {
                T* newListData = allocatorList[newAllocatorIndex].Allocate<T>();
                TypedUnsafe.MemCpy(newListData, handle.array, handle.size);
                Free(handle.array, handle.capacity);
                handle.SetArray(newListData, 1 << newAllocatorIndex);
            }

        }

        private static int GetAllocatorIndex(int maxAllocatorCount, uint capacity) {

            if (capacity < 4) capacity = 4;

            if (!BitUtil.IsPowerOfTwo(capacity)) {
                capacity = BitUtil.NextPowerOfTwo(capacity);
            }

            int idx = BitUtil.GetPowerOfTwoBitIndex(capacity) - 1;

            if (idx >= maxAllocatorCount) {
                idx = maxAllocatorCount - 1;
            }

            return idx;

        }

    }

}