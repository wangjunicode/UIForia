using Src.Rendering;

namespace Src.Animation {

    public struct AnimationOptions {

        public float loopTime;
        public int iterations;
        public float delay;
        public float duration;
        public float forwardStartDelay;
        public float reverseStartDelay;
        public AnimationDirection direction;
        public EasingFunction timingFunction;

        public AnimationOptions(float duration, float loopTime = -1) {
            this.duration = duration;
            this.loopTime = loopTime;
            this.iterations = 1;
            this.direction = AnimationDirection.Forward;
            this.delay = 0f;
            this.forwardStartDelay = 0f;
            this.reverseStartDelay = 0f;
            this.timingFunction = EasingFunction.Linear;
        }

        public AnimationOptions(float duration, EasingFunction easing) {
            this.duration = duration;
            this.iterations = 1;
            this.loopTime = 0f;
            this.delay = 0f;
            this.forwardStartDelay = 0f;
            this.reverseStartDelay = 0f;
            this.timingFunction = easing;
            this.direction = AnimationDirection.Forward;
        }

    }

}