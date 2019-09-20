using System;
using UIForia.Attributes;
using UIForia.Elements;
using UIForia.Util;

namespace Demo {
    
    [Template("Demo/Chat/Chat.xml")]
    public class Chat : UIElement {

        public string message;

        public Action onClose;

        public override void OnCreate() {
        //    SetEnabled(false);
            onClose = () => SetEnabled(false);
        }
    }
}
