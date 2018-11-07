using System;
using UIForia;

namespace Demo {

    [Template("Demos/ChatWindow/ExpandableList/ExpandableList.xml")]
    public class ExpandableList : UIElement {

        public bool isExpanded;
        public event Action onShow;
        public event Action onCollapse;

    }

}