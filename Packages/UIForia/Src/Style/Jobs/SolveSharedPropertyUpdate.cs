using System;
using System.Diagnostics;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct SolveSharedPropertyUpdate : IJob {

        public ElementMap rebuildBlocksMap;

        public DataList<StyleUsage>.Shared styleUsages;
        public DataList<StyleUsageQueryResult>.Shared styleUsageQueryResults;

        public DataList<StyleBlock> styleBlocks;
        public DataList<PropertyLocator> propertyDatabase;
        public DataList<TransitionLocator> transitionDatabase;

        public int totalPropertyTypeCount;

        [NativeDisableUnsafePtrRestriction] public PropertyUpdateSet* sharedUpdates; // this is our output, but it should be pre-allocated
        [NativeDisableUnsafePtrRestriction] public TransitionUpdateSet* transitionUpdates; // this is our output, but it should be pre-allocated

        [NativeDisableUnsafePtrRestriction] public LockedBumpAllocator* perFrameBumpAllocator;

        public void Execute() {
            Run();
        }

        private void Run() {

            TempList<StyleUsageInfo> styleUsageInfos = CreateStyleUsages(rebuildBlocksMap);

            int totalAppliedBlocks = CountTotalAppliedBlocks(styleUsageInfos);

            TempList<ElementStyleUsageRange> styleUsageRanges = GatherStyleUsageRanges(styleUsageInfos);

            // once we have style usage ranges, we can explode the applied blocks bitset into something we can iterate 
            // since we need to resolve the block indices into something that lives in the style database, this first pass
            // will just compute the index in the style database without de-referencing that memory to do a lookup
            TempList<BlockIndex> blockIndexList = CreateBlockIndexList(styleUsageRanges, styleUsageInfos, totalAppliedBlocks);

            styleUsageInfos.Dispose();

            // build our block usages
            TempList<BlockUsage> blockUsageList = CreateBlockUsages(styleUsageRanges, blockIndexList, totalAppliedBlocks);

            blockIndexList.Dispose();

            // sort block ranges by precedence 
            SortBlocksByPrecedence(styleUsageRanges, blockUsageList);

            DataList<ElementPropertyUsage> propertyUsages = GatherPropertyUsages(styleUsageRanges, blockUsageList, rebuildBlocksMap.PopCount());

            AllocateSharedUpdateLists(perFrameBumpAllocator, sharedUpdates, propertyUsages, totalPropertyTypeCount);

            PopulateSharedUpdateLists(sharedUpdates->updateLists, propertyUsages);

            // todo -- start move this to another job to be done in parallel 

            DataList<ElementTransitionUsage> transitionUsages = GatherTransitionUsages(styleUsageRanges, blockUsageList);

            AllocateTransitionUpdateLists(perFrameBumpAllocator, transitionUpdates, transitionUsages, totalPropertyTypeCount);

            PopulateTransitionUpdateLists(transitionUpdates->updateLists, transitionUsages);

            transitionUsages.Dispose();

            // todo -- end move this

            propertyUsages.Dispose();
            blockUsageList.Dispose();
            styleUsageRanges.Dispose();

        }

        private TempList<StyleUsageInfo> CreateStyleUsages(ElementMap mergedRebuild) {

            int styleUsageCount = CountStyleUsages(mergedRebuild);

            TempList<StyleUsageInfo> styleUsageInfos = TypedUnsafe.MallocUnsizedTempList<StyleUsageInfo>(styleUsageCount, Allocator.TempJob);
            
            for (int i = 0; i < styleUsages.size; i++) {

                if (!mergedRebuild.Get(styleUsages[i].elementId)) {
                    continue;
                }

                // gather data locally for processing 
                styleUsageInfos.array[styleUsageInfos.size++] = new StyleUsageInfo() {
                    elementId = styleUsages[i].elementId,
                    indexInSource = styleUsages[i].indexInStyleList,
                    appliedBlocks = styleUsageQueryResults[i].appliedBlocks,
                    styleId = styleUsageQueryResults[i].styleId
                };

            }

            // we want to sort the style usages by element so we have all of the element data sequentially 
            NativeSortExtension.Sort(styleUsageInfos.array, styleUsageCount);

            return styleUsageInfos;
        }

        private static int CountTotalAppliedBlocks(TempList<StyleUsageInfo> styleUsageInfos) {
            int retn = 0;
            for (int i = 0; i < styleUsageInfos.size; i++) {
                retn += styleUsageInfos[i].appliedBlocks.PopCount();
            }

            return retn;
        }

        private static TempList<ElementStyleUsageRange> GatherStyleUsageRanges(in TempList<StyleUsageInfo> styleUsages) {
            RangeInt range = new RangeInt();

            ElementId previousElementId = default;

            int usageRangeCount = 0;

            // todo -- make sure we aren't missing an index at the end, I think previousElementId = default handles it though

            if (styleUsages.size == 0) {
                return default;
            }

            for (int i = 0; i < styleUsages.size; i++) {
                StyleUsageInfo rebuildInfo = styleUsages.array[i];
                if (rebuildInfo.elementId != previousElementId) {
                    usageRangeCount++;
                    previousElementId = rebuildInfo.elementId;
                }
            }

            TempList<ElementStyleUsageRange> styleUsageRanges = TypedUnsafe.MallocUnsizedTempList<ElementStyleUsageRange>(usageRangeCount, Allocator.TempJob);
            previousElementId = styleUsages[0].elementId;
            range.length = 1;

            for (int i = 1; i < styleUsages.size; i++) {
                StyleUsageInfo rebuildInfo = styleUsages.array[i];

                if (rebuildInfo.elementId != previousElementId) {

                    styleUsageRanges.array[styleUsageRanges.size++] = new ElementStyleUsageRange() {
                        elementId = previousElementId,
                        styleUsageRange = range,
                        blockRange = default // set later on 
                    };

                    previousElementId = rebuildInfo.elementId;
                    range.start = i;
                    range.length = 0;
                }

                range.length++;

            }

            // handle last element 
            styleUsageRanges.array[styleUsageRanges.size++] = new ElementStyleUsageRange() {
                elementId = previousElementId,
                styleUsageRange = range
            };

            return styleUsageRanges;
        }

        private struct BlockIndex {

            public int index;
            public int sourceIndex;

        }

        private static TempList<BlockIndex> CreateBlockIndexList(in TempList<ElementStyleUsageRange> styleUsageRanges, in TempList<StyleUsageInfo> styleUsages, int totalAppliedBlocks) {

            TempList<BlockIndex> blockIndexList = TypedUnsafe.MallocUnsizedTempList<BlockIndex>(totalAppliedBlocks, Allocator.Temp);

            for (int i = 0; i < styleUsageRanges.size; i++) {
                RangeInt usageRange = styleUsageRanges.array[i].styleUsageRange;

                int startSize = blockIndexList.size;

                for (int r = usageRange.start; r < usageRange.end; r++) {
                    StyleUsageInfo styleUsage = styleUsages.array[r];
                    long appliedBlocks = (long) styleUsage.appliedBlocks;
                    int blockOffset = styleUsage.styleId.blockOffset;

                    while (appliedBlocks != 0) {
                        long t = appliedBlocks & -appliedBlocks;
                        int localBlockId = math.tzcnt((ulong) appliedBlocks);
                        appliedBlocks ^= t;
                        blockIndexList.array[blockIndexList.size++] = new BlockIndex() {
                            index = blockOffset + localBlockId,
                            sourceIndex = styleUsage.indexInSource,
                        };
                    }

                }

                int totalBlockCount = blockIndexList.size - startSize;

                styleUsageRanges.array[i].blockRange = new RangeInt(startSize, totalBlockCount);

            }

            return blockIndexList;
        }

        private int CountStyleUsages(ElementMap mergedRebuild) {
            int styleUsageCount = 0;

            for (int i = 0; i < styleUsages.size; i++) {
                if (mergedRebuild.Get(styleUsages[i].elementId)) {
                    styleUsageCount++;
                }
            }

            return styleUsageCount;
        }

        private void PopulateSharedUpdateLists(CheckedArray<PropertyUpdate>* sharedUpdates, DataList<ElementPropertyUsage> propertyUsages) {

            // could explore sorting by property id first

            int * counts = stackalloc int[totalPropertyTypeCount];
            
            for (int i = 0; i < propertyUsages.size; i++) {

                ref ElementPropertyUsage propertyUsage = ref propertyUsages[i];

                ref CheckedArray<PropertyUpdate> updateList = ref sharedUpdates[propertyUsage.propertyIndex];
                int idx = counts[propertyUsage.propertyIndex];
                updateList[idx] = new PropertyUpdate() {
                    variableNameId = propertyUsage.variableNameId,
                    dbValueLocation = propertyUsage.dbValueLocation,
                    elementId = propertyUsage.elementId
                };
                
                counts[propertyUsage.propertyIndex]++;
            }

        }

        private static void AllocateSharedUpdateLists(LockedBumpAllocator* bumpAllocator, PropertyUpdateSet* sharedUpdates, DataList<ElementPropertyUsage> propertyUsages, int totalPropertyTypeCount) {

            sharedUpdates->updateLists = bumpAllocator->Allocate<CheckedArray<PropertyUpdate>>(totalPropertyTypeCount);
            sharedUpdates->propertyUpdates = bumpAllocator->Allocate<PropertyUpdate>(propertyUsages.size);

            PropertyUpdate* allocPtr = sharedUpdates->propertyUpdates;

            TempList<int> propertyCountByType = TypedUnsafe.MallocClearedTempList<int>(totalPropertyTypeCount, Allocator.Temp);
            propertyCountByType.size = totalPropertyTypeCount;

            // for each property usage, track how many slots we need
            for (int i = 0; i < propertyUsages.size; i++) {
                propertyCountByType[propertyUsages[i].propertyIndex]++;
            }

            // now we can allocate exact sized arrays for all of our shared style updates
            for (int i = 0; i < totalPropertyTypeCount; i++) {
                int count = propertyCountByType[i];
                sharedUpdates->updateLists[i] = new CheckedArray<PropertyUpdate>(allocPtr, count);
                allocPtr += count;
            }

            propertyCountByType.Dispose();

        }

        private static void SortBlocksByPrecedence(in TempList<ElementStyleUsageRange> styleUsageRanges, in TempList<BlockUsage> blockUsageList) {
            StyleBlockComp styleBlockCmp = new StyleBlockComp();
            for (int i = 0; i < styleUsageRanges.size; i++) {
                RangeInt blockRange = styleUsageRanges.array[i].blockRange;
                // todo -- if size is small do a manual insertion sort instead of native sort
                NativeSortExtension.Sort(blockUsageList.array + blockRange.start, blockRange.length, styleBlockCmp);
            }
        }

        private TempList<BlockUsage> CreateBlockUsages(TempList<ElementStyleUsageRange> styleUsageRanges, TempList<BlockIndex> blockIndexList, int totalAppliedBlocks) {

            TempList<BlockUsage> blockUsageList = TypedUnsafe.MallocUnsizedTempList<BlockUsage>(totalAppliedBlocks, Allocator.TempJob);

            for (int i = 0; i < styleUsageRanges.size; i++) {
                RangeInt blockRange = styleUsageRanges.array[i].blockRange;

                for (int b = blockRange.start; b < blockRange.end; b++) {

                    StyleBlock block = styleBlocks[blockIndexList.array[b].index];

                    BlockUsageSortKey sortKey = block.sortKey;

                    sortKey.indexInSource = (byte)blockIndexList.array[b].sourceIndex;
                    
                    // todo -- if style usage was from a selector, that data needs to be applied here 

                    blockUsageList.array[blockUsageList.size++] = new BlockUsage() {
                        sortKey = sortKey,
                        // styleId = blockIndexList.array[b].styleId,
                        propertyStart = block.propertyStart,
                        propertyCount = block.propertyCount,
                        transitionCount = block.transitionCount,
                        transitionStart = block.transitionStart
                    };

                }

            }

            return blockUsageList;
        }

        private DataList<ElementTransitionUsage> GatherTransitionUsages(in TempList<ElementStyleUsageRange> styleUsageRanges, in TempList<BlockUsage> blockUsageList) {
            // make space to hold a property map
            int longsPerPropertyMap = LongBoolMap.GetMapSize(totalPropertyTypeCount);
            ulong* buffer = stackalloc ulong[longsPerPropertyMap];
            PropertyMap propertyMap = new PropertyMap(buffer, longsPerPropertyMap);

            int maxTransitionCount = 0;
            for (int i = 0; i < blockUsageList.size; i++) {
                maxTransitionCount += blockUsageList[i].transitionCount;
            }

            DataList<ElementTransitionUsage> transitionUsages = new DataList<ElementTransitionUsage>(maxTransitionCount, Allocator.TempJob);

            for (int i = 0; i < styleUsageRanges.size; i++) {
                ElementId elementId = styleUsageRanges.array[i].elementId;
                RangeInt blockRange = styleUsageRanges.array[i].blockRange;

                UnsafeUtility.MemClear(propertyMap.map, sizeof(long) * propertyMap.size);

                for (int b = blockRange.start; b < blockRange.end; b++) {

                    int start = blockUsageList.array[b].transitionStart;
                    int count = blockUsageList.array[b].transitionCount;
                    int end = start + count;

                    for (int p = start; p < end; p++) {
                        int index = transitionDatabase[p].propertyIndex;

                        if (!propertyMap.TrySetIndex(index)) {
                            continue;
                        }

                        transitionUsages.AddUnchecked(new ElementTransitionUsage() {
                            elementId = elementId,
                            propertyIndex = transitionDatabase[p].propertyIndex,
                            transitionIndex = transitionDatabase[p].transitionIndex
                        });

                    }

                }
            }

            return transitionUsages;

        }

        private static void AllocateTransitionUpdateLists(LockedBumpAllocator* bumpAllocator, TransitionUpdateSet* transitionUpdates, DataList<ElementTransitionUsage> transitionUsages, int totalPropertyTypeCount) {

            transitionUpdates->updateLists = bumpAllocator->Allocate<TransitionUpdateList>(totalPropertyTypeCount);
            transitionUpdates->pendingTransitionBuffer = bumpAllocator->Allocate<PendingTransition>(transitionUsages.size);

            PendingTransition* allocPtr = transitionUpdates->pendingTransitionBuffer;

            TempList<int> propertyCountByType = TypedUnsafe.MallocClearedTempList<int>(totalPropertyTypeCount, Allocator.Temp);
            propertyCountByType.size = totalPropertyTypeCount;

            // for each property usage, track how many slots we need
            for (int i = 0; i < transitionUsages.size; i++) {
                propertyCountByType[transitionUsages[i].propertyIndex]++;
            }

            // now we can allocate exact sized arrays for all of our shared style updates
            for (int i = 0; i < totalPropertyTypeCount; i++) {
                int count = propertyCountByType[i];
                transitionUpdates->updateLists[i].size = 0; // size will be set when adding the values
                transitionUpdates->updateLists[i].array = allocPtr;
                allocPtr += count;
            }

            propertyCountByType.Dispose();

        }

        private static void PopulateTransitionUpdateLists(TransitionUpdateList* transitionUpdates, DataList<ElementTransitionUsage> transitionUsages) {

            // could explore sorting by property id first for better locality 

            for (int i = 0; i < transitionUsages.size; i++) {

                ref ElementTransitionUsage transitionUsage = ref transitionUsages[i];

                ref TransitionUpdateList updateList = ref transitionUpdates[transitionUsage.propertyIndex];

                updateList.array[updateList.size++] = new PendingTransition() {
                    elementId = transitionUsage.elementId,
                    transitionId = transitionUsage.transitionIndex
                };
            }

        }

        private DataList<ElementPropertyUsage> GatherPropertyUsages(in TempList<ElementStyleUsageRange> styleUsageRanges, in TempList<BlockUsage> blockUsageList, int rebuiltElementCount) {

            // make space to hold a property map
            int longsPerPropertyMap = LongBoolMap.GetMapSize(totalPropertyTypeCount);
            ulong* buffer = stackalloc ulong[longsPerPropertyMap];
            PropertyMap propertyMap = new PropertyMap(buffer, longsPerPropertyMap);

            int maxPropertyCount = 0;
            for (int i = 0; i < blockUsageList.size; i++) {
                maxPropertyCount += blockUsageList[i].propertyCount;
            }

            DataList<ElementPropertyUsage> propertyUsages = new DataList<ElementPropertyUsage>(maxPropertyCount, Allocator.TempJob);

            for (int i = 0; i < styleUsageRanges.size; i++) {
                ElementId elementId = styleUsageRanges.array[i].elementId;
                RangeInt blockRange = styleUsageRanges.array[i].blockRange;

                UnsafeUtility.MemClear(propertyMap.map, sizeof(long) * propertyMap.size);

                for (int b = blockRange.start; b < blockRange.end; b++) {

                    int start = blockUsageList.array[b].propertyStart;
                    int count = blockUsageList.array[b].propertyCount;
                    int end = start + count;

                    for (int p = start; p < end; p++) {
                        int index = propertyDatabase[p].propertyTypeIndex;

                        if (!propertyMap.TrySetIndex(index)) {
                            continue;
                        }

                        propertyUsages.AddUnchecked(new ElementPropertyUsage() {
                            elementId = elementId,
                            propertyIndex = propertyDatabase[p].propertyTypeIndex,
                            variableNameId = propertyDatabase[p].variableNameId,
                            dbValueLocation = propertyDatabase[p].indexInPropertyValueTable
                        });

                    }

                }

            }

            return propertyUsages;
        }

        private struct StyleUsageInfo : IComparable<StyleUsageInfo> {

            public ElementId elementId;
            public BitSet appliedBlocks;
            public StyleId styleId;
            public int indexInSource;

            public int CompareTo(StyleUsageInfo other) {
                return elementId.index - other.elementId.index;
            }

        }

        private struct ElementStyleUsageRange {

            public ElementId elementId;
            public RangeInt styleUsageRange;
            public RangeInt blockRange;

        }

        private struct ElementTransitionUsage {

            public ElementId elementId;
            public ushort propertyIndex;
            public int transitionIndex;

        }

        [DebuggerDisplay("{DebugDisplay()}")]
        private struct ElementPropertyUsage {

            public ElementId elementId;
            public int dbValueLocation;
            public ushort propertyIndex;
            public ushort variableNameId;

            public string DebugDisplay() {
                PropertyId propertyId = (PropertyId) propertyIndex;
                return propertyId + ", " + elementId;
            }
        }

    }

}