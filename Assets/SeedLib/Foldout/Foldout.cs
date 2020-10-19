using UIForia.Attributes;
using UIForia.Elements;

namespace SeedLib {

    [Template("SeedLib/Foldout/Foldout.xml")]
    public class Foldout : UIElement {

        public bool expanded = true;
        public string title;

        public void ToggleExpanded() {
            expanded = !expanded;
        }
    }

}