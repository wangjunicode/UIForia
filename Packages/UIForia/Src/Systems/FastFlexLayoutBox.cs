using System;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Util;

namespace UIForia.Systems {

    public class FastFlexLayoutBox : FastLayoutBox {

        private StructList<Item> itemList;
        private StructList<Track> trackList;
        private LayoutDirection direction;
        private MainAxisAlignment mainAxisAlignment;
        private CrossAxisAlignment crossAxisAlignment;

        public struct Track {

            public float mainSize;
            public float crossSize;
            public float remainingSpace;
            public int startItemIndex;
            public int endItemIndex;

        }

        public FastFlexLayoutBox(UIElement element) : base(element) {
            this.itemList = new StructList<Item>();
        }

        public override void PerformLayout() {
            if (direction == LayoutDirection.Horizontal) {
                PerformLayoutHorizontal();
                Item[] items = itemList.array;
                FastLayoutBox child = firstChild;
                for (int i = 0; i < itemList.size; i++) {
                    ref Item item = ref items[i];
                    //   child.ApplyLayout(item.outputSize, default, default, default, default, default);
                    child = child.nextSibling;
                }
            }
            else {
                //    PerformLayoutVertical();
            }
        }

        public virtual void ComputeContentSize(Size blockSize) {
            // optimize for when we need to compute width & height together so we avoid a width pass and do both at once
        }

        public override float ComputeContentWidth(float blockWidth) {
            // no growing,
            // just get size for all children,
            // add them up.
            // return it
            return 0;
        }

        public override float ComputeContentHeight(float width, float blockHeight) {
            // need to grow on width to get proper height
            return 0;
        }

        public override float GetIntrinsicMinWidth() {
            Item[] items = itemList.array;
            FastLayoutBox child = firstChild;

            if (direction == LayoutDirection.Horizontal) {
                float min = 0;

                while (child != null) {
                    min += child.GetIntrinsicMinWidth();
                    child = child.nextSibling;
                }

                return min;
            }
            else {
                // intrinsic min for vertical direction is the max child intrinsic width
                float max = 0;

                while (child != null) {
                    float childMin = child.GetIntrinsicMinWidth();
                    if (childMin > max) max = childMin;
                    child = child.nextSibling;
                }

                return max;
            }
        }

        private void PerformLayoutHorizontalWrap() {
            trackList = trackList ?? new StructList<Track>(4);// todo -- see if tracks can be stack alloced or reused instead
        }

        private void PerformLayoutHorizontal() {
            Item[] items = itemList.array;
            FastLayoutBox child = firstChild;

            Size block = containingBox;

            if (widthMeasurement.prefUnit == UIMeasurementUnit.Content) {
                block.width = containingBox.width;
            }
            else {
                block.width = size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right;
            }

            if (heightMeasurement.prefUnit == UIMeasurementUnit.Content) {
                block.height = containingBox.height;
            }
            else {
                block.height = size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom;
            }

            Track track = default;

            int growPieces = 0;
            int shrinkPieces = 0;
            
            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                child.GetWidth(block.width, ref item.size);
                child.GetMarginHorizontal(ref item.outputWidth);
                growPieces += item.growFactor;
                shrinkPieces += item.shrinkFactor;
                track.mainSize += item.size.prefWidth;
                child = child.nextSibling;
            }

            float remaining = size.width - track.mainSize;

            if (remaining > 0) {
                Grow(ref track, growPieces);
            }
            else if (remaining < 0) {
                Shrink(ref track, shrinkPieces);
            }

            ApplyMainAxisPositioning(track, paddingBox.left + borderBox.left);

            child = firstChild;
            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                
                child.ApplyHorizontalLayout(item.mainAxisStart, block.width, item.outputWidth, item.size.prefWidth, default, Fit.Unset);
                
                item.outputWidth = child.size.width;
                
                child.GetHeight(child.size.width, block.height, ref item.size);

                if (item.size.prefHeight > track.crossSize) {
                    track.crossSize = item.size.prefHeight;
                }
                
                child = child.nextSibling;
            }

            float axisOffset = paddingBox.top + borderBox.top;

            Alignment verticalAlignment = default;
            Fit verticalFit = Fit.None;
            
            switch (crossAxisAlignment) {
                case CrossAxisAlignment.Unset:
                case CrossAxisAlignment.Start:
                    verticalAlignment = new Alignment(0, 0, AlignmentTarget.AllocatedBox);
                    break;
                case CrossAxisAlignment.Center:
                    verticalAlignment = new Alignment(0.5f, 0.5f, AlignmentTarget.AllocatedBox);
                    break;
                case CrossAxisAlignment.End:
                    verticalAlignment = new Alignment(1f, 1f, AlignmentTarget.AllocatedBox);
                    break;
                case CrossAxisAlignment.Stretch:
                    verticalFit = Fit.Grow;
                    break;
            }
            
            child = firstChild;
            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                child.ApplyLayoutVertical(axisOffset, block.height, item.outputHeight, item.size.prefHeight, verticalAlignment, verticalFit);
                child = child.nextSibling;
            }
            
            contentSize.width = track.mainSize;
            contentSize.height = track.crossSize;

            child = firstChild;
            
            for (int i = 0; i < itemList.size; i++) {
                child.Layout();
                child = child.nextSibling;
            }

        }

        private void ApplyMainAxisPositioning(Track track, float mainAxisOffset) {
            Item[] items = itemList.array;
            int itemCount = track.endItemIndex - track.startItemIndex;
            float spacerSize = 0;
            float offset = 0;

            if (track.remainingSpace > 0) {
                switch (mainAxisAlignment) {
                    case MainAxisAlignment.Unset:
                    case MainAxisAlignment.Start:
                        break;
                    
                    case MainAxisAlignment.Center:
                        mainAxisOffset *= 0.5f;
                        offset = mainAxisOffset + track.remainingSpace * 0.5f;
                        break;
                    
                    case MainAxisAlignment.End:
                        offset = track.remainingSpace;
                        break;
                    
                    case MainAxisAlignment.SpaceBetween: {
                        if (itemCount == 1) {
                            offset = track.remainingSpace * 0.5f;
                            break;
                        }

                        spacerSize = track.remainingSpace / (itemCount - 1);
                        offset = 0;
                        break;
                    }

                    case MainAxisAlignment.SpaceAround: {
                        if (itemCount == 1) {
                            offset = track.remainingSpace * 0.5f;
                            break;
                        }

                        spacerSize = (track.remainingSpace / itemCount);
                        offset = spacerSize * 0.5f;
                        break;
                        
                    }
                }
            }

            if (element.style.FlexLayoutDirection == LayoutDirection.Row) {
                for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                    items[i].mainAxisStart = items[i].margin.top + mainAxisOffset + offset;
                    offset += items[i].outputWidth + spacerSize;
                }
            }
            else {
                for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                    items[i].mainAxisStart = items[i].margin.left + mainAxisOffset + offset;
                    offset += items[i].outputWidth + spacerSize;
                }
            }
        }

        private void Grow(ref Track track, int pieces) {
            Item[] items = itemList.array;
            
            bool allocate = pieces > 0;
            float toAllocate = track.remainingSpace;
            float remainingSpace = track.remainingSpace;

            while (allocate && (int) remainingSpace > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remainingSpace / pieces;

                for (int i = 0; i < itemList.size; i++) {
                    ref Item item = ref items[i];
                    float max = item.size.maxWidth;
                    float output = item.outputWidth;
                    int growthFactor = item.growFactor;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    output = totalGrowth > max ? max : totalGrowth;

                    remainingSpace -= output - start;

                    item.outputWidth = output;
                }
            }

            track.mainSize += toAllocate - remainingSpace;
            track.remainingSpace = remainingSpace;
        }

        private void Shrink(ref Track track, int pieces) {
            int startIndex = track.startItemIndex;
            int endIndex = track.endItemIndex;

            Item[] items = itemList.array;

            float overflow = -track.remainingSpace;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;

                for (int i = startIndex; i < endIndex; i++) {
                    ref Item item = ref items[i];
                    float min = item.size.minWidth;
                    float output = item.size.prefWidth;
                    int shrinkFactor = item.shrinkFactor;

                    if (shrinkFactor == 0 || (int) output == (int) min || (int) output == 0) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    output = (totalShrink < min) ? min : totalShrink;
                    overflow += output - start;

                    item.outputWidth = output;
                }
            }

            track.remainingSpace = overflow;
        }

        public override void SetChildren(LightList<FastLayoutBox> children) {
            itemList.EnsureCapacity(children.size);
            itemList.size = children.size;
            Item[] items = itemList.array;

            for (int i = 0; i < children.size; i++) {
                ref Item item = ref items[i];
            }
        }

        public override void OnChildAdded(FastLayoutBox child, int index) {
            itemList.Insert(index, new Item() {
                layoutBox = child,
                growFactor = child.element.style.FlexItemGrow,
                shrinkFactor = child.element.style.FlexItemShrink,
            });
        }

        public struct Item {

            public float mainAxisStart;
            public float crossAxisStart;
            public float outputWidth;
            public float outputHeight;
            public int growFactor;
            public int shrinkFactor;
            public OffsetRect margin;
            public SizeConstraints size;
            public FastLayoutBox layoutBox;

        }

    }

}