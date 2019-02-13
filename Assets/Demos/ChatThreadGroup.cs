using System;
using System.Collections.Generic;

namespace Demo {

    [Serializable]
    public class ChatThreadGroup {

        public string name;
        public int selected;
        public List<ChatThread> threads;
    
    }

}