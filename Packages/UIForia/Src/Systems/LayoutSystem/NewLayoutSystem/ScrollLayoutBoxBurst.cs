using UIForia.Elements;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Layout {

    public enum ScrollBounds {

        Default

    }

    [BurstCompile]
    internal unsafe struct UpdateScrollVertical : IJob {

        public ElementTable<LayoutBoxUnion> layoutBoxTable;

        [NativeDisableUnsafePtrRestriction] public BurstLayoutRunner* runner;
        public ElementId scrollBoxHead;

        public void Execute() {
            ElementId ptr = scrollBoxHead;
            while (ptr != default) {
                ref LayoutBoxUnion box = ref layoutBoxTable.array[ptr.index];
                box.scroll.UpdateScrollValues(runner);
                ptr = box.scroll.nextScrollBox;
            }
        }

    }

    internal unsafe struct ScrollLayoutBoxBurst : ILayoutBox {

        public ElementId elementId;

        public ScrollValues* scrollValues;
        public ScrollBounds scrollBounds;
        public ElementId childrenElementId;
        public ElementId nextScrollBox;
        public ElementId prevScrollBox;

        public void OnInitialize(LayoutSystem layoutSystem, UIElement element, UIElement proxy) {
            elementId = element.id;
            childrenElementId = element.GetFirstChild().id;
            scrollValues = ((ScrollView) element).GetScrollValues();
            scrollBounds = ScrollBounds.Default; // todo 
            layoutSystem.RegisterScrollBox(elementId); // todo -- unregister or we probably crash !!!!! 
        }

        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return 0;
        }

        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return 0;
        }

        public void RunHorizontal(BurstLayoutRunner* runner) {
            ref BurstLayoutRunner refRunner = ref UnsafeUtility.AsRef<BurstLayoutRunner>(runner);
            ref LayoutInfo horizontalInfo = ref refRunner.GetHorizontalLayoutInfo(elementId);
            float contentAreaWidth = horizontalInfo.finalSize - (horizontalInfo.paddingBorderStart + horizontalInfo.paddingBorderEnd);

            float inset = horizontalInfo.paddingBorderStart;
            BlockSize blockSize = horizontalInfo.parentBlockSize;

            if (horizontalInfo.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = horizontalInfo.finalSize,
                    insetSize = horizontalInfo.finalSize - (horizontalInfo.paddingBorderStart + horizontalInfo.paddingBorderEnd)
                };
            }

            runner->GetWidths(this, blockSize, childrenElementId, out LayoutSize size);
            float clampWidth = size.Clamped + size.marginStart + size.marginEnd;
            float x = inset + size.marginStart;
            runner->ApplyLayoutHorizontal(childrenElementId, x, x, clampWidth, contentAreaWidth, blockSize, LayoutFit.None, horizontalInfo.finalSize);

            // assume its there for now (and enabled)
            if (scrollValues->showVertical && scrollValues->verticalTrackId.index != 0) {
                if (scrollValues->verticalGutterPosition == ScrollGutterSide.Min) {
                    runner->ApplyLayoutHorizontalOverride(scrollValues->verticalTrackId, 0, math.min(1, scrollValues->trackSize));
                }
                else {
                    runner->ApplyLayoutHorizontalOverride(scrollValues->verticalTrackId, scrollValues->trackSize, scrollValues->trackSize);
                }
            }

            scrollValues->actualWidth = horizontalInfo.finalSize;
        }

        public void RunVertical(BurstLayoutRunner* runner) {
            ref BurstLayoutRunner refRunner = ref UnsafeUtility.AsRef<BurstLayoutRunner>(runner);
            ref LayoutInfo verticalInfo = ref refRunner.GetVerticalLayoutInfo(elementId);

            // float contentAreaHeight = verticalInfo.finalSize - (verticalInfo.paddingBorderStart + verticalInfo.paddingBorderEnd);

            float inset = verticalInfo.paddingBorderStart;
            BlockSize blockSize = verticalInfo.parentBlockSize;

            if (verticalInfo.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = verticalInfo.finalSize,
                    insetSize = verticalInfo.finalSize - (verticalInfo.paddingBorderStart + verticalInfo.paddingBorderEnd)
                };
            }

            runner->GetHeights(this, blockSize, childrenElementId, out LayoutSize size);
            float clampHeight = size.Clamped + size.marginStart + size.marginEnd;
            float y = inset + size.marginStart;
            runner->ApplyLayoutVerticalOverride(childrenElementId, y, clampHeight, blockSize);

            if (scrollValues->showVertical && scrollValues->verticalTrackId.index != 0) {
                runner->ApplyLayoutVerticalOverride(scrollValues->verticalTrackId, 0, verticalInfo.finalSize, blockSize);
            }

            scrollValues->actualHeight = verticalInfo.finalSize;
        }

        public void UpdateScrollValues(BurstLayoutRunner* runner) {
            ref LayoutInfo horizontalInfo = ref runner->GetHorizontalLayoutInfo(elementId);
            ref LayoutInfo verticalInfo = ref runner->GetVerticalLayoutInfo(elementId);

            scrollValues->contentWidth = FindHorizontalMax(runner) + horizontalInfo.paddingBorderEnd;
            scrollValues->contentHeight = FindVerticalMax(runner) + verticalInfo.paddingBorderEnd;

            ref LayoutBoxInfo info = ref runner->GetLayoutBoxInfo(childrenElementId);
            info.scrollValues = scrollValues;
        }

        public float FindHorizontalMax(BurstLayoutRunner* runner) {
            ref LayoutHierarchyInfo hierarchyInfo = ref runner->layoutHierarchyTable[childrenElementId.index];
            ElementId ptr = hierarchyInfo.firstChildId;
            float xMax = 0;

            while (ptr != default) {
                ref LayoutBoxInfo boxInfo = ref runner->layoutBoxInfoTable[ptr.index];
                // todo -- text needs to be done based on computed bounds size, not 'actualSize' because lines can overflow the 'actualSize' values
                if (xMax < boxInfo.alignedPosition.x + boxInfo.actualSize.x) xMax = boxInfo.alignedPosition.x + boxInfo.actualSize.x;

                ptr = runner->layoutHierarchyTable[ptr.index].nextSiblingId;
            }

            return xMax;
        }

        public float FindVerticalMax(BurstLayoutRunner* runner) {
            ref LayoutHierarchyInfo hierarchyInfo = ref runner->GetLayoutHierarchy(childrenElementId);
            ElementId ptr = hierarchyInfo.firstChildId;

            float yMax = 0;

            while (ptr != default) {
                ref LayoutBoxInfo boxInfo = ref runner->layoutBoxInfoTable[ptr.index];

                if (yMax < boxInfo.alignedPosition.y + boxInfo.actualSize.y) yMax = boxInfo.alignedPosition.y + boxInfo.actualSize.y;

                ptr = runner->layoutHierarchyTable[ptr.index].nextSiblingId;
            }

            return yMax;
        }

        public float ComputeContentWidth(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            layoutRunner.GetWidths(this, blockSize, childrenElementId, out LayoutSize size);

            return size.Clamped;

            // return layoutBox->ComputeContentWidth(ref layoutRunner, blockSize);
        }

        public float ComputeContentHeight(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            layoutRunner.GetHeights(this, blockSize, childrenElementId, out LayoutSize size);

            return size.Clamped;

            // return layoutBox->ComputeContentHeight(ref layoutRunner, blockSize);
        }

        public void Dispose() {
            // layoutBox->Dispose();
            // I don't think I need to handle setting children scrollValues ptr to null because if I am disposing this box, all children boxes are disposed as well 
            // TypedUnsafe.Dispose(layoutBox, Allocator.Persistent);
            // childrenIds.Dispose();
            // layoutBox = null;
        }

        public void OnChildrenChanged(LayoutSystem layoutSystem) {
            // might not be needed but safer to do this
            // for (int i = 0; i < childrenIds.size; i++) {
            //     layoutSystem.layoutResultTable.array[childrenIds.array[i]].scrollValues = null;
            // }
            //
            // // todo -- make sure the scrollbars are not included in the children
            //
            // // layoutBox->OnChildrenChanged(layoutSystem);
            // ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutSystem.layoutHierarchyTable[elementId];
            //
            // int childCount = layoutHierarchyInfo.childCount;
            // childrenIds.size = 0;
            //
            // if (childCount == 0) {
            //     return;
            // }
            //
            // childrenIds.SetSize(childCount);
            // ElementId ptr = layoutHierarchyInfo.firstChildId;
            //
            // int idx = 0;
            // while (ptr != default) {
            //     childrenIds.array[idx++] = ptr.index;
            //     layoutSystem.layoutResultTable.array[ptr.index].scrollValues = scrollValues;
            //     ptr = layoutSystem.layoutHierarchyTable[ptr].nextSiblingId;
            // }
        }

        public void OnStylePropertiesChanged(LayoutSystem layoutSystem, UIElement element, StyleProperty[] properties, int propertyCount) {
            if (childrenElementId == default) return;
            ref LayoutBoxUnion childrenBox = ref layoutSystem.layoutBoxTable[childrenElementId];
            childrenBox.OnStylePropertiesChanged(layoutSystem, layoutSystem.elementSystem.instanceTable[childrenElementId.index], properties, propertyCount);
        }

        public void OnChildStyleChanged(LayoutSystem layoutSystem, ElementId childId, StyleProperty[] properties, int propertyCount) {
            // layoutBox->OnChildStyleChanged(layoutSystem, childId, properties, propertyCount);
        }

    }

}