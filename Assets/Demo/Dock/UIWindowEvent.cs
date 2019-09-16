using UIForia.Elements;
using UIForia.UIInput;

namespace Demo {
    
    public enum UIWindow {
        Chat,
    }
    
    public class UIWindowEvent : UIEvent {

        public readonly UIWindow Window;
        
        public UIWindowEvent(UIWindow window) : base("windowEvent") {
            this.Window = window;
        }

        public UIWindowEvent(UIWindow window, KeyboardInputEvent keyboardInputEvent) : base("windowEvent", keyboardInputEvent) {
            this.Window = window;
        }
    }
}
