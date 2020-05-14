using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct MergeStyleStatePairs : IJob {

        public StylePairUpdateType updateType;
        public DataList<StylePairUpdate>.Shared stylePairsList;
        public PerThread<StyleStatePairList> perThread_styleStatePairList;
        public int * listSizeResult;

        public void Execute() {

            int threadCount = perThread_styleStatePairList.GetUsedThreadCount();

            StyleStatePairList* gatheredPairList = stackalloc StyleStatePairList[threadCount];

            perThread_styleStatePairList.GatherUsedThreadData(gatheredPairList);

            int itemCount = 0;

            for (int i = 0; i < threadCount; i++) {

                itemCount += gatheredPairList[i].GetElementCountForType(updateType);
            }
            
            stylePairsList.SetSize(itemCount);
            StylePairUpdate* ptr = stylePairsList.GetArrayPointer();
          
            ListFilter listFilter = new ListFilter(updateType);
            
            for (int i = 0; i < threadCount; i++) {
                
                ptr += gatheredPairList[i].GetStyleStateUpdateList().FilterToSizedBuffer(ptr, listFilter);

            }

            if (listSizeResult != null) {
                *listSizeResult = itemCount;
            }
            
        }

    }

}