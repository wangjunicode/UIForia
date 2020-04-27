using Unity.Mathematics;
using UnityEngine;

namespace UIForia {

    public unsafe struct BatchRangeJobData {

        public readonly int index;
        public readonly int maxJobCount;
        public readonly int minBatchSize;

        public BatchRangeJobData(int index, int minBatchSize, int maxJobCount = -1) {
            this.index = index;
            this.minBatchSize = minBatchSize < 1 ? 1 : minBatchSize;
            this.maxJobCount = (maxJobCount <= 0) ? 8 : maxJobCount;
        }

        public bool TryGetJobBatchRange(int workItemCount, out RangeInt range) {
            range = default;
            if (workItemCount <= 0) {
                return false;
            }

            int jobCount = math.clamp((workItemCount / minBatchSize), 1, maxJobCount);

            if (index >= jobCount) {
                return false;
            }

            int rangeSize = (workItemCount / jobCount);
            int remaining = workItemCount - (rangeSize * jobCount);

            int* sizes = stackalloc int[jobCount];

            for (int i = 0; i < jobCount; i++) {
                sizes[i] = rangeSize;
            }

            int idx = 0;
            while (remaining > 0) {
                sizes[idx]++;
                remaining--;
                idx++;
                if (idx == jobCount) idx = 0;
            }

            int start = 0;
            for (int i = 0; i < index; i++) {
                start += sizes[i];
            }

            range = new RangeInt(start, sizes[index]);
            return true;
        }

    }

}