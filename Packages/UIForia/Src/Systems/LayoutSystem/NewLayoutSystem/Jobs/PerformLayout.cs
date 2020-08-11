using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Layout {

    [BurstCompile]
    internal unsafe struct RunLayoutHorizontal : IJob {

        public ElementTable<LayoutBoxUnion> layoutBoxTable;
        public DataList<ElementId>.Shared elementList;

        [NativeDisableUnsafePtrRestriction] public BurstLayoutRunner* runner;

        public void Execute() {
            // used for resolving auto size since parent shouldn't resolve auto size for ignored children
            RootLayoutBoxBurst fakeRoot = new RootLayoutBoxBurst();
            int count = elementList.size;

            ElementId* elementArray = elementList.GetArrayPointer();
            for (int i = 0; i < count; i++) {
                ElementId current = elementArray[i];

                int idx = current.id & ElementId.ENTITY_INDEX_MASK;
                
                // todo -- I still need to invalidate content caches when stuff changes
                ref LayoutHierarchyInfo layoutHierarchyInfo = ref runner->layoutHierarchyTable[idx]; //GetLayoutHierarchy(current);

                if (layoutHierarchyInfo.behavior == LayoutBehavior.Ignored) {

                    ref LayoutInfo parentLayoutInfo = ref runner->GetHorizontalLayoutInfo(layoutHierarchyInfo.parentId);

                    BlockSize blockSize = new BlockSize(parentLayoutInfo.finalSize, parentLayoutInfo.ContentAreaSize);

                    runner->GetWidths(fakeRoot, blockSize, current, out LayoutSize size);

                    blockSize.insetSize = size.Clamped; // todo -- padding/border subtraction
                    blockSize.outerSize = size.Clamped; 
                    runner->ApplyLayoutHorizontal(current, size.marginStart, size.marginStart, size.Clamped, parentLayoutInfo.finalSize, blockSize, LayoutFit.Default, parentLayoutInfo.finalSize);

                }

                ref LayoutInfo layoutInfo = ref runner->horizontalLayoutInfoTable[idx];
                
               // if (layoutInfo.requiresLayout) {
                    layoutBoxTable.array[idx].RunLayoutHorizontal(runner);
                    layoutInfo.requiresLayout = false;
               // }
            }
        }

    }

    [BurstCompile]
    internal unsafe struct RunLayoutVertical : IJob {

        public ElementTable<LayoutBoxUnion> layoutBoxTable;
        public DataList<ElementId>.Shared elementList;
        [NativeDisableUnsafePtrRestriction] public BurstLayoutRunner* runner;

        public void Execute() {

            // used for resolving auto size since parent shouldn't resolve auto size for ignored children
            RootLayoutBoxBurst fakeRoot = new RootLayoutBoxBurst();
            for (int i = 0; i < elementList.size; i++) {
                ElementId current = elementList[i];

                // todo -- I still need to invalidate content caches when stuff changes
                ref LayoutHierarchyInfo layoutHierarchyInfo = ref runner->GetLayoutHierarchy(current);

                if (layoutHierarchyInfo.behavior == LayoutBehavior.Ignored) {

                    ref LayoutInfo parentLayoutInfo = ref runner->GetVerticalLayoutInfo(layoutHierarchyInfo.parentId);

                    BlockSize blockSize = new BlockSize(parentLayoutInfo.finalSize, parentLayoutInfo.ContentAreaSize);

                    runner->GetHeights(fakeRoot, blockSize, current, out LayoutSize size);

                    runner->ApplyLayoutVertical(current, size.marginStart, size.marginStart, size.Clamped, parentLayoutInfo.finalSize, blockSize, LayoutFit.Default, parentLayoutInfo.finalSize);

                }

                ref LayoutInfo layoutInfo = ref runner->GetVerticalLayoutInfo(current);

               // if (layoutInfo.requiresLayout) {
                    layoutBoxTable[current].RunLayoutVertical(runner);
                    layoutInfo.requiresLayout = false;
               // }

            }

        }

    }

}