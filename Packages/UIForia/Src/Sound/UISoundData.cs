namespace UIForia.Sound {
    public struct UISoundData {
        public string Name;
        public string StyleSheetFileName;

        public string Asset;
        public float Pitch;
        public float Volume;
        public FloatRange PitchRange;
        public float Tempo;
        public UITimeMeasurement Duration;
        public int Iterations;
        public string MixerGroup;
    }
}
