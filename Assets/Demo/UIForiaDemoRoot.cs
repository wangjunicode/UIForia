using UIForia.Attributes;
using UIForia.Elements;

namespace Demo {

    [Template("Demo/UIForiaDemoRoot.xml")]
    public class UIForiaDemoRoot : UIElement {

        public UIPanel currentPanel = UIPanel.Dock;

        // public override void HandleUIEvent(UIEvent evt) {
        //     switch (evt) {
        //         case UIPanelEvent dockEvent:
        //             currentPanel = dockEvent.Panel;
        //             break;
        //         case UIWindowEvent windowEvent:
        //             switch (windowEvent.Window) {
        //                 case UIWindow.Chat:
        //                     Chat chat = FindFirstByType<Chat>();
        //                     chat.SetEnabled(!chat.isEnabled); 
        //                     break;
        //             }
        //             break;
        //     }
        // }
    }
}
