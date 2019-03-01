namespace UIForia.Elements {

    public class UISwitchCaseElement : UIElement {

        public UISwitchCaseElement() {
            flags |= UIElementFlags.ImplicitElement;
            flags |= UIElementFlags.BuiltIn;
//            flags &= ~(
//                UIElementFlags.Enabled |
//                UIElementFlags.RequiresRendering |
//                UIElementFlags.RequiresLayout
//            );
        }

    }

}