namespace UIForia.Style {

    public struct AnimationReference {

        public AnimationId animationId;
        public OptionId optionId;

        public static bool operator ==(AnimationReference a, AnimationReference b) {
            return a.animationId.id == b.animationId.id && a.optionId.id == b.optionId.id;
        }

        public static bool operator !=(AnimationReference a, AnimationReference b) {
            return !(a == b);
        }

    }

}