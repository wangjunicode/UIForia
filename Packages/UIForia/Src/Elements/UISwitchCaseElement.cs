namespace UIForia {

    public class UISwitchCaseElement : UIElement {

        public UISwitchCaseElement() {
            flags |= UIElementFlags.ImplicitElement;
//            flags &= ~(
//                UIElementFlags.Enabled |
//                UIElementFlags.RequiresRendering |
//                UIElementFlags.RequiresLayout
//            );
        }

    }

}