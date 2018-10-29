using Src;
using UnityEngine;

namespace Demo {

    [Template("Demos/ChatWindow/ChatMessageDisplay/ChatMessageDisplay.xml")]
    public class ChatMessageDisplay : UIElement {

        public ChatMessage message;
        public string iconUrl;

        [OnPropertyChanged(nameof(message))]
        private void OnMessageChanged(string name) {
            switch (message.from) {
                case "SNAER":
                    iconUrl = "icon_6";
                    break;
                case "EMILSSON":
                    iconUrl = "icon_4";
                    break;
                case "BERNHARDT":
                    iconUrl = "icon_8";
                    break;
            }
        }

    }

}