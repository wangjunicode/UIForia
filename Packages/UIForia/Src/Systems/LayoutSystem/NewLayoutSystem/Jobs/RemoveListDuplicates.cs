using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Systems {

    [BurstCompile]
    internal unsafe struct RemoveListDuplicates : IJob {

        public DataList<ElementId>.Shared list;

        public void Execute() {

            DataList<ElementId> buffer = new DataList<ElementId>(list.size, Allocator.Temp);

            buffer.CopyFrom(list.GetArrayPointer(), list.size);

            RemoveDuplicates(list, buffer);

        }

        private static void RemoveDuplicates(DataList<ElementId>.Shared target, DataList<ElementId> buffer) {
            buffer.size = 0;
            if (target.size > 1) {

                NativeSortExtension.Sort(target.GetArrayPointer(), target.size);

                for (int i = 0; i < target.size - 1; i++) {

                    if (target[i] != target[i + 1]) {
                        buffer.AddUnchecked(target[i]);
                    }

                }

                buffer.AddUnchecked(target[target.size - 1]);
            }

            TypedUnsafe.MemCpy(target.GetArrayPointer(), buffer.GetArrayPointer(), buffer.size);
            target.size = buffer.size;
        }

    }

}