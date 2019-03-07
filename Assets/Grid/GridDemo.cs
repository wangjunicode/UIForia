using UIForia.Attributes;
using UIForia.Elements;

namespace UnityEngine {
    
    [Template("Grid/GridDemo.xml")]
    public class GridDemo : UIElement {

        public string variableStyle = "varStyle0";
        
        private string backgroundStyleName = "background";
        private bool hover;

        public void OnHover() {
            hover = true;
        }

        public void OnHoverEnd() {
            hover = false;
        }

        [OnMouseClick]
        public void OnMouseClick() {
            if (variableStyle == string.Empty) {
                variableStyle = "varStyle0";
            }
            else {
                variableStyle = string.Empty;
            }
        }
    }  
  
}
