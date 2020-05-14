using UIForia.Util.Unsafe;
using Unity.Jobs;

namespace UIForia {

    public unsafe struct CreateNewSelectorsJob : IJob {

        public DataList<SelectorIdElementId>.Shared input_SelectorCreateList;

        public DataList<ActiveSelector>.Shared table_write_ActiveSelectors;
        public DataList<int>.Shared input_ActiveSelectorFreeList;


        public void Execute() {

            // if parallel we can remove and merge free lists
            // parallel needs a merge step, 1 free list index per thread/ job
            // could use free list scheme just need to merge free list heads and stitch up references
            // also want to free / clear buffers unless we leave them in place to be used by the next guy

            int sizeDiff = input_SelectorCreateList.size - input_ActiveSelectorFreeList.size;

            if (sizeDiff > 0) {
                table_write_ActiveSelectors.EnsureAdditionalCapacity(sizeDiff); // want to clear default here    
            }

            for (int i = 0; i < input_SelectorCreateList.size; i++) {

                SelectorIdElementId item = input_SelectorCreateList[i];

                int idx;

                if (i < input_ActiveSelectorFreeList.size) {
                    idx = input_ActiveSelectorFreeList[i];
                }
                else {
                    idx = table_write_ActiveSelectors.Reserve();
                }

                table_write_ActiveSelectors[idx].elementId = item.elementId;
                table_write_ActiveSelectors[idx].selectorIndex = item.selectorId;

            }

        }

    }

}