using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    public unsafe struct StyleResult {

        public int propertyCount;
        public int allocatorIndex;
        public byte* properties;

    }

    public unsafe struct StyleResultTable {

        public BufferList<int> updatesThisFrame;
        public UnmanagedList<StyleResult> table;
        public FixedBlockAllocator* allocatorList;

        public void Dispose() { }

        // system rule: pointers are valid only for 1 frame. as soon as the table is written to again pointers are invalidated

        private static int GetAllocatorIndex(uint propertyCount) {

            if (propertyCount < 4) propertyCount = 4;

            if (!BitUtil.IsPowerOfTwo(propertyCount)) {
                propertyCount = BitUtil.NextPowerOfTwo(propertyCount);
            }

            return BitUtil.GetPowerOfTwoBitIndex(propertyCount) - 2;

        }

        // wrapper around a pointer since I can't have a typed buffer of pointers :(
        private struct Reallocation {

            public StyleResult * result;

        }

        public void WriteUpdates(StyleRebuildResultList buildResultList) {

            updatesThisFrame.size = 0;
            
            UnmanagedPagedList<StyleRebuildResult> rebuildList = buildResultList.GetResultList();

            BufferList<Reallocation> reallocationList = new BufferList<Reallocation>(32, Allocator.Temp);
            
            // first pass will release any allocated blocks that are too small to hold their new content
            // I don't want to immediately allocate the new block because we might have cases where we 
            // allocate a new page because we didnt have a free block but now we do and can use it.
            for (int pageIndex = 0; pageIndex < rebuildList.pageCount; pageIndex++) {

                PagedListPage<StyleRebuildResult> page = rebuildList.GetPage(pageIndex);

                for (int i = 0; i < page.size; i++) {

                    ref StyleRebuildResult rebuildResult = ref page.array[i];

                    StyleResult * lastResult = table.GetPointer(rebuildResult.styleSetId);

                    uint newBlockSize = (uint) rebuildResult.propertyCount;

                    if (rebuildResult.propertyCount <= lastResult->propertyCount) {
                        continue;
                    }

                    int oldAllocatorIndex = lastResult->allocatorIndex;
                    int newAllocatorIndex = GetAllocatorIndex(newBlockSize);

                    // if old allocation index is too small, free it and reallocate a bigger one
                    // oldAllocatorIndex will be 0 if never allocated 
                    if (oldAllocatorIndex < newAllocatorIndex) {
                        
                        if (lastResult->properties != null) {
                            allocatorList[oldAllocatorIndex].Free(lastResult->properties);
                            lastResult->properties = null;
                        }

                        lastResult->allocatorIndex = newAllocatorIndex;
                        
                        reallocationList.Add(new Reallocation() {
                            result = lastResult
                        });
                        
                    }

                }

            }

            // I could probably sort this so allocators get hit in a uniform order. One step further would be to handle bulk allocation requests but allocators do not support this yet
            for (int i = 0; i < reallocationList.size; i++) {
                ref Reallocation x = ref reallocationList.array[i];
                x.result->properties = allocatorList[x.result->allocatorIndex].Allocate<byte>();
            }
            
            reallocationList.Dispose();
            
            // first pass will release any allocated blocks that are too small to hold their new content
            
            for (int pageIndex = 0; pageIndex < rebuildList.pageCount; pageIndex++) {

                PagedListPage<StyleRebuildResult> page = rebuildList.GetPage(pageIndex);

                for (int i = 0; i < page.size; i++) {

                    ref StyleRebuildResult rebuildResult = ref page.array[i];

                    ref StyleResult styleResult = ref table.GetReference(rebuildResult.styleSetId);

                    // we are leaving dead properties in the buffer on purpose if new count < old count
                    // because we just don't need to clear them. 
                    
                    if (rebuildResult.propertyCount > 0) {

                        PropertyId* keys = (PropertyId*) styleResult.properties;
                        PropertyData* values = (PropertyData*) (keys + rebuildResult.propertyCount);

                        TypedUnsafe.MemCpy(keys, rebuildResult.keys, rebuildResult.propertyCount);
                        TypedUnsafe.MemCpy(values, rebuildResult.values, rebuildResult.propertyCount);
                    }

                    styleResult.propertyCount = rebuildResult.propertyCount;

                    updatesThisFrame.Add(rebuildResult.styleSetId);
                    
                }

            }

        }

    }

}