using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Demo {

    [Template("Documentation/Features/ScrollDemo.xml")]
    public class ScrollDemo : UIElement {

        public RepeatableList<int> list;

        public override void OnCreate() {
            list = new RepeatableList<int>();
            for (int i = 0; i < 20; i++) {
                list.Add(i);
            }
        }

    }

}