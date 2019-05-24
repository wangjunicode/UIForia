namespace UIForia.Elements {

    public class UISlotElement : UIElement {

        public readonly string slotId;

        protected internal UISlotElement() {
            flags |= UIElementFlags.BuiltIn;
        }

        public UISlotElement(string slotId) : this() {
            this.slotId = slotId;
        }

        public override string GetDisplayName() {
            return "Slot::" + slotId;
        }
    }

}