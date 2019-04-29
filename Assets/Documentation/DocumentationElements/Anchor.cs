using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.DocumentationElements {

    [Template("Documentation/DocumentationElements/Anchor")]
    public class Anchor : UIElement {

        public string href;

        [OnMouseClick()]
        public void OnClick() {
            Application.RoutingSystem.FindRouter("demo").GoTo(href);
        }
    }
}
