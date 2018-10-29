using System.Collections.Generic;
using Src;

namespace Demo {

    [Template("Demos/ChatWindow/ChatThreadList/ChatThreadList.xml")]
    public class ChatThreadList : UIElement {

        public List<ChatThread> chatThreads;

    }

}