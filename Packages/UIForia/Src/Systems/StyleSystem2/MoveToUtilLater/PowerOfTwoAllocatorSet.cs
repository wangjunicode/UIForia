using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Util.Unsafe {

    public struct AllocatorId {

        public int id;

        public static implicit operator AllocatorId(int capacity) {
            return new AllocatorId() {
                id = capacity
            };
        }

        public static implicit operator int(AllocatorId capacity) {
            return capacity.id;
        }

    }

    public struct ByteCapacity {

        public int byteCount;

        public static implicit operator ByteCapacity(int capacity) {
            return new ByteCapacity() {
                byteCount = capacity
            };
        }

        public static implicit operator int(ByteCapacity capacity) {
            return capacity.byteCount;
        }

    }

    public unsafe struct Pow2AllocatorSet : IDisposable {

        public Allocator allocator;
        public int minAllocationSizeShift;
        public int overflowThresholdShift;
        public FixedBlockAllocator* fixedAllocators;
        public ListHandle overflowAllocations;

        public static Pow2AllocatorSet CreateFromSizeRange(int minCapacity, int maxCapacity, int largestBlocksPerPage, bool trackMemory = false) {
            int count = 0;
            minCapacity = BitUtil.EnsurePowerOfTwo(minCapacity);
            maxCapacity = BitUtil.EnsurePowerOfTwo(maxCapacity);
            int current = minCapacity;

            while (current <= maxCapacity) {
                count++;
                current *= 2;
            }

            FixedAllocatorDesc* blocks = stackalloc FixedAllocatorDesc[count];

            int blockSize = minCapacity;
            int blocksPerPage = BitUtil.EnsurePowerOfTwo(largestBlocksPerPage);
            int initialPageCount = 1;

            count = 0;
            while (blockSize <= maxCapacity) {
                blocks[count++] = new FixedAllocatorDesc(blockSize, blocksPerPage, initialPageCount, trackMemory);
                blockSize *= 2;
                blocksPerPage /= 2;
                if (blocksPerPage < 4) blocksPerPage = 4;
                if (count < 2) initialPageCount = 1;
            }

            return new Pow2AllocatorSet(blocks, count);
        }

        public Pow2AllocatorSet(FixedAllocatorDesc* desc, int count, Allocator allocator = Allocator.Persistent) {

            ValidateAllocatorDescriptions(desc, count);

            int minAllocationSize = desc[0].blockSize;
            int overflowThreshold = desc[count - 1].blockSize;

            this.allocator = allocator;
            this.overflowThresholdShift = BitUtil.GetPowerOfTwoBitIndex((uint) BitUtil.EnsurePowerOfTwo(overflowThreshold));
            this.minAllocationSizeShift = BitUtil.GetPowerOfTwoBitIndex((uint) BitUtil.EnsurePowerOfTwo(minAllocationSize));
            this.overflowAllocations = default;
            this.fixedAllocators = TypedUnsafe.Malloc<FixedBlockAllocator>(count, allocator);

            for (int i = 0; i < count; i++) {
                fixedAllocators[i] = new FixedBlockAllocator(desc[i].blockSize, desc[i].blocksPerPage, desc[i].initialPageCount, desc[i].memoryTracking);
            }

        }

        public int fixedAllocatorCount {
            get => (overflowThresholdShift - minAllocationSizeShift) + 1;
        }

        public AllocatorId AllocateBlock(int requiredByteCount, out void* ptr) {
            int allocatorIndex = GetAllocatorIndex(requiredByteCount);
            if (allocatorIndex == -1) {
                if (overflowAllocations.array == null) {
                    this.overflowAllocations = new ListHandle(4, UnsafeUtility.Malloc(4 * sizeof(IntPtr), 4, allocator), 0);
                }
                ptr = UnsafeUtility.Malloc(requiredByteCount, 4, allocator);
                UntypedListUtil.Add(ref overflowAllocations, (IntPtr) ptr, allocator);
                return requiredByteCount;
            }

            ptr = fixedAllocators[allocatorIndex].Allocate();
            return allocatorIndex;
        }

        public ByteCapacity Allocate(int requiredByteCount, out void* ptr) {
            int allocatorIndex = GetAllocatorIndex(requiredByteCount);
            if (allocatorIndex == -1) {
                if (overflowAllocations.array == null) {
                    this.overflowAllocations = new ListHandle(4, UnsafeUtility.Malloc(4 * sizeof(IntPtr), 4, allocator), 0);
                }
                ptr = UnsafeUtility.Malloc(requiredByteCount, 4, allocator);
                UntypedListUtil.Add(ref overflowAllocations, (IntPtr) ptr, allocator);
                return requiredByteCount;
            }

            ptr = fixedAllocators[allocatorIndex].Allocate();
            return fixedAllocators[allocatorIndex].blockSize;
        }

        public void Free(void* ptr, int capacity) {
            int allocatorIndex = GetAllocatorIndex(capacity);

            if (allocatorIndex == -1) {
                if (overflowAllocations.array == null) return;
                IntPtr* list = (IntPtr*) overflowAllocations.array;
                IntPtr castPtr = (IntPtr) ptr;
                for (int i = 0; i < overflowAllocations.size; i++) {
                    if (list[i] == castPtr) {
                        UnsafeUtility.Free(ptr, allocator);
                        UntypedListUtil.SwapRemove<IntPtr>(ref overflowAllocations, i);
                        return;
                    }
                }

            }
            else {
                fixedAllocators[allocatorIndex].Free(ptr);
            }

        }

        public int GetAllocatorIndex(int requiredCapacity) {
            if (requiredCapacity > 1 << overflowThresholdShift) {
                return -1;
            }

            if (requiredCapacity < 1 << minAllocationSizeShift) {
                requiredCapacity = 1 << minAllocationSizeShift;
            }

            int pow = BitUtil.EnsurePowerOfTwo(requiredCapacity);
            return BitUtil.GetPowerOfTwoBitIndex((uint) pow) - minAllocationSizeShift;
        }

        public void Dispose() {
            for (int i = 0; i < overflowAllocations.size; i++) {
                UnsafeUtility.Free((void*) overflowAllocations.Get<IntPtr>(i), allocator);
            }

            for (int i = 0; i < fixedAllocatorCount; i++) {
                fixedAllocators[i].Dispose();
            }

            this = default;

        }

        [BurstDiscard]
        private static void ValidateAllocatorDescriptions(FixedAllocatorDesc* desc, int count) {

            // considered invalid if not sequential powers of two
            int lastBlockSize = 0;

            if (count < 2) {
                throw new Exception("Pow2AllocatorSet requires at least 2 fixed size allocators");
            }

            for (int i = 0; i < count; i++) {

                if (!BitUtil.IsPowerOfTwo((uint) desc[i].blockSize)) {
                    throw new Exception("Blocks sizes must be power of two");
                }

                if (i != 0 && desc[i].blockSize != lastBlockSize * 2) {
                    throw new Exception("Blocks sizes must be sequential powers of two");
                }

                lastBlockSize = desc[i].blockSize;

            }

        }

        /// <summary>
        /// Warning! This will NOT handle bounds checking and will return null for overflow allocator.
        /// </summary>
        /// <param name="allocatorIndex"></param>
        /// <returns>pointer to the block allocated or null if index was invalid</returns>
        public void* AllocateByIndex(int allocatorIndex) {
            if (allocatorIndex < 0) return null;
            return fixedAllocators[allocatorIndex].Allocate();
        }

        public void FreeByIndex(void* ptr, int allocatorIndex) {
            if (allocatorIndex == -1) {
                IntPtr* list = (IntPtr*) overflowAllocations.array;
                IntPtr castPtr = (IntPtr) ptr;
                for (int i = 0; i < overflowAllocations.size; i++) {
                    if (list[i] == castPtr) {
                        UnsafeUtility.Free(ptr, allocator);
                        UntypedListUtil.SwapRemove<IntPtr>(ref overflowAllocations, i);
                        return;
                    }
                }

            }
            else {
                fixedAllocators[allocatorIndex].Free(ptr);
            }
        }

    }

    public struct FixedAllocatorDesc {

        public int blockSize;
        public int blocksPerPage;
        public int initialPageCount;
        public bool memoryTracking;

        public FixedAllocatorDesc(int blockSize, int blocksPerPage, int initialPageCount, bool memoryTracking = false) {
            this.blockSize = blockSize;
            this.blocksPerPage = blocksPerPage;
            this.initialPageCount = initialPageCount;
            this.memoryTracking = memoryTracking;
        }

    }

    public unsafe struct FixedAllocatorDesc<T> where T : unmanaged {

        private FixedAllocatorDesc desc;

        public FixedAllocatorDesc(int blockSize, int blocksPerPage, int initialPageCount, bool memoryTracking = false) {
            desc.blockSize = sizeof(T) * blockSize;
            desc.blocksPerPage = blocksPerPage;
            desc.initialPageCount = initialPageCount;
            desc.memoryTracking = memoryTracking;
        }

        public static implicit operator FixedAllocatorDesc(FixedAllocatorDesc<T> convert) {
            return convert.desc;
        }

    }

}