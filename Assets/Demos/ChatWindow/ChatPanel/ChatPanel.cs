using System.Collections.Generic;
using UIForia;
using UnityEngine;

namespace Demo {

    [Template("Demos/ChatWindow/ChatPanel/ChatPanel.xml")]
    public class ChatPanel : UIElement {

        public ChatData chatData;
        public int selectedGroupIdx;
        public int selectedThreadGroupIdx;
        public int selectedThreadIdx;

        public void SetSelectedChatGroup(int index) {
            Debug.Log("Set to: " + index);
        }

        public void CreateChatGroup() {
            Debug.Log("Created Group");
        }

    }

}