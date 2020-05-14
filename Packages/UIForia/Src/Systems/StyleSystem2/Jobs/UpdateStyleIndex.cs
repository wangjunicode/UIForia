using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public unsafe struct UpdateStyleIndex : IJob {

        [NoAlias] public IntMap<List_ElementId> styleIndex;
        [NoAlias] public ListAllocator<ElementId> styleIndexAllocator;
        [ReadOnly] [NoAlias] public ElementTable<ElementMetaInfo> elementMetaInfo;
        [ReadOnly] [NoAlias] public PerThread<StyleIndexUpdateSet> perThread_StyleIndexUpdater;

        public void Execute() {

            int threadCount = perThread_StyleIndexUpdater.GetUsedThreadCount();

            StyleIndexUpdateSet* sets = stackalloc StyleIndexUpdateSet[threadCount];

            perThread_StyleIndexUpdater.GatherUsedThreadData(sets);

            int addedCount = 0;
            int removedCount = 0;

            for (int i = 0; i < threadCount; i++) {
                addedCount += sets[i].addedStyles.size;
                removedCount += sets[i].removedStyles.size;
            }

            int max = addedCount > removedCount ? addedCount : removedCount;

            Allocator allocator = TypedUnsafe.GetTemporaryAllocator<StyleIndexUpdate>(max);

            DataList<StyleIndexUpdate> buffer = new DataList<StyleIndexUpdate>(max, allocator);

            StyleIndexUpdate* ptr = buffer.GetArrayPointer();

            buffer.size = removedCount;

            for (int i = 0; i < threadCount; i++) {
                TypedUnsafe.MemCpy(ptr, sets[i].removedStyles.array, sets[i].removedStyles.size);
                ptr += sets[i].removedStyles.size;
            }

            RemoveFromIndices(ref buffer);

            buffer.SetSize(addedCount);

            ptr = buffer.GetArrayPointer();

            for (int i = 0; i < threadCount; i++) {
                TypedUnsafe.MemCpy(ptr, sets[i].addedStyles.array, sets[i].addedStyles.size);
                ptr += sets[i].addedStyles.size;
            }

            AddToIndices(ref buffer);

            buffer.Dispose();

        }

        private void AddToIndices(ref DataList<StyleIndexUpdate> buffer) {

            NativeSortExtension.Sort(buffer.GetArrayPointer(), buffer.size);

            for (int i = 0; i < buffer.size; i++) {

                StyleId styleId = buffer[i].styleId;

                int startIdx = i++;

                while (i < buffer.size && buffer[i].styleId == styleId) {
                    i++;
                }

                int count = i - startIdx;

                ref List_ElementId list = ref *styleIndex.GetOrCreate(styleId);

                RemoveDeadElements(ref list);

                styleIndexAllocator.EnsureAdditionalCapacity(ref list, count);

                for (int j = startIdx; j < startIdx + count; j++) {
                    list.array[list.size++] = buffer[j].elementId;
                }

            }
        }

        private void RemoveFromIndices(ref DataList<StyleIndexUpdate> buffer) {

            NativeSortExtension.Sort(buffer.GetArrayPointer(), buffer.size);

            for (int i = 0; i < buffer.size; i++) {

                StyleId styleId = buffer[i].styleId;

                int startIdx = i++;

                while (i < buffer.size && buffer[i].styleId == styleId) {
                    i++;
                }

                int count = i - startIdx;

                if (!styleIndex.TryGetPointer(styleId, out List_ElementId* ptr)) {
                    continue;
                }

                ref List_ElementId list = ref *ptr;
                
                for (int j = startIdx; j < startIdx + count; j++) {

                    for (int idx = 0; idx < list.size; idx++) {
                        // swap remove if not present
                        if (list.array[idx] == buffer[j].elementId) {
                            list.array[idx] = list.array[--list.size];
                            break;
                        }
                    }

                }

                // leave the index in the map with size 0, no need to remove it
                if (list.size == 0) {
                    styleIndexAllocator.Free(ref list);
                }

            }

        }

        private void RemoveDeadElements(ref List_ElementId list) {
            for (int i = 0; i < list.size; i++) {

                ElementId elementId = list.array[i];

                if (!ElementSystem.IsAlive(elementId, elementMetaInfo)) {
                    list.array[i] = list.array[--list.size];
                }

            }
        }

    }

}