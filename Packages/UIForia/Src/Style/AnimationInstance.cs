namespace UIForia.Style {

    internal struct AnimationInstance {

        public long instanceId; // if started from code we'll also want some indication of that I think, animid is maybe negative

        // we need to resolve variables here in the scope of the origin element not the target. or some flag that says 'sourcevar' vs 'elementvar'
        public ElementId elementId; // which element this is running on

        // which properties should be written to. likely needs to be an offset to a fixed block so we can re-size if needed when more properties are added
        // public Handle_PropertyKeyBlock propertyMap;
        // public Handle_AnimationInfo info; // key frame data, trigger data, events / call backs, etc. lives in style db or if created from code will need 

        public float elapsedTotalTime;
        public float elapsedIterationTime;
        public int iterationCount;
        public int frameCount;
        public float totalProgress;
        public float iterationProgress;
        public AnimationState state;
        public int currentIteration;
        public int animationId;
        public AnimationDirection direction;
        public float duration;

    }

}