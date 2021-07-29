using UIForia.Rendering;

namespace UIForia.Style {

    internal struct Transition<T> {

        public ElementId elementId;
        public int transitionId;
        public T prevValue;
        public T nextValue;
        public int delay;
        public int duration;
        public int elapsedTime;
        public Bezier bezier;
        public TransitionState state;
        public EasingFunction easing;

    }

}