namespace UIForia.Elements {

    public class UISlotDefinition : UIContainerElement {

        public readonly string slotId;

        public UISlotDefinition() {
        }


        public override string GetDisplayName() {
            return "DefineSlot:" + slotId;
        }
    }
    
    public class UISlotOverride : UIElement {

        public readonly string slotId;

        public UISlotOverride() {}

        public override string GetDisplayName() {
            return "Slot:" + slotId;
        }
    }

}