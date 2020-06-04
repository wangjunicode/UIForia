using UIForia.Attributes;
using UIForia.Elements;

namespace SpaceGameDemo.SpacePanel {
    [Template("SpaceGameDemo/SpacePanel/SpacePanel.xml")]
    public class SpacePanel : UIElement {
        public string currentActivePanel;

        public string name;

        public string GetScreenState(string spacePanel) {
            return Controllers.GetSpacePanelController().GetSpacePanelState(spacePanel);
        }
    }
}