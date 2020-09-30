using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;

namespace UIForia.Layout {

    // stack ignores extra space distribution since we never have any, items are always allocated the entire space so 
    // that fit properties will fill the whole layout box. aligning items works though
    internal unsafe struct StackLayoutBoxBurst : ILayoutBox {

        public ElementId elementId;
        public float alignHorizontal;
        public float alignVertical;
        public LayoutFit fitHorizontal;
        public LayoutFit fitVertical;

        public void OnInitialize(LayoutSystem layoutSystem, UIElement element, UIElement proxy) {
            this.elementId = element.id;
            alignHorizontal = proxy.style.AlignItemsHorizontal;
            alignVertical = proxy.style.AlignItemsVertical;
            fitHorizontal = proxy.style.FitItemsHorizontal;
            fitVertical = proxy.style.FitItemsVertical;
        }

        public void Dispose() { }

        public void OnChildrenChanged(LayoutSystem layoutSystem) { }

        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return 0;
        }

        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return 0;
        }

        public void RunHorizontal(BurstLayoutRunner* runner) {
            ref LayoutHierarchyInfo layoutHierarchyInfo = ref runner->GetLayoutHierarchy(elementId);
            ref LayoutInfo info = ref runner->GetHorizontalLayoutInfo(elementId);

            float contentAreaWidth = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd);

            float alignment = alignHorizontal;

            float inset = info.paddingBorderStart;

            BlockSize blockSize = info.parentBlockSize;

            if (info.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = info.finalSize,
                    insetSize = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd)
                };
            }

            ElementId ptr = layoutHierarchyInfo.firstChildId;

            // todo -- if positioned via align items, we should set the local position of the element, not the aligned position
            // that way we still play nice with the element's own alignment values 
            while (ptr != default) {
                runner->GetWidths(this, blockSize, ptr, out LayoutSize size);
                float clampWidth = size.Clamped + size.marginStart + size.marginEnd;
                float x = inset + size.marginStart;
                float originBase = x;
                float originOffset = contentAreaWidth * alignment;
                float alignedPosition = originBase + originOffset + (clampWidth * -alignment);
                runner->ApplyLayoutHorizontal(ptr, x, alignedPosition, clampWidth, contentAreaWidth, blockSize, LayoutFit.None, info.finalSize);
                ptr = runner->GetLayoutHierarchy(ptr).nextSiblingId;
            }

        }

        public void RunVertical(BurstLayoutRunner* runner) {
            ref LayoutHierarchyInfo layoutHierarchyInfo = ref runner->GetLayoutHierarchy(elementId);
            ref LayoutInfo info = ref runner->GetVerticalLayoutInfo(elementId);

            float contentAreaHeight = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd);

            float alignment = alignVertical;

            float inset = info.paddingBorderStart;

            BlockSize blockSize = info.parentBlockSize;

            if (info.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = info.finalSize,
                    insetSize = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd)
                };
            }

            ElementId ptr = layoutHierarchyInfo.firstChildId;

            while (ptr != default) {
                runner->GetHeights(this, blockSize, ptr, out LayoutSize size);
                float clampedHeight = size.Clamped;

                float y = inset + size.marginStart;
                float originBase = y;
                float originOffset = contentAreaHeight * alignment;
                float alignedPosition = originBase + originOffset + (clampedHeight * -alignment);
                runner->ApplyLayoutVertical(ptr, y, alignedPosition, clampedHeight, contentAreaHeight, blockSize, LayoutFit.None, info.finalSize);
                ptr = runner->GetLayoutHierarchy(ptr).nextSiblingId;
            }

        }

        public float ComputeContentWidth(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {

            ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutRunner.GetLayoutHierarchy(elementId);

            ElementId ptr = layoutHierarchyInfo.firstChildId;
            float retn = 0f;

            while (ptr != default) {
                layoutRunner.GetWidths(this, blockSize, ptr, out LayoutSize size);
                float clampWidth = size.Clamped + size.marginStart + size.marginEnd;
                if (clampWidth > retn) retn = clampWidth;
                ptr = layoutRunner.GetLayoutHierarchy(ptr).nextSiblingId;
            }

            return retn;
        }

        public float ComputeContentHeight(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {

            ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutRunner.GetLayoutHierarchy(elementId);

            ElementId ptr = layoutHierarchyInfo.firstChildId;
            float retn = 0f;

            while (ptr != default) {
                layoutRunner.GetHeights(this, blockSize, ptr, out LayoutSize size);
                float clampedHeight = size.Clamped + size.marginStart + size.marginEnd;
                if (clampedHeight > retn) retn = clampedHeight;
                ptr = layoutRunner.GetLayoutHierarchy(ptr).nextSiblingId;
            }

            return retn;
        }

        public void OnStylePropertiesChanged(LayoutSystem layoutSystem, UIElement element, StyleProperty[] propertyList, int propertyCount) {
            for (int i = 0; i < propertyCount; i++) {
                // note: space distribution is ignored, we don't care if it changes
                switch (propertyList[i].propertyId) {
                    case StylePropertyId.AlignItemsHorizontal:
                        alignHorizontal = propertyList[i].AsFloat;
                        break;

                    case StylePropertyId.FitItemsHorizontal:
                        fitHorizontal = propertyList[i].AsLayoutFit;
                        break;

                    case StylePropertyId.AlignItemsVertical:
                        alignVertical = propertyList[i].AsFloat;
                        break;

                    case StylePropertyId.FitItemsVertical:
                        fitVertical = propertyList[i].AsLayoutFit;
                        break;
                }
            }
        }

        public void OnChildStyleChanged(LayoutSystem layoutSystem, ElementId childId, StyleProperty[] properties, int propertyCount) { }

        public float GetActualContentWidth(ref BurstLayoutRunner runner) {
            return 0;
        }

        public float GetActualContentHeight(ref BurstLayoutRunner runner) {
            return 0;
        }

    }

}