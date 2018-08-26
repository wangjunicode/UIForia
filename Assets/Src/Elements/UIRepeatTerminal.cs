namespace Src {

    public class UIRepeatTerminal : UIElement {

        public UIRepeatTerminal() {
            flags |= UIElementFlags.ImplicitElement;
            flags &= ~(UIElementFlags.RequiresLayout | UIElementFlags.RequiresRendering);
        }

    }

}