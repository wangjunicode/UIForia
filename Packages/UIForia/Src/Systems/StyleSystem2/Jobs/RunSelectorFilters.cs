using System;
using UIForia.Util.Unsafe;
using Unity.Jobs;

namespace UIForia {

    public unsafe struct RunSelectorFilters : IJob, IJobParallelForDeferBatched {

        public SelectorRunInfoList input_SelectorRunInfoList;

        public IntListMap<AttributeInfo> map_ElementAttributes;
        public DataList<int> table_ElementTagTable;
        public DataList<SelectorIdElementId>.Shared inout_RemoveSelectorEffectList;

        public DataList<WhereFilterCandidate> output_WhereFilterCandidates;
        public DataList<SelectorMatch> output_Matches;
        public BurstableAttributeDatabase attributeDatabase;

        public void Execute(int start, int end) {
            throw new NotImplementedException();
        }

        public void Execute() {

            PagedList<SelectorRunInfo> runInfo = input_SelectorRunInfoList.GetRunInfo();

            for (int pageIndex = 0; pageIndex < runInfo.pageCount; pageIndex++) {

                PagedListPage<SelectorRunInfo> page = runInfo.GetPage(pageIndex);

                for (int i = 0; i < page.size; i++) {

                    ref SelectorRunInfo info = ref page.array[i];

                    for (int filterIndex = 0; filterIndex < info.filterCount; filterIndex++) {

                        ref ResolvedSelectorFilter filter = ref info.filters[filterIndex];

                        bool inverted = (filter.filterType & SelectorFilterType.Inverted) != 0;
                        SelectorFilterType filterType = filter.filterType & ~SelectorFilterType.Inverted;

                        switch (filterType) {

                            case SelectorFilterType.ElementsWithTag:
                                RunTagFilter(filter, inverted, info.candidates, ref info.candidateCount);
                                break;

                            case SelectorFilterType.ElementsWithState:
                                RunStateFilter(filter, inverted, info.candidates, ref info.candidateCount);
                                break;

                            case SelectorFilterType.ElementsWithStyle:
                                RunStyleFilter(filter, inverted, info.candidates, ref info.candidateCount);
                                break;

                            case SelectorFilterType.ElementsWithAttribute:
                                RunAttributeFilter(filter, inverted, info.candidates, ref info.candidateCount);
                                break;

                            case SelectorFilterType.ElementsWithAttribute_ValueContains:
                                RunAttrCompareFilter(filter, AttributeOperator.Contains, inverted, info.candidates, ref info.candidateCount);
                                break;

                            case SelectorFilterType.ElementsWithAttribute_ValueEquals:
                                RunAttrCompareFilter(filter, AttributeOperator.Equal, inverted, info.candidates, ref info.candidateCount);
                                break;

                            case SelectorFilterType.ElementsWithAttribute_ValueStartsWith:
                                RunAttrCompareFilter(filter, AttributeOperator.StartsWith, inverted, info.candidates, ref info.candidateCount);
                                break;

                            case SelectorFilterType.ElementsWithAttribute_ValueEndsWith:
                                RunAttrCompareFilter(filter, AttributeOperator.EndsWith, inverted, info.candidates, ref info.candidateCount);
                                break;

                        }

                        if (info.candidateCount == 0) {
                            break;
                        }

                    }

                    if (info.candidateCount == 0) {
                        // no match add to removal list to mark any previous matches as no longer matched
                        inout_RemoveSelectorEffectList.Add(new SelectorIdElementId(info.hostId, info.selectorId));
                    }
                    else if (info.whereFilterId != -1) {
                        for (int j = 0; j < info.candidateCount; j++) {
                            output_WhereFilterCandidates.Add(new WhereFilterCandidate() {
                                candidateId = info.candidates[j],
                                selectorIndex = info.selectorId,
                                hostElementId = info.hostId,
                                whereFilterId = info.whereFilterId
                            });
                        }
                    }
                    else {
                        // all remaining candidates are actually matched
                        for (int j = 0; j < info.candidateCount; j++) {
                            output_Matches.Add(new SelectorMatch() {
                                selectorId = info.selectorId,
                                sourceElementId = info.hostId,
                                targetElementId = info.candidates[j]
                            });
                        }

                    }

                }

            }

        }

        private void RunTagFilter(ResolvedSelectorFilter filter, bool expectation, ElementId* candidates, ref int candidateCount) {
            // todo -- could optimize to a. binary search, b. check a lookup table if list is large
            for (int i = 0; i < candidateCount; i++) {

                bool found = false;
                ElementId candidate = candidates[i];

                for (int j = 0; j < filter.indexTableSize; j++) {
                    // todo better vectorize this
                    if (filter.indexTable[j] == candidate) {
                        found = true;
                        break;
                    }
                }

                if (found != expectation) {
                    candidates[i--] = candidates[--candidateCount];
                }

            }

        }

        private void RunStateFilter(ResolvedSelectorFilter filter, bool expectation, ElementId* candidates, ref int candidateCount) {
            // todo -- could optimize to a. binary search, b. check a lookup table if list is large
            for (int i = 0; i < candidateCount; i++) {

                bool found = false;
                ElementId candidate = candidates[i];

                for (int j = 0; j < filter.indexTableSize; j++) {
                    // todo better vectorize this
                    if (filter.indexTable[j] == candidate) {
                        found = true;
                        break;
                    }
                }

                if (found != expectation) {
                    candidates[i--] = candidates[--candidateCount];
                }

            }

        }

        private void RunStyleFilter(ResolvedSelectorFilter filter, bool expectation, ElementId* candidates, ref int candidateCount) {
            // todo -- could optimize to a. binary search, b. check a lookup table if list is large
            for (int i = 0; i < candidateCount; i++) {

                bool found = false;
                ElementId candidate = candidates[i];

                for (int j = 0; j < filter.indexTableSize; j++) {
                    // todo better vectorize this
                    if (filter.indexTable[j] == candidate) {
                        found = true;
                        break;
                    }
                }

                if (found != expectation) {
                    candidates[i--] = candidates[--candidateCount];
                }

            }

        }

        private void RunAttributeFilter(ResolvedSelectorFilter filter, bool expectation, ElementId* candidates, ref int candidateCount) {
            for (int i = 0; i < candidateCount; i++) {

                ElementId candidate = candidates[i];
                bool found = false;

                // todo -- find a way to vectorize this maybe. the break prevents it
                for (int j = 0; j < filter.indexTableSize; j++) {
                    if (filter.indexTable[j] == candidate) {
                        found = true;
                        break;
                    }
                }

                if (found != expectation) {
                    candidates[i--] = candidates[--candidateCount];
                }

            }
        }
        
        private enum AttributeOperator {

            Equal,
            Contains,
            StartsWith,
            EndsWith

        }

        private void RunAttrCompareFilter(in ResolvedSelectorFilter filter, AttributeOperator op, bool expectation, ElementId* candidates, ref int candidateCount) {
            RunAttributeFilter(filter, true, candidates, ref candidateCount);
            
            int keyIndex = filter.key;
            int valueIndex = filter.value;

            StringHandle expectedToContain = default;

            if (op != AttributeOperator.Equal) {
                expectedToContain = attributeDatabase.Get(filter.value);
            }

            // for each remaining attribute, test that attr value is equal to expectation
            for (int i = 0; i < candidateCount; i++) {

                ElementId candidateId = candidates[i];

                map_ElementAttributes.TryGetValue(candidateId.id, out TypedListHandle<AttributeInfo> attributes);

                for (int attrIdx = 0; attrIdx < attributes.size; attrIdx++) {
                    ref AttributeInfo attributeInfo = ref attributes[attrIdx];

                    // this is guaranteed to hit unless we have a critical issue in the attribute database
                    if (attributeInfo.keyIndex == keyIndex) {

                        if (RunAttributeOperator(op, valueIndex, attributeInfo.valueIndex, expectedToContain) != expectation) {
                            candidates[i--] = candidates[--candidateCount];
                        }
                        
                        break;

                    }

                }

            }

        }
        
        private bool RunAttributeOperator(AttributeOperator op, int valueIndex, int attrValueIndex, StringHandle expectedHandle) {
            switch (op) {

                case AttributeOperator.Equal:
                    return valueIndex == attrValueIndex;

                case AttributeOperator.Contains: {
                    StringHandle valueHandle = attributeDatabase.Get(attrValueIndex);
                    return BurstableStringUtil.Contains(valueHandle, expectedHandle);
                }

                case AttributeOperator.StartsWith: {
                    StringHandle valueHandle = attributeDatabase.Get(attrValueIndex);
                    return BurstableStringUtil.StartsWith(valueHandle, expectedHandle);
                }

                case AttributeOperator.EndsWith: {
                    StringHandle valueHandle = attributeDatabase.Get(attrValueIndex);
                    return BurstableStringUtil.EndsWith(valueHandle, expectedHandle);
                }

                default:
                    return false;
            }
        }

   
    }

}