namespace UIForia.Input {

    public class EventPropagator {

        public bool isConsumed;
        public bool shouldStopPropagation;
        
        public MouseState mouseState;

        public void Reset(MouseState mouseState) {
            this.mouseState = mouseState;
            shouldStopPropagation = false;
            isConsumed = false;
        }
        
    }

}