using System.Collections.Generic;
using Src;
using UnityEngine;

namespace Demo {

    public class DataStore {

        public ChatData chatData;
        public int selectedChatGroup;

    }

    [Template("Demos/ChatWindow/ChatPanel/ChatPanel.xml")]
    public class ChatPanel : UIElement {

        public ChatData chatData;
        public int selectedGroupIdx;

        public void SetSelectedChatGroup(int index) {
            Debug.Log("Set to: " + index);
        }

        public void CreateChatGroup() {
            Debug.Log("Created Group");
        }

    }

}