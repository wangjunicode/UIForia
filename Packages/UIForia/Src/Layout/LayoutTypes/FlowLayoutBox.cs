using System;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Layout.LayoutTypes {

    public class FlowLayoutBox : LayoutBox {

        private static readonly LightList<FlexItem> widths;
        private static readonly LightList<FlexItem> heights;
        private static readonly LightList<FlexTrack> tracks;

        static FlowLayoutBox() {
            widths = new LightList<FlexItem>();
            heights = new LightList<FlexItem>();
            tracks = new LightList<FlexTrack>();
        }
        
        public FlowLayoutBox(UIElement element) : base(element) {}

        public override void RunLayout() {
            if (children.Count == 0) {
                actualWidth = allocatedWidth; //GetWidths().clampedSize;
                actualHeight = allocatedHeight; //GetHeights(actualWidth).clampedSize;
                return;
            }

            tracks.Clear(); // todo -- recycle theses
            if (style.FlexLayoutDirection == LayoutDirection.Column) {
                RunColumnLayout();
            }
            else {
                RunRowLayout();
            }
        }

        protected void RunColumnLayout() {
            
            widths.EnsureCapacity(children.Count);
            heights.EnsureCapacity(children.Count);
            tracks.EnsureCapacity(children.Count);
            
            widths.DangerousClear();
            heights.DangerousClear();
            tracks.DangerousClear();
            
            FlexTrack track = new FlexTrack();

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                FlexItem width = new FlexItem();
                LayoutBoxSize widthSize = child.GetWidths();
                
                width.childIndex = i;
                width.order = child.style.FlexItemOrder;
                width.minSize = widthSize.minSize;
                width.maxSize = widthSize.maxSize;
                width.outputSize = widthSize.clampedSize;
                
                widths[i] = width;
                
            }
            
            float adjustedWidth = allocatedWidth - PaddingHorizontal - BorderHorizontal;
            track.remainingSpace = adjustedWidth - track.mainSize;

            if (track.remainingSpace > 0) {
                track = GrowTrack(track, widths.List);
            }
            else if (track.remainingSpace < 0) {
                track = GrowTrack(track, widths.List);
            }
            

        }

        private FlexTrack GrowTrack(FlexTrack track, FlexItem[] items) {
            int pieces = 0;

            int startIndex = track.startItem;
            int endIndex = startIndex + track.itemCount;

            for (int i = startIndex; i < endIndex; i++) {
                items[i].flexFactor = children[items[i].childIndex].style.FlexItemGrow;
                pieces += items[i].flexFactor;
            }

            bool allocate = pieces > 0;
            float remainingSpace = track.remainingSpace;
            while (allocate && (int) remainingSpace > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remainingSpace / pieces;

                for (int i = startIndex; i < endIndex; i++) {
                    float max = items[i].maxSize;
                    float output = items[i].outputSize;
                    int growthFactor = items[i].flexFactor;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    output = totalGrowth > max ? max : totalGrowth;

                    remainingSpace -= output - start;

                    items[i].outputSize = output;
                }
            }

            track.remainingSpace = remainingSpace;
            return track;
        }
        
        private FlexTrack DoShrinkTrack(FlexTrack track, FlexItem[] items, float[] outputs) {
            int pieces = 0;
            int startIndex = track.startItem;
            int endIndex = startIndex + track.itemCount;

            for (int i = startIndex; i < endIndex; i++) {
                items[i].flexFactor = children[items[i].childIndex].style.FlexItemShrink;
                pieces += items[i].shrinkFactor;
                outputs[i] = items[i].outputSize;
            }

            float overflow = -track.remainingSpace;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;

                for (int i = startIndex; i < endIndex; i++) {
                    float min = items[i].minSize;
                    float output = outputs[i];
                    int shrinkFactor = items[i].shrinkFactor;

                    if (shrinkFactor == 0 || (int) output == (int) min || (int) output == 0) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    output = (totalShrink < min) ? min : totalShrink;
                    overflow += output - start;

                    outputs[i] = output;
                }
            }

            return 
//            return overflow;
        }
        
        protected void RunRowLayout() {
            
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
            public float remainingSpace;

        }

        private struct FlexItem : IComparable<FlexItem> {

            public int childIndex;
            public float axisStart;
            public float outputSize;
            public float maxSize;
            public float minSize;
            public int flexFactor;
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