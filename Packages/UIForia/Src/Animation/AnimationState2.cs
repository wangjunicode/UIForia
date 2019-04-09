using UIForia.Elements;
using UIForia.Systems;

namespace UIForia.Animation {

    public struct AnimationState2 {

        public readonly UIElement target;

        public float elapsedTotalTime;
        public float elapsedIterationTime;
        public int iterationCount;
        public int frameCount;
        public float totalProgress;
        public float iterationProgress;
        public UITaskState status;
        public int currentIteration;

    }

}