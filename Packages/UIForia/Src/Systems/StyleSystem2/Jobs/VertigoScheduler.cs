using System;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    public unsafe struct MergePerThreadPageLists<T> : IJob where T : unmanaged {

        public UnmanagedList<T> outputList;
        public UnmanagedPagedList<T>.PerThread perThreadLists;
        public bool disposeOnCompletion;

        public void Execute() {
            perThreadLists.ToUnmanagedList(outputList);
            if (disposeOnCompletion) {
                perThreadLists.Dispose();
            }
        }

    }

    public class VertigoScheduler {

        public struct SchedulerStep {

            private readonly JobHandle handle;

            internal SchedulerStep(JobHandle handle) {
                this.handle = handle;
            }

            public SchedulerStep Then<T>(T job) where T : struct, IJob {
                return new SchedulerStep(job.Schedule(handle));
            }

            public SchedulerStep Then<T, U>(T job, U job2) where T : struct, IJob where U : struct, IJob {
                return new SchedulerStep(JobHandle.CombineDependencies(job.Schedule(handle), job2.Schedule(handle)));
            }

            public SchedulerStep ThenParallelForRange<T>(int arrayLength, int minBatchSize, T job) where T : struct, IJobParallelForBatch {
                return new SchedulerStep(job.ScheduleBatch(arrayLength, minBatchSize));
            }

            public static implicit operator JobHandle(SchedulerStep schedulerStep) {
                return schedulerStep.handle;
            }

        }

        public static SchedulerStep ParallelFor<T>(int itemCount, int minBatchSize, T job) where T : struct, IJobParallelFor {
            return new SchedulerStep(job.Schedule(itemCount, minBatchSize));
        }

        public static SchedulerStep ParallelForRange<T>(int itemCount, int minBatchSize, T job) where T : struct, IJobParallelForBatch {
            return new SchedulerStep(job.ScheduleBatch(itemCount, minBatchSize));
        }

        public static SchedulerStep Await(JobHandle handle) {
            return new SchedulerStep(handle);
        }
        
        public static SchedulerStep Await(JobHandle handle0, JobHandle handle1) {
            return new SchedulerStep(JobHandle.CombineDependencies(handle0, handle1));
        }

        public static SchedulerStep Await(JobHandle handle0, JobHandle handle1, JobHandle handle2) {
            return new SchedulerStep(JobHandle.CombineDependencies(handle0, handle1));
        }
        
        public static SchedulerStep Await(JobHandle handle0, JobHandle handle1, JobHandle handle2, JobHandle handle3, JobHandle handle4) {
            NativeArray<JobHandle> array = new NativeArray<JobHandle>(5, Allocator.Temp);
            array[0] = handle0;
            array[1] = handle1;
            array[2] = handle2;
            array[3] = handle3;
            array[4] = handle4;
            SchedulerStep retn = new SchedulerStep(JobHandle.CombineDependencies(array));
            array.Dispose();
            return retn;
        }
        
        public static SchedulerStep Await(JobHandle handle0, JobHandle handle1, JobHandle handle2, JobHandle handle3) {
            NativeArray<JobHandle> array = new NativeArray<JobHandle>(4, Allocator.Temp);
            array[0] = handle0;
            array[1] = handle1;
            array[2] = handle2;
            array[3] = handle3;
            SchedulerStep retn = new SchedulerStep(JobHandle.CombineDependencies(array));
            array.Dispose();
            return retn;
        }
        
        public static SchedulerStep Await<T>(T job) where T : struct, IJob {
            return new SchedulerStep(job.Schedule());
        }
        

    }

}