using UIForia.Style;

namespace UIForia {

    public enum AnimationCommandType {

        Play,
        Pause,
        Stop,
        Reset

    }

    internal struct AnimationCommand {

        public AnimationCommandType type;
        public int executionToken; // optional
        public ElementId elementId; // optional
        public AnimationReference animationReference;

    }

}