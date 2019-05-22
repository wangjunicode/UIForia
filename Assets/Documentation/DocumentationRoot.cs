using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation {

    [Template("Documentation/DocumentationRoot.xml")]
    public class DocumentationRoot : UIElement {

        public static string defaultRoute = "/index";

        public override void OnCreate() {
            Application.RoutingSystem.FindRouter("demo").GoTo(defaultRoute);
        }
    }

}
