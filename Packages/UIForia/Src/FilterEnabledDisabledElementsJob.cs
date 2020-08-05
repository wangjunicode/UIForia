using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia {

    [BurstCompile]
    internal unsafe struct FilterEnabledDisabledElementsJob : IJob {

        public ElementTable<ElementMetaInfo> metaTable;
        public DataList<ElementId>.Shared enabledElements;
        public DataList<ElementId>.Shared disabledElements;

        public void Execute() {

            enabledElements.FilterSwapRemove(new FilterEnabled() {
                metaTable = metaTable
            });

            disabledElements.FilterSwapRemove(new FilterDisabled() {
                metaTable = metaTable
            });

            int bufferSize = enabledElements.size > disabledElements.size
                ? enabledElements.size
                : disabledElements.size;

            DataList<ElementId> buffer = new DataList<ElementId>(bufferSize, Allocator.Temp);
            RemoveDuplicates(enabledElements, buffer);
            RemoveDuplicates(disabledElements, buffer);

            for (int i = 0; i < enabledElements.size; i++) {
                metaTable[enabledElements[i]].flags |= UIElementFlags.EnableStateChanged;
            }

            buffer.Dispose();

        }

        private static void RemoveDuplicates(DataList<ElementId>.Shared target, DataList<ElementId> buffer) {
            buffer.size = 0;
            if (target.size >= 1) {

                NativeSortExtension.Sort(target.GetArrayPointer(), target.size, new ElementIdComp());

                for (int i = 0; i < target.size - 1; i++) {

                    if (target[i] != target[i + 1]) {
                        buffer.AddUnchecked(target[i]);
                    }

                }

                buffer.AddUnchecked(target[target.size - 1]);

                TypedUnsafe.MemCpy(target.GetArrayPointer(), buffer.GetArrayPointer(), buffer.size);
                target.size = buffer.size;
            }
        }

        private struct ElementIdComp : IComparer<ElementId> {

            public int Compare(ElementId x, ElementId y) {
                return x.id - y.id;
            }

        }

        private struct FilterDisabled : IListFilter<ElementId> {

            public ElementTable<ElementMetaInfo> metaTable;

            // this is removing elements that were disabled and then enabled again in the same frame
            public bool Filter(in ElementId item) {
                bool generationMatch = metaTable[item].generation == item.generation;
                // if generation doesn't match, the element was destroyed, keep it in this list 
                if (!generationMatch) {
                    return true;
                }
                // if element not currently enabled, keep it in this list
                return (metaTable[item].flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet;
            }

        }

        private struct FilterEnabled : IListFilter<ElementId> {

            public ElementTable<ElementMetaInfo> metaTable;

            // this is removing elements that were enabled and then disabled or destroyed in the same frame
            public bool Filter(in ElementId item) {
                return metaTable[item].generation == item.generation && (metaTable[item].flags & UIElementFlags.EnabledFlagSet) == UIElementFlags.EnabledFlagSet;
            }

        }

    }

}