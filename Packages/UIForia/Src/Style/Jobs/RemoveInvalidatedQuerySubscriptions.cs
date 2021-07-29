using UIForia.ListTypes;
using UIForia.Style;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    internal unsafe struct RemoveInvalidatedQuerySubscriptions : IJob, IJobParallelForBatch, IUIForiaParallel {

        public ElementMap deadOrDisabledMap;
        public ElementMap rebuildStylesMap;

        [NativeDisableUnsafePtrRestriction, NoAlias]
        public DataList<List_QuerySubscription>.Shared queryTableState;


        public ParallelParams parallel { get; set; }

        public void Execute() {
            RunImpl(0, queryTableState.size);
        }

        public void Execute(int startIndex, int count) {
            RunImpl(startIndex, startIndex + count);
        }

        private void RunImpl(int start, int end) {

            ulong* map = stackalloc ulong[rebuildStylesMap.longCount];
            ElementMap invalidMap = new ElementMap(map, rebuildStylesMap.longCount);
            invalidMap.Combine(deadOrDisabledMap);
            invalidMap.Combine(rebuildStylesMap);
            
            for (int i = start; i < end; i++) {
                
                // todo -- rebuildStylesMap
                ref List_QuerySubscription subscriptionList = ref queryTableState[i];

                for (int s = 0; s < subscriptionList.size; s++) {
                    QuerySubscription subscription = subscriptionList.array[s];
                    if (invalidMap.Get(subscription.styleUsage.elementId)) {
                        subscriptionList.array[s--] = subscriptionList.array[--subscriptionList.size];
                    }
                }

            }
        }

    }

}