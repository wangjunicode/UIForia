using UIForia.Rendering;

namespace UIForia.Animation {

    public struct ProcessedKeyFrame {

        public readonly float time;
        public readonly StyleKeyFrameValue value;
        
        public ProcessedKeyFrame(float time, StyleKeyFrameValue value) {
            this.time = time;
            this.value = value;
        }

    }

}