using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct ComputeStyleStateDiff : IJobParallelForBatch {

        [NativeSetThreadIndex] public int threadIndex;

        public UnmanagedList<SharedStyleChangeSet> changeSets;
        public UnmanagedPagedList<StyleStateGroup>.PerThread addedStyleStateGroups;
        public UnmanagedPagedList<StyleStateGroup>.PerThread removedStyleStateGroups;

        private const int k_MaxStyleStateSize = StyleSet.k_MaxSharedStyles * 4; // max styles each with max 4 states

        public void Execute(int startIndex, int count) {
      
            UnmanagedList<StyleStateGroup> addBuffer = new UnmanagedList<StyleStateGroup>(k_MaxStyleStateSize, Allocator.TempJob);
            UnmanagedList<StyleStateGroup> oldStyleStates = new UnmanagedList<StyleStateGroup>(k_MaxStyleStateSize, Allocator.Temp);
            UnmanagedList<StyleStateGroup> newStyleStates = new UnmanagedList<StyleStateGroup>(k_MaxStyleStateSize, Allocator.Temp);

            UnmanagedPagedList<StyleStateGroup> addedList = addedStyleStateGroups.GetListForThread(threadIndex);
            UnmanagedPagedList<StyleStateGroup> removedList = removedStyleStateGroups.GetListForThread(threadIndex);

            int endIndex = startIndex + count;

            for (int changeSetIdx = startIndex; changeSetIdx < endIndex; changeSetIdx++) {

                oldStyleStates.size = 0;
                newStyleStates.size = 0;

                SharedStyleChangeSet changeSet = changeSets.array[changeSetIdx];

                StyleId* oldStyles = changeSet.oldStyles;
                StyleId* newStyles = changeSet.newStyles;

                StyleState2 originalState = (StyleState2) (int) changeSet.originalState;
                StyleState2 newState = (StyleState2) (int) changeSet.newState;

                int oldStyleCount = changeSet.oldStyleCount;
                int newStyleCount = changeSet.newStyleCount;

                for (int i = 0; i < oldStyleCount; i++) {
                    StyleId styleId = oldStyles[i];
                    // order is important!
                    MaybeAddStyleGroup(ref oldStyleStates, originalState, StyleState2.Active, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref oldStyleStates, originalState, StyleState2.Focused, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref oldStyleStates, originalState, StyleState2.Hover, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref oldStyleStates, originalState, StyleState2.Normal, styleId, changeSet.styleDataId);
                }

                for (int i = 0; i < newStyleCount; i++) {
                    StyleId styleId = newStyles[i];
                    // order is important!
                    MaybeAddStyleGroup(ref newStyleStates, newState, StyleState2.Active, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref newStyleStates, newState, StyleState2.Focused, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref newStyleStates, newState, StyleState2.Hover, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref newStyleStates, newState, StyleState2.Normal, styleId, changeSet.styleDataId);

                }

                for (int i = 0; i < newStyleStates.size; i++) {

                    StyleStateGroup newGroup = newStyleStates.array[i];

                    int idx = -1;

                    for (int j = 0; j < oldStyleStates.size; j++) {
                        StyleStateGroup group = oldStyleStates.array[i];
                        if (group.state == newGroup.state && group.styleId == newGroup.styleId) {
                            idx = group.index;
                            oldStyleStates.array[j] = oldStyleStates.array[--oldStyleStates.size];
                            break;
                        }
                    }

                    if (idx == -1) {
                        // this is a new combination
                        addBuffer.Add(newGroup); 
                    }
                    else if (idx != i) { // index changed
                    }

                }

                addedList.AddRange(addBuffer.array, addBuffer.size);
                removedList.AddRange(oldStyleStates.array, oldStyleStates.size);

            }

            addBuffer.Dispose();
            oldStyleStates.Dispose();
            newStyleStates.Dispose();
        }
        
        private static void MaybeAddStyleGroup(ref  UnmanagedList<StyleStateGroup> list, StyleState2 checkState, StyleState2 targetState, StyleId styleId, int styleSetId) {
            if ((checkState & targetState) != 0 && styleId.DefinesState(targetState)) {
                int idx = list.size;
                list.array[list.size++] = new StyleStateGroup(idx, targetState, styleId, styleSetId);
            }
        }

    }

}