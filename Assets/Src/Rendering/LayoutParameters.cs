using System.Diagnostics;
using Src.Layout;

namespace Rendering {

    public enum SizeCalculation {

        Unset,
        IgnoreScale,
        IgnoreRotation,
        AABB,
        OBB,
        TranslationAsOffsetFromLayout
    }

    public struct LayoutItemParameters {

        public int growthFactor;
        public int shrinkFactor;
        public GridPlacementParameters gridPlacement;
       // public FlexLayoutParameters flexParameters;

    }
    
    [DebuggerDisplay("{type}, {direction}")]
    public struct LayoutParameters {

        // todo -- can be compressed w/ flags
        public LayoutType type;
        public LayoutWrap wrap;
        public LayoutFlowType flow;
        public LayoutDirection direction;
        public MainAxisAlignment mainAxisAlignment;
        public CrossAxisAlignment crossAxisAlignment;

//        public bool ignoreOutOfFlowForSizing;
//        public bool ignoreScale;
//        public bool ignoreRotations;

        public LayoutParameters(
            LayoutType type,
            LayoutWrap wrap,
            LayoutFlowType flow,
            LayoutDirection direction,
            MainAxisAlignment mainAxisAlignment,
            CrossAxisAlignment crossAxisAlignment) {
            this.type = type;
            this.wrap = wrap;
            this.flow = flow;
            this.direction = direction;
            this.mainAxisAlignment = mainAxisAlignment;
            this.crossAxisAlignment = crossAxisAlignment;
        }

        public static LayoutParameters Unset => new LayoutParameters(
            LayoutType.Unset,
            LayoutWrap.Unset, 
            LayoutFlowType.Unset, 
            LayoutDirection.Unset,
            MainAxisAlignment.Unset, 
            CrossAxisAlignment.Unset
        );

        public override bool Equals(object obj) {
            if (ReferenceEquals(null, obj)) return false;
            return obj is LayoutParameters && this == ((LayoutParameters) obj);
        }

        public bool Equals(LayoutParameters other) {
            return type == other.type 
                   && wrap == other.wrap 
                   && flow == other.flow
                   && direction == other.direction 
                   && mainAxisAlignment == other.mainAxisAlignment
                   && crossAxisAlignment == other.crossAxisAlignment;
        }

        public override int GetHashCode() {
            unchecked {
                var hashCode = (int) type;
                hashCode = (hashCode * 397) ^ (int) wrap;
                hashCode = (hashCode * 397) ^ (int) flow;
                hashCode = (hashCode * 397) ^ (int) direction;
                hashCode = (hashCode * 397) ^ (int) mainAxisAlignment;
                hashCode = (hashCode * 397) ^ (int) crossAxisAlignment;
                return hashCode;
            }
        }
        
        public static bool operator ==(LayoutParameters self, LayoutParameters other) {
            return self.type == other.type
                   && self.wrap == other.wrap
                   && self.flow == other.flow
                   && self.direction == other.direction
                   && self.mainAxisAlignment == other.mainAxisAlignment
                   && self.crossAxisAlignment == other.crossAxisAlignment;
        }

        public static bool operator !=(LayoutParameters self, LayoutParameters other) {
            return !(self == other);
        }

    }

}