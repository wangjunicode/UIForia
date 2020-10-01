using System.Collections.Generic;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UIForia.Util;

namespace Documentation.Features {

    [Template("Features/ScrollDemo.xml")]
    public class ScrollDemo : UIElement {

        public List<int> list;

        public override void OnCreate() {
            list = new List<int>();
            for (int i = 0; i < 20; i++) {
                list.Add(i);
            }
        }
    }

}
