using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

   // [BurstCompile]
    public struct UpdateTraversalTable : IJob {

        public ElementId rootId;
        public ElementTable<ElementMetaInfo> metaTable;
        public ElementTable<ElementTraversalInfo> traversalTable;
        public ElementTable<HierarchyInfo> hierarchyTable;

        // this could be two jobs done in parallel however we'd have some false sharing contention
        // because of writes into the traversal table
        
        // todo -- if we end up using stack traversals a lot it might be better to store the stack externally
        // so we dont keep resizing all the time. or maybe it doesn't matter, we'll need to test it out
        public void Execute() {

            ushort ftbIndex = 0;
            ushort btfIndex = 0;

            DataList<ElementId> stack = new DataList<ElementId>(64, Allocator.TempJob);

            stack[stack.size++] = rootId;

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
            
            stack[stack.size++] = rootId;

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

            stack.Dispose();
        }

    }

}
