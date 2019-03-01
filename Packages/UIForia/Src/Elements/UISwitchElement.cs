namespace UIForia.Elements {

    public class UISwitchElement : UIElement {

        public UISwitchElement() {
            flags |= UIElementFlags.Enabled;
            flags |= UIElementFlags.ImplicitElement;
            flags |= UIElementFlags.BuiltIn;
           // flags &= ~(UIElementFlags.RequiresRendering | UIElementFlags.RequiresLayout);
        }

    }

}