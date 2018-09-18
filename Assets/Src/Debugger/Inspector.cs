using Src;
using Src.Rendering;

namespace Debugger {
    
    [Template("Src/Debugger/Inspector.xml")]
    public class Inspector : UIElement {

        public override void OnCreate() {
            UIElement mask = FindById("maskthing");
            mask.style.computedStyle.overflowX = Overflow.Scroll;
            mask.style.computedStyle.overflowY = Overflow.Scroll;
        }

    }

}