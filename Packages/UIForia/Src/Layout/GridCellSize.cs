using UIForia.Layout.LayoutTypes;

namespace UIForia.Systems {

    public struct GridCellSize {

        public float value;
        public GridTemplateUnit unit;

        public GridCellSize(float value, GridTemplateUnit unit) {
            this.value = value;
            this.unit = unit;
        }

    }

}