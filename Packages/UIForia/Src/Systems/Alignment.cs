namespace UIForia.Systems {

    public struct Alignment {

        public float offset;
        public float pivot;
        public AlignmentTarget target;

        public Alignment(float offset, float pivot, AlignmentTarget target) {
            this.offset = offset;
            this.pivot = pivot;
            this.target = target;
        }

    }

    public enum AlignmentTarget {

        ActualBox,
        AllocatedBox,
        Parent,
        ParentContentArea,
        View,
        Application,
        Screen

    }
    

}