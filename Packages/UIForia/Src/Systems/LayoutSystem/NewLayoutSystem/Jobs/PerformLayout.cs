using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;

namespace UIForia.Layout {

    [BurstCompile]
    internal unsafe struct RunLayoutHorizontal : IJob {

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
                    
                    ref LayoutInfo parentLayoutInfo = ref runner->GetHorizontalLayoutInfo(layoutHierarchyInfo.parentId);

                    BlockSize blockSize = new BlockSize(parentLayoutInfo.finalSize, parentLayoutInfo.ContentAreaSize);

                    runner->GetWidths(fakeRoot, blockSize, current, out LayoutSize size);

                    runner->ApplyLayoutHorizontal(current, size.marginStart, size.marginStart, size.Clamped, parentLayoutInfo.finalSize, blockSize, LayoutFit.Default, parentLayoutInfo.finalSize);

                }

                layoutBoxTable[current].RunLayoutHorizontal(runner);
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

                layoutBoxTable[current].RunLayoutVertical(runner);
            }

        }

    }

}