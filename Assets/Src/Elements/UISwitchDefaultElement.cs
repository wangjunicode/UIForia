namespace UIForia {

    public class UISwitchDefaultElement : UIElement {

        public UISwitchDefaultElement() {
            flags |= UIElementFlags.ImplicitElement;
            flags &= ~(
                UIElementFlags.Enabled |
                UIElementFlags.RequiresRendering |
                UIElementFlags.RequiresLayout
            );
        }

    }

}