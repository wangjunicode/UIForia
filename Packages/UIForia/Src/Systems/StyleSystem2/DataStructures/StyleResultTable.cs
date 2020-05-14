using System.Diagnostics;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    public unsafe struct StyleResultDebugView {

        public int propertyCount;
        public int allocatorIndex;
        public StyleProperty2[] properties;
        
        public StyleResultDebugView(StyleResult target) {
            this.propertyCount = target.propertyCount;
            this.allocatorIndex = target.allocatorIndex;
            this.properties = new StyleProperty2[propertyCount];
            
            PropertyId* keys = (PropertyId*) target.properties;
            PropertyData* values = (PropertyData*) (keys + target.propertyCount);
            
            for (int i = 0; i < propertyCount; i++) {
                properties[i] = new StyleProperty2(keys[i], values[i].value);
            }
        }

    }

    [AssertSize(16)]
    [DebuggerTypeProxy(typeof(StyleResultDebugView))]
    public unsafe struct StyleResult {

        public int propertyCount;
        public int allocatorIndex;
        public byte* properties;

        public PropertyId* keys {
            get => (PropertyId*) properties;
        }
        public PropertyData* values {
            get => (PropertyData*) (keys + propertyCount);
        }
        
        public StyleProperty2 GetProperty(PropertyId propertyId) {
            PropertyId* pKeys = keys;
            
            for (int i = 0; i < propertyCount; i++) {
                if (pKeys[i] == propertyId) {
                    return new StyleProperty2(pKeys[i], values[i].value);
                }
            }

            return default;
        }

        public bool HasProperty(PropertyId propertyId) {
            return TryGetProperty(propertyId, out StyleProperty2 _);
        }

        public bool TryGetProperty(PropertyId propertyId, out StyleProperty2 property) {
            PropertyId* pKeys = keys;
            
            for (int i = 0; i < propertyCount; i++) {
                if (pKeys[i] == propertyId) {
                    property = new StyleProperty2(pKeys[i], values[i].value);
                    return true;
                }
            }

            property = default;
            return false;
        }

    }

    public unsafe struct StyleResultTable {

        private DataList<StyleResult> table;

        public DisposedDataList<FixedBlockAllocator>.Shared allocatorList;

        public StyleResultTable(Allocator allocator) {
            this.table = new DataList<StyleResult>(16, allocator, NativeArrayOptions.ClearMemory);
            this.allocatorList = default;
        }
        
        public void EnsureElementCount(int count) {
            table.SetSize(count, NativeArrayOptions.ClearMemory);
        }

        public StyleResult this[int id] {
            get => table[id];
        }
        
        public void Dispose() {
            table.Dispose();
        }

        // system rule: pointers are valid only for 1 frame. as soon as the table is written to again pointers are invalidated

        private static int GetAllocatorIndex(uint propertyCount) {

            if (propertyCount < 4) propertyCount = 4;

            if (!BitUtil.IsPowerOfTwo(propertyCount)) {
                propertyCount = BitUtil.NextPowerOfTwo(propertyCount);
            }

            return BitUtil.GetPowerOfTwoBitIndex(propertyCount) - 1;

        }
        
        public void WriteUpdates(StyleRebuildResultList * buildResultList) {
            // todo -- i dont trust this code atm
            
            PagedList<StyleRebuildResult> rebuildList = buildResultList->GetResultList();

            PointerList<StyleResult> reallocationList = new PointerList<StyleResult>(16, Allocator.Temp);

            // first pass will release any allocated blocks that are too small to hold their new content
            // I don't want to immediately allocate the new block because we might have cases where we 
            // allocate a new page because we didnt have a free block but now we do and can use it.
            for (int pageIndex = 0; pageIndex < rebuildList.pageCount; pageIndex++) {

                PagedListPage<StyleRebuildResult> page = rebuildList.GetPage(pageIndex);

                for (int i = 0; i < page.size; i++) {

                    ref StyleRebuildResult rebuildResult = ref page.array[i];

                    StyleResult* lastResult = table.GetPointer(rebuildResult.elementId.index);

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

                        reallocationList.Add(lastResult);

                    }

                }

            }

            // I could probably sort this so allocators get hit in a uniform order. One step further would be to handle bulk allocation requests but allocators do not support this yet
            for (int i = 0; i < reallocationList.size; i++) {
                StyleResult* x = reallocationList[i];
                x->properties = allocatorList[x->allocatorIndex].Allocate<byte>();
            }

            reallocationList.Dispose();

            // first pass will release any allocated blocks that are too small to hold their new content

            for (int pageIndex = 0; pageIndex < rebuildList.pageCount; pageIndex++) {

                PagedListPage<StyleRebuildResult> page = rebuildList.GetPage(pageIndex);

                for (int i = 0; i < page.size; i++) {

                    ref StyleRebuildResult rebuildResult = ref page.array[i];

                    ref StyleResult styleResult = ref table.GetReference(rebuildResult.elementId.index);

                    // we are leaving dead properties in the buffer on purpose if new count < old count
                    // because we just don't need to clear them.
                    
                    styleResult.propertyCount = rebuildResult.propertyCount;

                    if (rebuildResult.propertyCount > 0) {

                        PropertyId* keys = (PropertyId*) styleResult.properties;
                        PropertyData* values = (PropertyData*) (keys + rebuildResult.propertyCount);
                        //
                        // TypedUnsafe.MemCpy(keys, rebuildResult.keys, rebuildResult.propertyCount);
                        // TypedUnsafe.MemCpy(values, rebuildResult.values, rebuildResult.propertyCount);
                    }
                    
                }

            }

        }

    }

}