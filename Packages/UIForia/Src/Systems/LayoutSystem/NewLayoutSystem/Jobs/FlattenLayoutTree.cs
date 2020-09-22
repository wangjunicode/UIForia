using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Layout {

    [BurstCompile]
    internal unsafe struct FlattenLayoutTree : IJob {

        public ElementId viewRootId;
        public ElementTable<ElementTraversalInfo> traversalTable;
        public ElementTable<LayoutHierarchyInfo> layoutHierarchyTable;
        public ElementTable<ElementMetaInfo> metaTable;
        public DataList<ElementId>.Shared elementList;
        public DataList<ElementId>.Shared parentList;
        public DataList<ElementId>.Shared ignoredList;
        public int viewActiveElementCount;

        public void Execute() {

            if (viewActiveElementCount <= 0) {
                return;
            }

            Allocator stackAllocator = TypedUnsafe.GetTemporaryAllocatorLabel<ElementId>(viewActiveElementCount);
            DataList<ElementId> stack = new DataList<ElementId>(viewActiveElementCount, stackAllocator);

            elementList.EnsureCapacity(viewActiveElementCount);
            parentList.EnsureCapacity(viewActiveElementCount);

            stack.Add(viewRootId);

            // inlining a lot of this because outside of burst this was slow

            ElementId* s = stack.GetArrayPointer();
            LayoutHierarchyInfo* layoutHTable = layoutHierarchyTable.array;
            int stackSize = 1;
            ElementId* elist = elementList.GetArrayPointer();
            ElementId* plist = parentList.GetArrayPointer();
            int eSize = 0;
            int pSize = 0;

            // todo -- I think this isn't handling all hierarchies correctly
            // probably need to recurse / defer ignored somehow
            
            
            const int ENTITY_INDEX_BITS = 24;
            const int ENTITY_INDEX_MASK = (1 << ENTITY_INDEX_BITS) - 1;

            while (stackSize != 0) {
                ElementId current = s[--stackSize];
                
                ref LayoutHierarchyInfo hierarchyInfo = ref layoutHTable[current.id & ENTITY_INDEX_MASK];

                if (hierarchyInfo.behavior != LayoutBehavior.Normal) {//  && hierarchyInfo.behavior != LayoutBehavior.__Special__) {
                    continue;
                }
                
                elist[eSize++] = current;
                plist[pSize++] = hierarchyInfo.parentId;

                ElementId childPtr = hierarchyInfo.lastChildId;

                while (childPtr != default) {
                    s[stackSize++] = childPtr;
                    childPtr = layoutHTable[childPtr.id & ENTITY_INDEX_MASK].prevSiblingId;
                }

            }

            if (ignoredList.size > 0) {

                for (int i = 0; i < ignoredList.size; i++) {
                    ElementId elementId = ignoredList[i];
                    if (ElementSystem.IsDeadOrDisabled(elementId, metaTable)) {
                        ignoredList[i--] = ignoredList[--ignoredList.size];
                    }
                }

                if (ignoredList.size > 1) {
                    NativeSortExtension.Sort(ignoredList.GetArrayPointer(), ignoredList.size, new ElementFTBHierarchySort(traversalTable));
                }
            }

            for (int i = 0; i < ignoredList.size; i++) {
                s[stackSize++] = ignoredList[i];

                while (stackSize != 0) {
                    ElementId current = s[--stackSize];
                    
                    ref LayoutHierarchyInfo hierarchyInfo = ref layoutHTable[current.id & ENTITY_INDEX_MASK];

                    elist[eSize++] = current;
                    plist[pSize++] = hierarchyInfo.parentId;

                    ElementId childPtr = hierarchyInfo.lastChildId;

                    while (childPtr != default) {
                        s[stackSize++] = childPtr;
                        childPtr = layoutHTable[childPtr.id & ENTITY_INDEX_MASK].prevSiblingId;
                    }
                }

            }

            elementList.size = eSize;
            parentList.size = pSize;
            stack.Dispose();
        }

    }

}