using UIForia.Util.Unsafe;
using Unity.Jobs;

namespace UIForia {

    public unsafe struct RemoveDeadSelectorsJob : IJob {

        public DataList<SelectorIdElementId>.Shared input_SelectorKillList;

        public DataList<ActiveSelector>.Shared table_ActiveSelectors;
        public DataList<int>.Shared output_ActiveSelectorFreeList;

        public UnmanagedLongMap<int> table_ActiveSelectorIndexMap;

        public void Execute() {

            // if parallel we can remove and merge free lists
            // parallel needs a merge step, 1 free list index per thread/ job
            // could use free list scheme just need to merge free list heads and stitch up references
            // also want to free / clear buffers unless we leave them in place to be used by the next guy

            for (int i = 0; i < input_SelectorKillList.size; i++) {

                SelectorIdElementId item = input_SelectorKillList[i];

                if (table_ActiveSelectorIndexMap.TryRemove(item.longVal, out int index)) {

                    if (table_ActiveSelectors[index].targets.size != 0) {
                        // selector rebuild list.Add(elementId);    
                    }

                    table_ActiveSelectors[index] = default;
                    output_ActiveSelectorFreeList.Add(index);
                }

            }

        }

    }

}