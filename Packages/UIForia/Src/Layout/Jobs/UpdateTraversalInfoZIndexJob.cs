using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia.Style {
    
    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct UpdateTraversalInfoZIndexJob : IJob {

        public CheckedArray<TraversalInfo> traversalTable;
        
        public CheckedArray<ElementId> activeElementList;

        public CheckedArray<int> parentIndexByActiveElementIndex;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public StyleTables* styleTables;
        
        public void Execute() {
            DataList<int> computedZIndices = new DataList<int>(activeElementList.size, Allocator.Temp);
            computedZIndices.size = activeElementList.size;

            for (int i = 0; i < activeElementList.size; i++) {
                ElementId elementId = activeElementList[i];
                int parent = parentIndexByActiveElementIndex[i];
                int z = styleTables->ZIndex[elementId.index];
                if (parent == -1) {
                    computedZIndices[i] = z;
                }
                else {
                    computedZIndices[i] = z == 0 ? computedZIndices[parent] : z;
                }
                traversalTable.Get(elementId.index).zIndex = styleTables->ZIndex[elementId.index];
            }
            
            computedZIndices.Dispose();
        }
    }
}