using UIForia.Attributes;
using UIForia.Elements;

namespace Demo {
    
    [Template("Demo/Chat/Chat.xml")]
    public class Chat : UIElement {

        public override void OnCreate() {
            SetEnabled(false);
        }
    }
}
