namespace UIForia.Layout.LayoutTypes {

    public struct GridTrackSize {

        public readonly float minValue;
        public readonly float maxValue;

        public readonly GridTemplateUnit minUnit;
        public readonly GridTemplateUnit maxUnit;

        public GridTrackSize(float value, GridTemplateUnit unit = GridTemplateUnit.Pixel) {
            this.minUnit = unit;
            this.minValue = value;
            this.maxUnit = unit;
            this.maxValue = value;
        }

        public static GridTrackSize Unset => new GridTrackSize(0, GridTemplateUnit.Unset);
        public static GridTrackSize MaxContent => new GridTrackSize(1f, GridTemplateUnit.MaxContent);
        public static GridTrackSize MinContent => new GridTrackSize(1f, GridTemplateUnit.MinContent);
        public static GridTrackSize Flex => new GridTrackSize(1f, GridTemplateUnit.Flex);

        public static bool operator ==(GridTrackSize a, GridTrackSize b) {
            return a.minValue == b.minValue
                   && a.minUnit == b.minUnit
                   && a.maxValue == b.maxValue
                   && a.maxUnit == b.maxUnit;
        }

        public static bool operator !=(GridTrackSize a, GridTrackSize b) {
            return !(a == b);
        }

        public static implicit operator GridTrackSize(float value) {
            return new GridTrackSize(value, GridTemplateUnit.Pixel);
        }
        
        public static implicit operator GridTrackSize(int value) {
            return new GridTrackSize(value, GridTemplateUnit.Pixel);
        }

    }

}