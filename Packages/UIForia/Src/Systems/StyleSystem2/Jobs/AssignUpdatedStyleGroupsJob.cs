using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    // [BurstCompile]
    public unsafe struct AssignUpdatedStyleGroupsJob : IJob {

        [ReadOnly] public UnsafeList<StyleSetData> dataMap;
        [ReadOnly] public UnsafeSpan<SharedStyleChangeSet> changeSets;

        public void Execute() { // todo -- maybe better as ParallelBatchedRange

            for (int changeSetIdx = 0; changeSetIdx < changeSets.size; changeSetIdx++) {

                SharedStyleChangeSet changeSet = changeSets.array[changeSetIdx];

                ref StyleSetData data = ref dataMap.array[changeSet.styleDataId];

                data.state = changeSet.newState;
                data.sharedStyleCount = changeSet.newStyleCount;
                data.changeSetId = ushort.MaxValue;
                
                StyleId* newStyles = changeSet.newStyles;

                // this is copying the updated styles back, even if they didnt change
                // (like when element enters a new state but no styles were defined for it)
                // better to just do the work than track whether we need to or not
                for (int i = 0; i < changeSet.newStyleCount; i++) {
                    data.sharedStyles[i] = newStyles[i];
                }

            }

        }

    }

}