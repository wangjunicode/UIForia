using System.Collections.Generic;
using Src;

namespace Demo {

    [Template("Demos/ChatWindow/ChatTextDisplay/ChatTextDisplay.xml")]
    public class ChatTextDisplay : UIElement {

        public ChatThread thread;
        public List<ChatMessage> filteredMessages;

    }

}