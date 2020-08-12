using UIForia.Elements;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

namespace UIForia.Layout {

    public enum ScrollBounds {

        Default

    }

    internal unsafe struct ScrollLayoutBoxBurst : ILayoutBox {

        public ElementId elementId;
        public LayoutBoxUnion* layoutBox;
        public ScrollValues* scrollValues;
        public List_Int32 childrenIds;
        public ScrollBounds scrollBounds;

        public void OnInitialize(LayoutSystem layoutSystem, UIElement element) {
            elementId = element.id;
            childrenIds = new List_Int32(8, Allocator.Persistent);
            LayoutBoxType boxType = LayoutBoxUnion.GetLayoutBoxTypeForProxy(element);
            layoutBox = TypedUnsafe.Malloc<LayoutBoxUnion>(Allocator.Persistent);
            layoutBox->Initialize(boxType, layoutSystem, element);
            scrollValues = ((ScrollView) element).GetScrollValues();
            scrollBounds = ScrollBounds.Default; // todo 
        }

        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return layoutBox->ResolveAutoWidth(ref runner, elementId, blockSize);
        }

        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return layoutBox->ResolveAutoHeight(ref runner, elementId, blockSize);
        }

        public void RunHorizontal(BurstLayoutRunner* runner) {
            // todo -- scroll tracks & handles
            // probably want a special layoutbox type for those
            ref BurstLayoutRunner refRunner = ref UnsafeUtilityEx.AsRef<BurstLayoutRunner>(runner);
            layoutBox->RunLayoutHorizontal(runner);
            scrollValues->actualWidth = refRunner.GetHorizontalLayoutInfo(elementId).finalSize;
            scrollValues->contentWidth = layoutBox->GetActualContentWidth(ref refRunner);
            scrollValues->isOverflowingX = scrollValues->contentWidth > scrollValues->actualWidth;
           // runner->scrollWidthUpdates.Add(elementId);
        }

        public float FindHorizontalMax(BurstLayoutRunner* runner) {
            ref LayoutHierarchyInfo hierarchyInfo = ref runner->GetLayoutHierarchy(elementId);
            ElementId ptr = hierarchyInfo.firstChildId;

            float xMax = float.MinValue;

            while (ptr != default) {
                
                ref LayoutBoxInfo boxInfo = ref runner->layoutBoxInfoTable[ptr.index];

                if (xMax < boxInfo.alignedPosition.x + boxInfo.actualSize.x) xMax = boxInfo.alignedPosition.x + boxInfo.actualSize.x;

                ptr = hierarchyInfo.nextSiblingId;
            }

            return 0;
        }

        public void RunVertical(BurstLayoutRunner* runner) {
            // todo -- scroll tracks & handles
            // probably want a special layoutbox type for those
            ref BurstLayoutRunner refRunner = ref UnsafeUtilityEx.AsRef<BurstLayoutRunner>(runner);
            layoutBox->RunLayoutVertical(runner);
            scrollValues->actualHeight = refRunner.GetVerticalLayoutInfo(elementId).finalSize;
            scrollValues->contentHeight = layoutBox->GetActualContentHeight(ref refRunner);
            scrollValues->isOverflowingY = scrollValues->contentHeight > scrollValues->actualHeight;
        }

        public float ComputeContentWidth(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            return layoutBox->ComputeContentWidth(ref layoutRunner, blockSize);
        }

        public float ComputeContentHeight(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            return layoutBox->ComputeContentHeight(ref layoutRunner, blockSize);
        }

        public void Dispose() {
            layoutBox->Dispose();
            // I don't think I need to handle setting children scrollValues ptr to null because if I am disposing this box, all children boxes are disposed as well 
            TypedUnsafe.Dispose(layoutBox, Allocator.Persistent);
            childrenIds.Dispose();
            layoutBox = null;
        }

        public void OnChildrenChanged(LayoutSystem layoutSystem) {
            // might not be needed but safer to do this
            for (int i = 0; i < childrenIds.size; i++) {
                layoutSystem.layoutResultTable.array[childrenIds.array[i]].scrollValues = null;
            }

            // todo -- make sure the scrollbars are not included in the children

            layoutBox->OnChildrenChanged(layoutSystem);
            ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutSystem.layoutHierarchyTable[elementId];

            int childCount = layoutHierarchyInfo.childCount;
            childrenIds.size = 0;

            if (childCount == 0) {
                return;
            }

            childrenIds.SetSize(childCount);
            ElementId ptr = layoutHierarchyInfo.firstChildId;

            int idx = 0;
            while (ptr != default) {
                childrenIds.array[idx++] = ptr.index;
                layoutSystem.layoutResultTable.array[ptr.index].scrollValues = scrollValues;
                ptr = layoutSystem.layoutHierarchyTable[ptr].nextSiblingId;
            }
        }

        public void OnStylePropertiesChanged(LayoutSystem layoutSystem, UIElement element, StyleProperty[] properties, int propertyCount) {
            layoutBox->OnStylePropertiesChanged(layoutSystem, element, properties, propertyCount);
        }

        public void OnChildStyleChanged(LayoutSystem layoutSystem, ElementId childId, StyleProperty[] properties, int propertyCount) {
            layoutBox->OnChildStyleChanged(layoutSystem, childId, properties, propertyCount);
        }

        public float GetActualContentWidth(ref BurstLayoutRunner runner) {
            return 0;
        }

        public float GetActualContentHeight(ref BurstLayoutRunner runner) {
            return 0;
        }

    }

}