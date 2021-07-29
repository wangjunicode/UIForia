using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct CreateHierarchyJob : IJob {

        public CheckedArray<ushort> depthTable;
        public CheckedArray<ElementId> rootIds;
        public DataList<ElementId> activeElementList;
        public CheckedArray<HierarchyInfo> hierarchyTable;
        public CheckedArray<TraversalInfo> traversalTable;
        public CheckedArray<RuntimeTraversalInfo> runtimeInfoTable;
        
        public void Execute() {

            TypedUnsafe.MemClear(hierarchyTable.array, hierarchyTable.size);
            TypedUnsafe.MemClear(traversalTable.array, traversalTable.size);
            
            int idx = 0;
            while (idx < activeElementList.size) {
                ElementId elementId = activeElementList.array[idx];
                ref HierarchyInfo parent = ref hierarchyTable.Get(elementId.index);
                idx = Step(idx, ref parent, elementId);
            }
            
            CreateTraversalTable();

        }

        private void CreateTraversalTable() {
            int btfIndex = 0;

            DataList<ElementId> stack = new DataList<ElementId>(math.min(512, hierarchyTable.size), Allocator.Temp);

            for (int rootIdx = 0; rootIdx < rootIds.size; rootIdx++) {

                stack.array[stack.size++] = rootIds[rootIdx];

                while (stack.size != 0) {

                    ElementId current = stack.array[--stack.size];
                    int currentIdx = current.id & ElementId.k_IndexMask;
                    int childCount = hierarchyTable.array[currentIdx].childCount;

                    ref TraversalInfo value = ref traversalTable.array[currentIdx];
                    value.btfIndex = btfIndex++;
                    value.ftbIndex = runtimeInfoTable.array[currentIdx].index;
                    value.depth = depthTable.array[currentIdx];
                    // will be set in the UpdateTraversalInfoZIndexJob
                    value.zIndex = 0;

                    stack.EnsureAdditionalCapacity(childCount);

                    ElementId childPtr = hierarchyTable[currentIdx].firstChildId;

                    for (int i = 0; i < childCount; i++) {
                        stack.array[stack.size++] = childPtr;
                        childPtr = hierarchyTable[childPtr.id & ElementId.k_IndexMask].nextSiblingId;

                    }

                }
            }

            stack.Dispose();
        }

        // should return index of next sibling or next element 
        private int Step(int idx, ref HierarchyInfo parent, ElementId elementId) {

            RuntimeTraversalInfo info = runtimeInfoTable[elementId.index];
            bool hasChildren = info.lastChildIndex != 0 && info.lastChildIndex != info.index + 1;

            if (!hasChildren) {
                parent.firstChildId = default;
                parent.childCount = 0;
                return idx + 1;
            }

            ElementId prevElementId = default;

            int i = idx + 1;

            ushort childCount = 0;
            parent.firstChildId = activeElementList[i];

            while (i != info.lastChildIndex && i < activeElementList.size) {
                ElementId currentId = activeElementList[i];
                
                ref HierarchyInfo ptr = ref hierarchyTable.Get(currentId.index);
                ptr.prevSiblingId = prevElementId;
                ptr.siblingIndex = childCount++;
                
                i = Step(i, ref ptr, currentId);
                
                if (prevElementId != default) {
                    ref HierarchyInfo prev = ref hierarchyTable.Get(prevElementId.index);
                    prev.nextSiblingId = currentId;
                }
                else {
                    ptr.nextSiblingId = default;
                }
                
                prevElementId = currentId;
                
            }

            
            
            parent.childCount = childCount;

            return i;

        }

    }

}