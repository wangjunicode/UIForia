using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace UI {

    [Template("WindowTitleButton.xml")]
    public class WindowTitleButton : UIElement {

        [OnMouseEnter]
        public void OnMouseEnter(MouseInputEvent evt) {
            // foreach (UIElement child in FindChildAt(0).GetChildren()) {
            //     child.SetAttribute("hovered", "true");
            // }
        }

        [OnMouseExit()]
        public void OnMouseExit(MouseInputEvent evt) {
            // foreach (UIElement child in FindChildAt(0).GetChildren()) {
                // child.SetAttribute("hovered", null);
            // }
        }
    }
}