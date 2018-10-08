using Rendering;

namespace Src {

    public class ChatGroup {

        public int id;
        public string name;
        public string iconUrl;
        public int unreadCount;

        public ChatGroup(string icon, string name, int unreadCount) {
            this.iconUrl = icon;
            this.name = name;
            this.unreadCount = unreadCount;
        }

    }

    [Template("Demos/ChatGroupIcon.xml")]
    public class ChatGroupIcon : UIElement {

        public ChatGroup chatGroup;

        public class Styles {

            [ExportStyle("container")]
            public static UIStyle Container() {
                return new UIStyle() {
                    PreferredWidth = 64f,
                    PreferredHeight = 64f
                };
            }
            
            [ExportStyle("chat-icon")]
            public static UIStyle ChatIcon() {
                return new UIStyle() {
                    PreferredWidth = 64f,
                    PreferredHeight = 64f
                };
            }

        }

    }

}