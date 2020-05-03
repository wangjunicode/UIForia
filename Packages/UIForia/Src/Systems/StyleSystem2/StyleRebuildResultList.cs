using System.Runtime.InteropServices;
using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia {

    // this is an unfortunate size right now
    public unsafe struct StyleRebuildResult {

        public int styleSetId;
        public int propertyCount;
        public PropertyId* keys;
        public PropertyData* values;

        public bool TryGetProperty(PropertyId propertyId, out PropertyData retn) {
            
            for (int i = 0; i < propertyCount; i++) {
                if (keys[i] == propertyId) {
                    retn = values[i];
                    return true;
                }
            }

            retn = default;
            return false;
        }

        public PropertyData GetProperty(PropertyId propertyId) {
            for (int i = 0; i < propertyCount; i++) {
                if (keys[i] == propertyId) {
                    return values[i];
                }
            }
            return default;
        }

    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct StyleRebuildResultList : IPerThreadCompatible, IMergeableToList<StyleRebuildResult> {

        // todo merge keyList and valueList into one allocation
        // maybe also merge info list. grow from ends a-la bitsquid 
        public PagedListState* infoList;
        public PagedListState* keyList;
        public PagedListState* valueList;

        public StyleRebuildResultList(Allocator allocator) : this() {
            InitializeForThread(allocator);
        }

        public void InitializeForThread(Allocator allocator) {
            this.infoList = new UnmanagedPagedList<StyleRebuildResult>(64, allocator).GetStatePointer();
            this.keyList = new UnmanagedPagedList<PropertyId>(256, allocator).GetStatePointer();
            this.valueList = new UnmanagedPagedList<PropertyData>(256, allocator).GetStatePointer();
        }

        public bool IsInitialized {
            get => infoList != null && keyList != null && valueList != null;
        }

        // Rebuild result contains pointers that will reference key and value lists
        // these lists must NOT be cleared or reordered, or changed in anyway other than append
        // until the end of the frame.
        public void Add(int styleSetId, int propertyCount, PropertyId* keys, PropertyData* values) {
            infoList->AddItem<StyleRebuildResult>(new StyleRebuildResult() {
                styleSetId = styleSetId,
                propertyCount = propertyCount,
                keys = keyList->AddRange<PropertyId>(keys, propertyCount),
                values = valueList->AddRange<PropertyData>(values, propertyCount)
            });
        }

        public UnmanagedPagedList<StyleRebuildResult> GetResultList() {
            return new UnmanagedPagedList<StyleRebuildResult>(infoList);
        }

        public void Dispose() {
            if (infoList != null) {
                infoList->Dispose();
            }

            if (keyList != null) {
                keyList->Dispose();
            }

            if (valueList != null) {
                valueList->Dispose();
            }

            this = default;
        }

        public void Merge(UnmanagedList<StyleRebuildResult> output) {
            infoList->FlattenToList<StyleRebuildResult>(output);
        }

        public int ItemCount() {
            return infoList->totalItemCount;
        }

        public void Clear() {
            infoList->Clear();
            keyList->Clear();
            valueList->Clear();
        }

        // only used for testing!
        public StyleRebuildResult GetResultForId(int id) {

            UnmanagedPagedList<StyleRebuildResult> list = new UnmanagedPagedList<StyleRebuildResult>(infoList);
            
            for (int p = 0; p < list.pageCount; p++) {
                
                PagedListPage<StyleRebuildResult> page = list.GetPage(p);
                
                for (int i = 0; i < page.size; i++) {
                    if (page.array[i].styleSetId == id) {
                        return page.array[i];
                    }
                }
            }

            return default;
            
        }

    }

}