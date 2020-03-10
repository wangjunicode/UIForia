using UIForia.Util;

namespace UIForia.Elements {
    
    public abstract class UISlotBase : UIElement {

        public string slotId;

    }

    [RecordFilePath]
    public class UISlotDefinition : UISlotBase {

        public override string GetDisplayName() {
            return "define:" + slotId;
        }

    }

}