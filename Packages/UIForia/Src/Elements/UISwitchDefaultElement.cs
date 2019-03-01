namespace UIForia.Elements {

    public class UISwitchDefaultElement : UIElement {

        public UISwitchDefaultElement() {
            flags |= UIElementFlags.ImplicitElement;
            flags |= UIElementFlags.BuiltIn;
         //  flags &= ~(
                //UIElementFlags.Enabled |
               // UIElementFlags.RequiresRendering |
              //  UIElementFlags.RequiresLayout
           // );
        }

    }

}