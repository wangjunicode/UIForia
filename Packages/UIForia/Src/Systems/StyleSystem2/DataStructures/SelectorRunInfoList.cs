using UIForia.Util.Unsafe;
using Unity.Collections;

namespace UIForia.Util.Unsafe {}

namespace UIForia {

    public unsafe struct SelectorRunInfoList : IPerThreadCompatible {

        // todo -- make this a single allocation to a paged byte stream or similar
        private PagedListState* candidatesList;
        private PagedListState* filterList;

        private PagedListState* selectorRunInfoList;

        public void Dispose() {
            if (candidatesList != null) {
                candidatesList->Dispose();
            }

            if (filterList != null) {
                filterList->Dispose();
            }

            if (selectorRunInfoList != null) {
                selectorRunInfoList->Dispose();
            }

            this = default;
        }

        public bool IsInitialized {
            get => candidatesList != null && filterList != null && selectorRunInfoList != null;
        }

        public void InitializeForThread(Allocator allocator) {
            candidatesList = new PagedList<int>(1024, allocator).GetStatePointer();
            filterList = new PagedList<ResolvedSelectorFilter>(256, allocator).GetStatePointer();
            selectorRunInfoList = new PagedList<SelectorRunInfo>(64, allocator).GetStatePointer();
        }

        public void Add(ElementId hostElementId, int selectorIndex, int * candidateBuffer, int candidateCount, ResolvedSelectorFilter* filterBuffer, int filterCount, int whereFilterId) {

            selectorRunInfoList->AddItem<SelectorRunInfo>(
                new SelectorRunInfo() {
                    candidateCount = candidateCount,
                    filters = filterList->AddRange(filterBuffer, filterCount),
                    filterCount = filterCount,
                    candidates = candidatesList->AddRange(candidateBuffer, candidateCount),
                    hostId = hostElementId,
                    selectorId = selectorIndex,
                    whereFilterId = whereFilterId,
                }
            );

        }

        public PagedList<SelectorRunInfo> GetRunInfo() {
            return new PagedList<SelectorRunInfo>(selectorRunInfoList);
        }

    }

}