using UIForia.Rendering;

namespace UIForia.Style {

    internal struct TransitionDefinition {

        public ushort propertyId;
        public int delay;
        public int duration;
        public EasingFunction easing;
        public Bezier bezier;

    }
}