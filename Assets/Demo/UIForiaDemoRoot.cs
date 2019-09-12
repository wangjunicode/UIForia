using UIForia.Attributes;
using UIForia.Elements;

namespace Demo {

    public enum UIPanel {
        Dock,
        Building
    }

    [Template("Demo/UIForiaDemoRoot.xml")]
    public class UIForiaDemoRoot : UIElement {

        public UIPanel currentPanel = UIPanel.Dock;

        public override void HandleUIEvent(UIEvent evt) {
            if (evt is UIPanelEvent dockEvent) {
                currentPanel = dockEvent.Panel;
            }
        }
    }
}
