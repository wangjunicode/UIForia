using UIForia.Attributes;
using UIForia.Elements;

namespace Demo {
    
    [Template("Demo/Chat/Chat.xml")]
    public class Chat : UIElement {
        
        public void SetActive(bool isActive) {
            SetAttribute("placement", isActive ? "show" : "hide");
        }

        public override void OnCreate() {
            SetActive(false);
        }

    }
}
