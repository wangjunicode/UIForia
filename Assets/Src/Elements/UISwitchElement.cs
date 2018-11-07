namespace UIForia {

    public class UISwitchElement : UIElement {

        public UISwitchElement() {
            flags |= UIElementFlags.Enabled;
            flags |= UIElementFlags.ImplicitElement;
            flags &= ~(UIElementFlags.RequiresRendering | UIElementFlags.RequiresLayout);
        }

    }

}