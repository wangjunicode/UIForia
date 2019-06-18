namespace UIForia.Elements {
    public class UIScrollEvent : UIEvent {

        public readonly float ScrollDestinationX;
        public readonly float ScrollDestinationY;
        
        public UIScrollEvent(float scrollDestinationX, float scrollDestinationY) : base("UIScroll") {
            ScrollDestinationX = scrollDestinationX;
            ScrollDestinationY = scrollDestinationY;
        }

    }
}
