namespace UIForia.Elements {
    public class UIScrollEvent : UIEvent {

        public readonly float ScrollDestinationX;
        public readonly float ScrollDestinationY;
        
        /// <summary>
        /// Trigger this event in any child of a ScrollView to scroll to the desired position.
        /// As for the values: make sure to normalize your scroll position between 0 and 1 (both inclusive).
        /// 
        /// </summary>
        /// <param name="scrollDestinationX">Relative scroll position normalized between 0 and 1</param>
        /// <param name="scrollDestinationY">Relative scroll position normalized between 0 and 1</param>
        public UIScrollEvent(float scrollDestinationX, float scrollDestinationY) : base("UIScroll") {
            ScrollDestinationX = scrollDestinationX;
            ScrollDestinationY = scrollDestinationY;
        }

    }
}
