using System;
using System.Collections.Generic;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    // gathers selector info when using a filter that can be indexed, rather than doing a 
    // hierarchy search for descendents. The only case where a query cannot be built off
    // of an index is when all filters are negative or there is only a where filter fn. 
    public unsafe struct GatherIndexedSelectorRunInfo : IJob, IJobParallelForBatch {

        public DataList<SelectorIdElementId>.Shared output_RemoveSelectorEffectList;
        public PerThread<SelectorRunInfoList> output_SelectorRunInfoList;
        
        public DataList<ActiveSelector>.Shared table_ActiveSelectors;
        public DataList<SelectorQuery>.Shared table_SelectorQueries;
        public DataList<ElementTraversalInfo>.Shared table_TraversalInfo;

        public ElementIndex table_ElementIndex;

        [NativeSetThreadIndex] public int threadIndex;

        public void Execute() {
            Run(0, table_ActiveSelectors.size);
        }

        public void Execute(int start, int count) {
            Run(start, count);
        }

        private void Run(int start, int count) {

            int end = start + count;

            ref SelectorRunInfoList output = ref output_SelectorRunInfoList.GetForThread(threadIndex);

            DataList<ResolvedSelectorFilter> filterBuffer = new DataList<ResolvedSelectorFilter>(16, Allocator.Temp);
            DataList<int> candidateBuffer = new DataList<int>(64, Allocator.Temp);

            // todo -- still nothing here doing the 'i dont need to update' check, can add that later if needed.

            for (int index = start; index < end; index++) {

                ActiveSelector selector = table_ActiveSelectors[index];

                ref SelectorQuery query = ref table_SelectorQueries[selector.selectorIndex];

                bool hasPossibleResults = GatherFilterData(query, ref filterBuffer);

                if (!hasPossibleResults) {
                    output_RemoveSelectorEffectList.Add(new SelectorIdElementId(selector.elementId, selector.selectorIndex));
                    continue;
                }

                // todo -- replace with bubble sort
                NativeSortExtension.Sort(filterBuffer.GetArrayPointer(), filterBuffer.size, new TableSizeComparer());

                for (int i = 0; i < filterBuffer.size; i++) {
                    if ((filterBuffer[i].filterType & SelectorFilterType.Inverted) == 0) {

                        ResolvedSelectorFilter tmp = filterBuffer[i];
                        filterBuffer[i] = filterBuffer[0];
                        filterBuffer[0] = tmp;

                        break;
                    }
                }

                BuildCandidateList(selector.elementId, query.source, filterBuffer[0].indexTable, filterBuffer[0].indexTableSize, ref candidateBuffer);
                
                output.Add(
                    selector.elementId,
                    selector.selectorIndex,
                    candidateBuffer.GetArrayPointer(),
                    candidateBuffer.size,
                    filterBuffer.GetArrayPointer() + 1, // built the initial list off of the filter buffer so we dont want to include that in indices to check against since we've effectively done it already
                    filterBuffer.size - 1,
                    query.whereFilterId
                );

            }

            candidateBuffer.Dispose();
            filterBuffer.Dispose();

        }

        private bool GatherFilterData(in SelectorQuery query, ref DataList<ResolvedSelectorFilter> buffer) {

            buffer.SetSize(0);

            buffer.EnsureCapacity(query.filterCount);

            for (int i = 0; i < query.filterCount; i++) {

                ref SelectorFilter filter = ref query.filters[i];
                bool isPositive = (filter.filterType & SelectorFilterType.Inverted) != 0;
                SelectorFilterType filterType = filter.filterType & ~SelectorFilterType.Inverted;

                TypedListHandle<ElementId> list = default;

                switch (filterType) {

                    case SelectorFilterType.ElementsWithTag: {
                        table_ElementIndex.tagIndex.TryGetValue(filter.key, out list);
                        break;
                    }

                    case SelectorFilterType.ElementsWithState: {
                        table_ElementIndex.stateIndex.TryGetValue(filter.key, out list);
                        break;
                    }

                    case SelectorFilterType.ElementsWithStyle: {
                        table_ElementIndex.styleIndex.TryGetValue(filter.key, out list);
                        break;
                    }

                    case SelectorFilterType.ElementsWithAttribute:
                    case SelectorFilterType.ElementsWithAttribute_ValueContains:
                    case SelectorFilterType.ElementsWithAttribute_ValueStartsWith:
                    case SelectorFilterType.ElementsWithAttribute_ValueEndsWith:
                    case SelectorFilterType.ElementsWithAttribute_ValueEquals: {
                        table_ElementIndex.attributeIndex.TryGetValue(filter.key, out list);
                        break;
                    }

                }

                if (isPositive && list.size == 0) return false;

                buffer.AddUnchecked(new ResolvedSelectorFilter() {
                    filterType = filter.filterType,
                    indexTableSize = list.size,
                    indexTable = (int*) list.array,
                    key = filter.key,
                    value = filter.value
                });

            }

            return true;
        }

        // todo -- this needs to check for dead & disabled elements and reject them
        private void BuildCandidateList(ElementId sourceElementId, SelectionSource querySource, int* candidates, int candidateCount, ref DataList<int> candidateBuffer) {

            ElementTraversalInfo sourceTraversalInfo = table_TraversalInfo[sourceElementId.index];
            candidateBuffer.EnsureCapacity(candidateCount);
            candidateBuffer.SetSize(0);

            switch (querySource) {

                case SelectionSource.Children: {
                    for (int i = 0; i < candidateCount; i++) {
                        int candidate = candidates[i];
                        if (table_TraversalInfo[candidate].IsChildOf(sourceTraversalInfo)) {
                            candidateBuffer.AddUnchecked(candidate);
                        }
                    }

                    break;
                }

                case SelectionSource.Descendents: {

                    for (int i = 0; i < candidateCount; i++) {
                        int candidate = candidates[i];
                        if (table_TraversalInfo[candidate].IsTemplateDescendentOf(sourceTraversalInfo)) {
                            candidateBuffer.AddUnchecked(candidate);
                        }
                    }

                    break;
                }

                case SelectionSource.UnscopedDescendents: {

                    for (int i = 0; i < candidateCount; i++) {
                        int candidate = candidates[i];

                        if (table_TraversalInfo[candidate].IsDescendentOf(sourceTraversalInfo)) {
                            candidateBuffer.AddUnchecked(candidate);
                        }
                    }

                    break;
                }

                case SelectionSource.LexicalChildren: {
                    for (int i = 0; i < candidateCount; i++) {
                        int candidate = candidates[i];

                        if (table_TraversalInfo[candidate].IsTemplateChildOf(sourceTraversalInfo)) {
                            candidateBuffer.AddUnchecked(candidate);
                        }
                    }

                    break;
                }

                case SelectionSource.LexicalDescendents: {
                    throw new NotImplementedException();
                    break;
                }

            }
        }

        private struct TableSizeComparer : IComparer<ResolvedSelectorFilter> {

            public int Compare(ResolvedSelectorFilter x, ResolvedSelectorFilter y) {
                if (x.indexTableSize == y.indexTableSize) {
                    return x.filterType - y.filterType;
                }

                return x.indexTableSize - y.indexTableSize;
            }

        }

    }

}