namespace UIForia.Elements {

    public class UISlotOverride : UIElement {

        public readonly string slotId;

        public UISlotOverride() {}

        public override string GetDisplayName() {
            return "Slot:" + slotId;
        }
    }

}