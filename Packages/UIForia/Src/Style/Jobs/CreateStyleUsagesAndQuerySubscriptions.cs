using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct CreateStyleUsagesAndQuerySubscriptions : IJob {

        public ElementMap rebuildStylesMap;
        public ElementMap deadOrDisabledMap;
        public LongBoolMap newSubscriberMap;

        public DataList<StyleUsage>.Shared styleUsages; // persistent
        public DataList<int>.Shared styleUsageToIndex; // persistent
        public DataList<StyleUsageQueryResult>.Shared styleUsageQueryResults; // persistent
        public DataList<StyleId> styleIdTable;
        public CheckedArray<StyleInfo> styleInfoTable;

        public CheckedArray<StyleDesc> styles;
        public CheckedArray<QueryPair> queryPairs;
        public DataList<int>.Shared styleUsageIdFreeList;
        public int styleIndexOffset;

        // this a per-frame list, only valid until subscriptions finish
        public DataList<QuerySubscription>.Shared newQuerySubscriptions;

        public HeapAllocated<int> styleUsageIdGenerator;

        private void Solve() {

            // I assume selectors have already run at this point 

            ulong* buffer = TypedUnsafe.MallocCleared<ulong>(rebuildStylesMap.longCount, Allocator.Temp);

            ElementMap map = new ElementMap(buffer, rebuildStylesMap.longCount);
            map.Combine(rebuildStylesMap);
            map.Combine(deadOrDisabledMap);

            // swap remove any rebuilt or dead elements' style usages
            for (int i = 0; i < styleUsages.size; i++) {
                // remove if rebuilt or disabled 
                if (map.Get(styleUsages[i].elementId)) {
                    
                    styleUsageIdFreeList.Add(styleUsages[i].id);
                    
                    int swapIndex = styleUsages.size - 1;
                    styleUsageQueryResults[i] = styleUsageQueryResults[swapIndex];
                    styleUsages[i] = styleUsages[swapIndex];
                    styleUsageQueryResults.size--;
                    styleUsages.size--;
                    i--;
                }
            }

            TypedUnsafe.Dispose(buffer, Allocator.Temp);

            // set prev to current & reset current
            for (int i = 0; i < styleUsageQueryResults.size; i++) {
                styleUsageQueryResults[i].prevResults = styleUsageQueryResults[i].currResults;
                styleUsageQueryResults[i].currResults = default;
            }

            TempList<ElementId> toRebuildList = rebuildStylesMap.ToTempList(Allocator.Temp);

            int startIndex = styleUsageQueryResults.size;

            // for every element we are rebuilding, get its style list, for each style we add a usage and a corresponding usageQueryResult
            for (int i = 0; i < toRebuildList.size; i++) {

                // note this ids are index only, generation data was lost in the map 
                ElementId elementId = toRebuildList.array[i];

                SmallListSlice slice = styleInfoTable[elementId.index].listSlice; // slice is a range that points into the styleId table
                int styleCount = slice.length;
                int start = slice.start;
                styleUsages.EnsureAdditionalCapacity(styleCount);               // style usage and styleUsageQueryResults are always 1 to 1 on the same index
                styleUsageQueryResults.EnsureAdditionalCapacity(styleCount);

                for (int s = 0; s < styleCount; s++) {

                    StyleId styleId = styleIdTable[start + s];

                    // assume styles are already validated, belong to this app, etc,  

                    StyleUsage styleUsage = AllocateElementStyleId(elementId, s);

                    styleUsages.AddUnchecked(styleUsage);

                    styleUsageQueryResults.AddUnchecked(new StyleUsageQueryResult() {
                        styleId = styleId,
                        appliedBlocks = default,
                        currResults = default,
                        prevResults = default
                    });
                }
            }

            toRebuildList.Dispose();
            
            newQuerySubscriptions.size = 0;

            // for all newly added styles (because elements are new or because we rebuild the element's style)
            // we need to sign up for any querys we want results from. We do this by looping through the style's
            // list of query ids (known at compile time, lives on the StyleDesc), then dump the 
            for (int i = startIndex; i < styleUsages.size; i++) {

                StyleUsage styleUsage = styleUsages[i];

                // todo -- try to remove dependent read
                int styleIndex = (int) styleUsageQueryResults[i].styleId.id;
                StyleDesc styleDesc = styles[styleIndex - styleIndexOffset];

                int queryCount = styleDesc.queryLocationInfo.queryCount;
                int queryOffset = styleDesc.queryLocationInfo.queryOffset;

                newQuerySubscriptions.EnsureAdditionalCapacity(queryCount);

                for (int q = 0; q < queryCount; q++) {
                    // could use a map to indicate which queries have new subscribers 

                    QueryPair queryPair = queryPairs[queryOffset + q];

                    newSubscriberMap.Set(queryPair.queryId.id); // map lets me skip searches later on in the query subscription checks

                    newQuerySubscriptions.AddUnchecked(new QuerySubscription() {
                        targetConditionIndex = queryPair.bitIdx,
                        queryId = queryPair.queryId,
                        styleUsage = styleUsage,
                    });
                }
            }

            // update index list
            styleUsageToIndex.SetSize(styleUsageIdGenerator.Get());

            // todo -- this seems defunct, remove it?
            for (int i = 0; i < styleUsageToIndex.size; i++) {
                styleUsageToIndex[i] = -1;
            }

            for (int i = 0; i < styleUsages.size; i++) {
                int usageId = styleUsages[i].id;
                styleUsageToIndex[usageId] = i;
            }

            // sorted by type so then each query table can binary search for itself
            NativeSortExtension.Sort(newQuerySubscriptions.GetArrayPointer(), newQuerySubscriptions.size);

        }

        private StyleUsage AllocateElementStyleId(ElementId elementId, int styleIndex) {
            int id;
            if (styleUsageIdFreeList.size > 0) {
                id = styleUsageIdFreeList[styleUsageIdFreeList.size - 1];
                styleUsageIdFreeList.size--;
            }
            else {
                id = styleUsageIdGenerator.Get();
                styleUsageIdGenerator.Set(id + 1);
            }

            return new StyleUsage(id, styleIndex, elementId);
        }

        public void Execute() {
            Solve();
        }

    }

}