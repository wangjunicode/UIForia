namespace UIForia.Animation {

    public struct ProcessedStyleKeyFrame {

        public readonly float time;
        public readonly StyleKeyFrameValue value;
        
        public ProcessedStyleKeyFrame(float time, StyleKeyFrameValue value) {
            this.time = time;
            this.value = value;
        }

    }

}