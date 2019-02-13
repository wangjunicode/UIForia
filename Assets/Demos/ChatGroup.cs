using System;
using System.Collections.Generic;

namespace Demo {

    [Serializable]
    public class ChatGroup {

        public int id;
        public string name;
        public string iconUrl;
        public int unreadCount;

        public List<ChatThreadGroup> threadGroups;

    }

}