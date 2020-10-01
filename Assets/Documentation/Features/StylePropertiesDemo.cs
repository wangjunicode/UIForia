using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;

namespace Documentation.Features {
    
    [Template("Features/StylePropertiesDemo.xml")]
    public class StylePropertiesDemo : UIElement {

        public Visibility visibility;

        public void OnChangeVisibility(Visibility visibility) {
            this.visibility = visibility;
        }
    }
}
