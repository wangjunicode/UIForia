using UIForia.Attributes;
using UIForia.Elements;
using UnityEngine;

namespace Documentation.DocumentationElements {
    
    [ContainerElement]
    public class ClearButton : UIElement {

        public int value;
        
        [OnKeyDown(KeyCode.Escape)]
        public void ClearInput() {
            value = 0;
        }
    }
}
