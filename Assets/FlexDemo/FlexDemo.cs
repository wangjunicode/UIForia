using System.Runtime.CompilerServices;
using SVGX;
using UIForia;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Parsing.Style;
using UIForia.Rendering;

namespace UnityEngine {
    
    [Template("FlexDemo/FlexDemo.xml")]
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
