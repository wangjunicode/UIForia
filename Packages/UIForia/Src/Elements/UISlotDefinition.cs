namespace UIForia.Elements {

    public class UISlotDefinition : UIContainerElement {

        public readonly string slotId;

        public UISlotDefinition() {
        }


        public override string GetDisplayName() {
            return "DefineSlot:" + slotId;
        }
    }
    
    public class UISlotContent : UIElement {

        public readonly string slotId;

        public UISlotContent() {}

        public override string GetDisplayName() {
            return "Slot:" + slotId;
        }
    }

}