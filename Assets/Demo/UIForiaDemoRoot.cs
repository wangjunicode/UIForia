using System;
using UIForia.Attributes;
using UIForia.Elements;

namespace Demo {

    public enum UIPanel {
        Menu,
        Building
    }

    public class ApplicationState {

        public UIPanel CurrentPanel;
    }

    [Template("Demo/UIForiaDemoRoot.xml")]
    public class UIForiaDemoRoot : UIElement {

        private ApplicationState state = new ApplicationState();

        public override void OnEnable() {
            ChangePanel(UIPanel.Menu);
        }

        public override void HandleUIEvent(UIEvent evt) {
            if (evt is DockEvent dockEvent) {
                ChangePanel(dockEvent.Panel);
            }
        }

        private void ChangePanel(UIPanel panel) {
            state.CurrentPanel = panel;
            switch (panel) {
                case UIPanel.Menu:
                    FindFirstByType<Dock>()?.SetAttribute("placement", "show");
                    FindFirstByType<BuildingDesigner>()?.SetAttribute("placement", "hide");
                    break;
                case UIPanel.Building:
                    FindFirstByType<Dock>()?.SetAttribute("placement", "hide");
                    FindFirstByType<BuildingDesigner>()?.SetAttribute("placement", "show");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(panel), panel, null);
            }
        }
    }
}
