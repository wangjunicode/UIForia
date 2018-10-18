using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    // todo -- pool this
    // todo -- handle incremental layout, ie no layout when not required or only run partial like alignment change

    [DebuggerDisplay("{element.ToString()}")]
    public class FlexLayoutBox : LayoutBox {

        private FlexItem[] widths;
        private FlexItem[] heights;
        private readonly List<FlexTrack> tracks;

        private static float[] scratchFloats = ArrayPool<float>.Empty;

        public FlexLayoutBox(LayoutSystem layoutSystem, UIElement element)
            : base(layoutSystem, element) {
            tracks = ListPool<FlexTrack>.Get();
            widths = ArrayPool<FlexItem>.GetMinSize(4); // todo -- make static
            heights = ArrayPool<FlexItem>.GetMinSize(4); // todo -- make static
        }

        protected override float ComputeContentWidth() {
            if (style.FlexLayoutDirection == LayoutDirection.Row) {
                float max = 0;
                for (int i = 0; i < children.Count; i++) {
                    LayoutBox child = children[i];

                    widths[i] = new FlexItem();
                    widths[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                    widths[i].childIndex = i;
                    widths[i].outputSize = child.GetWidths().clampedSize;

                    if (widths[i].outputSize > max) max = widths[i].outputSize;

                    widths[i].crossAxisAlignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
                        ? child.style.FlexItemSelfAlignment
                        : style.FlexLayoutCrossAxisAlignment;
                }

                float adjustedHeight = style.PreferredHeight.IsFixed
                    ? GetPreferredHeight(0) - PaddingVertical - BorderVertical
                    : float.MaxValue;

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
                    widths[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
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
                    widths[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                    widths[i].growthFactor = child.style.FlexItemGrowthFactor;
                    widths[i].shrinkFactor = child.style.FlexItemShrinkFactor;
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
                actualWidth = GetWidths().clampedSize;
                actualHeight = GetHeights(actualWidth).clampedSize;
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
                widths[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                widths[i].growthFactor = child.style.FlexItemGrowthFactor;
                widths[i].shrinkFactor = child.style.FlexItemShrinkFactor;
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
                heights[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);

                LayoutBoxSize childHeights = child.GetHeights(widths[i].outputSize);

                heights[i].minSize = childHeights.minSize;
                heights[i].maxSize = childHeights.maxSize;
                heights[i].outputSize = childHeights.clampedSize;

                heights[i].crossAxisAlignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
                    ? child.style.FlexItemSelfAlignment
                    : style.FlexLayoutCrossAxisAlignment;
            }

            Array.Sort(heights, 0, children.Count);

            if (tracks.Count > 1) {
                for (int i = 0; i < tracks.Count; i++) {
                    FlexTrack track = tracks[i];

                    float targetHeight = 0;

                    for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                        if (heights[j].outputSize > targetHeight) {
                            targetHeight = heights[j].outputSize;
                        }
                    }

                    trackCrossAxisStart = PositionCrossAxisColumn(trackCrossAxisStart, track, targetHeight);

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

                trackCrossAxisStart = PositionCrossAxisColumn(trackCrossAxisStart, track, allocatedHeight - PaddingVertical - BorderVertical);

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

        private void RunRowLayout() {
            float max = 0;
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                widths[i] = new FlexItem();
                widths[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                widths[i].childIndex = i;
                widths[i].outputSize = child.GetWidths().clampedSize;

                if (widths[i].outputSize > max) max = widths[i].outputSize;

                widths[i].crossAxisAlignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
                    ? child.style.FlexItemSelfAlignment
                    : style.FlexLayoutCrossAxisAlignment;
            }

            for (int i = 0; i < children.Count; i++) {
                if (widths[i].crossAxisAlignment == CrossAxisAlignment.Stretch) {
                    widths[i].outputSize = max; // todo -- limit to max width?
                }
            }

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
                trackCrossAxisStart = PositionCrossAxisRow(trackCrossAxisStart, track, tracks.Count > 1 ? track.crossSize : adjustedWidth);

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

            actualWidth = Mathf.Max(allocatedWidth, maxWidth);
            actualHeight = Mathf.Max(allocatedHeight, largestTrackSize);
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
                    heights[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                    heights[i].growthFactor = child.style.FlexItemGrowthFactor;
                    heights[i].shrinkFactor = child.style.FlexItemShrinkFactor;
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
                heights[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                heights[i].growthFactor = child.style.FlexItemGrowthFactor;
                heights[i].shrinkFactor = child.style.FlexItemShrinkFactor;

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
                            heights[j].outputSize = Mathf.Max(heights[j].minSize, Mathf.Min(child.GetPreferredHeight(currentItemWidth), heights[j].maxSize));
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

        private float PositionCrossAxisRow(float axisOffset, FlexTrack track, float targetSize) {
            float crossSize = 0;
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

        private float PositionCrossAxisColumn(float axisOffset, FlexTrack track, float targetSize) {
            float maxHeight = 0;
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
        
        private static void AlignMainAxis(FlexTrack track, FlexItem[] items, MainAxisAlignment mainAxisAlignment, float mainAxisOffset) {
            float spacerSize = 0;
            float offset = 0;
            int itemCount = track.itemCount;

            if (track.remainingSpace > 0) {
                switch (mainAxisAlignment) {
                    case MainAxisAlignment.Unset:
                    case MainAxisAlignment.Start:
                        break;
                    case MainAxisAlignment.Center:
                        mainAxisOffset *= 0.5f;
                        offset = track.remainingSpace * 0.5f;
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
                int styleOrder = BitUtil.GetHighBits(order);
                int otherStyleOrder = BitUtil.GetHighBits(other.order);
                if (styleOrder != otherStyleOrder) {
                    return styleOrder < otherStyleOrder ? -1 : 1;
                }

                int sourceOrder = BitUtil.GetLowBits(order);
                return sourceOrder < BitUtil.GetLowBits(other.order) ? -1 : 1;
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