using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace Demo.BuildingDesigner {

    [Template("Demo/BuildingDesigner/MenuItem.xml")]
    public class MenuItem : UIElement {

        public string ImageUrl;

        public string Label;

        private UIElement label;

        public override void OnCreate() {
            label = FindById("label");
        }

        [OnMouseEnter()]
        public void OnEnter(MouseInputEvent evt) {
            label.SetAttribute("show", "true");
            // label.style.SetTextColor(Color.red, StyleState.Normal);
        }

        [OnMouseExit()]
        public void OnMouseExit(MouseInputEvent evt) {
            // label.style.SetTextColor(Color.white, StyleState.Normal);
            label.SetAttribute("show", null);
        }

    }
}
