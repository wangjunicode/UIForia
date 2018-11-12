using System.Collections.Generic;
using UIForia.Rendering;
using UIForia;
using UIForia.Layout;
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
            child?.style.PlayAnimation(ChatWindow_Styles.KeyFrameAnimateTransform());
        }
        

    }

}