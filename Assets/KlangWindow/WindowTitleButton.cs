using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace UI {

    [Template("KlangWindow/WindowTitleButton.xml")]
    public class WindowTitleButton : UIElement {

        [OnMouseEnter]
        public void OnMouseEnter(MouseInputEvent evt) {
            foreach (var child in GetChild(0).GetChildren()) {
                child.SetAttribute("hovered", "true");
            }
        }

        [OnMouseExit()]
        public void OnMouseExit(MouseInputEvent evt) {
            foreach (var child in GetChild(0).GetChildren()) {
                child.SetAttribute("hovered", null);
            }
        }
    }
}