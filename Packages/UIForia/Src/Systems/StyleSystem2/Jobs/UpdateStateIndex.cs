using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct UpdateStateIndex : IJob {

        public DataList<ElementId>.Shared activeIndex;
        public DataList<ElementId>.Shared focusIndex;
        public DataList<ElementId>.Shared hoverIndex;

        [ReadOnly] public ElementTable<ElementMetaInfo> metaTable;
        [ReadOnly] public DataList<StyleStateUpdate> stateUpdates;

        // in theory we could set up 3 jobs and do the index updates in parallel
        // in practice we probably dont have enough data to merit that
        public void Execute() {
            Run(0, stateUpdates.size);
        }

        private void Run(int start, int end) {

            int count = end - start;

            // over allocating because I dont expect a high element count and the size of the data type is just 4 bytes
            DataList<ElementId> buffer = new DataList<ElementId>(count, TypedUnsafe.GetTemporaryAllocator<ElementId>(count));

            // this is relatively naive in implementation but I never expect to have more than 100ish elements
            // pass through here so its not a problem until its a problem
            
            UpdateIndex(ref buffer, activeIndex, StyleState2.Active, start, end);
            UpdateIndex(ref buffer, focusIndex, StyleState2.Focused, start, end);
            UpdateIndex(ref buffer, hoverIndex, StyleState2.Hover, start, end);

            buffer.Dispose();

        }

        private void UpdateIndex(ref DataList<ElementId> buffer, DataList<ElementId>.Shared indexList, StyleState2 state, int start, int end) {

            buffer.size = 0;
            for (int updateIndex = start; updateIndex < end; updateIndex++) {

                ref StyleStateUpdate update = ref stateUpdates[updateIndex];

                if (ExitedState(update, state)) {
                    buffer.AddUnchecked(update.elementId);
                }

            }

            indexList.SetSize(ElementSystem.RemoveDeadElements(metaTable, indexList.GetArrayPointer(), indexList.size));

            for (int i = 0; i < buffer.size; i++) {

                ElementId target = buffer[i];
                for (int j = 0; j < indexList.size; j++) {
                    if (target == indexList[j]) {
                        indexList.SwapRemove(j);
                        break;
                    }
                }

            }
            
            buffer.size = 0;
            for (int updateIndex = start; updateIndex < end; updateIndex++) {

                ref StyleStateUpdate update = ref stateUpdates[updateIndex];

                if (EnteredState(update, state)) {
                    buffer.AddUnchecked(update.elementId);
                }

            }

            if (buffer.size > 0) {
                indexList.AddRange(buffer.GetArrayPointer(), buffer.size);
            }

        }

        private static bool EnteredState(in StyleStateUpdate update, StyleState2 target) {
            return (update.originalState & target) == 0 && (update.updatedState & target) != 0;
        }

        private static bool ExitedState(in StyleStateUpdate update, StyleState2 target) {
            return (update.updatedState & target) == 0 && (update.originalState & target) != 0;
        }

    }

}