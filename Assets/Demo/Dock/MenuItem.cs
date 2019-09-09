using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace Demo {

    [Template("Demo/Dock/MenuItem.xml")]
    public class MenuItem : UIElement {

        public string ImageUrl;

        public string Label;

        public int NotificationCount;

        private UIElement label;

        public override void OnCreate() {
            label = FindById("label");
        }

        [OnMouseEnter()]
        public void OnEnter(MouseInputEvent evt) {
            label.SetAttribute("show", "true");
        }

        [OnMouseExit()]
        public void OnMouseExit(MouseInputEvent evt) {
            label.SetAttribute("show", null);
        }
    }
}
