using UIForia.Attributes;
using UIForia.Elements;

namespace UI {

    public class SideBarActionEvent : UIEvent {
        public SideBarActionEvent() : base("sidebaraction") {
        }
    }

    [Template("KlangWindow/SidebarItem.xml")]
    public class SidebarItem : UIElement {

        public string icon;
        public string tooltip;

        [OnMouseClick]
        public void OnClick() {
            TriggerEvent(new SideBarActionEvent());
        }
    }

}