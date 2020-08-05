using UIForia.Attributes;
using UIForia.Elements;

namespace SpaceGameDemo.SpacePanel {
    [Template("SpaceGameDemo/SpacePanel/SpacePanel.xml")]
    public class SpacePanel : UIElement {
        // Parameter / Property
        public string name;

        public string GetScreenState(string spacePanel) {
            return Controllers.GetSpacePanelController().GetSpacePanelState(spacePanel);
        }
    }
}