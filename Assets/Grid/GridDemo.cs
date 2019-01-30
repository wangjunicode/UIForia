using UIForia;

namespace UnityEngine {
    
    [Template("Grid/GridDemo.xml")]
    public class GridDemo : UIElement {

        private string backgroundStyleName = "background";
        private bool hover;

        public void OnHover() {
            hover = true;
        }

        public void OnHoverEnd() {
            hover = false;
        }

    }  
  
}
