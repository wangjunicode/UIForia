using UIForia.Attributes;
using UIForia.Elements;

namespace SpaceGameDemo.Toggle {
    
    [Template("SpaceGameDemo/Toggle/Toggle.xml")]
    public class Toggle : UIElement {
        // Parameter / Property
        public bool value;

        [OnMouseClick]
        public void OnMouseClick() {
            value = !value;
        }
    }
}