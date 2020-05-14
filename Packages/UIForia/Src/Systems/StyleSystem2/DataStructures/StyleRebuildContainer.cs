using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;

namespace UIForia {

    public unsafe struct StyleRebuildContainer : IPerThreadCompatible {

        public PropertyId* scratchIds;
        public PropertyData* scratchData;

        public PagedListState* rebuiltStyleData; // PagedList<byte>
        public PagedListState* rebuildResultList; // PagedList<StyleRebuildResult>

        public StyleRebuildContainer(Allocator allocator) : this() {
            InitializeForThread(allocator);
        }

        public void InitializeForThread(Allocator allocator) {
            int byteCount = TypedUnsafe.ByteSize<PropertyId, PropertyData>(512);
            TypedUnsafe.MallocSplitBuffer(out scratchIds, out scratchData, VertigoStyleSystem.k_MaxStyleProperties, Allocator.TempJob);
            rebuiltStyleData = new PagedList<byte>(byteCount, allocator).GetStatePointer();
            rebuildResultList = new PagedList<StyleRebuildResult>(128, allocator).GetStatePointer();
        }
        
        public PagedList<StyleRebuildResult> GetResultList() {
            return new PagedList<StyleRebuildResult>(rebuildResultList);
        }

        public bool IsInitialized {
            get => scratchIds != null && rebuildResultList != null && rebuiltStyleData != null;
        }

        public void SetRebuildResult(ElementId elementId, int propertyCount, [NoAlias] PropertyId* keys, [NoAlias] PropertyData* values) {

            byte* properties = null;

            if (propertyCount > 0) {
                int byteCount = TypedUnsafe.ByteSize(keys, values, propertyCount);
                properties = rebuiltStyleData->Reserve<byte>(byteCount);
                PropertyId* keyDest = (PropertyId*) properties;
                PropertyData* valueDest = (PropertyData*) (properties + propertyCount);

                TypedUnsafe.MemCpy(keyDest, keys, propertyCount);
                TypedUnsafe.MemCpy(valueDest, values, propertyCount);

            }

            rebuildResultList->AddItem(new StyleRebuildResult() {
                elementId = elementId,
                properties = properties,
                propertyCount = propertyCount
            });

        }

        public void Dispose() {
            TypedUnsafe.Dispose(scratchIds, Allocator.TempJob);
            rebuildResultList->Dispose();
            rebuiltStyleData->Dispose();
            this = default;
        }

    }

}