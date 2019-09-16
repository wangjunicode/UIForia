using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace Demo {

    public enum UIPanel {
        Dock,
        Building,
        Chat,
    }

    [Template("Demo/UIForiaDemoRoot.xml")]
    public class UIForiaDemoRoot : UIElement {

        public UIPanel currentPanel = UIPanel.Dock;

        private Chat chat;

        public override void OnCreate() {
            UIView view = Application.CreateView("Colony Chat", new Rect(0,0, Screen.width, Screen.height), typeof(Chat));
            view.focusOnMouseDown = true;
            chat = (Chat)view.RootElement.GetChild(0);
        }

        public override void HandleUIEvent(UIEvent evt) {
            if (evt is UIPanelEvent dockEvent) {
                currentPanel = dockEvent.Panel;
                chat.SetActive(currentPanel == UIPanel.Chat);
            }
        }
    }
}
