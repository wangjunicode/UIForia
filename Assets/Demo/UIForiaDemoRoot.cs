using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;

namespace Demo {

    [Template("Demo/UIForiaDemoRoot.xml")]
    public class UIForiaDemoRoot : UIElement {

        public UIPanel currentPanel = UIPanel.Dock;

        public override void HandleUIEvent(UIEvent evt) {
            switch (evt) {
                case UIPanelEvent dockEvent:
                    currentPanel = dockEvent.Panel;
                    break;
                case UIWindowEvent windowEvent:
                    switch (windowEvent.Window) {
                        case UIWindow.Chat:
                            Chat chat = FindFirstByType<Chat>();
                            chat.SetEnabled(!chat.isEnabled);
                            chat.style.SetVisibility(chat.isEnabled ? Visibility.Visible : Visibility.Hidden, StyleState.Normal);
                            break;
                    }
                    break;
            }
        }
    }
}
