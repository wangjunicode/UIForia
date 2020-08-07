using UIForia.Elements;
using UIForia.Util;
using Unity.Mathematics;

namespace UIForia.Layout {

    internal unsafe struct LayoutBoxInfo {

        public ElementId layoutParentId;
        public float2 alignedPosition;
        public float2 allocatedPosition;
        public float2 actualSize;
        public float2 allocatedSize;
        public OffsetRect margin;
        public OffsetRect border;
        public OffsetRect padding;
        public float emSize;
        public ElementId clipperId;
        public bool isCulled;
        public bool sizeChanged;
        public ScrollValues* scrollValues;

        public float allocatedWidth {
            get => allocatedSize.x;
        }
        
        public float allocatedHeight {
            get => allocatedSize.y;
        }

        public float actualWidth {
            get => actualSize.x;
        }

        public float actualHeight {
            get => actualSize.y;
        }

        public float paddingBorderStartHorizontal {
            get => padding.left + border.left;
        }

        public float paddingBorderEndHorizontal {
            get => padding.right + border.right;
        }

        public float paddingBorderStartVertical {
            get => padding.top + border.top;
        }

        public float paddingBorderEndVertical {
            get => padding.bottom + border.bottom;
        }

        public float ContentAreaWidth {
            get => actualSize.x - padding.left - border.left - padding.right - border.right;
        }

        public float ContentAreaHeight {
            get => actualSize.y - padding.top - border.top - padding.bottom - border.bottom;
        }

    }

}