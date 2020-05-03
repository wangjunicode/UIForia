using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia.Util.Unsafe {

    public unsafe struct PagedAllocatorHandle {

        public int blockCount;
        public void* ptr;

        public PagedAllocatorHandle(void* ptr, int blockCount) {
            this.ptr = ptr;
            this.blockCount = blockCount;
        }

    }

    public unsafe struct PagedBlockAllocator {

        private PagedListState* listState;
        
        private struct BlockAllocator {

            private PagedListState* state;

            private Block* nextBlock;

            private int count;
            private int allocListCapacity;

            private ushort blockSize;
            private ushort blocksPerPage;

            private void** allocations;

            public PagedAllocatorHandle Allocate() {
                if (nextBlock == null) {
                    // full (or empty, same effect)
                    AddAllocationPage();
                }

                void* ptr = nextBlock;
                nextBlock = nextBlock->next;
                return new PagedAllocatorHandle(ptr, blockSize);
            }

            private void ResizeAllocations() { }

            private void AddAllocationPage() {
                if (allocations == null) {
                    ResizeAllocations();
                }

                allocListCapacity = 4;
                allocations = (void**) UnsafeUtility.Malloc(sizeof(void*) * allocListCapacity, 4, Allocator.Persistent);
                UnsafeUtility.MemClear(allocations, sizeof(void*) * allocListCapacity);
                allocations[0] = UnsafeUtility.Malloc(blockSize * blocksPerPage, 4, Allocator.Persistent);
                // split block into chunks of appropriate size, setup pointers
                Block* p = (Block*) allocations[0];
                nextBlock = p;
                int stepSize = blockSize * sizeof(void*);
                for (int i = 0; i < blocksPerPage - 1; i++) {
                    p->next = p + stepSize;
                    p = p->next;
                }
            }

            public void Free(PagedAllocatorHandle handle) {
                Block* freed = (Block*) handle.ptr;
                freed->next = nextBlock;
                nextBlock = freed;
            }

            // public static unsafe BlockAllocator* Create(int blockCount) { }

            public unsafe bool TryAllocate(out void* ptr) {
                if (nextBlock != null) {
                    ptr = nextBlock;
                    nextBlock = nextBlock->next;
                }

                ptr = null;
                return false;
            }

            public unsafe void Borrow(void* ptr, int i) {
                
            }

        }

        private BlockAllocator* subAllocators;

        public void Free(PagedAllocatorHandle handle) {
           UnsafeUtility.Free(handle.ptr, Allocator.Persistent);   
        }

        public PagedAllocatorHandle Allocate(int size) {
            void* memory = UnsafeUtility.Malloc(size, 4, Allocator.Persistent);
            int blockCount = size >> 3; // divide by 8
            UnmanagedPagedList<long> list = new UnmanagedPagedList<long>(listState);
            list.Add((long) memory);
            return new PagedAllocatorHandle(memory, blockCount);
            // size = BitUtil.NextMultipleOf8(math.min(8, size) - 1);
            //
            // // block count will be between 1 and 16
            //
            // if (subAllocators[blockCount].TryAllocate(out void* ptr)) {
            //     return new PagedAllocatorHandle(ptr, blockCount);
            // }
            //
            // // 4 8 16 24
            // for (int i = blockCount + 1; i < 16; i++) {
            //     if (subAllocators[i].TryAllocate(out ptr)) {
            //         subAllocators[blockCount].Borrow(ptr, i);
            //         return subAllocators[blockCount].Allocate();
            //     }
            // }
            //
            // // allocate new page
            // // give it to largest allocator
            // // will be naturally canabilized as needed.
            //
            // return subAllocators[blockCount].Allocate();

        }

    }

    public unsafe struct Block {

        public Block* next;

    }

}