namespace UIForia.Elements {
    public class UIEvent {

        public UIElement origin;

        public readonly string eventType;
        
        private bool propagating;

        protected UIEvent(string eventType) {
            this.eventType = eventType;
            propagating = true;
        }

        public void StopPropagation() {
            propagating = false;
        }

        public bool IsPropagating() {
            return propagating;
        }
    }
    
}
