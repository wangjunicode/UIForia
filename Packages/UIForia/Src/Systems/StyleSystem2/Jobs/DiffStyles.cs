using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    public struct CullDeadAndDisabledElementChanges : IJob {

        public ElementTable<ElementMetaInfo> metaTable;
        public DataList<SharedStyleUpdate>.Shared sharedChangeSets;

        public void Execute() {

            for (int i = 0; i < sharedChangeSets.size; i++) {
                if (ElementSystem.IsDeadOrDisabled(sharedChangeSets[i].elementId, metaTable)) {
                    sharedChangeSets[i--] = sharedChangeSets[--sharedChangeSets.size];
                }
            }

        }

    }

    [BurstCompile]
    public unsafe struct DiffStyles : IJob, IJobParallelForBatch, IVertigoParallel {

        [NativeSetThreadIndex] public int threadIndex;

        [ReadOnly] [NoAlias] public DataList<SharedStyleUpdate>.Shared sharedChangeSets;
        [WriteOnly] [NoAlias] public PerThread<StyleIndexUpdateSet> perThread_StyleIndexUpdater;

        public void Execute() {
            Run(0, sharedChangeSets.size);
        }

        public void Execute(int start, int count) {
            Run(start, start + count);
        }

        private void Run(int start, int end) {

            ref StyleIndexUpdateSet indexUpdateSet = ref perThread_StyleIndexUpdater.GetForThread(threadIndex);

            for (int updateIndex = start; updateIndex < end; updateIndex++) {

                ref SharedStyleUpdate update = ref sharedChangeSets[updateIndex];

                int updatedStyleCount = update.updatedStyleCount;
                int originalStyleCount = update.originalStyleCount;

                StyleId* originalStyles = update.originalStyles;
                StyleId* updatedStyles = update.updatedStyles;

                for (int i = 0; i < updatedStyleCount; i++) {

                    if (ContainsAndRemove(updatedStyles[i], originalStyles, ref originalStyleCount)) {
                        indexUpdateSet.Add(new StyleIndexUpdate(update.elementId, updatedStyles[i]));
                    }

                }

                for (int i = 0; i < originalStyleCount; i++) {
                    indexUpdateSet.Remove(new StyleIndexUpdate(update.elementId, originalStyles[i]));
                }

            }

        }

        private static bool ContainsAndRemove(StyleId styleId, StyleId* list, ref int count) {
            for (int i = 0; i < count; i++) {
                if (list[i] == styleId) {
                    list[i] = list[--count];
                    return true;
                }
            }

            return false;
        }

        public ParallelParams parallel { get; set; }

    }

}