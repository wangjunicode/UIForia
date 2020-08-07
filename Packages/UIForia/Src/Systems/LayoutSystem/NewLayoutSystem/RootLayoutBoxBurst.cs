using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;

namespace UIForia.Layout {

    internal unsafe struct RootLayoutBoxBurst : ILayoutBox {

        public ElementId childId;
        public ElementId elementId;

        public void OnInitialize(LayoutSystem layoutSystem, UIElement element) {
            this.elementId = element.id;
        }

        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, UIMeasurement measurement, in BlockSize blockSize) {
            return runner.viewParameters.viewWidth; // maybe content size?
        }

        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, UIMeasurement measurement, in BlockSize blockSize) {
            return runner.viewParameters.viewHeight;
        }

        public void RunHorizontal(BurstLayoutRunner* runner) {
            if (childId == default) {
                return;
            }

            BlockSize blockSize = new BlockSize() {
                outerSize = runner->viewParameters.viewWidth,
                insetSize = runner->viewParameters.viewWidth
            };

            runner->GetWidths(this, blockSize, childId, out LayoutSize layoutSize);
            runner->ApplyLayoutHorizontal(childId, 0, 0, layoutSize.Clamped, blockSize.outerSize, blockSize, LayoutFit.None, blockSize.outerSize);

        }

        public void RunVertical(BurstLayoutRunner* runner) {
            if (childId == default) {
                return;
            }

            BlockSize blockSize = new BlockSize() {
                outerSize = runner->viewParameters.viewHeight,
                insetSize = runner->viewParameters.viewHeight
            };

            runner->GetHeights(this, blockSize, childId, out LayoutSize layoutSize);
            runner->ApplyLayoutVertical(childId, 0, 0, layoutSize.Clamped, blockSize.outerSize, blockSize, LayoutFit.None, blockSize.outerSize);
        }

        public float ComputeContentWidth(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            return 0;
        }

        public float ComputeContentHeight(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            return 0;
        }

        public void OnStylePropertiesChanged(LayoutSystem layoutSystem, UIElement element, StyleProperty[] properties, int propertyCount) { }

        public void OnChildStyleChanged(LayoutSystem layoutSystem, ElementId childId, StyleProperty[] properties, int propertyCount) { }

        public float GetActualContentWidth(ref BurstLayoutRunner runner) {
            return 0;
        }

        public float GetActualContentHeight(ref BurstLayoutRunner runner) {
            return 0;
        }

        public void Dispose() { }

        public void OnChildrenChanged(LayoutSystem layoutSystem) {
            this.childId = layoutSystem.layoutHierarchyTable[elementId].firstChildId;
        }

    }

}