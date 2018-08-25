namespace Src {

    public class UIRepeatChild : UIElement {

        public UIRepeatChild() {
            flags |= UIElementFlags.ImplicitElement;
            flags &= ~(UIElementFlags.RequiresLayout);
            flags &= ~(UIElementFlags.RequiresRendering);
        }
        
    }

}