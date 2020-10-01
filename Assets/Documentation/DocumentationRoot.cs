using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation {

    [Template("DocumentationRoot.xml")]
    public class DocumentationRoot : UIElement {

        public string myValue = "hello";
        public void ToggleMenu() {
            UIElement nav = FindById("nav");
            nav.SetAttribute("collapsed", nav.GetAttribute("collapsed") != null ? null : "true");
        }
        
    }

}
