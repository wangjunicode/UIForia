using System;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace UIForia.Util.Unsafe {

    /// <summary>
    /// Memory allocator that gets pages of memory from Unity and acts as a pool over that.
    /// Uses a fixed block size scheme where it will always allocate blocks of bytes of the same size.
    ///
    /// Also support some memory verification. If enabled will detect memory leaks and disallow invalid
    /// pointers from being freed.
    ///
    /// Tracks free blocks in a linked list re-using the memory from the free blocks. 
    /// </summary>
    [AssertSize(32)]
    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FixedBlockAllocator : IDisposable {

        internal ushort blockSize;
        internal ushort blocksPerPage;
        internal ushort pageListSize;
        internal ushort pageListCapacity;
        
        internal void** listPointer;
        internal Block* freeList;
        internal UntypedLongMap * blockMap;

        public FixedBlockAllocator(int blockSize, int blocksPerPage, int initialPageCount, bool trackMemory = false) {

            this.blockSize = (ushort)blockSize;
            this.blocksPerPage = (ushort)blocksPerPage;
            PointerListUtil.Initialize((ushort)initialPageCount, out pageListSize, out pageListCapacity, out listPointer, Allocator.Persistent);
            this.freeList = null;
            this.blockMap = trackMemory
                ? UntypedLongMap.Create<long>(blocksPerPage * initialPageCount, 0.75f, Allocator.Persistent)
                : default;

            if (initialPageCount > 0) {
                AllocatePage();
            }
            
        }

        public T* Allocate<T>() where T : unmanaged {
            return (T*) (Allocate());
        }
        
        public void* Allocate() {
            if (freeList != null) {
                Block* retn = freeList;
                freeList = freeList->next;
                return retn;
            }
            else {
                AllocatePage();
                Block* retn = freeList;
                freeList = freeList->next;
                return retn;
            }
        }
     
        private void AllocatePage() {

            if (freeList != null) return;

            freeList = (Block*) UnsafeUtility.Malloc(blockSize * blocksPerPage, 4, Allocator.Persistent);

            // add to the list of pages we allocated. Resizes if needed
            PointerListUtil.AddPointerItem(ref pageListSize, ref pageListCapacity, ref listPointer, freeList, Allocator.Persistent);

            Block* ptr = freeList;

            byte* pageBase = (byte*) freeList;

            // setup the chain of next pointers in the page we just allocated
            
            // if the block map is null we are not using memory tracking
            // when not using memory tracking, skip the cost of adding to the map
            if (blockMap != null) {
                for (int i = 0; i < blocksPerPage - 1; i++) {
                    ptr->next = (Block*) (pageBase + ((i + 1) * blockSize));
                    ptr = ptr->next;
                    blockMap->Add((long) ptr, (long) freeList); // if we are tracking memory, we convert each pointer we allocate to long and make sure it's in the map with the base address that allocated it.
                }
            }
            else {
                // same as above but with fewer if checks for the hot path if we aren't using memory tracking
                for (int i = 0; i < blocksPerPage - 1; i++) {
                    ptr->next = (Block*) (pageBase + ((i + 1) * blockSize));
                    ptr = ptr->next;
                }
            }

            ptr->next = null; // last in chain points to unknown memory. this fixes that

            if (blockMap != null) {
                blockMap->Add((long) ptr, (long) freeList);
            }
        }

        public bool Free(void* ptr) {

            if (blockMap != null && !blockMap->TryGetValue((long) ptr, out long _)) {
                Debug.LogError("Freed a block in allocator that was not allocated by it");
                return false;
            }

            Block* b = (Block*) ptr;
            b->next = freeList;
            freeList = b;
            return true;
        }

        // walks the free list and ensures we touched the same number of nodes as we have pages / block size
        public void EnsureFullyReleased() {

            Block* ptr = freeList;

            int count = 0;
            while (ptr != null) {
                count++;
                ptr = ptr->next;
            }

            if (count != (pageListSize * blocksPerPage)) {
                Debug.LogError("Some blocks were not released from allocator");
            }

        }

        // frees the backing pages
        public void Dispose() {
            if (blockMap != null) {
                blockMap->Dispose();
            }
            PointerListUtil.Dispose(pageListSize, ref listPointer);
            this = default;

        }

    }

}