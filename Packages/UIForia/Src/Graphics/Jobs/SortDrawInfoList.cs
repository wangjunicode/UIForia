using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Graphics {

    [BurstCompile]
    internal unsafe struct SortDrawInfoList : IJob {

        public DataList<DrawInfo2>.Shared drawList;

        public void Execute() {

            if (drawList.size < 500) {
                // todo -- this is only good if we are already sorted!
                BubbleSort(drawList.GetArrayPointer(), drawList.size);
            }
            else {
                NativeSortExtension.Sort(drawList.GetArrayPointer(), drawList.size);
            }

        }

        private static void BubbleSort(DrawInfo2* array, int count)  {
            int n = count;
            do {
                int sw = 0; // last swap index

                for (int i = 0; i < n - 1; i++) {
                    int cmpVal = array[i].drawSortId.sortId - array[i + 1].drawSortId.sortId;
                    if (cmpVal > 0) {
                        DrawInfo2 temp = array[i];
                        array[i] = array[i + 1];
                        array[i + 1] = temp;
                        //Save swap position
                        sw = i + 1;
                    }
                }

                //We do not need to visit all elements
                //we only need to go as far as the last swap
                n = sw;
            }

            //Once n = 1 then the whole list is sorted
            while (n > 1);

        }

    }

}