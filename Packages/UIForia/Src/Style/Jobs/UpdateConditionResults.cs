using System.Diagnostics;
using System.Threading;
using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Style {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct UpdateConditionResults : IJob, IUIForiaParallel {

        public DataList<LongBoolMap>.Shared results;
        public DataList<QuerySubscription>.Shared newQuerySubscriptions;
        public DataList<List_QuerySubscription>.Shared querySubscriptionLists;

        public LongBoolMap hasNewSubscriberMap;

        public DataList<StyleUsageQueryResult>.Shared styleUsageResults;

        public DataList<int>.Shared styleUsageToIndex;

        public CheckedArray<int> elementIdToIndex;

        public ParallelParams parallel { get; set; }

        private void Run(int start, int end) {

            for (int qIdx = start; qIdx < end; qIdx++) {

                if (hasNewSubscriberMap.Get(qIdx)) {
                    RangeInt newSubscribers = GetNewSubscriberRange(qIdx);
                    querySubscriptionLists[qIdx].AddRange(newQuerySubscriptions.GetArrayPointer() + newSubscribers.start, newSubscribers.length);
                }

            }

            for (int qIdx = start; qIdx < end; qIdx++) {

                ref List_QuerySubscription subscriptionList = ref querySubscriptionLists[qIdx];

                LongBoolMap resultMap = results[qIdx];

                for (int s = 0; s < subscriptionList.size; s++) {
                    QuerySubscription sub = subscriptionList.array[s];

                    // todo -- lift this out of the loop here 
                    int flattenedIndex = elementIdToIndex[sub.styleUsage.elementId.index];

                    if (!resultMap.Get(flattenedIndex)) {
                        continue;
                    }

                    int targetIndex = styleUsageToIndex[sub.styleUsage.id];

                    fixed (ulong* p = &styleUsageResults[targetIndex].currResults.value) {
                        long* castValue = (long*) p;
                        long initialValue;
                        long computedValue;
                        do {
                            initialValue = (long) styleUsageResults[targetIndex].currResults;
                            computedValue = (long) (initialValue | (1L << sub.targetConditionIndex));
                        } while (Interlocked.CompareExchange(ref castValue[0], computedValue, initialValue) != initialValue);
                    }

                }

            }

        }

        private RangeInt GetNewSubscriberRange(int queryIndex) {
            // todo -- binary search i guess is better since we are definitely sorted already 
            for (int i = 0; i < newQuerySubscriptions.size; i++) {

                if (newQuerySubscriptions[i].queryId.id != queryIndex) {
                    continue;
                }

                for (int j = i + 1; j < newQuerySubscriptions.size; j++) {
                    if (newQuerySubscriptions[j].queryId.id != queryIndex) {
                        return new RangeInt(i, j - i);
                    }
                }

                return new RangeInt(i, newQuerySubscriptions.size - i);
            }

            return default;
        }

        public void Execute() {
            Run(0, querySubscriptionLists.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

    }

}