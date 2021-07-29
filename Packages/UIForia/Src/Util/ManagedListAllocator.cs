using System;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Util {

    internal class ManagedListAllocator<T>  {

        private string name;

        private int allocPtr;
        private int minAllocSize;
        private int maxAllocSize;

        public T[] memory;
        private int[] blockAllocators;

        public ManagedListAllocator(string name, int minAllocSize, int maxAllocSize, float baseAllocationMultiple = 16) {
            minAllocSize = math.ceilpow2(minAllocSize);
            maxAllocSize = math.ceilpow2(maxAllocSize);
            int minAllocSizeLog2 = math.ceillog2(minAllocSize);
            int maxAllocSizeLog2 = math.ceillog2(maxAllocSize);

            this.name = name;
            this.minAllocSize = minAllocSize;
            this.maxAllocSize = maxAllocSize;
            this.allocPtr = 0;
            this.blockAllocators = new int[maxAllocSizeLog2 - minAllocSizeLog2];
            if (baseAllocationMultiple < 0) {
                baseAllocationMultiple = 1;
            }
            this.memory = new T[(int)(maxAllocSize * baseAllocationMultiple)];
            for (int i = 0; i < blockAllocators.Length; i++) {
                blockAllocators[i] = -1;
            }
        }

        public int MaxAllocation => allocPtr;

        public SmallListSlice AllocateSlice(int count) {
            RangeInt range = Allocate(count);
            return new SmallListSlice() {
                length = 0,
                start = range.start,
                capacity = (ushort)range.length
            };
        }

        public RangeInt Allocate(int count) {
            if (count < minAllocSize) count = minAllocSize;
            if (count > maxAllocSize) {
                throw new Exception($"Cannot allocate list of size greater than {maxAllocSize} using allocator: {name}. Tried to allocate {count} items.");
            }

            count = math.ceilpow2(count);
            int minLog2 = math.ceillog2(minAllocSize);
            int countLog2 = math.ceillog2(count);
            int allocatorIndex = countLog2 - minLog2;

            int next = blockAllocators[allocatorIndex];
            
            if (next != -1) {
                ref int ptr = ref UnsafeUtility.As<T, int>(ref memory[next]);
                blockAllocators[allocatorIndex] = ptr;
                return new RangeInt(next, count);
            }

            if (allocPtr + count > memory.Length) {
                Array.Resize(ref memory, memory.Length * 2);
            }

            int start = allocPtr;
            allocPtr += count;

            return new RangeInt(start, count);
        }

        public void Clear() {
            allocPtr = 0;
            for (int i = 0; i < blockAllocators.Length; i++) {
                blockAllocators[i] = -1;
            }
        }

        public void Free(RangeInt rangeInt) {
            Free(rangeInt.start, (ushort) rangeInt.length);
        }

        public void Free(int start, ushort capacity) {
            
            if (capacity == 0) return;
            
            if (capacity > maxAllocSize || capacity < minAllocSize) {
                throw new Exception($"Failed to free allocation of size {capacity} because it is outside the range of this allocator: {name}");
            }

            int allocatorIndex = math.ceillog2((int) capacity) - math.ceillog2(minAllocSize);

            ref int blockAllocator = ref blockAllocators[allocatorIndex];

            ref int ptr = ref UnsafeUtility.As<T, int>(ref memory[start]);
            ptr = blockAllocator;
            blockAllocator = start;
        }

        public void Add(ref SmallListSlice slice, in T item) {
            if (slice.length + 1 <= slice.capacity) {
                memory[slice.start + slice.length] = item;
                slice.length++;
            }
            else {
                RangeInt range = Allocate(slice.length + 1);
                for (int i = slice.start; i < slice.start + slice.length; i++) {
                    memory[range.start + i] = memory[i];
                }

                memory[range.start + slice.length] = item;
                
                Free((ushort)slice.start, (ushort)slice.capacity);
                
                slice.start = range.start;
                slice.capacity = (ushort) range.length;
                slice.length++;
            }
        }

    }

}