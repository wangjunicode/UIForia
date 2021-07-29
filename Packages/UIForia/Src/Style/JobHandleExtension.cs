using Unity.Jobs;

namespace UIForia {

    public static class JobHandleExtension {

        public static bool SingleThreadedJobs;
        
        public static JobHandle Then<T>(this JobHandle self, in T job) where T : struct, IJob {
            if (SingleThreadedJobs) {
                job.Run();
                return default;
            }

            return job.Schedule(self);
        }

        public static JobHandle And(this JobHandle self, JobHandle handle) {
            return JobHandle.CombineDependencies(self, handle);
        }

        public static JobHandle ThenParallel<T>(this JobHandle self, in T job) where T : struct, IUIForiaParallel {
            int itemCount = job.parallel.itemCount < 1 ? 1 : job.parallel.itemCount;
            int batchSize = job.parallel.minBatchSize < 1 ? 1 : job.parallel.minBatchSize;
            if (SingleThreadedJobs) {
                job.Run();
                return default;
            }

            return job.ScheduleBatch(itemCount, batchSize, self);
        }

    }

}