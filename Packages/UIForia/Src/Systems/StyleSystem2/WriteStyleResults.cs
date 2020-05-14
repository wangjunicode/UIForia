using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct WriteStyleResults : IJob {

        public Pow2AllocatorSet allocator;
        public ElementTable<StyleResult> styleResultTable;
        public PerThread<StyleRebuildContainer> perThread_RebuildContainer;

        public void Execute() {

            int threadCount = perThread_RebuildContainer.GetUsedThreadCount();

            StyleRebuildContainer* containerList = stackalloc StyleRebuildContainer[threadCount];

            perThread_RebuildContainer.GatherUsedThreadData(containerList);

            for (int i = 0; i < threadCount; i++) {
                containerList[i].GetResultList().Foreach(new ReleaseOldValues() {
                    allocator = allocator,
                    table = styleResultTable
                });
            }

            for (int i = 0; i < threadCount; i++) {
                containerList[i].GetResultList().Foreach(new WriteStyleUpdates() {
                    allocator = allocator,
                    table = styleResultTable
                });
            }

        }

        private struct WriteStyleUpdates : IListForeach<StyleRebuildResult> {

            public Pow2AllocatorSet allocator;
            public ElementTable<StyleResult> table;

            public void Run(in StyleRebuildResult item) {
                
                ref StyleResult current = ref table[item.elementId];

                if (current.properties == null && current.propertyCount > 0) {
                    current.properties = (byte*)allocator.AllocateByIndex(current.allocatorIndex);
                }

                if (current.propertyCount > 0) {
                    int byteSize = TypedUnsafe.ByteSize<PropertyId, PropertyData>(current.propertyCount);
                    UnsafeUtility.MemCpy(current.properties, item.properties, byteSize);
                }
                
            }

        }

        private struct ReleaseOldValues : IListForeach<StyleRebuildResult> {

            public Pow2AllocatorSet allocator;
            public ElementTable<StyleResult> table;

            public void Run(in StyleRebuildResult item) {

                ref StyleResult current = ref table[item.elementId];

                int newCount = item.propertyCount;

                int oldIndex = current.allocatorIndex;
                int newIndex = allocator.GetAllocatorIndex(TypedUnsafe.ByteSize<PropertyId, PropertyData>(newCount));

                current.allocatorIndex = newIndex;
                current.propertyCount = newCount;
                
                if (oldIndex != newIndex) {

                    if (current.properties != null) {
                        allocator.FreeByIndex(current.properties, oldIndex);
                    }

                    current.properties = null;

                }

            }

        }

    }

}