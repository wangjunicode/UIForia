using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct ResolveSelectorsJob2 : IJob {

        public DataList<StylePairUpdate>.Shared stylePairsList;
        public IntMap<List_SelectorTypeIndex> selectorIdMap;

        public void Execute() {

            for (int i = 0; i < stylePairsList.size; i++) {

                ref StylePairUpdate item = ref stylePairsList[i];

                for (int j = 0; j < item.stylePairCount; j++) {
                    StyleId styleId = item.styleStatePairs[j].styleId;
                    StyleState2 state = item.styleStatePairs[j].state;

                    if (!styleId.HasSelectorsInState(state)) {
                        continue;
                    }

                    int id = StyleDatabase.MakeSelectorKey(styleId, state);

                    selectorIdMap.TryGetValue(id, out List_SelectorTypeIndex selectorIndexList);

                    for (int selectorIndex = 0; selectorIndex < selectorIndexList.size; selectorIndex++) {
                        if (selectorIndexList.array[selectorIndex].usesIndices) {
                            // outputList_Indexed.Add(new SelectorIdElementId(item.elementId, selectorIndexList[selectorIndex].index));
                        }
                        else {
                            // outputList.Add(new SelectorIdElementId(item.elementId, selectorIndexList[selectorIndex].index));
                        }
                    }

                }

            }
        }

    }

    [BurstCompile]
    public struct ResolveSelectorsJob : IJob {

        [ReadOnly] public IntListMap<SelectorTypeIndex> table_SelectorIdMap;
        [ReadOnly] public DataList<StyleStateElementId>.Shared input_StyleStateElementIdList;

        [WriteOnly] public DataList<SelectorIdElementId>.Shared output_SelectorList;

        [WriteOnly] public DataList<SelectorIdElementId>.Shared output_SelectorList_Indexed;

        public void Execute() {

            // blast through all the style updates in the the input list and output two lists. 1 filled with selectors that use indexed filters and 1 with selectors that do not
            ResolveSelectors(input_StyleStateElementIdList, output_SelectorList, output_SelectorList_Indexed, table_SelectorIdMap);

        }

        private static void ResolveSelectors(
            [NoAlias] DataList<StyleStateElementId>.Shared sourceList,
            [NoAlias] DataList<SelectorIdElementId>.Shared outputList,
            [NoAlias] DataList<SelectorIdElementId>.Shared outputList_Indexed,
            [NoAlias] IntListMap<SelectorTypeIndex> selectorIdMap) {

            for (int i = 0; i < sourceList.size; i++) {

                ref StyleStateElementId item = ref sourceList.GetReference(i);

                if (!item.styleId.HasSelectorsInState(item.state)) {
                    continue;
                }

                selectorIdMap.TryGetValue(BitUtil.SetHighLowBits(item.styleId.index, (int) item.state), out TypedListHandle<SelectorTypeIndex> selectorIndexList);

                for (int selectorIndex = 0; selectorIndex < selectorIndexList.size; selectorIndex++) {
                    if (selectorIndexList[selectorIndex].usesIndices) {
                        outputList_Indexed.Add(new SelectorIdElementId(item.elementId, selectorIndexList[selectorIndex].index));
                    }
                    else {
                        outputList.Add(new SelectorIdElementId(item.elementId, selectorIndexList[selectorIndex].index));
                    }
                }

            }

        }

    }

}