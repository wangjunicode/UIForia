using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UIForia.Util;
using UnityEngine;

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

        public void ScrollUp(MouseInputEvent evt) {
            TriggerEvent(new UIScrollEvent(-1, 0));
        }

        public void ScrollRight(MouseInputEvent evt) {
            TriggerEvent(new UIScrollEvent(Screen.width, -1));
        }

        public void ScrollDown(MouseInputEvent evt) {
            TriggerEvent(new UIScrollEvent(-1, Screen.height));
        }

        public void ScrollLeft(MouseInputEvent evt) {
            TriggerEvent(new UIScrollEvent(0, -1));
        }

    }

}
