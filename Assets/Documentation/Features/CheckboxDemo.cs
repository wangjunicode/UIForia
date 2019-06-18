using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.Features {

    [Template("Documentation/Features/CheckboxDemo.xml")]
    public class CheckboxDemo : UIElement {

        public bool value1Checked;
        public int toggleCount;
        
        public void OnToggled(bool val) {
            toggleCount++;
        }

    }

}