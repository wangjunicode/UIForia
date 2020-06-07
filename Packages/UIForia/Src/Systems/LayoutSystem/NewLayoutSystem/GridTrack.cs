using UIForia.Layout.LayoutTypes;
using UIForia.Systems;

namespace UIForia.Layout {

    public struct GridTrack {

        public float position;
        public float size;
        public int autoPlacementCursor;
        public float resolvedBaseSize;
        public float resolvedGrowLimit;
        public float resolvedShrinkLimit;
        public GridCellDefinition cellDefinition;
        public float maxContentContribution;
        public float minContentContribution;

        public GridTrack(in GridCellDefinition cellDefinition) {
            this.cellDefinition = cellDefinition;
            this.position = 0;
            this.size = 0;
            this.autoPlacementCursor = 0;
            this.resolvedBaseSize = 0;
            this.resolvedGrowLimit = 0;
            this.resolvedShrinkLimit = 0;
            this.maxContentContribution = 0;
            this.minContentContribution = 0;
        }

        public bool IsIntrinsic {
            get {
                return
                    (cellDefinition.baseSize.unit & GridTemplateUnit.Intrinsic) != 0 ||
                    (cellDefinition.shrinkLimit.unit & GridTemplateUnit.Intrinsic) != 0 ||
                    (cellDefinition.growLimit.unit & GridTemplateUnit.Intrinsic) != 0;
            }
        }

    }

}