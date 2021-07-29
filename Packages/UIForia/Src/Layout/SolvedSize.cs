namespace UIForia.Layout {

    internal struct SolvedSize {

        public float value;
        public SolvedSizeUnit unit;

        public SolvedSize(float value, SolvedSizeUnit unit) {
            this.value = value;
            this.unit = unit;
        }
        
        public bool IsFixed {
            get { return unit == SolvedSizeUnit.Pixel; }
        }

    }
    
    internal enum SolvedSizeUnit : byte {

        Pixel = Unit.Pixel,
        Controlled = Unit.Controlled,
        Percent = Unit.Percent,
        Stretch = Unit.Stretch,
        MaxChild = Unit.MaxChild,
        MinChild = Unit.MinChild,
        Content = Unit.Content,
        StretchContent = Unit.StretchContent,
        FitContent = Unit.FitContent,
        ParentSize = Unit.ParentSize,
        FillRemaining = Unit.FillRemaining,
        Animation = byte.MaxValue,

    }

}