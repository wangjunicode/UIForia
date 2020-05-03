using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {



    /// <summary>
    /// Convert a change set of StyleIds into lists of StyleStatePair, ignoring combinations that do not exist
    /// such as an element being hovered but a style not defining any hover state
    /// </summary>
    public unsafe struct ConvertStyleIdsToStatePairs : IJobParallelForBatch, IJob {

        [NativeSetThreadIndex] public int threadIndex;

        public SharedStyleChangeSet sharedStyleChangeSet;
        public PerThread<ConvertedStyleList> perThreadOutput;

        public void Execute() {
            Run(0, sharedStyleChangeSet.Size);    
        }

        public void Execute(int idx) {
            Run(idx, 1);
        }
        
        public void Execute(int start, int count) {
            Run(start, count);
        }

        private void Run(int start, int count) {

            ConvertedStyleList output = perThreadOutput.GetForThread(threadIndex);

            BufferList<StyleStatePair> buffer = new BufferList<StyleStatePair>(64, Allocator.TempJob);

            int end = start + count;

            for (int changeIndex = start; changeIndex < end; changeIndex++) {
                SharedStyleChangeEntry c = sharedStyleChangeSet.entries[changeIndex];

                buffer.EnsureCapacity((c.oldStyleCount * 4) + (c.newStyleCount * 4));

                int oldCount = 0;
                int newCount = 0;

                StyleId* pOldStyles = c.pOldStyles;
                StyleId* pNewStyles = c.pNewStyles;
                StyleState2 oldState = (StyleState2) c.oldState;
                StyleState2 newState = (StyleState2) c.newState;

                StyleState2 currentState = StyleState2.Active;

                for (ushort stateIdx = 0; stateIdx < 4; stateIdx++) {

                    if ((oldState & currentState) == 0) {
                        currentState = (StyleState2) ((int) currentState >> 1);
                        continue;
                    }

                    for (int styleIdx = 0; styleIdx < c.oldStyleCount; styleIdx++) {

                        StyleId styleId = pOldStyles[styleIdx];

                        if ((styleId.definedStates & currentState) != 0) {
                            buffer.array[buffer.size++] = new StyleStatePair(styleId, currentState);
                            oldCount++;
                        }

                    }

                    currentState = (StyleState2) ((int) currentState >> 1);
                }

                currentState = StyleState2.Active;

                for (int stateIdx = 0; stateIdx < 4; stateIdx++) {

                    if ((newState & currentState) == 0) {
                        currentState = (StyleState2) ((int) currentState >> 1);
                        continue;
                    }

                    for (int styleIdx = 0; styleIdx < c.newStyleCount; styleIdx++) {
                        StyleId styleId = pNewStyles[styleIdx];
                        if ((styleId.definedStates & currentState) != 0) {
                            buffer.array[buffer.size++] = new StyleStatePair(styleId, currentState);
                            newCount++;
                        }
                    }

                    currentState = (StyleState2) ((int) currentState >> 1);
                }

                // if totally equal -> don't add to output since there is no effect
                // this can happen when state was changed but no styles cared.
                // often the case for hover/focus/active 
                if (oldCount == newCount && UnsafeUtility.MemCmp(buffer.array, buffer.array + oldCount, newCount * sizeof(StyleStatePair)) == 0) {
                    continue;
                }

                output.Add(c.styleDataId, buffer.array, oldCount, newCount);

            }

            buffer.Dispose();

        }

    }

}