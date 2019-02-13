using UIForia;
using UnityEngine;

namespace Demo {

    [Template("Demos/ChatWindow/ChatWindow.xml")]
    public class ChatWindow : UIElement {

        public ChatData chatData;
        public int selectedChatGroup;

        public string textValue;
        
        public override void OnReady() {
            TextAsset json = Resources.Load<TextAsset>("ChatData");
            chatData = JsonUtility.FromJson<ChatData>(json.text);
            selectedChatGroup = 0;
            
            UIElement child = FindById("child-to-animate");
            child?.style.PlayAnimation(ChatWindow_Styles.KeyFrameAnimateTransform());
        }

        [OnPropertyChanged(nameof(textValue))]
        public void OnChange(string propName) {
            
        }

        public override void OnUpdate() {
//            style.SetTransformRotation(style.TransformRotation + (360f * Time.deltaTime), StyleState.Normal);
        }

    }

}