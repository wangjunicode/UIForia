using UIForia.Attributes;
using UIForia.Elements;

namespace Documentation.Features {

    [Template("Features/CheckboxDemo.xml")]
    public class CheckboxDemo : UIElement {

        public bool value1Checked;
        public int toggleCount;
        
        public void OnToggled() {
            toggleCount++;
        }

    }

}