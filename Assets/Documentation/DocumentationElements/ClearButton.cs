using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace Documentation.DocumentationElements {
    public class ClearButton : UIContainerElement {

        public int value;
        
        [OnKeyDown(KeyCode.Escape)]
        public void ClearInput() {
            value = 0;
        }
    }
}
