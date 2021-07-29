using UIForia.Rendering;

namespace UIForia.Style {

    public struct AnimationOptions {

        public int iterations; // <= 0 is infinite
        public UITimeMeasurement duration; // time to play key frames
        
        public UITimeMeasurement startDelay;
        public UITimeMeasurement forwardStartDelay; // time between reverse direction and forward 
        public UITimeMeasurement reverseStartDelay; // time between forward direction and reverse
        
        public AnimationDirection direction;
        public AnimationLoopType loopType;
        public EasingFunction easingFunction; // todo add Easing.Custom?
        public AnimationFillMode fillMode;
        public int startTriggerId;
        public int completeTriggerId;

        public Bezier bezier;

    }

}