using UIForia.Elements;
using UIForia.UIInput;

namespace Demo {

    public enum UIPanel {
        Dock,
        Building,
    }

    public class UIPanelEvent : UIEvent {

        public readonly UIPanel Panel;

        public UIPanelEvent(UIPanel panel) : base("panelEvent") {
            this.Panel = panel;
        }

        public UIPanelEvent(UIPanel panel, KeyboardInputEvent keyboardInputEvent) : base("panelEvent", keyboardInputEvent) {
            this.Panel = panel;
        }
    }
}
