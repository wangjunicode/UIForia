using System;
using System.Collections.Generic;
using System.Diagnostics;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout.LayoutTypes {

    public class FlexLayoutBox2 : LayoutBox {

        public LightList<Item> items;
        public LightList<Track> tracks;

        public OffsetRect padding;
        public OffsetRect border;
        public MainAxisAlignment mainAxisAlignment;
        public CrossAxisAlignment crossAxisAlignment;

        public struct Item {

            public int elementId;
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

        public FlexLayoutBox2(UIElement element) : base(element) {
            this.items = new LightList<Item>(4);
            this.tracks = new LightList<Track>(4);
            crossAxisAlignment = style.FlexLayoutCrossAxisAlignment;
            mainAxisAlignment = style.FlexLayoutMainAxisAlignment;
        }

        protected override void OnChildAdded(LayoutBox child) {
            OnChildEnabled(child);
        }

        public override void OnChildEnabled(LayoutBox enabledChild) {
            children.Add(enabledChild); // todo -- insert in sort order
            items.Add(new Item());
            Item[] itemList = items.List;
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

            RequestContentSizeChangeLayout();
        }

        public override void OnChildDisabled(LayoutBox disabledChild) {
            children.Remove(disabledChild);
            Item[] itemList = items.List;
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

            RequestContentSizeChangeLayout();
        }

        public override void OnStylePropertyChanged(StyleProperty property) {
            base.OnStylePropertyChanged(property);
            switch (property.propertyId) {
                case StylePropertyId.FlexLayoutDirection:
                case StylePropertyId.FlexLayoutCrossAxisAlignment:
                case StylePropertyId.FlexLayoutMainAxisAlignment:
                    crossAxisAlignment = style.FlexLayoutCrossAxisAlignment;
                    mainAxisAlignment = style.FlexLayoutMainAxisAlignment;
                    for (int i = 0; i < children.Count; i++) {
                        Item[] itemList = items.List;
                        CrossAxisAlignment childCrossAlignment = children[i].style.FlexItemSelfAlignment;
                        if (childCrossAlignment == CrossAxisAlignment.Unset) {
                            childCrossAlignment = crossAxisAlignment;
                        }

                        itemList[i].crossAxisAlignment = childCrossAlignment;
                    }

                    break;
            }
        }

        public override void OnChildStylePropertyChanged(LayoutBox child, StyleProperty property) {
            base.OnChildStylePropertyChanged(child, property);
            switch (property.propertyId) {
//                case StylePropertyId.FlexItemOrder:
                case StylePropertyId.FlexItemGrow:
                case StylePropertyId.FlexItemShrink:
                case StylePropertyId.FlexItemSelfAlignment:
                    for (int i = 0; i < children.Count; i++) {
                        if (children[i] == child) {
                            Item[] itemList = items.List;
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

        public override void RunLayout() {
            padding = new OffsetRect(PaddingTop, PaddingRight, PaddingBottom, PaddingLeft);
            border = new OffsetRect(BorderTop, BorderRight, BorderBottom, BorderLeft);

            if (children.Count == 0) {
                actualWidth = allocatedWidth;
                actualHeight = allocatedHeight;
                return;
            }

            if (style.FlexLayoutDirection == LayoutDirection.Column) {
                Size size = RunColumnLayout(false);
                actualWidth = size.width;
                actualHeight = size.height;
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
                Size size = RunRowLayout(false);
                actualWidth = size.width;
                actualHeight = size.height;
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

            Item[] itemList = items.List;
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
            currentTrack.crossSize = Mathf.Max(maxWidth, adjustedWidth);
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
                    track = GrowTrack(track, items.List);
                }
                else if (track.remainingSpace < 0) {
                    track = ShrinkTrack(track, items.List);
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
            Item[] itemList = items.List;

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

            Track[] trackList = tracks.List;

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

            currentTrack.crossSize = Mathf.Max(maxHeight, adjustedHeight);

            for (int i = 0; i < tracks.Count; i++) {
                Track track = trackList[i];
                track.crossSize = Mathf.Max(maxHeight, adjustedHeight);
                PositionCrossAxisColumn(0, paddingBorderTop, track);
            }

            return new Size(currentTrack.mainSize, currentTrack.crossSize);
        }

        private void AlignMainAxis(Track track, float mainAxisOffset) {
            Item[] itemList = items.List;
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
            Item[] itemList = items.List;
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
            Item[] itemList = items.List;
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

    // todo -- pool this
    // todo -- handle incremental layout, ie no layout when not required or only run partial like alignment change
    // todo -- handle reverse 
    [DebuggerDisplay("{element.ToString()}")]
    public class FlexLayoutBox : LayoutBox {

        private FlexItem[] widths;
        private FlexItem[] heights;
        private readonly List<FlexTrack> tracks;

        private static float[] scratchFloats = ArrayPool<float>.Empty;

        public FlexLayoutBox(UIElement element) : base(element) {
            tracks = ListPool<FlexTrack>.Get();
            widths = ArrayPool<FlexItem>.GetMinSize(4);
            heights = ArrayPool<FlexItem>.GetMinSize(4);
        }

        // sorting should happen once
        // use only 1 array
        // handle margin
        // consider handling rotation
        // re-use content layout code
        // avoid branching
        // remove wrap support? move to different layout type maybe
        // dont allocate flex tracks

        protected override float ComputeContentWidth() {
            if (style.FlexLayoutDirection == LayoutDirection.Row) {
                float max = 0;
                for (int i = 0; i < children.Count; i++) {
                    LayoutBox child = children[i];

                    widths[i] = new FlexItem();
                    widths[i].order = child.style.FlexItemOrder;
                    widths[i].childIndex = i;
                    widths[i].outputSize = child.GetWidths().clampedSize;

                    widths[i].totalSize = widths[i].outputSize + child.GetMarginHorizontal();

                    if (widths[i].totalSize > max) max = widths[i].totalSize;

                    widths[i].crossAxisAlignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
                        ? child.style.FlexItemSelfAlignment
                        : style.FlexLayoutCrossAxisAlignment;
                }

                float adjustedHeight = style.PreferredHeight.IsFixed
                    ? GetPreferredHeight(0) - PaddingVertical - BorderVertical
                    : float.MaxValue;

                // todo -- avoid double sort
                Array.Sort(widths, 0, children.Count);
                Array.Sort(heights, 0, children.Count);

                FillWrappedRowTracks(adjustedHeight);

                float contentWidth = 0;

                for (int i = 0; i < tracks.Count; i++) {
                    contentWidth += tracks[i].crossSize;
                }

                return contentWidth;
            }
            else {
                float max = 0f;
                for (int i = 0; i < children.Count; i++) {
                    LayoutBox child = children[i];
                    max += child.GetWidths().clampedSize;
                }

                return max;
            }
        }

        protected override float ComputeContentHeight(float width) {
            if (style.FlexLayoutDirection == LayoutDirection.Row) {
                float cachedAllocatedWidth = width;
                allocatedWidth = width;
                for (int i = 0; i < children.Count; i++) {
                    LayoutBox child = children[i];

                    widths[i] = new FlexItem();
                    widths[i].order = child.style.FlexItemOrder;
                    widths[i].childIndex = i;
                    widths[i].outputSize = child.GetWidths().clampedSize;
                }

                Array.Sort(widths, 0, children.Count);
                Array.Sort(heights, 0, children.Count);

                FillWrappedRowTracks(float.MaxValue);

                float contentHeight = 0f;
                for (int i = 0; i < tracks.Count; i++) {
                    FlexTrack track = tracks[i];

                    if (track.mainSize > contentHeight) {
                        contentHeight = track.mainSize;
                    }
                }

                allocatedWidth = cachedAllocatedWidth;
                return contentHeight;
            }
            else {
                float cachedAllocatedWidth = width;
                allocatedWidth = width;
                for (int i = 0; i < children.Count; i++) {
                    LayoutBox child = children[i];
                    widths[i] = new FlexItem();
                    widths[i].childIndex = i;
                    widths[i].order = child.style.FlexItemOrder;
                    widths[i].growthFactor = child.style.FlexItemGrow;
                    widths[i].shrinkFactor = child.style.FlexItemShrink;
                    LayoutBoxSize widthSize = child.GetWidths();
                    widths[i].minSize = widthSize.minSize;
                    widths[i].maxSize = widthSize.maxSize;
                    widths[i].outputSize = widthSize.clampedSize;
                }

                Array.Sort(widths, 0, children.Count);

                float adjustedWidth = width - PaddingHorizontal - BorderHorizontal;

                FillColumnTracks(adjustedWidth);

                float largestTrackMainSize = 0;
                float paddingBorderLeft = PaddingLeft + BorderLeft;
                MainAxisAlignment mainAxisAlignment = style.FlexLayoutMainAxisAlignment;

                for (int i = 0; i < tracks.Count; i++) {
                    FlexTrack track = tracks[i];
                    float remainingSpace = adjustedWidth - track.mainSize;

                    if (remainingSpace > 0) {
                        GrowTrack(track, widths);
                    }
                    else if (remainingSpace < 0) {
                        ShrinkTrack(track, widths);
                    }

                    if (largestTrackMainSize < track.mainSize) {
                        largestTrackMainSize = track.mainSize;
                    }

                    AlignMainAxis(track, widths, mainAxisAlignment, paddingBorderLeft);
                }

                float maxExtent = 0;

                // todo -- this probably doesn't handle wrapping correctly

                for (int i = 0; i < tracks.Count; i++) {
                    float trackHeight = 0;
                    FlexTrack track = tracks[i];
                    for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                        float height = children[widths[j].childIndex].GetHeights(widths[j].outputSize).clampedSize;
                        if (height > trackHeight) {
                            trackHeight = height;
                        }
                    }

                    maxExtent += trackHeight;
                }

                allocatedWidth = cachedAllocatedWidth;
                return maxExtent;
            }
        }

        private float[] GetScratchFloats() {
            if (scratchFloats.Length < children.Count) {
                ArrayPool<float>.Resize(ref scratchFloats, children.Count);
            }

            return scratchFloats;
        }

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

        private void RunColumnLayout() {
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                widths[i] = new FlexItem();
                widths[i].childIndex = i;
                widths[i].order = child.style.FlexItemOrder;
                widths[i].growthFactor = child.style.FlexItemGrow;
                widths[i].shrinkFactor = child.style.FlexItemShrink;
                LayoutBoxSize widthSize = child.GetWidths();
                widths[i].minSize = widthSize.minSize;
                widths[i].maxSize = widthSize.maxSize;
                widths[i].outputSize = widthSize.clampedSize;
            }

            Array.Sort(widths, 0, children.Count);

            float adjustedWidth = allocatedWidth - PaddingHorizontal - BorderHorizontal;

            FillColumnTracks(adjustedWidth);

            float largestTrackMainSize = 0;
            float trackCrossAxisStart = 0;
            float paddingBorderLeft = PaddingLeft + BorderLeft;
            MainAxisAlignment mainAxisAlignment = style.FlexLayoutMainAxisAlignment;

            for (int i = 0; i < tracks.Count; i++) {
                FlexTrack track = tracks[i];
                float remainingSpace = adjustedWidth - track.mainSize;

                if (remainingSpace > 0) {
                    GrowTrack(track, widths);
                }
                else if (remainingSpace < 0) {
                    ShrinkTrack(track, widths);
                }

                if (largestTrackMainSize < track.mainSize) {
                    largestTrackMainSize = track.mainSize;
                }

                AlignMainAxis(track, widths, mainAxisAlignment, paddingBorderLeft);
            }

            // must be created AFTER grow / shrink to compute a proper height value
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                heights[i] = new FlexItem();
                heights[i].childIndex = i;
                heights[i].order = child.style.FlexItemOrder;

                LayoutBoxSize childHeights = child.GetHeights(widths[i].outputSize);

                heights[i].minSize = childHeights.minSize;
                heights[i].maxSize = childHeights.maxSize;
                heights[i].outputSize = childHeights.clampedSize;

                heights[i].crossAxisAlignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
                    ? child.style.FlexItemSelfAlignment
                    : style.FlexLayoutCrossAxisAlignment;
            }

            Array.Sort(heights, 0, children.Count);

            float paddingTop = PaddingTop + BorderTop;
            if (tracks.Count > 1) {
                for (int i = 0; i < tracks.Count; i++) {
                    FlexTrack track = tracks[i];

                    float targetHeight = 0;

                    for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                        if (heights[j].outputSize > targetHeight) {
                            targetHeight = heights[j].outputSize;
                        }
                    }

                    trackCrossAxisStart = PositionCrossAxisColumn(trackCrossAxisStart, paddingTop, track, targetHeight);

                    for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                        children[widths[j].childIndex].SetAllocatedRect(
                            widths[j].axisStart,
                            heights[j].axisStart,
                            widths[j].outputSize,
                            heights[j].outputSize
                        );
                    }
                }
            }
            else {
                FlexTrack track = tracks[0];

                trackCrossAxisStart = PositionCrossAxisColumn(trackCrossAxisStart, paddingTop, track,
                    allocatedHeight - PaddingVertical - BorderVertical);

                for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                    children[widths[j].childIndex].SetAllocatedRect(
                        widths[j].axisStart,
                        heights[j].axisStart,
                        widths[j].outputSize,
                        heights[j].outputSize
                    );
                }
            }

            // todo -- padding / border?
            actualWidth = Mathf.Max(allocatedWidth, largestTrackMainSize);
            actualHeight = Mathf.Max(allocatedHeight, trackCrossAxisStart);
        }

        private Size RunRowLayout() {
            float max = 0;
            CrossAxisAlignment crossAxisAlignment = style.FlexLayoutCrossAxisAlignment;

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                widths[i] = new FlexItem();
                widths[i].order = child.style.FlexItemOrder;
                widths[i].childIndex = i;


                OffsetRect margin = new OffsetRect(
                    0, child.GetMarginRight(),
                    0, child.GetMarginLeft()
                );

                CrossAxisAlignment childCrossAlignment = child.style.FlexItemSelfAlignment;

                widths[i].outputSize = child.GetWidths().clampedSize + margin.left + margin.right;
                widths[i].crossAxisAlignment = childCrossAlignment != CrossAxisAlignment.Unset
                    ? childCrossAlignment
                    : crossAxisAlignment;

                if (widths[i].outputSize > max) max = widths[i].outputSize;
            }

            for (int i = 0; i < children.Count; i++) {
                if (widths[i].crossAxisAlignment == CrossAxisAlignment.Stretch) {
                    widths[i].outputSize = max; // todo -- limit to max width?
                }
            }

            float paddingBorderLeft = PaddingLeft + BorderLeft;
            float adjustedWidth = allocatedWidth - PaddingHorizontal - BorderHorizontal;
            float adjustedHeight = allocatedHeight - PaddingVertical - BorderVertical;
            float paddingBorderTop = PaddingTop + BorderTop;

            float maxWidth = 0;
            float trackCrossAxisStart = 0;

            FillWrappedRowTracks(adjustedHeight);

            Array.Sort(widths, 0, children.Count);
            Array.Sort(heights, 0, children.Count);

            // todo -- consider expanding tracks along cross axis if adjusted width > track cross size sum
            // this would affect heights for stretched items like text or anything with preserved aspect ratio 

            float largestTrackSize = 0;
            for (int i = 0; i < tracks.Count; i++) {
                FlexTrack track = tracks[i];
                float remainingSpace = adjustedHeight - track.mainSize;

                if (remainingSpace > 0) {
                    GrowTrack(track, heights);
                }
                else if (remainingSpace < 0) {
                    ShrinkTrack(track, heights);
                }

                if (track.mainSize > largestTrackSize) {
                    largestTrackSize = track.mainSize;
                }

                AlignMainAxis(track, heights, style.FlexLayoutMainAxisAlignment, paddingBorderTop);
                trackCrossAxisStart = PositionCrossAxisRow(trackCrossAxisStart, paddingBorderLeft, track,
                    tracks.Count == 1 ? adjustedWidth : track.crossSize);

                for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                    children[widths[j].childIndex].SetAllocatedXAndWidth(widths[j].axisStart, widths[j].outputSize);
                    children[heights[j].childIndex].SetAllocatedYAndHeight(heights[j].axisStart, heights[j].outputSize);
                }
            }

            FlexTrack lastTrack = tracks[tracks.Count - 1];
            for (int j = lastTrack.startItem; j < lastTrack.startItem + lastTrack.itemCount; j++) {
                if (widths[j].AxisEnd > maxWidth) {
                    maxWidth = widths[j].AxisEnd;
                }
            }

            actualWidth = Mathf.Max(allocatedWidth, maxWidth - PaddingRight - BorderRight);
            actualHeight = Mathf.Max(allocatedHeight, largestTrackSize); // padding / border?

            return new Size(actualWidth, actualHeight);
        }

        private void FillColumnTracks(float targetSize) {
            tracks.Clear();
            FlexTrack currentTrack = new FlexTrack();
            if (style.FlexLayoutWrap != LayoutWrap.Wrap) {
                for (int i = 0; i < children.Count; i++) {
                    currentTrack.mainSize += widths[i].outputSize;
                }

                currentTrack.startItem = 0;
                currentTrack.itemCount = children.Count;
                currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                tracks.Add(currentTrack);
                return;
            }

            for (int i = 0; i < children.Count; i++) {
                float size = widths[i].outputSize;

                if (currentTrack.mainSize + size < targetSize) {
                    currentTrack.mainSize += size;
                    currentTrack.itemCount++;
                }
                else if (size >= targetSize) {
                    if (currentTrack.itemCount != 0) {
                        currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                        tracks.Add(currentTrack);
                    }

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i;
                    currentTrack.mainSize = size;
                    currentTrack.itemCount = 1;
                    currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                    tracks.Add(currentTrack);

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i + 1;
                }
                else {
                    currentTrack.itemCount++;
                    currentTrack.mainSize += size;
                    currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                    //try to shrink here if possible

                    // if after shrinking there is still overflow, start a new track
                    if (TryShrinkTrack(currentTrack, widths)) {
                        tracks.Add(currentTrack);
                        currentTrack = new FlexTrack();
                        currentTrack.startItem = i + 1;
                    }
                    else {
                        currentTrack.itemCount--;
                        currentTrack.mainSize -= size;
                        currentTrack.remainingSpace = targetSize - currentTrack.mainSize;

                        tracks.Add(currentTrack);

                        currentTrack = new FlexTrack();
                        currentTrack.startItem = i;
                        currentTrack.itemCount = 1;
                        currentTrack.mainSize = size;
                    }
                }
            }

            currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
            tracks.Add(currentTrack);
        }

        private void FillWrappedRowTracks(float targetSize) {
            tracks.Clear();
            FlexTrack currentTrack = new FlexTrack();

            if (style.FlexLayoutWrap != LayoutWrap.Wrap) {
                CrossAxisAlignment alignment = style.FlexLayoutCrossAxisAlignment;
                float maxWidth = 0f;

                for (int i = 0; i < children.Count; i++) {
                    maxWidth = Mathf.Max(maxWidth, widths[i].outputSize);
                }

                for (int i = 0; i < children.Count; i++) {
                    LayoutBox child = children[i];
                    heights[i] = new FlexItem();
                    heights[i].childIndex = i;
                    heights[i].order = child.style.FlexItemOrder;
                    heights[i].growthFactor = child.style.FlexItemGrow;
                    heights[i].shrinkFactor = child.style.FlexItemShrink;
                    CrossAxisAlignment itemAlignment = child.style.FlexItemSelfAlignment;
                    if (itemAlignment == CrossAxisAlignment.Unset) {
                        itemAlignment = alignment;
                    }

                    if (alignment == CrossAxisAlignment.Stretch) {
                        widths[i].outputSize = maxWidth;
                    }

                    LayoutBoxSize childHeights = child.GetHeights(widths[i].outputSize);
                    heights[i].minSize = childHeights.minSize;
                    heights[i].maxSize = childHeights.maxSize;
                    heights[i].outputSize = childHeights.clampedSize;

                    currentTrack.mainSize += childHeights.clampedSize;
                }

                currentTrack.crossSize = maxWidth;
                currentTrack.startItem = 0;
                currentTrack.itemCount = children.Count;
                currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                tracks.Add(currentTrack);
                return;
            }

            // for each item
            // place in track
            // if track overflowing & more than 1 item
            // remove
            // stretch if needed
            // recalc heights for stretched items
            // grow / shrink track
            // start new track
            // add item to track

            float[] tmpWidths = ArrayPool<float>.GetMinSize(currentTrack.itemCount);
            float[] tmpHeights = ArrayPool<float>.GetMinSize(currentTrack.itemCount);

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                heights[i] = new FlexItem();
                heights[i].childIndex = i;
                heights[i].order = child.style.FlexItemOrder;
                heights[i].growthFactor = child.style.FlexItemGrow;
                heights[i].shrinkFactor = child.style.FlexItemShrink;

                float prfSize = children[i].GetPreferredHeight(widths[i].outputSize);
                float minSize = children[i].GetMinHeight(prfSize);
                float maxSize = children[i].GetMaxHeight(prfSize);

                heights[i].minSize = minSize;
                heights[i].maxSize = maxSize;

                float currentItemHeight = Mathf.Max(minSize, Mathf.Min(prfSize, maxSize));
                float currentItemWidth = widths[i].outputSize;

                heights[i].outputSize = currentItemHeight;

                if (currentTrack.itemCount == 0) {
                    currentTrack.startItem = i;
                    currentTrack.itemCount = 1;
                    currentTrack.mainSize = currentItemHeight;
                    currentTrack.crossSize = currentItemWidth;
                    currentTrack.remainingSpace = targetSize - currentItemHeight;

                    if (currentItemHeight > targetSize) {
                        ShrinkTrack(currentTrack, heights);
                        tracks.Add(currentTrack);
                        currentTrack = new FlexTrack();
                        currentTrack.startItem = i + 1;
                    }

                    continue;
                }

                // if current item is wider, we need to try applying stretch which might change the size of the track items
                if (currentItemWidth > currentTrack.crossSize) {
                    float newMainSize = 0f;
                    float oldMainSize = currentTrack.mainSize;

                    for (int j = currentTrack.startItem; j < currentTrack.startItem + currentTrack.itemCount; j++) {
                        tmpWidths[j] = widths[j].outputSize;
                        tmpHeights[j] = heights[j].outputSize;
                        if (widths[j].crossAxisAlignment == CrossAxisAlignment.Stretch) {
                            widths[j].outputSize = currentItemWidth;
                            heights[j].outputSize = Mathf.Max(heights[j].minSize,
                                Mathf.Min(child.GetPreferredHeight(currentItemWidth), heights[j].maxSize));
                            newMainSize += tmpHeights[j];
                        }
                    }

                    // if after stretching we fit, add the item to this track
                    if (newMainSize + currentItemHeight <= targetSize) {
                        currentTrack.crossSize = currentItemWidth;
                        currentTrack.mainSize += currentItemHeight;
                        currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                        currentTrack.itemCount++;

                        continue;
                    }

                    // otherwise, maybe we can shrink (need to add the item to also be shrunk)
                    currentTrack.itemCount++;
                    currentTrack.mainSize = newMainSize;
                    currentTrack.remainingSpace = targetSize - currentTrack.mainSize;

                    // we fit after shrinking
                    if (TryShrinkTrack(currentTrack, heights)) {
                        // apply the updated cross size
                        currentTrack.crossSize = currentItemWidth;
                    }
                    // still doesn't fit after shrinking, restore the old values and start a new track
                    else {
                        for (int j = currentTrack.startItem; j < currentTrack.startItem + currentTrack.itemCount; j++) {
                            widths[j].outputSize = tmpWidths[j];
                            heights[j].outputSize = tmpHeights[j];
                        }

                        currentTrack.itemCount--;
                        currentTrack.mainSize = oldMainSize;
                        currentTrack.remainingSpace = targetSize - oldMainSize;
                        tracks.Add(currentTrack);

                        currentTrack = new FlexTrack();
                        currentTrack.startItem = i;
                        currentTrack.itemCount = 1;
                        currentTrack.mainSize = currentItemHeight;
                        currentTrack.crossSize = currentItemWidth;
                        currentTrack.remainingSpace = targetSize - currentTrack.mainSize;

                        if (currentTrack.remainingSpace < 0) {
                            ShrinkTrack(currentTrack, heights);
                            tracks.Add(currentTrack);
                            currentTrack = new FlexTrack();
                            currentTrack.startItem = i + 1;
                        }
                    }
                }
                // we fit out right, add to current track
                else if (currentTrack.mainSize + currentItemHeight <= targetSize) {
                    currentTrack.mainSize += currentItemHeight;
                    currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                    currentTrack.itemCount++;
                }
                // we don't fit and we can't stretch, try shrinking
                else if (AddAndTryShrinkTrack(currentTrack, heights)) { }
                // we definitely don't fit, start a new track
                else {
                    tracks.Add(currentTrack);

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i;
                    currentTrack.itemCount = 1;
                    currentTrack.mainSize = currentItemHeight;
                    currentTrack.crossSize = currentItemWidth;
                    currentTrack.remainingSpace = targetSize - currentTrack.mainSize;

                    if (currentTrack.remainingSpace < 0) {
                        ShrinkTrack(currentTrack, heights);
                        tracks.Add(currentTrack);
                        currentTrack = new FlexTrack();
                        currentTrack.startItem = i + 1;
                    }
                }
            }

            if (currentTrack.itemCount > 0) {
                tracks.Add(currentTrack);
            }

            ArrayPool<float>.Release(ref tmpWidths);
            ArrayPool<float>.Release(ref tmpHeights);
        }

        private float PositionCrossAxisRow(float axisOffset, float padding, FlexTrack track, float targetSize) {
            float crossSize = 0;
            axisOffset += padding;
            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
                switch (widths[i].crossAxisAlignment) {
                    case CrossAxisAlignment.Center:
                        widths[i].axisStart = (targetSize * 0.5f) - (widths[i].outputSize * 0.5f);
                        break;

                    case CrossAxisAlignment.End:
                        widths[i].axisStart = targetSize - widths[i].outputSize;
                        break;

                    case CrossAxisAlignment.Start:
                        widths[i].axisStart = 0;
                        break;

                    case CrossAxisAlignment.Stretch:
                        widths[i].axisStart = 0;
                        widths[i].outputSize = targetSize;
                        break;
                    default:
                        widths[i].axisStart = 0;
                        break;
                }

                widths[i].axisStart += axisOffset;
                crossSize += widths[i].outputSize;
            }

            return axisOffset + crossSize;
        }

        private float PositionCrossAxisColumn(float axisOffset, float paddingTop, FlexTrack track, float targetSize) {
            float maxHeight = 0;
            axisOffset += paddingTop;
            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
                switch (heights[i].crossAxisAlignment) {
                    case CrossAxisAlignment.Center:
                        heights[i].axisStart = (targetSize * 0.5f) - (heights[i].outputSize * 0.5f);
                        break;

                    case CrossAxisAlignment.End:
                        heights[i].axisStart = targetSize - heights[i].outputSize;
                        break;

                    case CrossAxisAlignment.Start:
                        heights[i].axisStart = 0;
                        break;

                    case CrossAxisAlignment.Stretch:
                        heights[i].axisStart = 0;
                        heights[i].outputSize = targetSize;
                        break;
                    default:
                        heights[i].axisStart = 0;
                        break;
                }

                heights[i].axisStart += axisOffset;
                if (heights[i].outputSize > maxHeight) {
                    maxHeight = heights[i].outputSize;
                }
            }

            return axisOffset + maxHeight;
        }

        private static void AlignMainAxis(FlexTrack track, FlexItem[] items, MainAxisAlignment mainAxisAlignment,
            float mainAxisOffset) {
            float spacerSize = 0;
            float offset = 0;
            int itemCount = track.itemCount;

            if (track.remainingSpace > 0) {
                switch (mainAxisAlignment) {
                    case MainAxisAlignment.Unset:
                    case MainAxisAlignment.Start:
                        break;
                    case MainAxisAlignment.Center:
                        mainAxisOffset *= 0.5f; //todo seems wrong to add this twice
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

            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
                items[i].axisStart = mainAxisOffset + offset;
                offset += items[i].outputSize + spacerSize;
            }
        }

        private static void GrowTrack(FlexTrack track, FlexItem[] items) {
            int pieces = 0;

            int startIndex = track.startItem;
            int endIndex = startIndex + track.itemCount;

            for (int i = startIndex; i < endIndex; i++) {
                pieces += items[i].growthFactor;
            }

            bool allocate = pieces > 0;
            float remainingSpace = track.remainingSpace;
            while (allocate && (int) remainingSpace > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remainingSpace / pieces;

                for (int i = startIndex; i < endIndex; i++) {
                    float max = items[i].maxSize;
                    float output = items[i].outputSize;
                    int growthFactor = items[i].growthFactor;

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
        }

        protected override void OnChildAdded(LayoutBox child) {
            if (!child.element.isEnabled || child.IsIgnored) {
                return;
            }

            int idx = FindLayoutSiblingIndex(child.element);

            if (idx <= children.Count) {
                children.Insert(idx, child);
            }
            else {
                children.Add(child);
            }

            if (widths.Length <= children.Count) {
                ArrayPool<FlexItem>.Resize(ref widths, children.Count);
            }

            if (heights.Length <= children.Count) {
                ArrayPool<FlexItem>.Resize(ref heights, children.Count);
            }

            if (child.element.isEnabled) {
                RequestContentSizeChangeLayout();
            }
        }

        public override void OnChildEnabled(LayoutBox child) {
            OnChildAdded(child);
        }

        public override void OnChildDisabled(LayoutBox child) {
            children.Remove(child);
            RequestContentSizeChangeLayout();
        }

        public override void OnChildStylePropertyChanged(LayoutBox child, StyleProperty property) {
            switch (property.propertyId) {
                case StylePropertyId.FlexItemGrow:
                case StylePropertyId.FlexItemShrink:
                case StylePropertyId.FlexItemSelfAlignment:
                case StylePropertyId.FlexItemOrder:
                    markedForLayout = true;
                    break;
            }

            base.OnChildStylePropertyChanged(child, property);
        }

        private static float DoShrinkTrack(FlexTrack track, FlexItem[] items, float[] outputs) {
            int pieces = 0;
            int startIndex = track.startItem;
            int endIndex = startIndex + track.itemCount;

            for (int i = startIndex; i < endIndex; i++) {
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

            return overflow;
        }

        private void ShrinkTrack(FlexTrack track, FlexItem[] items) {
            float[] outputs = GetScratchFloats();
            track.remainingSpace = DoShrinkTrack(track, items, outputs);
            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
                items[i].outputSize = outputs[i];
            }
        }

        private bool AddAndTryShrinkTrack(FlexTrack track, FlexItem[] items) {
            track.itemCount++;
            float size = items[track.startItem + track.itemCount - 1].outputSize;
            track.mainSize += size;
            track.remainingSpace -= size;
            if (TryShrinkTrack(track, items)) {
                return true;
            }
            else {
                track.mainSize -= size;
                track.remainingSpace += size;
                track.itemCount--;
                return false;
            }
        }

        private bool TryShrinkTrack(FlexTrack track, FlexItem[] items) {
            if (track.remainingSpace >= 0) return false;
            float[] outputs = GetScratchFloats();
            float overflow = DoShrinkTrack(track, items, outputs);
            if (overflow <= 0) {
                track.remainingSpace = overflow;
                track.mainSize = 0;
                for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
                    items[i].outputSize = outputs[i];
                    track.mainSize += outputs[i];
                }

                return true;
            }

            return false;
        }

        // todo make this smaller & don't use interface to avoid boxing
        private struct FlexItem : IComparable<FlexItem> {

            public int childIndex;
            public float axisStart;
            public float outputSize;
            public float totalSize;
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

        private class FlexTrack {

            public int startItem;
            public int itemCount;
            public float mainSize;
            public float remainingSpace;
            public float crossSize;

        }

    }

}