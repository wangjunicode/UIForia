using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct BuildSharedStyles : IJobParallelForBatch, IJob {

        [NativeSetThreadIndex] public int threadIndex;

        [NativeDisableUnsafePtrRestriction]
        public byte* staticStyleBuffer;
        public UnmanagedList<ModuleCondition> table_ModuleConditions;
        public UnmanagedList<ConvertedStyleId> convertedStyleList;
        public UnmanagedList<StaticStyleInfo> table_StyleInfo;
        public PerThread<StyleRebuildResultList> perThread_RebuiltResult;

        public void Execute() {
            Run(0, convertedStyleList.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, count);
        }

        private void Run(int startIndex, int itemCount) {

            ref StyleRebuildResultList resultList = ref perThread_RebuiltResult.GetForThread(threadIndex);
            
            int end = startIndex + itemCount;

            byte* buffer = (byte*)UnsafeUtility.Malloc((sizeof(PropertyId) + sizeof(PropertyData)) * VertigoStyleSystem.k_MaxStyleProperties, 4, Allocator.TempJob);

            PropertyId* idbuffer = (PropertyId*) buffer;
            PropertyData* valueBuffer = (PropertyData*) (idbuffer + VertigoStyleSystem.k_MaxStyleProperties);
            
            for ( int buildIndex = startIndex; buildIndex < end; buildIndex++) {

                int size = 0;
                
                IntBoolMap map = new IntBoolMap(new BitBuffer256().ptr, 256);

                ConvertedStyleId converted = convertedStyleList[buildIndex];

                StyleStatePair* stylePairs = converted.pNewStyles;

                for (int styleIndex = 0; styleIndex < converted.newStyleCount; styleIndex++) {
                    ref StyleStatePair stylePair = ref stylePairs[styleIndex];

                    // this might be looked up multiple times for same style id but in that case
                    // its almost certainly in cache so don't worry about it
                    StaticStyleInfo staticStyle = table_StyleInfo[stylePair.styleId.index];
                    ModuleCondition conditionMask = staticStyle.conditionMask; //table_ModuleConditions[stylePair.styleId.index];
                    
                    int offset = 0;
                    int count = 0;

                    switch (stylePair.state) {
                        case StyleState2.Normal:
                            offset = staticStyle.normalOffset;
                            count = staticStyle.normalCount;
                            break;

                        case StyleState2.Hover:
                            offset = staticStyle.hoverOffset;
                            count = staticStyle.hoverCount;
                            break;

                        case StyleState2.Focused:
                            offset = staticStyle.focusOffset;
                            count = staticStyle.focusCount;
                            break;

                        case StyleState2.Active:
                            offset = staticStyle.activeOffset;
                            count = staticStyle.activeCount;
                            break;
                    }

                    // could also used paged list for property data but would then need compute page index and lookup that pointer
                    // since we're going for speed > memory waste here I'm not resorting to that yet.
                    StaticPropertyId* keys = (StaticPropertyId*) (staticStyleBuffer + staticStyle.propertyOffsetInBytes + offset);
                    PropertyData* data = (PropertyData*) (keys + staticStyle.totalPropertyCount);

                    for (int k = 0; k < count; k++) {

                        ref StaticPropertyId key = ref keys[k];

                        // todo -- might be backwards, check it out
                        // if ((key.conditionRequirement & conditionMask) == key.conditionRequirement) {
                        //     continue;
                        // }

                        if (map.TrySetIndex(key.propertyId.index)) {
                            idbuffer[size] = key.propertyId;
                            valueBuffer[size] = data[k];
                            size++;
                            // propertyIdBuffer.array[propertyIdBuffer.size++] = key.propertyId;
                            // propertyDataBuffer.array[propertyDataBuffer.size++] = data[k];
                        }

                    }

                }

                resultList.Add(converted.styleSetId, size, idbuffer, valueBuffer);

            }

            UnsafeUtility.Free(buffer, Allocator.TempJob);

        }

    }

}