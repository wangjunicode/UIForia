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

            while (stack.size != 0) {
                ElementId current = stack[--stack.size];

                ref LayoutHierarchyInfo hierarchyInfo = ref layoutHierarchyTable[current];

                if (hierarchyInfo.behavior != LayoutBehavior.Normal) {
                    continue;
                }

                elementList.AddUnchecked(current);
                parentList.AddUnchecked(hierarchyInfo.parentId);

                ElementId childPtr = hierarchyInfo.lastChildId;

                while (childPtr != default) {
                    stack.AddUnchecked(childPtr);
                    childPtr = layoutHierarchyTable[childPtr].prevSiblingId;
                }

            }

            if (ignoredList.size > 1) {
                NativeSortExtension.Sort(ignoredList.GetArrayPointer(), ignoredList.size, new ElementFTBHierarchySort(traversalTable));
            }

            for (int i = 0; i < ignoredList.size; i++) {
                stack.Add(ignoredList[i]);

                while (stack.size != 0) {
                    ElementId current = stack[--stack.size];

                    ref LayoutHierarchyInfo hierarchyInfo = ref layoutHierarchyTable[current];
                    
                    elementList.AddUnchecked(current);
                    parentList.AddUnchecked(hierarchyInfo.parentId);

                    ElementId childPtr = hierarchyInfo.lastChildId;

                    while (childPtr != default) {
                        stack.AddUnchecked(childPtr);
                        childPtr = layoutHierarchyTable[childPtr].prevSiblingId;
                    }
                }

            }

            stack.Dispose();
        }

    }

}