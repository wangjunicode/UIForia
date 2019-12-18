namespace UIForia.Elements {

    public class UISlotDefinition : UIContainerElement {

        public readonly string slotId;

        public UISlotDefinition() {
        }


        public override string GetDisplayName() {
            return "DefineSlot:" + slotId;
        }
    }

}