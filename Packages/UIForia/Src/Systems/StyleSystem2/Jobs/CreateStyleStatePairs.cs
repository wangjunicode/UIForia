using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct CreateStyleStatePairs : IJobParallelForBatch, IJob, IVertigoParallel {

        [NativeSetThreadIndex] public int threadIndex;

        public DataList<SharedStyleUpdate>.Shared sharedChangeSets;
        public PerThread<StyleStatePairList> perThread_styleStatePairList;

        public ParallelParams parallel { get; set; }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        public void Execute() {
            Run(0, sharedChangeSets.size);
        }

        private void Run(int start, int end) {

            ref StyleStatePairList styleStatePairList = ref perThread_styleStatePairList.GetForThread(threadIndex);

            StyleStatePair* buffer = styleStatePairList.GetScratchBuffer();

            StyleState2* stateList = stackalloc StyleState2[4];

            for (int changeIndex = start; changeIndex < end; changeIndex++) {

                ref SharedStyleUpdate update = ref sharedChangeSets[changeIndex];

                StyleId* originalStyles = update.originalStyles;
                StyleId* updatedStyles = update.updatedStyles;

                StyleState2 originalState = update.originalState;
                StyleState2 updatedState = update.updatedState;

                int originalStyleCount = update.originalStyleCount;
                int updatedStyleCount = update.updatedStyleCount;

                int oldCount = MakeStyleStatePairs(stateList, originalState, originalStyleCount, originalStyles, buffer);
                int newCount = MakeStyleStatePairs(stateList, updatedState, updatedStyleCount, updatedStyles, buffer + oldCount);

                // no changes between new pair set and old pair set, nobody in the rest of the system cares, drop the change
                if (oldCount == newCount && UnsafeUtility.MemCmp(buffer, buffer + oldCount, newCount * sizeof(StyleStatePair)) == 0) {
                    continue;
                }

                StyleStatePair* oldPairList = buffer;
                StyleStatePair* newPairList = buffer + oldCount;

                // re-using and manipulating the buffer below so we need to set this now
                styleStatePairList.SetCurrentPairs(update.elementId, newPairList, newCount);
                
                for (int i = 0; i < newCount; i++) {
                    StyleStatePair newPair = newPairList[i];
                    for (int j = 0; j < oldCount; j++) {
                        // if the target pair is in both new and old lists, remove it from both
                        if (oldPairList[j].val == newPair.val) {
                            oldPairList[j] = oldPairList[--oldCount];
                            newPairList[i] = newPairList[--newCount];
                            i--;
                            break;
                        }
                    }

                }

                styleStatePairList.SetAddedPairs(update.elementId, newPairList, newCount);
                styleStatePairList.SetRemovedPairs(update.elementId, oldPairList, oldCount);

            }

        }

        private static int MakeStyleStatePairs(StyleState2* stateList, StyleState2 state, int styleCount, StyleId* styleIds, StyleStatePair* buffer) {
            int bufferSize = 0;

            int count = 0;

            if ((state & StyleState2.Active) != 0) stateList[count++] = StyleState2.Active;
            if ((state & StyleState2.Focused) != 0) stateList[count++] = StyleState2.Focused;
            if ((state & StyleState2.Hover) != 0) stateList[count++] = StyleState2.Hover;
            if ((state & StyleState2.Normal) != 0) stateList[count++] = StyleState2.Normal;

            for (int stateIndex = 0; stateIndex < count; stateIndex++) {

                StyleState2 currentState = stateList[stateIndex];

                for (int i = 0; i < styleCount; i++) {

                    StyleId styleId = styleIds[i];

                    if ((styleId.definedStates & currentState) != 0) {
                        buffer[bufferSize++] = new StyleStatePair(styleId, currentState);
                    }
                }

            }

            return bufferSize;
        }

    }

}