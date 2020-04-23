using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct ProcessSharedStyleUpdatesJob : IJob {

        public NativeList<int> rebuildList;
        public NativeList<StyleStateGroup> addedList;
        public NativeList<StyleStateGroup> removedList;
        public UnsafeSpan<SharedStyleChangeSet> changeSets;

        private const int k_MaxStyleStateSize = StyleSet.k_MaxSharedStyles * 4; // max styles each with max 4 states

        public void Execute() {

            NativeArray<StyleStateGroup> oldStyleStateList = new NativeArray<StyleStateGroup>(k_MaxStyleStateSize, Allocator.Temp);
            NativeArray<StyleStateGroup> newStyleStateList = new NativeArray<StyleStateGroup>(k_MaxStyleStateSize, Allocator.Temp);

            UnsafeSizedBuffer<StyleStateGroup> oldStyleStates = new UnsafeSizedBuffer<StyleStateGroup>(oldStyleStateList);
            UnsafeSizedBuffer<StyleStateGroup> newStyleStates = new UnsafeSizedBuffer<StyleStateGroup>(newStyleStateList);

            for (int changeSetIdx = 0; changeSetIdx < changeSets.size; changeSetIdx++) {

                oldStyleStates.size = 0;
                newStyleStates.size = 0;

                SharedStyleChangeSet changeSet = changeSets.array[changeSetIdx];

                // todo -- if state and shared styles didn't change, no-op out of here 
                
                StyleId* oldStyles = changeSet.oldStyles;
                StyleId* newStyles = changeSet.newStyles;

                StyleState2 originalState = (StyleState2)(int)changeSet.originalState;
                StyleState2 newState = (StyleState2)(int)changeSet.newState;
                
                bool needsRebuild = false;

                int oldStyleCount = changeSet.oldStyleCount;
                int newStyleCount = changeSet.newStyleCount;
                
                for (int i = 0; i < oldStyleCount; i++) {
                    StyleId styleId = oldStyles[i];
                    // order is important!
                    MaybeAddStyleGroup(ref oldStyleStates, originalState, StyleState2.Normal, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref oldStyleStates, originalState, StyleState2.Hover, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref oldStyleStates, originalState, StyleState2.Focused, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref oldStyleStates, originalState, StyleState2.Active, styleId, changeSet.styleDataId);
                }

                for (int i = 0; i < newStyleCount; i++) {
                    StyleId styleId = newStyles[i];
                    // order is important!
                    MaybeAddStyleGroup(ref newStyleStates, newState, StyleState2.Normal, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref newStyleStates, newState, StyleState2.Hover, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref newStyleStates, newState, StyleState2.Focused, styleId, changeSet.styleDataId);
                    MaybeAddStyleGroup(ref newStyleStates, newState, StyleState2.Active, styleId, changeSet.styleDataId);

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
                        addedList.Add(newGroup);
                        needsRebuild = true;
                    }
                    else if (idx != i) { // index changed
                        needsRebuild = true;
                    }

                }

                for (int i = 0; i < oldStyleStates.size; i++) {
                    needsRebuild = true;
                    removedList.Add(oldStyleStates.array[i]);
                }

                if (needsRebuild) {
                    rebuildList.Add(changeSet.styleDataId);
                }

            }

            oldStyleStateList.Dispose();
            newStyleStateList.Dispose();
        }

        private static void MaybeAddStyleGroup(ref UnsafeSizedBuffer<StyleStateGroup> list, StyleState2 checkState, StyleState2 targetState, StyleId styleId, int styleSetId) {
            if ((checkState & targetState) != 0 && styleId.DefinesState(targetState)) {
                int idx = list.size;
                list.array[list.size++] = new StyleStateGroup(idx, targetState, styleId, styleSetId);
            }
        }

    }

}