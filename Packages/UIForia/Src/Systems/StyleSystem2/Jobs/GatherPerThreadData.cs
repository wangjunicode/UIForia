using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct MergePerThreadData<T, U> : IJob where T : unmanaged, IPerThreadCompatible, IMergeableToList<U> where U : unmanaged {

        public PerThread<T> perThread;
        public UnmanagedList<U> gatheredOutput;

        public void Execute() {
            
            BufferList<T> gather = new BufferList<T>(perThread.GetUsedThreadCount(), Allocator.Temp);
            
            gather.size = perThread.GatherUsedThreadData(gather.array);
            
            int totalItemCount = 0;

            for (int i = 0; i < gather.size; i++) {
                totalItemCount += gather[i].ItemCount();
            }

            gatheredOutput.EnsureCapacity(totalItemCount);
            
            for (int i = 0; i < gather.size; i++) {
                gather[i].Merge(gatheredOutput);
            }
            
            gather.Dispose();
            
        }

    }
    
    public unsafe struct GatherPerThreadData<T> : IJob where T : unmanaged, IPerThreadCompatible {

        public PerThread<T> perThread;
        public UnmanagedList<T> gatheredOutput;

        public void Execute() {

            gatheredOutput.EnsureCapacity(perThread.GetUsedThreadCount());
            
            gatheredOutput.size = perThread.GatherUsedThreadData(gatheredOutput.GetBuffer());
            
        }

    }

}