using UIForia.Attributes;
using UIForia.Elements;
using UIForia.UIInput;

namespace UnityEngine {
    
    [Template("Documentation/Features/OrderOrder.xml")]
    public class OrderOrder : UIElement{

        public void PrintMe(MouseInputEvent evt, UIElement element) {
            Debug.Log($"You clicked  {evt.Origin.id}, the fired {element.id} z: {element.layoutResult.zIndex}");
        }
    }
}
