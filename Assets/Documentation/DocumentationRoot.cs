using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Windows;

namespace Documentation {

    [Template("Documentation/DocumentationRoot.xml")]
    public class DocumentationRoot : UIWindow {

        public void ToggleMenu() {
            UIElement nav = FindById("nav");
            nav.SetAttribute("collapsed", nav.GetAttribute("collapsed") != null ? null : "true");
        }
        
    }

}
