using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Rendering;

namespace Documentation.Features {
    
    [Template("Documentation/Features/FlexDemo.xml")]
    public class FlexDemo : UIElement {

        public bool renderSecond;

        public void ClickFirst() {
            renderSecond = true;
        }

        public override void OnCreate() {
            
            style.SetPainter("Painter1", StyleState.Normal);
            
        }
    }
}
