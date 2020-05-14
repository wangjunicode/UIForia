using System;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    public unsafe struct MergePerThreadPageLists<T> : IJob where T : unmanaged {

        public BufferList<T> outputList;
        public PagedList<T>.PerThread perThreadLists;
        public bool disposeOnCompletion;

        public void Execute() {
            perThreadLists.ToUnmanagedList(outputList);
            if (disposeOnCompletion) {
                perThreadLists.Dispose();
            }
        }

    }

    public unsafe struct MergePerThreadPageSplitBuffers<T, U> : IJob where T : unmanaged where U : unmanaged {

        public SplitBuffer<T, U> outputList;
        public PagedSplitBufferList<T, U>.PerThread perThreadLists;
        public bool disposeOnCompletion;

        public void Execute() {
            perThreadLists.ToSplitBuffer(outputList);
            if (disposeOnCompletion) {
                perThreadLists.Dispose();
            }
        }

    }

    public struct ParallelParams {

        public readonly int itemCount;
        public readonly int minBatchSize;

        public ParallelParams(int itemCount, int minBatchSize) {
            this.itemCount = itemCount;
            this.minBatchSize = minBatchSize;
        }

        public unsafe struct Deferred {

            public readonly int* itemCount;
            public readonly int minBatchSize;

            public Deferred(int* itemCount, int minBatchSize) {
                this.itemCount = itemCount;
                this.minBatchSize = minBatchSize;
            }

        }

    }

    public interface IVertigoParallelDeferred : IJob, IJobParallelForDeferBatched {

        ParallelParams.Deferred defer { get; }

    }

    public interface IVertigoParallel : IJob, IJobParallelForBatch {

        ParallelParams parallel { get; }

    }

    public class VertigoScheduler {

        public unsafe struct SchedulerStep {

            private readonly JobHandle handle;

            internal SchedulerStep(JobHandle handle) {
                this.handle = handle;
            }

            public SchedulerStep Then<T>(out JobHandle outHandle, T job) where T : struct, IJob {
                outHandle = job.Schedule(handle);
                return new SchedulerStep(outHandle);
            }

            public SchedulerStep Then<T>(T job) where T : struct, IJob {
                return new SchedulerStep(job.Schedule(handle));
            }

            public SchedulerStep ThenParallel<T>(T job) where T : struct, IVertigoParallel {
                ParallelParams par = job.parallel;
            
                if (par.itemCount <= 0) {
                    return new SchedulerStep(handle);
                }

                int minBatchSize = par.minBatchSize < 1 ? 1 : par.minBatchSize;
            
                if (par.itemCount <= minBatchSize) {
                    return new SchedulerStep(job.Schedule(handle));    
                }
            
                return new SchedulerStep(job.ScheduleBatch(par.itemCount, minBatchSize, handle));
            }
            
            public SchedulerStep ThenDeferParallel<T>(T job) where T : struct, IVertigoParallelDeferred {
                ParallelParams.Deferred par = job.defer;
                int minBatchSize = par.minBatchSize < 1 ? 1 : par.minBatchSize;
                return new SchedulerStep(job.Schedule(par.itemCount, minBatchSize, handle));
            }
            
            public SchedulerStep Then<T, U>(T job, U job2) where T : struct, IJob where U : struct, IJob {
                return new SchedulerStep(JobHandle.CombineDependencies(job.Schedule(handle), job2.Schedule(handle)));
            }

            public SchedulerStep ThenParallelForRange<T>(int arrayLength, int minBatchSize, T job) where T : struct, IJobParallelForBatch {
                return new SchedulerStep(job.ScheduleBatch(arrayLength, minBatchSize, handle));
            }

            public static implicit operator JobHandle(SchedulerStep schedulerStep) {
                return schedulerStep.handle;
            }

            public SchedulerStep ThenParallelForRangeDefer<T>(int* size, int minBatchCount, T job) where T : struct, IJobParallelForDeferBatched {
                return new SchedulerStep(job.Schedule(size, minBatchCount, handle));
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

      
        public static SchedulerStep Parallel<T>(T job) where T : struct, IJob, IJobParallelForBatch, IVertigoParallel {
            ParallelParams par = job.parallel;
            
            if (par.itemCount <= 0) {
                return new SchedulerStep();
            }

            int minBatchSize = par.minBatchSize < 1 ? 1 : par.minBatchSize;
            
            if (par.itemCount <= minBatchSize) {
                return new SchedulerStep(job.Schedule());    
            }
            
            return new SchedulerStep(job.ScheduleBatch(par.itemCount, minBatchSize));
        }
        
        // public static SchedulerStep AwaitParallel<T>(T job) where T : struct, IJob, IJobParallelForBatch, IVertigoParallel {
        //     ParallelParams par = job.parallel;
        //     
        //     if (par.itemCount <= 0) {
        //         return new SchedulerStep();
        //     }
        //
        //     par.minBatchSize = par.minBatchSize < 1 ? 1 : par.minBatchSize;
        //     
        //     if (par.itemCount <= par.minBatchSize) {
        //         return new SchedulerStep(job.Schedule());    
        //     }
        //     
        //     return new SchedulerStep(job.ScheduleBatch(par.itemCount, par.minBatchSize));
        // }

        public static SchedulerStep Await<T>(T job) where T : struct, IJob {
            return new SchedulerStep(job.Schedule());
        }

    }

}