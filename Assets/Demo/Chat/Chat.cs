using UIForia.Attributes;
using UIForia.Elements;

namespace Demo {
    
    [Template("Demo/Chat/Chat.xml")]
    public class Chat : UIElement {

        public string message;
        
        public override void OnCreate() {
        //    SetEnabled(false);
        }
    }
}
