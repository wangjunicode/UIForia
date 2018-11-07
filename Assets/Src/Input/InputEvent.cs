namespace UIForia.Input {

    public class InputEvent {

        public readonly InputEventType type;
        private bool m_ShouldStopPropagation;
        private bool m_ShouldStopLateralPropagation;

        public InputEvent(InputEventType type) {
            this.type = type;
        }

        public bool ShouldStopPropagation => m_ShouldStopPropagation;
        public bool ShouldStopPropagationImmediately => m_ShouldStopLateralPropagation;

        public void StopPropagation() {
            m_ShouldStopPropagation = true;
        }

        public void StopPropagationImmediately() {
            m_ShouldStopLateralPropagation = true;
        }

    }

}