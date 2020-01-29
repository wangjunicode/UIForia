namespace UIForia.Elements {

    public class UISlotForward : UIElement {

        public readonly string slotId;

        public UISlotForward() {}

        public override string GetDisplayName() {
            return "Slot:" + slotId;
        }
    }

}