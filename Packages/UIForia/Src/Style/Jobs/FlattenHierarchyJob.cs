using System;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal struct FlattenHierarchyJob : IJob {
        
        /// <summary>
        /// A map of all the active element IDs in the order
        /// they were touched this frame.
        /// 
        /// activeElementIds
        /// ================
        /// index (sequential index)
        /// elementID
        /// 
        /// </summary>
        public CheckedArray<ElementId> activeElementIds;
        
        /// <summary>
        /// A map of all the existing element IDs mapping to
        /// indices in the active element ID table.
        ///
        /// 0-based indices.
        ///
        /// allElementIdsToActiveIdIndex
        /// ============================
        /// index (* -> ElementId.index)
        /// value (* -> activeElementIds.index)
        /// 
        /// </summary>
        public CheckedArray<int> activeIndexByElementId;

        /// <summary>
        /// Maps a style state to every existing element ID index.
        ///
        /// stateTable
        /// ==========
        /// index (* -> allElementIdsToActiveIdIndex.index)
        /// style state
        /// 
        /// </summary>
        public CheckedArray<StyleState> stateTable;
        
        /// <summary>
        /// Maps a style state to every active element ID index.
        /// </summary>
        public CheckedArray<StyleState> flattenedStateTable;
        
        public CheckedArray<int> parentIndexByActiveElementIndex;

        /// <summary>
        /// Maps an elementID to its parent's elementID.
        ///
        /// 1-based indices, because the 0 index of an element id is a default value.
        /// 
        /// elementIdToParentId
        /// ===================
        /// index (* -> ElementId.index)
        /// ElementId (parent's ElementId)
        /// 
        /// </summary>
        public CheckedArray<ElementId> elementIdToParentId;
        
        public CheckedArray<int> childCountByActiveIndex;
        public CheckedArray<int> siblingIndexByActiveIndex;
        
        public unsafe void Execute() {
            UnsafeUtility.MemSet(activeIndexByElementId.array, byte.MaxValue, sizeof(int) * activeIndexByElementId.size);
            TypedUnsafe.MemClear(childCountByActiveIndex.array, childCountByActiveIndex.size);
            TypedUnsafe.MemClear(siblingIndexByActiveIndex.array, siblingIndexByActiveIndex.size);

            for (int activeElementIndex = 0; activeElementIndex < activeElementIds.size; activeElementIndex++) {
                ElementId elementId = activeElementIds[activeElementIndex];
                int elementIdIndex = elementId.index;
                activeIndexByElementId[elementIdIndex] = activeElementIndex;

                ElementId parentId = elementIdToParentId[elementIdIndex];
                int activeParentIndex = activeIndexByElementId[parentId.index];
                parentIndexByActiveElementIndex[activeElementIndex] = activeParentIndex;
                
                // the root element has a parent index of -1, so we're using the math.max/min trick here to default to
                // an existing id, namely the id of the root element itself. in order to avoid adding the root element
                // to its own childCount or siblingIndex we apply a second math.min/max trick.
                int activeParentIndexOrDefault = math.max(activeParentIndex, 0);

                // only the root element has a parentId.index of 0 AND activeParentIndexOrDefault = 0
                // it's like writing if (parent.index == 0 && activeParentIndex < 0) { childCount += 0; }
                childCountByActiveIndex[activeParentIndexOrDefault] += math.min(activeParentIndexOrDefault + parentId.index, 1);

                // the current childCount of the current element's parent must be the sibling index of the current element. 
                // if the current element is the root element the count will always be 0, because we start with it.
                siblingIndexByActiveIndex[activeElementIndex] = math.max(childCountByActiveIndex[activeParentIndexOrDefault] - 1, 0);
                
                flattenedStateTable[activeElementIndex] = stateTable[elementIdIndex];
            }
        }

    }

}