using UIForia.Parsing;
using UIForia.Util;

namespace UIForia.Elements {
    
    public abstract class UISlotBase : UIElement {

        public string slotId;

    }

    [RecordFilePath]
    public class UISlotDefinition : UISlotBase {

        internal SlotType slotType;
        
        public override string GetDisplayName() {
            return "define:" + slotId;
        }

    }

}