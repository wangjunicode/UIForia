namespace UIForia.Elements {

    public class UIChildrenElement : UIElement {

        internal UIChildrenElement() {
            flags |= UIElementFlags.BuiltIn;
        }

        public override string GetDisplayName() {
            return "Children";
        }

    }

}