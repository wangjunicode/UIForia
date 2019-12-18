namespace UIForia.Elements {

    public class UISlotExtern : UIElement {

        public readonly string slotId;

        public UISlotExtern() {}

        public override string GetDisplayName() {
            return "Slot:" + slotId;
        }
    }

}