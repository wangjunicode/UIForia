using System.Diagnostics;
using UIForia.Elements;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Layout {

    [BurstCompile]
    public struct UpdateEmTable : IJob {

        public ElementId rootId;
        public ElementTable<EmValue> emTable;
        public ElementTable<HierarchyInfo> hierarchyTable;
        public ElementTable<ElementMetaInfo> metaTable;

        public float viewWidth;
        public float viewHeight;
        public int activeElementCount;

        public struct EmEntry {

            public ElementId elementId;
            public float resolveParentFontSize;

        }

        public unsafe void Execute() {

            if (activeElementCount <= 0) {
                return;
            }

            DataList<EmEntry> stack = new DataList<EmEntry>(activeElementCount, Allocator.Temp);

            stack[stack.size++] = new EmEntry() {
                elementId = rootId,
                resolveParentFontSize = 18f // default
            };
            EmEntry* s = stack.GetArrayPointer();
            int stackSize = 1;
            while (stackSize != 0) {

                EmEntry current = s[--stackSize];
                int elementIdx = current.elementId.id & ElementId.ENTITY_INDEX_MASK;

                ref EmValue emValue = ref emTable.array[elementIdx];

                emValue.previousValue = emValue.resolvedValue;
                
                switch (emValue.styleValue.unit) {

                    default:
                    case UIFixedUnit.Unset:
                        emValue.resolvedValue = current.resolveParentFontSize;
                        break;

                    case UIFixedUnit.Pixel:
                        emValue.resolvedValue = emValue.styleValue.value;
                        break;

                    case UIFixedUnit.Percent:
                        emValue.resolvedValue = current.resolveParentFontSize * emValue.styleValue.value;
                        break;

                    case UIFixedUnit.Em:
                        emValue.resolvedValue = current.resolveParentFontSize * emValue.styleValue.value;
                        break;

                    case UIFixedUnit.ViewportWidth:
                        emValue.resolvedValue = viewWidth * emValue.styleValue.value;
                        break;

                    case UIFixedUnit.ViewportHeight:
                        emValue.resolvedValue = viewHeight * emValue.styleValue.value;
                        break;

                }

                int childCount = hierarchyTable.array[elementIdx].childCount;
                ElementId childPtr = hierarchyTable.array[elementIdx].lastChildId;

                for (int i = 0; i < childCount; i++) {

                    int childIdx = childPtr.id & ElementId.ENTITY_INDEX_MASK;
                    if (!(metaTable.array[childIdx].generation != childPtr.generation || (metaTable.array[childIdx].flags & UIElementFlags.EnabledFlagSet) != UIElementFlags.EnabledFlagSet)) {
                        s[stackSize++] = new EmEntry() {
                            elementId = childPtr,
                            resolveParentFontSize = emValue.resolvedValue
                        };
                    }

                    childPtr = hierarchyTable.array[childIdx].prevSiblingId;

                }

            }

            stack.Dispose();
        }

    }

}