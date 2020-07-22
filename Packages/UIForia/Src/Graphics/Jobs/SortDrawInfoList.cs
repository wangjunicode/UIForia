using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Graphics {

    [BurstCompile]
    internal unsafe struct SortDrawInfoList : IJob {

        public DataList<DrawInfo2>.Shared drawList;

        public void Execute() {

            NativeSortExtension.Sort(drawList.GetArrayPointer(), drawList.size, new DrawInfoComp());

        }

    }

}