namespace Src.Elements {

    public abstract class UIPrimitiveElement : UIElement {

        protected UIPrimitiveElement() {
            flags |= UIElementFlags.Primitive;
        }

    }

}