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
using UnityEngine;

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
            runner->ApplyLayoutHorizontalOverride(childrenElementId, x, clampWidth, blockSize);

            float gutterJoinSize = 0;

            if (scrollValues->showVertical && scrollValues->showHorizontal) {
                gutterJoinSize = scrollValues->trackSize;
            }

            // override block size for gutter
            blockSize = new BlockSize() {
                outerSize = horizontalInfo.finalSize - gutterJoinSize,
                insetSize = horizontalInfo.finalSize - gutterJoinSize
            };

            if (scrollValues->showHorizontal) {
                if (scrollValues->showVertical && scrollValues->verticalGutterPosition == ScrollGutterSide.Min) {
                    runner->ApplyLayoutHorizontalOverride(scrollValues->horizontalTrackId, gutterJoinSize, horizontalInfo.finalSize - gutterJoinSize, blockSize);
                }
                else {
                    runner->ApplyLayoutHorizontalOverride(scrollValues->horizontalTrackId, 0, horizontalInfo.finalSize - gutterJoinSize, blockSize);
                }
            }

            if (scrollValues->showVertical) {
                if (scrollValues->verticalGutterPosition == ScrollGutterSide.Min) {
                    runner->ApplyLayoutHorizontalOverride(scrollValues->verticalTrackId, 0, scrollValues->trackSize, blockSize);
                }
                else {
                    runner->ApplyLayoutHorizontalOverride(scrollValues->verticalTrackId, horizontalInfo.finalSize - scrollValues->trackSize, scrollValues->trackSize, blockSize);
                }
            }

            scrollValues->actualWidth = horizontalInfo.finalSize - (horizontalInfo.paddingBorderStart + horizontalInfo.paddingBorderEnd + (scrollValues->showVertical ? scrollValues->trackSize : 0));

        }

        public void RunVertical(BurstLayoutRunner* runner) {

            ref BurstLayoutRunner refRunner = ref UnsafeUtility.AsRef<BurstLayoutRunner>(runner);
            ref LayoutInfo verticalInfo = ref refRunner.GetVerticalLayoutInfo(elementId);

            ref LayoutInfo hInfo = ref refRunner.horizontalLayoutInfoTable[scrollValues->verticalTrackId.index];
            ref LayoutInfo vInfo = ref refRunner.verticalLayoutInfoTable[scrollValues->verticalTrackId.index];
            hInfo.isBlockProvider = true;
            vInfo.isBlockProvider = true;

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
            float gutterJoinSize = 0;

            if (scrollValues->showVertical && scrollValues->showHorizontal) {
                gutterJoinSize = math.max(1, scrollValues->trackSize);
            }

            // override block size for gutter
            blockSize = new BlockSize() {
                outerSize = verticalInfo.finalSize - gutterJoinSize,
                insetSize = verticalInfo.finalSize - gutterJoinSize
            };

            if (scrollValues->showVertical) {
                runner->ApplyLayoutVerticalOverride(scrollValues->verticalTrackId, 0, verticalInfo.finalSize - gutterJoinSize, blockSize);
            }

            if (scrollValues->showHorizontal) {
                if (scrollValues->horizontalGutterPosition == ScrollGutterSide.Min) {
                    runner->ApplyLayoutVerticalOverride(scrollValues->horizontalTrackId, 0, scrollValues->trackSize, blockSize);
                }
                else {
                    runner->ApplyLayoutVerticalOverride(scrollValues->horizontalTrackId, verticalInfo.finalSize - scrollValues->trackSize, scrollValues->trackSize, blockSize);
                }

            }

            scrollValues->actualHeight = verticalInfo.finalSize - (verticalInfo.paddingBorderStart + verticalInfo.paddingBorderEnd + (scrollValues->showHorizontal ? scrollValues->trackSize : 0));

        }

        public void UpdateScrollValues(BurstLayoutRunner* runner) {
         //   ref LayoutInfo horizontalInfo = ref runner->GetHorizontalLayoutInfo(elementId);
         //   ref LayoutInfo verticalInfo = ref runner->GetVerticalLayoutInfo(elementId);

            scrollValues->contentWidth = FindHorizontalMax(runner);
            scrollValues->contentHeight = FindVerticalMax(runner); // + verticalInfo.paddingBorderEnd;

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
            ref LayoutInfo hInfo = ref layoutRunner.horizontalLayoutInfoTable[scrollValues->verticalTrackId.index];
            ref LayoutInfo vInfo = ref layoutRunner.verticalLayoutInfoTable[scrollValues->verticalTrackId.index];
            hInfo.isBlockProvider = true;
            vInfo.isBlockProvider = true;

            layoutRunner.GetWidths(this, blockSize, childrenElementId, out LayoutSize size);

            return size.Clamped;
        }

        public float ComputeContentHeight(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {

            layoutRunner.GetHeights(this, blockSize, childrenElementId, out LayoutSize size);

            return size.Clamped;
        }

        public void Dispose() { }

        public void OnChildrenChanged(LayoutSystem layoutSystem) {
            // we explicitly provide sizes to the gutters, make absolutely sure they are marked as block providers
            ref LayoutInfo hInfo = ref layoutSystem.horizontalLayoutInfoTable[scrollValues->verticalTrackId];
            ref LayoutInfo vInfo = ref layoutSystem.verticalLayoutInfoTable[scrollValues->verticalTrackId];
            hInfo.isBlockProvider = true;
            vInfo.isBlockProvider = true;
            hInfo = ref layoutSystem.horizontalLayoutInfoTable[scrollValues->horizontalTrackId];
            vInfo = ref layoutSystem.verticalLayoutInfoTable[scrollValues->horizontalTrackId];
            hInfo.isBlockProvider = true;
            vInfo.isBlockProvider = true;
        }

        public void OnStylePropertiesChanged(LayoutSystem layoutSystem, UIElement element, StyleProperty[] properties, int propertyCount) {
            if (childrenElementId == default) return;
            ref LayoutBoxUnion childrenBox = ref layoutSystem.layoutBoxTable[childrenElementId];
            childrenBox.OnStylePropertiesChanged(layoutSystem, layoutSystem.elementSystem.instanceTable[childrenElementId.index], properties, propertyCount);
        }

        public void OnChildStyleChanged(LayoutSystem layoutSystem, ElementId childId, StyleProperty[] properties, int propertyCount) {
            // layoutBox->OnChildStyleChanged(layoutSystem, childId, properties, propertyCount);
            ref LayoutInfo hInfo = ref layoutSystem.horizontalLayoutInfoTable[scrollValues->verticalTrackId];
            ref LayoutInfo vInfo = ref layoutSystem.verticalLayoutInfoTable[scrollValues->verticalTrackId];
            hInfo.isBlockProvider = true;
            vInfo.isBlockProvider = true;
        }

    }

}