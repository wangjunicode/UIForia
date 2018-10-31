using System.Collections.Generic;
using Rendering;
using Src;
using Src.Layout;
using UnityEngine;

namespace Demo {

    [Template("Demos/ChatWindow/ChatWindow.xml")]
    public class ChatWindow : UIElement {

        public ChatData chatData;
        public int selectedChatGroup;

        public override void OnReady() {
            TextAsset json = Resources.Load<TextAsset>("ChatData");
            chatData = JsonUtility.FromJson<ChatData>(json.text);
            selectedChatGroup = 0;
            
            UIElement child = FindById("child-to-animate");
            if (child != null) {
                child.style.PlayAnimation(ChatWindow_Styles.KeyFrameAnimateTransform());
            }
        }
        

    }

}