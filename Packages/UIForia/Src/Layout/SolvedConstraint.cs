namespace UIForia.Layout {

    internal struct SolveSizeSet {

        public float prefValue;
        public float minValue;
        public float maxValue;

        public SolvedSizeUnit prefUnit;
        public SolvedConstraintUnit minUnit;
        public SolvedConstraintUnit maxUnit;

    }

    internal struct SolvedConstraint {
        
        public float value;
        public SolvedConstraintUnit unit;

        public SolvedConstraint(float value, SolvedConstraintUnit unit) {
            this.value = value;
            this.unit = unit;
        }

    }
    
    internal enum SolvedConstraintUnit : byte {

        Pixel = Unit.Pixel,
        MaxChild = Unit.MaxChild,
        MinChild = Unit.MinChild,
        Content = Unit.Content,
        Percent = Unit.Percent,
        ParentSize = Unit.ParentSize,
        Animation = byte.MaxValue

    }

}