namespace Src {

    public class ChatData {

        public string name;
        public string iconUrl;
        public int unreadCount;
        public bool activityStatus;

        public ChatData(string name, string iconUrl, bool isActive, int unreadCount) {
            this.name = name;
            this.iconUrl = iconUrl;
            this.activityStatus = isActive;
            this.unreadCount = unreadCount;
        }
    }

}