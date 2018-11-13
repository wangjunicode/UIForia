using System;
using UIForia.Util;

namespace UIForia.Layout.LayoutTypes {

    public class FlowLayoutBox : LayoutBox {

        private FlexItem[] widths;
        private FlexItem[] heights;
        private readonly LightList<FlexTrack> tracks;
        
        public FlowLayoutBox(UIElement element) : base(element) { }

        public override void RunLayout() {
            
        }

        protected void UpdateLayoutData() {
            // figure out if we need sorting
            // dont fill grow / shrink until we need it
            // 
        }
        
        private struct FlexTrack {

            public int startItem;
            public int itemCount;
            public float mainSize;
            public float crossSize;

        }

        private struct FlexItem : IComparable<FlexItem> {

            public int childIndex;
            public float axisStart;
            public float outputSize;
            public float maxSize;
            public float minSize;
            public int growthFactor;
            public int shrinkFactor;
            public CrossAxisAlignment crossAxisAlignment;
            public int order;

            public float AxisEnd => axisStart + outputSize;

            public int CompareTo(FlexItem other) {
                if (order == other.order) {
                    return childIndex > other.childIndex ? 1 : -1;
                }

                return order > other.order ? 1 : -1;
            }

        }
        
    }


}