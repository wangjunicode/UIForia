using UIForia.Util;

namespace UIForia.Elements {

    [RecordFilePath]
    public class UISlotOverride : UISlotBase {

        public override string GetDisplayName() {
            return "override:" + slotId;
        }

    }

}