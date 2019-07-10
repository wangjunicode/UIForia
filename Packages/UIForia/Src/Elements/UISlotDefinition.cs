namespace UIForia.Elements {

    public class UISlotDefinition : UIElement {

        public readonly string slotId;

        protected internal UISlotDefinition() {
            flags |= UIElementFlags.BuiltIn;
        }

        public UISlotDefinition(string slotId) : this() {
            this.slotId = slotId;
        }

        public override string GetDisplayName() {
            return "DefineSlot:" + slotId;
        }
    }
    
    public class UISlotContent : UIElement {

        public readonly string slotId;

        protected internal UISlotContent() {
            flags |= UIElementFlags.BuiltIn;
        }

        public UISlotContent(string slotId) : this() {
            this.slotId = slotId;
        }

        public override string GetDisplayName() {
            return "Slot:" + slotId;
        }
    }

}