using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;

namespace UIForia.Layout {

    internal unsafe struct RadialLayoutBoxBurst : ILayoutBox {

        public ElementId elementId;

        public void OnInitialize(LayoutSystem layoutSystem, UIElement element) {
            this.elementId = element.id;
        }

        public void Dispose() { }

        public void RunHorizontal(BurstLayoutRunner* runner) { }

        public void RunVertical(BurstLayoutRunner* runner) { }

        public float ComputeContentWidth(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            return 0;
        }

        public float ComputeContentHeight(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            return 0;
        }

        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return 0;
        }

        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return 0;
        }

        public void OnStylePropertiesChanged(LayoutSystem layoutSystem, UIElement element, StyleProperty[] properties, int propertyCount) { }

        public void OnChildStyleChanged(LayoutSystem layoutSystem, ElementId childId, StyleProperty[] properties, int propertyCount) { }

        public void OnChildrenChanged(LayoutSystem layoutSystem) { }

    }

}