using System;
using UIForia.Attributes;
using UIForia.Elements;

namespace Demo {

    [Template("Demos/ChatWindow/ExpandableList/ExpandableList.xml")]
    public class ExpandableList : UIElement {

        public bool isExpanded;
        public event Action onShow;
        public event Action onCollapse;

    }

}