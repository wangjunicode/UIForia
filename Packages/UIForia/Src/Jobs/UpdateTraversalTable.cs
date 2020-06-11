using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public struct UpdateTraversalTable : IJob {

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

                    traversalTable[current].ftbIndex = ftbIndex++;

                    int childCount = hierarchyTable[current].childCount;

                    stack.EnsureAdditionalCapacity(childCount);

                    ElementId childPtr = hierarchyTable[current].lastChildId;

                    for (int i = 0; i < childCount; i++) {

                        if (!ElementSystem.IsDeadOrDisabled(childPtr, metaTable)) {
                            stack.AddUnchecked(childPtr);
                        }

                        childPtr = hierarchyTable[childPtr].prevSiblingId;

                    }

                }
            }

            for (int rootIdx = 0; rootIdx < rootIds.size; rootIdx++) {

                stack[stack.size++] = rootIds[rootIdx];

                while (stack.size != 0) {

                    ElementId current = stack[--stack.size];

                    traversalTable[current].btfIndex = btfIndex++;

                    int childCount = hierarchyTable[current].childCount;

                    ElementId childPtr = hierarchyTable[current].firstChildId;

                    for (int i = 0; i < childCount; i++) {

                        if (!ElementSystem.IsDeadOrDisabled(childPtr, metaTable)) {
                            stack.AddUnchecked(childPtr);
                        }

                        childPtr = hierarchyTable[childPtr].nextSiblingId;

                    }

                }
            }

            stack.Dispose();
        }

    }

}