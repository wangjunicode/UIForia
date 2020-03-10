using UIForia.Util;

namespace UIForia.Elements {

    [RecordFilePath]
    public class UISlotForward : UISlotOverride {

        public override string GetDisplayName() {
            return "forward:" + slotId;
        }

    }

}