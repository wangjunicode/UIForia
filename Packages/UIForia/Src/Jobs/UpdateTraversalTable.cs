using UIForia.Elements;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct UpdateTraversalTable : IJob {

        public ElementTable<ElementMetaInfo> metaTable;
        public ElementTable<ElementTraversalInfo> traversalTable;
        public ElementTable<HierarchyInfo> hierarchyTable;
        public DataList<ElementId>.Shared rootIds;

        // this could be two jobs done in parallel however we'd have some false sharing contention
        // because of writes into the traversal table

        // todo -- runs slow in managed, inline all the things and pre-allocate max size stack based on total number of active elements in application
        
        public void Execute() {

            int ftbIndex = 0;
            int btfIndex = 0;

            DataList<ElementId> stack = new DataList<ElementId>(512, Allocator.Temp);
            
            for (int rootIdx = 0; rootIdx < rootIds.size; rootIdx++) {

                stack[stack.size++] = rootIds[rootIdx];

                while (stack.size != 0) {

                    ElementId current = stack[--stack.size];

                    int currentIdx = current.index;
                    
                    traversalTable.array[currentIdx].ftbIndex = ftbIndex++;

                    int childCount = hierarchyTable.array[currentIdx].childCount;

                    stack.EnsureAdditionalCapacity(childCount);
                    ElementId* stackArray = stack.GetArrayPointer();

                    ElementId childPtr = hierarchyTable.array[currentIdx].lastChildId;

                    int size = stack.size;
                    for (int i = 0; i < childCount; i++) {

                        // if (!ElementSystem.IsDeadOrDisabled(childPtr, metaTable)) {
                        int idx = childPtr.index;
                        if (!(metaTable.array[idx].generation != childPtr.generation || (metaTable.array[idx].flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet)) {
                            stackArray[size++] = childPtr;
                        }

                        childPtr = hierarchyTable.array[childPtr.index].prevSiblingId;

                    }

                    stack.size = size;
                }
            }

            for (int rootIdx = 0; rootIdx < rootIds.size; rootIdx++) {

                stack[stack.size++] = rootIds[rootIdx];

                while (stack.size != 0) {

                    ElementId current = stack[--stack.size];
                    int currentIdx = current.index;

                    traversalTable.array[currentIdx].btfIndex = btfIndex++;

                    int childCount = hierarchyTable.array[currentIdx].childCount;

                    ElementId childPtr = hierarchyTable.array[currentIdx].firstChildId;

                    for (int i = 0; i < childCount; i++) {

                        if (!(metaTable.array[childPtr.id & ElementId.ENTITY_INDEX_MASK].generation != childPtr.generation || (metaTable.array[childPtr.id & ElementId.ENTITY_INDEX_MASK].flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet)) {
                            stack.AddUnchecked(childPtr);
                        }

                        childPtr = hierarchyTable.array[childPtr.index].nextSiblingId;

                    }

                }
            }

            stack.Dispose();
        }

    }

}