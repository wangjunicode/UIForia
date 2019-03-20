using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;
using UnityEngine;

namespace Demo {

    [Template("InputDemo/InputDemo.xml")]
    public class InputDemo : UIElement {

        [OnMouseClick]
        public void OnMouseClick(MouseInputEvent evt) {
            
            if (evt.IsDoubleClick) {
                Debug.Log("Double");
            }

            if (evt.IsTripleClick) {
                Debug.Log("Triple");
            }
        }

    }

}