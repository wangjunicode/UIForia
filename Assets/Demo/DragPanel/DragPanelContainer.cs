using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;

namespace Demo.DragPanel {

    [Template("Demo/DragPanel/DragPanelContainer.xml")]
    public class DragPanelContainer : UIElement {

        public List<string> items = new List<string>() {
            "one", "two", "three"
        };

    }

}