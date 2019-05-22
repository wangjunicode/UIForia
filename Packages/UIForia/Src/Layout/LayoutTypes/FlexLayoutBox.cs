using System;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout.LayoutTypes {

    public class FlexLayoutBox : LayoutBox, IPoolableLayoutBox {

        public LightList<Item> items;
        public LightList<Track> tracks;

        public OffsetRect padding;
        public OffsetRect border;
        public MainAxisAlignment mainAxisAlignment;
        public CrossAxisAlignment crossAxisAlignment;

        public struct Item {

            public float mainAxisStart;
            public float crossAxisStart;
            public float mainSize;
            public float crossSize;
            public float minSize;
            public float maxSize;
            public int growFactor;
            public int shrinkFactor;
            public OffsetRect margin;
            public CrossAxisAlignment crossAxisAlignment;

        }

        public struct Track {

            public float mainSize;
            public float crossSize;
            public float remainingSpace;
            public int startItemIndex;
            public int endItemIndex;

        }

        public FlexLayoutBox() {
            this.items = new LightList<Item>(4);
            this.tracks = new LightList<Track>(4);
        }

        public bool IsInPool { get; set; }

        public override void OnRelease() {
            base.OnRelease();
            this.items.QuickClear();
            this.tracks.QuickClear();
        }

        protected override void OnChildrenChanged() {
            items.Clear();
            items.EnsureCapacity(children.Count);
            this.crossAxisAlignment = style.FlexLayoutCrossAxisAlignment;
            this.mainAxisAlignment = style.FlexLayoutMainAxisAlignment;
            Item[] itemList = items.Array;
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                CrossAxisAlignment childCrossAlignment = children[i].style.FlexItemSelfAlignment;
                if (childCrossAlignment == CrossAxisAlignment.Unset) {
                    childCrossAlignment = crossAxisAlignment;
                }

                itemList[i].growFactor = child.style.FlexItemGrow;
                itemList[i].shrinkFactor = child.style.FlexItemShrink;
                itemList[i].crossAxisAlignment = childCrossAlignment;
            }
        }

        public override void OnStylePropertyChanged(StructList<StyleProperty> properties) {
            for (var index = 0; index < properties.Count; index++) {
                StyleProperty property = properties[index];
                switch (property.propertyId) {
                    case StylePropertyId.FlexLayoutDirection:
                    case StylePropertyId.FlexLayoutCrossAxisAlignment:
                    case StylePropertyId.FlexLayoutMainAxisAlignment:
                        crossAxisAlignment = style.FlexLayoutCrossAxisAlignment;
                        mainAxisAlignment = style.FlexLayoutMainAxisAlignment;
                        Item[] itemList = items.Array;
                        for (int i = 0; i < children.Count; i++) {
                            CrossAxisAlignment childCrossAlignment = children[i].style.FlexItemSelfAlignment;
                            if (childCrossAlignment == CrossAxisAlignment.Unset) {
                                childCrossAlignment = crossAxisAlignment;
                            }

                            itemList[i].crossAxisAlignment = childCrossAlignment;
                        }

                        break;
                }
            }
        }

        public override void OnChildStylePropertyChanged(LayoutBox child, StructList<StyleProperty> properties) {
            for (var index = 0; index < properties.Count; index++) {
                StyleProperty property = properties[index];
                switch (property.propertyId) {
                    case StylePropertyId.FlexItemGrow:
                    case StylePropertyId.FlexItemShrink:
                    case StylePropertyId.FlexItemSelfAlignment:

                        Item[] itemList = items.Array;
                        for (int i = 0; i < children.Count; i++) {
                            if (children[i] == child) {
                                CrossAxisAlignment childCrossAlignment = child.style.FlexItemSelfAlignment;
                                if (childCrossAlignment == CrossAxisAlignment.Unset) {
                                    childCrossAlignment = crossAxisAlignment;
                                }

                                itemList[i].crossAxisAlignment = childCrossAlignment;
                                itemList[i].growFactor = child.style.FlexItemGrow;
                                itemList[i].shrinkFactor = child.style.FlexItemShrink;
                            }
                        }

                        break;
                }
            }
        }

        public override void RunLayout() {
            padding = new OffsetRect(PaddingTop, PaddingRight, PaddingBottom, PaddingLeft);
            border = new OffsetRect(BorderTop, BorderRight, BorderBottom, BorderLeft);

            if (children.Count == 0) {
                actualWidth = allocatedWidth;
                actualHeight = allocatedHeight;
                return;
            }

            Size size = default;

            if (style.FlexLayoutDirection == LayoutDirection.Column) {
                size = RunColumnLayout(false);
                for (int i = 0; i < children.Count; i++) {
                    Item item = items[i];
                    children[i].SetAllocatedRect(
                        item.mainAxisStart,
                        item.crossAxisStart,
                        Mathf.Max(0, item.mainSize - item.margin.left - item.margin.right),
                        Mathf.Max(0, item.crossSize - item.margin.top - item.margin.bottom)
                    );
                }
            }
            else {
                size = RunRowLayout(false);
                for (int i = 0; i < children.Count; i++) {
                    Item item = items[i];
                    children[i].SetAllocatedRect(
                        item.crossAxisStart,
                        item.mainAxisStart,
                        Mathf.Max(0, item.crossSize - item.margin.left - item.margin.right),
                        Mathf.Max(0, item.mainSize - item.margin.top - item.margin.bottom)
                    );
                }
            }

            if (allocatedWidth > size.width) {
                actualWidth = allocatedWidth;
            }
            else {
                actualWidth = size.width + padding.Horizontal + border.Horizontal;
            }

            if (allocatedHeight > size.height) {
                actualHeight = allocatedHeight;
            }
            else {
                actualHeight = size.height + padding.Vertical + border.Vertical;
            }
        }

        protected override float ComputeContentWidth() {
            if (children.Count == 0) {
                return 0;
            }

            if (style.FlexLayoutDirection == LayoutDirection.Column) {
                Size size = RunColumnLayout(true);
                return size.width;
            }
            else {
                Size size = RunRowLayout(true); // todo -- either cache this result or early-out
                return size.width;
            }
        }

        protected override float ComputeContentHeight(float width) {
            if (children.Count == 0) {
                return 0;
            }

            float cachedWidth = allocatedWidth;
            allocatedWidth = width;
            Size size;
            if (style.FlexLayoutDirection == LayoutDirection.Column) {
                size = RunColumnLayout(true);
            }
            else {
                size = RunRowLayout(true);
            }

            allocatedWidth = cachedWidth;
            return size.height;
        }

        private Size RunRowLayout(bool measureOnly) {
            float paddingBorderTop = padding.top + border.top;
            float paddingBorderBottom = padding.bottom + border.bottom;
            float paddingBorderLeft = padding.left + border.left;
            float paddingBorderRight = padding.right + border.right;
            float adjustedWidth = allocatedWidth - paddingBorderLeft - paddingBorderRight;
            float adjustedHeight = allocatedHeight - paddingBorderTop - paddingBorderBottom;

            float maxWidth = 0f;
            tracks.Clear();

            Item[] itemList = items.Array;
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                LayoutBoxSize widthSize = child.GetWidths();
                OffsetRect itemMargin = new OffsetRect(
                    0, child.GetMarginRight(),
                    0, child.GetMarginLeft()
                );
                itemList[i].margin = itemMargin;
                itemList[i].crossSize = widthSize.clampedSize + itemMargin.left + itemMargin.right;
                maxWidth = itemList[i].crossSize > maxWidth ? itemList[i].crossSize : maxWidth;
            }

            Track currentTrack = new Track();
            currentTrack.crossSize = measureOnly ? maxWidth : Mathf.Max(maxWidth, adjustedWidth);
            currentTrack.startItemIndex = 0;
            currentTrack.endItemIndex = children.Count;

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                Item item = items[i];

                if (item.crossAxisAlignment == CrossAxisAlignment.Stretch) {
                    item.crossSize = maxWidth;
                }

                OffsetRect itemMargin = new OffsetRect(
                    child.GetMarginTop(item.crossSize), item.margin.right,
                    child.GetMarginBottom(item.crossSize), item.margin.left
                );
                item.margin = itemMargin;
                LayoutBoxSize childHeights = child.GetHeights(item.crossSize - item.margin.Horizontal);
                item.minSize = childHeights.minSize;
                item.maxSize = childHeights.maxSize;
                item.mainSize = childHeights.clampedSize + itemMargin.Vertical;

                items[i] = item;

                currentTrack.mainSize += item.mainSize;
            }

            // supports only 1 track for now
            tracks.Add(currentTrack);

            if (measureOnly) {
                return new Size(currentTrack.crossSize, currentTrack.mainSize);
            }

            // stretching width can technically expand height but we don't handle that yet.

            float totalHeight = 0;
            float totalWidth = 0;

            for (int i = 0; i < tracks.Count; i++) {
                Track track = tracks[i];
                track.remainingSpace = adjustedHeight - track.mainSize;

                if (track.remainingSpace > 0) {
                    track = GrowTrack(track, items.Array);
                }
                else if (track.remainingSpace < 0) {
                    track = ShrinkTrack(track, items.Array);
                }

                AlignMainAxis(track, paddingBorderTop);
                PositionCrossAxisRow(0, paddingBorderLeft, track);

                totalHeight += track.mainSize;
                totalWidth = track.crossSize > totalWidth ? track.crossSize : totalWidth;
            }

            return new Size(totalWidth, totalHeight);
        }

        private Size RunColumnLayout(bool measureOnly) {
            float paddingBorderTop = padding.top + border.top;
            float paddingBorderBottom = padding.bottom + border.bottom;
            float paddingBorderLeft = padding.left + border.left;
            float paddingBorderRight = padding.right + border.right;
            float adjustedWidth = allocatedWidth - paddingBorderLeft - paddingBorderRight;
            float adjustedHeight = allocatedHeight - paddingBorderTop - paddingBorderBottom;

            tracks.Clear();
            Item[] itemList = items.Array;

            Track currentTrack = new Track();
            currentTrack.startItemIndex = 0;
            currentTrack.endItemIndex = children.Count;

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                LayoutBoxSize widthSize = child.GetWidths();
                itemList[i].margin = new OffsetRect(0, child.GetMarginRight(), 0, child.GetMarginLeft());
                itemList[i].mainSize = itemList[i].margin.Horizontal + widthSize.clampedSize;
                itemList[i].minSize = widthSize.minSize;
                itemList[i].maxSize = widthSize.maxSize;
                currentTrack.mainSize += itemList[i].mainSize;
            }

            tracks.Add(currentTrack);

            Track[] trackList = tracks.Array;

            for (int i = 0; i < tracks.Count; i++) {
                Track track = trackList[i];
                track.remainingSpace = adjustedWidth - track.mainSize;
                if (track.remainingSpace > 0) {
                    GrowTrack(track, itemList);
                }
                else if (track.remainingSpace < 0) {
                    ShrinkTrack(track, itemList);
                }

                AlignMainAxis(track, paddingBorderLeft);
            }

            float maxHeight = 0f;
            for (int i = 0; i < children.Count; i++) {
                float width = itemList[i].mainSize - itemList[i].margin.Horizontal;
                OffsetRect margin = itemList[i].margin;

                itemList[i].margin = new OffsetRect(children[i].GetMarginTop(width), margin.right, children[i].GetMarginBottom(width), margin.left);
                itemList[i].crossSize = children[i].GetHeights(itemList[i].mainSize - itemList[i].margin.Horizontal).clampedSize + itemList[i].margin.Vertical;
                maxHeight = Mathf.Max(maxHeight, itemList[i].crossSize);
            }

            currentTrack.crossSize = measureOnly ? maxHeight : Mathf.Max(maxHeight, adjustedHeight);

            for (int i = 0; i < tracks.Count; i++) {
                Track track = trackList[i];
                track.crossSize = Mathf.Max(maxHeight, adjustedHeight);
                PositionCrossAxisColumn(0, paddingBorderTop, track);
            }

            return new Size(currentTrack.mainSize, currentTrack.crossSize);
        }

        private void AlignMainAxis(Track track, float mainAxisOffset) {
            Item[] itemList = items.Array;
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

                    default:
                        throw new ArgumentOutOfRangeException(nameof(mainAxisAlignment), mainAxisAlignment, null);
                }
            }

            if (style.FlexLayoutDirection == LayoutDirection.Row) {
                for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                    itemList[i].mainAxisStart = itemList[i].margin.top + mainAxisOffset + offset;
                    offset += itemList[i].mainSize + spacerSize;
                }
            }
            else {
                for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                    itemList[i].mainAxisStart = itemList[i].margin.left + mainAxisOffset + offset;
                    offset += itemList[i].mainSize + spacerSize;
                }
            }
        }

        private void PositionCrossAxisColumn(float axisOffset, float padding, Track track) {
            axisOffset += padding;
            Item[] itemList = items.Array;
            for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                switch (items[i].crossAxisAlignment) {
                    case CrossAxisAlignment.Center:
                        itemList[i].crossAxisStart = (track.crossSize * 0.5f) - (itemList[i].crossSize * 0.5f) + itemList[i].margin.top;
                        break;

                    case CrossAxisAlignment.End:
                        itemList[i].crossAxisStart = track.crossSize - itemList[i].crossSize + itemList[i].margin.top;
                        break;

                    case CrossAxisAlignment.Start:
                        itemList[i].crossAxisStart = itemList[i].margin.top;
                        break;

                    case CrossAxisAlignment.Stretch:
                        itemList[i].crossAxisStart = itemList[i].margin.top;
                        itemList[i].crossSize = track.crossSize;
                        break;

                    default:
                        itemList[i].crossAxisStart = itemList[i].margin.top;
                        break;
                }

                itemList[i].crossAxisStart += axisOffset;
            }
        }

        private void PositionCrossAxisRow(float axisOffset, float padding, Track track) {
            axisOffset += padding;
            Item[] itemList = items.Array;
            for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                switch (items[i].crossAxisAlignment) {
                    case CrossAxisAlignment.Center:
                        itemList[i].crossAxisStart = (track.crossSize * 0.5f) - (itemList[i].crossSize * 0.5f) + itemList[i].margin.left;
                        break;

                    case CrossAxisAlignment.End:
                        itemList[i].crossAxisStart = track.crossSize - itemList[i].crossSize + itemList[i].margin.left;
                        break;

                    case CrossAxisAlignment.Start:
                        itemList[i].crossAxisStart = itemList[i].margin.left;
                        break;

                    case CrossAxisAlignment.Stretch:
                        itemList[i].crossAxisStart = itemList[i].margin.left;
                        itemList[i].crossSize = track.crossSize;
                        break;
                    default:
                        itemList[i].crossAxisStart = itemList[i].margin.left;
                        break;
                }

                itemList[i].crossAxisStart += axisOffset;
            }
        }

        private static Track GrowTrack(Track track, Item[] items) {
            int pieces = 0;

            int startIndex = track.startItemIndex;
            int endIndex = track.endItemIndex;

            for (int i = startIndex; i < endIndex; i++) {
                pieces += items[i].growFactor;
            }

            bool allocate = pieces > 0;
            float remainingSpace = track.remainingSpace;
            float toAllocate = remainingSpace;
            while (allocate && (int) remainingSpace > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remainingSpace / pieces;

                for (int i = startIndex; i < endIndex; i++) {
                    float max = items[i].maxSize;
                    float output = items[i].mainSize;
                    int growthFactor = items[i].growFactor;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    output = totalGrowth > max ? max : totalGrowth;

                    remainingSpace -= output - start;

                    items[i].mainSize = output;
                }
            }

            track.mainSize += toAllocate - remainingSpace;
            track.remainingSpace = remainingSpace;
            return track;
        }

        private static Track ShrinkTrack(Track track, Item[] items) {
            int pieces = 0;
            int startIndex = track.startItemIndex;
            int endIndex = track.endItemIndex;

            for (int i = startIndex; i < endIndex; i++) {
                pieces += items[i].shrinkFactor;
            }

            float overflow = -track.remainingSpace;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;

                for (int i = startIndex; i < endIndex; i++) {
                    float min = items[i].minSize;
                    float output = items[i].mainSize;
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

                    items[i].mainSize = output;
                }
            }

            track.remainingSpace = overflow;
            return track;
        }

    }

}