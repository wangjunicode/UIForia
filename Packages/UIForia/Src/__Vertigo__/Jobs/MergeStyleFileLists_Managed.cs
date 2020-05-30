using System;
using UIForia.Util;
using Unity.Jobs;

namespace UIForia {

    public unsafe struct MergeStyleFileLists_Managed : IJob {

        public GCHandleList<StyleFile> mergeResult;
        public PerThreadList<StyleFile> perThread_styleFileList;
        public HeapAllocated<int> lengthResult;

        public void Execute() {
            
            LightList<StyleFile>[] perThreadLists = perThread_styleFileList.handle.Get();
            
            int count = 0;
            
            for (int i = 0; i < perThreadLists.Length; i++) {
                
                if (perThreadLists[i] != null) count += perThreadLists[i].size;
                
            }

            LightList<StyleFile> output = mergeResult.Get();
            
            output.EnsureCapacity(count);
            
            for (int i = 0; i < perThreadLists.Length; i++) {
                
                if (perThreadLists[i] != null) {
                    LightList<StyleFile> list = perThreadLists[i];
                    Array.Copy(list.array, 0, output.array, output.size, list.size);
                    output.size += list.size;
                }
                
            }

            lengthResult.Set(output.size);

        }

    }

}