using UIForia.Elements;
using UIForia.ListTypes;
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

            List_ElementId stack = new List_ElementId(512, Allocator.Temp);

            for (int rootIdx = 0; rootIdx < rootIds.size; rootIdx++) {

                stack.array[stack.size++] = rootIds[rootIdx];

                while (stack.size != 0) {

                    ElementId current = stack.array[--stack.size];

                    int currentIdx = current.id & ElementId.ENTITY_INDEX_MASK;

                    traversalTable.array[currentIdx].ftbIndex = ftbIndex++;

                    int childCount = hierarchyTable.array[currentIdx].childCount;

                    if (stack.size + childCount >= stack.Capacity) {
                        stack.EnsureAdditionalCapacity(childCount);
                    }

                    ElementId childPtr = hierarchyTable.array[currentIdx].lastChildId;

                    int size = stack.size;
                    for (int i = 0; i < childCount; i++) {

                        int idx = childPtr.id & ElementId.ENTITY_INDEX_MASK;
                        if (!(metaTable.array[idx].generation != childPtr.generation || (metaTable.array[idx].flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet)) {
                            stack.array[size++] = childPtr;
                        }

                        childPtr = hierarchyTable.array[idx].prevSiblingId;

                    }

                    stack.size = size;
                }
            }

            for (int rootIdx = 0; rootIdx < rootIds.size; rootIdx++) {

                stack.array[stack.size++] = rootIds[rootIdx];

                while (stack.size != 0) {

                    ElementId current = stack.array[--stack.size];
                    int currentIdx = current.id & ElementId.ENTITY_INDEX_MASK;

                    traversalTable.array[currentIdx].btfIndex = btfIndex++;

                    int childCount = hierarchyTable.array[currentIdx].childCount;

                    ElementId childPtr = hierarchyTable.array[currentIdx].firstChildId;

                    for (int i = 0; i < childCount; i++) {
                        int idx = childPtr.id & ElementId.ENTITY_INDEX_MASK;

                        if (!(metaTable.array[idx].generation != childPtr.generation || (metaTable.array[idx].flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet)) {
                            stack.array[stack.size++] = childPtr;
                        }

                        childPtr = hierarchyTable.array[idx].nextSiblingId;

                    }

                }
            }

            stack.Dispose();
        }

    }

}