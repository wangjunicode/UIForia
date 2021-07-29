using System;
using System.Diagnostics;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using Debug = UnityEngine.Debug;

namespace UIForia.Unsafe {

    /// <summary>
    /// A simple pool allocator intended to be used to allocate lists of unmanaged types.
    /// Memory returned is NOT cleared, user must clear it if desired.
    /// Uses a set of fixed block allocators but doesn't commit memory into the block sizes
    /// until it actually needs to, which should reduce memory waste dramatically.
    ///
    /// This will not recycle blocks between sizes because I don't want to pay the price of coalescing
    /// neighboring blocks that are free. (a-la Buddy allocator)
    /// </summary>
    public unsafe struct ListAllocator : IDisposable {

        // min alloc size = 128 bytes
        public const int k_MinAllocSizeLog2 = 7; // min size we support (actualSize = 1 << k_MinAllocSizeLog2)
        public const int k_MaxAllocSizeLog2 = 16; // max size we support (actualSize = 1 << k_MaxAllocSizeLog2)
        public const int k_PageSizeMultiple = 16; // reserve enough space for k_PageSizeMultiple * the largest allocation size
        public const int k_AllocatorSize = 16; // sizeof(BlockAllocator) is not supported as a constant unfortunately

        private int pageCapacity; // how many pages we can possibly hold
        private int pageCount; // how many pages we actively hold
        private Page* pages; // list of pages

        private int overflowAllocCount;
        private int overflowAllocCapacity;
        private OverflowAllocation* overflowAllocations;

        // inline storage for our blocks, C# limitation means we have to declare this as a byte array and cast it when accessing.
        // these blocks re-use their own memory to inline a singly linked list of free blocks
        private fixed byte allocatorBytes[k_AllocatorSize * (k_MaxAllocSizeLog2 - k_MinAllocSizeLog2)];

        private BlockAllocator* blockAllocators {
            get {
                fixed (byte* ptr = allocatorBytes) {
                    return (BlockAllocator*) ptr;
                }
            }
        }

        public static ListAllocator CreateAllocator() {
            ListAllocator retn = new ListAllocator();
            const int cnt = k_MaxAllocSizeLog2 - k_MinAllocSizeLog2;
            for (int i = 0; i < cnt; i++) {
                retn.blockAllocators[i] = default;
            }

            retn.pages = (Page*) UnsafeUtility.Malloc(sizeof(Page) * 8, UnsafeUtility.AlignOf<Page>(), Allocator.Persistent);
            retn.pageCapacity = 8;
            retn.pageCount = 0;

            return retn;
        }

        public void Dispose() {

            if (pages != null) {
                for (int i = 0; i < pageCount; i++) {
                    UnsafeUtility.Free(pages[i].memory, Allocator.Persistent);
                }

                UnsafeUtility.Free(pages, Allocator.Persistent);
            }

            if (overflowAllocations != null) {
                for (int i = 0; i < overflowAllocCount; i++) {
                    UnsafeUtility.Free(overflowAllocations[i].memory, Allocator.Persistent);
                }

                UnsafeUtility.Free(overflowAllocations, Allocator.Persistent);
            }

            this = default;
        }

        public void AddToAllocList<T>(ref T* array, ref int currentCount, ref int capacity, T* values, int addedCount) where T : unmanaged {
            if (capacity == 0) {
                AllocatedList<T> alloc = Allocate<T>(addedCount);
                capacity = alloc.capacity;
                array = alloc.array;
                currentCount = addedCount;
                TypedUnsafe.MemCpy(array, values, addedCount);
            }
            else if (currentCount + addedCount > capacity) {
                AllocatedList<T> alloc = Allocate<T>(currentCount + addedCount);

                if (currentCount != 0) {
                    TypedUnsafe.MemCpy(alloc.array, array, currentCount);
                }

                TypedUnsafe.MemCpy(alloc.array + currentCount, values, addedCount);

                Free(array, capacity);
                capacity = alloc.capacity;
                array = alloc.array;
                currentCount += addedCount;
            }
            else {
                TypedUnsafe.MemCpy(array + currentCount, values, addedCount);
                currentCount += addedCount;
            }
        }

        public void AddToAllocList<T>(ref T* array, ref int count, ref int capacity, in T value) where T : unmanaged {
            if (capacity == 0) {
                AllocatedList<T> alloc = Allocate<T>(count + 1);
                capacity = alloc.capacity;
                array = alloc.array;
            }
            else if (count + 1 > capacity) {
                AllocatedList<T> alloc = Allocate<T>(count + 1);
                TypedUnsafe.MemCpy(alloc.array, array, count);
                Free(array, capacity);
                capacity = alloc.capacity;
                array = alloc.array;
            }

            array[count++] = value;
        }

        public void AddToAllocList<T>(ref T* array, ref ushort count, ref ushort capacity, in T value) where T : unmanaged {
            if (capacity == 0) {
                AllocatedList<T> alloc = Allocate<T>(count + 1);
                capacity = (ushort) alloc.capacity;
                array = alloc.array;
            }
            else if (count + 1 > capacity) {
                AllocatedList<T> alloc = Allocate<T>(count + 1);
                TypedUnsafe.MemCpy(alloc.array, array, count);
                Free(array, capacity);
                capacity = (ushort) alloc.capacity;
                array = alloc.array;
            }

            array[count++] = value;
        }

        /// <summary>
        /// Allocates a list that returns an array pointer and a capacity 
        /// </summary>
        /// <param name="count">How many elements must fit into this allocation</param>
        /// <typeparam name="T">Type of value we expect a list of</typeparam>
        /// <returns>A list struct that contains an array pointer and a max capacity for type T</returns>
        public AllocatedList<T> Allocate<T>(int count = 1) where T : unmanaged {
            if (count < 1) count = 1;
            // make sure we allocate at least our minimum byte count
            int byteCount = math.ceilpow2(math.max(1 << k_MinAllocSizeLog2, sizeof(T) * count));
            int byteCountLog2 = math.ceillog2(byteCount);
            int idx = byteCountLog2 - k_MinAllocSizeLog2; // offset the index by our min

            int listCapacity = byteCount / sizeof(T);

            // if index is too high, we overflow and fall back to a direct malloc, this will not be pooled
            if (idx > (k_MaxAllocSizeLog2 - k_MinAllocSizeLog2)) {
                count = (int) (count * 1.5); // over allocate a bit, since we aren't pooling
                AllocatedList<T> retn = new AllocatedList<T>() {
                    array = (T*) UnsafeUtility.Malloc(count * sizeof(T), UnsafeUtility.AlignOf<T>(), Allocator.Persistent),
                    capacity = count,
                };

                OverflowAllocation overflowAllocation = new OverflowAllocation() {
                    memory = (byte*) retn.array,
                    capacity = count * sizeof(T)
                };

                AddToList(overflowAllocation, ref overflowAllocCapacity, ref overflowAllocCount, ref overflowAllocations);

                return retn;
            }

            // grab the block allocator for this index. It might be uninitialized or might not have any memory blocks in its pool
            ref BlockAllocator allocator = ref blockAllocators[idx];

            // if we have a free memory block in this allocator
            // remove the head of the list, update the list pointer
            // bump our allocation stats 
            if (allocator.next != null) {
                BlockHeader* ptr = allocator.next;
                allocator.next = ptr->next;
                allocator.totalAllocations++;
                allocator.outstandingAllocations++;
                return new AllocatedList<T>() {
                    array = (T*) ptr,
                    capacity = listCapacity
                };
            }

            // if the allocator for the block size doesn't have any free blocks, we scan through our allocated pages
            // and find one that can support an additional allocation of our desired size
            for (int i = 0; i < pageCount; i++) {
                ref Page page = ref pages[i];
                if (page.allocated + byteCount <= page.capacity) {
                    byte* ptr = (page.memory + page.allocated);
                    page.allocated += byteCount;
                    return new AllocatedList<T>() {
                        array = (T*) ptr,
                        capacity = listCapacity
                    };
                }
            }

            // if no pages can support our allocation size, make a new one
            Page newPage = new Page();
            newPage.capacity = k_PageSizeMultiple * (1 << k_MaxAllocSizeLog2); // reserve enough space for k_PageSizeMultiple * the largest allocation size
            newPage.memory = (byte*) UnsafeUtility.Malloc(newPage.capacity, UnsafeUtility.AlignOf<long>(), Allocator.Persistent);
            newPage.allocated = byteCount;

            AddToList(newPage, ref pageCapacity, ref pageCount, ref pages);

            return new AllocatedList<T>() {
                array = (T*) newPage.memory,
                capacity = listCapacity
            };
        }

        private static void AddToList<T>(T item, ref int capacity, ref int count, ref T* list) where T : unmanaged {
            if (count + 1 > capacity) {
                int minCap = math.max(8, capacity * 2);
                T* newList = (T*) UnsafeUtility.Malloc(sizeof(T) * minCap, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);

                if (capacity > 0) {
                    UnsafeUtility.MemCpy(newList, list, sizeof(T) * capacity);
                    UnsafeUtility.Free(list, Allocator.Persistent);
                }

                capacity = minCap;
                list = newList;
            }

            list[count++] = item;
        }

        /// <summary>
        /// Free a list of type T. Offers zero double-free protection!!!
        /// </summary>
        /// <param name="array">list to free</param>
        /// <param name="listCapacity">list capacity, used to resolve the bucket</param>
        /// <typeparam name="T">Element type</typeparam>
        public void Free<T>(T* array, int listCapacity) where T : unmanaged {

            if (array == null) return;

            int idx = GetIndex(sizeof(T) * listCapacity);

            if (idx > (k_MaxAllocSizeLog2 - k_MinAllocSizeLog2)) {
                // overflow, just free it like we normally do
                // todo -- track a look-aside list of these overflows so we can dispose from the root on clean up
                for (int i = 0; i < overflowAllocCount; i++) {
                    if (overflowAllocations[i].memory == array) {
                        overflowAllocations[i] = overflowAllocations[--overflowAllocCount];
                        UnsafeUtility.Free(array, Allocator.Persistent);
                        return;
                    }
                }

                return;
            }

            // find the allocator and update the free list and stats accordingly
            ref BlockAllocator allocator = ref blockAllocators[idx];

            BlockHeader* blockHeader = (BlockHeader*) array;
            blockHeader->next = allocator.next;
            allocator.next = blockHeader;
            allocator.outstandingAllocations--;
        }

        public void Free<T>(AllocatedList<T> list) where T : unmanaged {
            Free(list.array, list.capacity);
        }

        /// <summary>
        /// Returns how many allocations fitting the bucket of `size` were made
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public int GetTotalAllocationsForBlock(int size) {
            int idx = GetIndex(size);

            if (idx > (k_MaxAllocSizeLog2 - k_MinAllocSizeLog2)) {
                return -1;
            }

            return blockAllocators[idx].totalAllocations;
        }

        /// <summary>
        /// Returns how many allocations fitting the bucket of `size` have not yet been freed
        /// </summary>
        public int GetOutstandingAllocationsForBlock(int size) {
            int idx = GetIndex(size);

            if (idx > (k_MaxAllocSizeLog2 - k_MinAllocSizeLog2)) {
                return -1;
            }

            return blockAllocators[idx].outstandingAllocations;
        }

        private static int GetIndex(int size) {
            int byteCount = math.ceilpow2(math.max(1 << k_MinAllocSizeLog2, size));
            return math.ceillog2(byteCount) - k_MinAllocSizeLog2;
        }

        /// <summary>
        /// Returns the bucket size for a theoretical allocation of `count`
        /// </summary>
        public int GetAllocationSize<T>(int count = 1) where T : unmanaged {
            if (count < 1) count = 1;
            int idx = GetIndex(sizeof(T) * count);

            if (idx > (k_MaxAllocSizeLog2 - k_MinAllocSizeLog2)) {
                return 0;
            }

            return 1 << (idx + k_MinAllocSizeLog2);
        }

        /// <summary>
        /// Gets the number of blocks for the bucket handling `size` that are free to be recycled
        /// </summary>
        /// <param name="size"></param>
        /// <returns></returns>
        public int GetFreeBlocksForSize(int size) {
            int idx = GetIndex(size);

            if (idx > (k_MaxAllocSizeLog2 - k_MinAllocSizeLog2)) {
                return 0;
            }

            BlockHeader* ptr = blockAllocators[idx].next;
            int retn = 0;
            while (ptr != null) {
                retn++;
                ptr = ptr->next;
            }

            return retn;
        }

        public PageInfo GetAllocatedPageInfo(int pageIdx) {
            if (pageIdx < 0 || pageIdx >= pageCapacity) {
                return default;
            }

            return new PageInfo() {
                capacity = pages[pageIdx].capacity,
                allocated = pages[pageIdx].allocated,
                remaining = pages[pageIdx].capacity - pages[pageIdx].allocated
            };
        }

        public int GetAllocatedPageCount() {
            return pageCount;
        }

        public int GetOverflowAllocCount() {
            return overflowAllocCount;
        }

        private struct BlockHeader {

            public BlockHeader* next;

        }

        // AssertSize(16);
        private struct BlockAllocator {

            public int totalAllocations;
            public int outstandingAllocations;
            public BlockHeader* next;

        }

        private struct Page {

            public byte* memory;
            public int capacity;
            public int allocated;

        }

        public struct PageInfo {

            public int capacity;
            public int allocated;
            public int remaining;

        }

        private struct OverflowAllocation {

            public byte* memory;
            public int capacity;

        }

        public int GetCapacity<T>(int count) where T : unmanaged {
            int byteCount = math.ceilpow2(math.max(1 << k_MinAllocSizeLog2, sizeof(T) * count));
            return byteCount / sizeof(T);
        }

    }

    /// <summary>
    /// A simple pool allocator intended to be used to allocate lists of unmanaged types.
    /// Memory returned is NOT cleared, user must clear it if desired.
    /// Uses a set of fixed block allocators but doesn't commit memory into the block sizes
    /// until it actually needs to, which should reduce memory waste dramatically.
    ///
    /// This will not recycle blocks between sizes because I don't want to pay the price of coalescing
    /// neighboring blocks that are free. (a-la Buddy allocator)
    /// </summary>
    public unsafe struct ListAllocatorSized : IDisposable {

        public int minAllocSize;
        public int maxAllocSize;

        // public const int k_PageSizeMultiple = 16; // reserve enough space for k_PageSizeMultiple * the largest allocation size

        public int pageSize;

        private int pageCapacity; // how many pages we can possibly hold
        private int pageCount; // how many pages we actively hold
        [NativeDisableUnsafePtrRestriction] private Page* pages; // list of pages

        private int overflowAllocCount;
        private int overflowAllocCapacity;
        [NativeDisableUnsafePtrRestriction] private OverflowAllocation* overflowAllocations;

        [NativeDisableUnsafePtrRestriction] private BlockAllocator* blockAllocators;

        public static ListAllocatorSized CreateAllocator(int minAllocSize, int maxAllocSize, int pageSize = 16384) {
            ListAllocatorSized retn = new ListAllocatorSized();
            retn.pageSize = pageSize;
            if (minAllocSize < 8) minAllocSize = 8; // needs to at least hold enough room for our pointer 
            minAllocSize = math.ceilpow2(minAllocSize);
            maxAllocSize = math.ceilpow2(maxAllocSize);
            retn.minAllocSize = minAllocSize;
            retn.maxAllocSize = maxAllocSize;

            int cnt = maxAllocSize - minAllocSize;

            retn.blockAllocators = TypedUnsafe.Malloc<BlockAllocator>(cnt, Allocator.Persistent);
            TypedUnsafe.MemClear(retn.blockAllocators, cnt);

            retn.pages = (Page*) UnsafeUtility.Malloc(sizeof(Page) * 8, UnsafeUtility.AlignOf<Page>(), Allocator.Persistent);
            retn.pageCapacity = 8;
            retn.pageCount = 0;

            return retn;
        }

        public void Clear() {
            
            if (pages != null) {
                for (int i = 0; i < pageCount; i++) {
                    pages[i].allocated = 0;
                }
            }
            if (overflowAllocations != null) {
                for (int i = 0; i < overflowAllocCount; i++) {
                    UnsafeUtility.Free(overflowAllocations[i].memory, Allocator.Persistent);
                }

                UnsafeUtility.Free(overflowAllocations, Allocator.Persistent);
                overflowAllocations = null;
                overflowAllocCapacity = 0;
                overflowAllocCount = 0;
            }
        }
        
        public void Dispose() {

            TypedUnsafe.Dispose(blockAllocators, Allocator.Persistent);

            if (pages != null) {
                for (int i = 0; i < pageCount; i++) {
                    UnsafeUtility.Free(pages[i].memory, Allocator.Persistent);
                }

                UnsafeUtility.Free(pages, Allocator.Persistent);
            }

            if (overflowAllocations != null) {
                for (int i = 0; i < overflowAllocCount; i++) {
                    UnsafeUtility.Free(overflowAllocations[i].memory, Allocator.Persistent);
                }

                UnsafeUtility.Free(overflowAllocations, Allocator.Persistent);
            }

            this = default;
        }

        public void AddToAllocList<T>(ref T* array, ref int currentCount, ref int capacity, T* values, int addedCount) where T : unmanaged {
            if (addedCount == 0) return;

            if (capacity == 0) {
                AllocatedList<T> alloc = Allocate<T>(addedCount);
                capacity = alloc.capacity;
                array = alloc.array;
                currentCount = addedCount;
                TypedUnsafe.MemCpy(array, values, addedCount);
            }
            else if (currentCount + addedCount > capacity) {
                AllocatedList<T> alloc = Allocate<T>(currentCount + addedCount);

                if (currentCount != 0) {
                    TypedUnsafe.MemCpy(alloc.array, array, currentCount);
                }

                TypedUnsafe.MemCpy(alloc.array + currentCount, values, addedCount);

                Free(array, capacity);
                capacity = alloc.capacity;
                array = alloc.array;
                currentCount += addedCount;
            }
            else {
                TypedUnsafe.MemCpy(array + currentCount, values, addedCount);
                currentCount += addedCount;
            }
        }

        public void AddToAllocList<T>(ref T* array, ref int count, ref int capacity, in T value) where T : unmanaged {
            if (capacity == 0) {
                AllocatedList<T> alloc = Allocate<T>(count + 1);
                capacity = alloc.capacity;
                array = alloc.array;
            }
            else if (count + 1 > capacity) {
                AllocatedList<T> alloc = Allocate<T>(count + 1);
                TypedUnsafe.MemCpy(alloc.array, array, count);
                Free(array, capacity);
                capacity = alloc.capacity;
                array = alloc.array;
            }

            array[count++] = value;
        }

        public void AddToAllocList<T>(ref T* array, ref ushort count, ref ushort capacity, in T value) where T : unmanaged {
            if (capacity == 0) {
                AllocatedList<T> alloc = Allocate<T>(count + 1);
                capacity = (ushort) alloc.capacity;
                array = alloc.array;
            }
            else if (count + 1 > capacity) {
                AllocatedList<T> alloc = Allocate<T>(count + 1);
                TypedUnsafe.MemCpy(alloc.array, array, count);
                Free(array, capacity);
                capacity = (ushort) alloc.capacity;
                array = alloc.array;
            }

            array[count++] = value;
        }

        /// <summary>
        /// Allocates a list that returns an array pointer and a capacity 
        /// </summary>
        /// <param name="count">How many elements must fit into this allocation</param>
        /// <param name="stretchOverflowFactor">When exceeding the size of this allocator, how much do we over allocate when falling back to malloc</param>
        /// <typeparam name="T">Type of value we expect a list of</typeparam>
        /// <returns>A list struct that contains an array pointer and a max capacity for type T</returns>
        public AllocatedList<T> Allocate<T>(int count = 1, float stretchOverflowFactor = 1.5f) where T : unmanaged {
            if (count < 1) count = 1;
            // make sure we allocate at least our minimum byte count
            int byteCount = math.max(minAllocSize, math.ceilpow2(sizeof(T) * count));

            int listCapacity = byteCount / sizeof(T);

            // if index is too high, we overflow and fall back to a direct malloc, this will not be pooled
            if (byteCount > maxAllocSize) {
                count = (int) (count * math.max(1, stretchOverflowFactor)); // over allocate a bit, since we aren't pooling
                AllocatedList<T> retn = new AllocatedList<T>() {
                    array = (T*) UnsafeUtility.Malloc(count * sizeof(T), UnsafeUtility.AlignOf<T>(), Allocator.Persistent),
                    capacity = count,
                };

                OverflowAllocation overflowAllocation = new OverflowAllocation() {
                    memory = (byte*) retn.array,
                    capacity = count * sizeof(T)
                };

                AddToList(overflowAllocation, ref overflowAllocCapacity, ref overflowAllocCount, ref overflowAllocations);

                return retn;
            }

            // int idx = math.tzcnt(byteCount) - math.tzcnt(minAllocSize);
            int idx = math.ceillog2(byteCount) - math.ceillog2(minAllocSize);

            // grab the block allocator for this index. It might be uninitialized or might not have any memory blocks in its pool
            ref BlockAllocator allocator = ref blockAllocators[idx];

            // if we have a free memory block in this allocator
            // remove the head of the list, update the list pointer
            if (allocator.next != null) {
                BlockHeader* ptr = allocator.next;
                allocator.next = ptr->next;
                return new AllocatedList<T>() {
                    array = (T*) ptr,
                    capacity = listCapacity
                };
            }

            // if the allocator for the block size doesn't have any free blocks, we scan through our allocated pages
            // and find one that can support an additional allocation of our desired size
            for (int i = 0; i < pageCount; i++) {
                ref Page page = ref pages[i];
                if (page.allocated + byteCount <= page.capacity) {
                    byte* ptr = (page.memory + page.allocated);
                    page.allocated += byteCount;
                    return new AllocatedList<T>() {
                        array = (T*) ptr,
                        capacity = listCapacity
                    };
                }
            }

            // if no pages can support our allocation size, make a new one
            Page newPage = new Page();
            newPage.capacity = math.max(pageSize, byteCount);
            newPage.memory = (byte*) UnsafeUtility.Malloc(newPage.capacity, UnsafeUtility.AlignOf<long>(), Allocator.Persistent);
            newPage.allocated = byteCount;

            AddToList(newPage, ref pageCapacity, ref pageCount, ref pages);

            return new AllocatedList<T>() {
                array = (T*) newPage.memory,
                capacity = listCapacity
            };
        }

        private static void AddToList<T>(T item, ref int capacity, ref int count, ref T* list) where T : unmanaged {
            if (count + 1 > capacity) {
                int minCap = math.max(8, capacity * 2);
                T* newList = (T*) UnsafeUtility.Malloc(sizeof(T) * minCap, UnsafeUtility.AlignOf<T>(), Allocator.Persistent);

                if (capacity > 0) {
                    UnsafeUtility.MemCpy(newList, list, sizeof(T) * capacity);
                    UnsafeUtility.Free(list, Allocator.Persistent);
                }

                capacity = minCap;
                list = newList;
            }

            list[count++] = item;
        }
        
        /// <summary>
        /// Free a list of type T. Offers zero double-free protection!!!
        /// </summary>
        /// <param name="array">list to free</param>
        /// <param name="listCapacity">list capacity, used to resolve the bucket</param>
        /// <typeparam name="T">Element type</typeparam>
        public void Free<T>(T* array, int listCapacity) where T : unmanaged {

            if (array == null) return;

            // round capacity up to nearest power of 2, min clamp at our minAllocSize
            int byteCount = math.max(minAllocSize, math.ceilpow2(sizeof(T) * listCapacity));

            if (byteCount > maxAllocSize) {
                // overflow, just free it like we normally do
                // todo -- track a look-aside list of these overflows so we can dispose from the root on clean up
                for (int i = 0; i < overflowAllocCount; i++) {
                    if (overflowAllocations[i].memory == array) {
                        overflowAllocations[i] = overflowAllocations[--overflowAllocCount];
                        UnsafeUtility.Free(array, Allocator.Persistent);
                        return;
                    }
                }

                return;
            }

            // the index of our block pool is the difference between bit index of byteCount (which is pow2) and minAllocSize(also pow2)
            // int idx = math.tzcnt(byteCount) - math.tzcnt(minAllocSize);
            int idx = math.ceillog2(byteCount) - math.ceillog2(minAllocSize);

            // find the allocator and update the free list and stats accordingly
            ref BlockAllocator allocator = ref blockAllocators[idx];

            BlockHeader* blockHeader = (BlockHeader*) array;
            blockHeader->next = allocator.next;
            allocator.next = blockHeader;
        }

        public void Free<T>(AllocatedList<T> list) where T : unmanaged {
            Free(list.array, list.capacity);
        }

        private int GetIndex(int size) {
            int byteCount = math.max(minAllocSize, math.ceilpow2(size));
            return math.ceillog2(byteCount - minAllocSize);
        }

        private struct BlockHeader {

            public BlockHeader* next;

        }

        private struct BlockAllocator {

            public BlockHeader* next;

        }

        private struct Page {

            public byte* memory;
            public int capacity;
            public int allocated;

        }

        private struct OverflowAllocation {

            public byte* memory;
            public int capacity;

        }

        public int GetPageCount() {
            return pageCount;
        }

        public void DumpPageStats() {
            for (int i = 0; i < pageCount; i++) {
                var page = pages[i];
                Debug.Log("Page " + i + " :: size: " + page.capacity + " allocated: " + page.allocated + " fill amount = " + ((float) page.allocated / (float) page.capacity));
            }
        }

        public void Reallocate<T>(ref T* buffer, ref int capacity, int newCapacity) where T : unmanaged {
            if (buffer == null) {
                AllocatedList<T> alloc = Allocate<T>(newCapacity);
                buffer = alloc.array;
                capacity = alloc.capacity;
            }
            else if (newCapacity > capacity) {
                Free(buffer, capacity);
                AllocatedList<T> alloc = Allocate<T>(newCapacity);
                buffer = alloc.array;
                capacity = alloc.capacity;
            }
        }

        public int GetCapacityFromSize<T>(int count) where T : unmanaged {
            int byteCount = math.max(minAllocSize, math.ceilpow2(sizeof(T) * count));
            int listCapacity = byteCount / sizeof(T);
            return listCapacity;
        }

      

    }

}