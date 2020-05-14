using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia {

    public unsafe struct BuildSharedStyles : IVertigoParallelDeferred {

        [NativeSetThreadIndex] public int threadIndex;

        public BurstableStyleDatabase styleDatabase;
        public DataList<StylePairUpdate>.Shared styleUpdates;
        public PerThread<StyleRebuildContainer> perThread_RebuildContainer;

        public ParallelParams.Deferred defer { get; set; }

        public void Execute(int start, int count) {
            Run(start, start + count);
        }

        public void Execute() {
            Run(0, styleUpdates.size);
        }

        private void Run(int start, int end) {

            ref StyleRebuildContainer rebuildContainer = ref perThread_RebuildContainer.GetForThread(threadIndex);

            PropertyId* idbuffer = rebuildContainer.scratchIds;
            PropertyData* valueBuffer = rebuildContainer.scratchData;

            for (int buildIndex = start; buildIndex < end; buildIndex++) {

                IntBoolMap map = new IntBoolMap(new BitBuffer256().ptr, 256);

                ref StylePairUpdate styleStateUpdate = ref styleUpdates[buildIndex];

                StyleStatePair* styleStatePairs = styleStateUpdate.styleStatePairs;

                int stylePairCount = styleStateUpdate.stylePairCount;
                
                int propertyCount = 0;

                for (int styleIndex = 0; styleIndex < stylePairCount; styleIndex++) {
                    ref StyleStatePair stylePair = ref styleStatePairs[styleIndex];

                    int count = styleDatabase.GetStyleProperties(
                        stylePair.styleId,
                        stylePair.state,
                        out StaticPropertyId* keys,
                        out PropertyData* data,
                        out ModuleCondition conditionMask
                    );

                    for (int k = 0; k < count; k++) {

                        ref StaticPropertyId key = ref keys[k];

                        if ((key.conditionRequirement & conditionMask) != conditionMask) {
                            continue;
                        }

                        if (map.TrySetIndex(key.propertyId.index)) {
                            idbuffer[propertyCount] = key.propertyId;
                            valueBuffer[propertyCount] = data[k];
                            propertyCount++;
                        }

                    }

                }

                rebuildContainer.SetRebuildResult(styleStateUpdate.elementId, propertyCount, idbuffer, valueBuffer);

            }

        }


    }

}