using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    // todo -- pool this
    // todo -- handle incremental layout, ie no layout when not required or only run partial like alignment change

    [DebuggerDisplay("{element.ToString()}")]
    public class FlexLayoutBox : LayoutBox {

        private List<FlexTrack> tracks;

        private FlexItem[] widths;
        private FlexItem[] heights;
        private List<LayoutBox> ignoredChildren;

        public FlexLayoutBox(LayoutSystem2 layoutSystem, UIElement element)
            : base(layoutSystem, element) {
            ignoredChildren = ListPool<LayoutBox>.Get();
            tracks = ListPool<FlexTrack>.Get();
            widths = ArrayPool<FlexItem>.GetMinSize(4);
            heights = ArrayPool<FlexItem>.GetMinSize(4);
        }

//        public void RunLayout() {
//            if (children.Count == 0) return;
//            tracks.Clear();
//
//            if (style.FlexLayoutDirection == LayoutDirection.Column) {
//                RunFullColumnLayout();
//            }
//            else {
//                RunFullRowLayout();
//            }
//
//            for (int i = 0; i < ignoredChildren.Count; i++) {
//                LayoutBox child = ignoredChildren[i];
//                if (!child.element.isEnabled) continue;
//                float width = Mathf.Max(child.MinWidth, Mathf.Min(child.PreferredWidth, child.MaxWidth));
//                float height = Mathf.Max(child.MinHeight, Mathf.Min(child.PreferredHeight, child.MaxHeight));
//                child.SetAllocatedRect(child.TransformX, child.TransformY, width, height);
//            }
//        }

        protected override Size RunContentSizeLayout() {
            if (style.FlexLayoutDirection == LayoutDirection.Row) {
                float maxWidth = 0;
                float totalHeight = 0;
                for (int i = 0; i < children.Count; i++) {
                    LayoutBox child = children[i];
                    if (child.element.isEnabled) {
                        maxWidth = Mathf.Max(maxWidth, Mathf.Max(child.MinWidth, Mathf.Min(child.PreferredWidth, child.MaxWidth)));
                        totalHeight += Mathf.Max(child.MinHeight, Mathf.Min(child.PreferredHeight, child.MaxHeight));
                    }
                }

                return new Size(maxWidth, totalHeight);
            }

            float maxHeight = 0;
            float totalWidth = 0;
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                if (child.element.isEnabled) {
                    float width = Mathf.Max(child.MinWidth, Mathf.Min(child.PreferredWidth, child.MaxWidth));
                    totalWidth += width;
                    maxHeight = Mathf.Max(maxHeight, Mathf.Max(child.MinHeight, Mathf.Min(child.GetPreferredHeightForWidth(width), child.MaxHeight)));
                }
            }

            return new Size(totalWidth, maxHeight);
        }

        public override void OnChildStylePropertyChanged(LayoutBox child, StyleProperty property) {
            if (child.element.isEnabled && ignoredChildren.Contains(child)) {
                switch (property.propertyId) {
                    case StylePropertyId.TransformPositionX:
                    case StylePropertyId.TransformPositionY:
                        float width = Mathf.Max(child.MinWidth, Mathf.Min(child.PreferredWidth, child.MaxWidth));
                        float height = Mathf.Max(child.MinHeight, Mathf.Min(child.PreferredHeight, child.MaxHeight));
                        child.SetAllocatedRect(child.TransformX, child.TransformY, width, height);
                        break;
                }
            }
        }

        public override void RunWidthLayout() {
            if (children.Count == 0) return;
            tracks.Clear();
            if (style.FlexLayoutDirection == LayoutDirection.Column) {
                RunWidthColumnLayout();
            }
            else {
                RunWidthRowLayout();
            }
        }

        public override void RunHeightLayout() {
            if (style.FlexLayoutDirection == LayoutDirection.Column) {
                RunHeightColumnLayout();
            }
            else {
                RunHeightRowLayout();
            }
        }


        private void RunWidthColumnLayout() {
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                widths[i] = new FlexItem();
                widths[i].childIndex = i;
                widths[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                widths[i].growthFactor = child.style.FlexItemGrowthFactor;
                widths[i].shrinkFactor = child.style.FlexItemShrinkFactor;
                widths[i].minSize = Mathf.Max(0, child.MinWidth);
                widths[i].maxSize = Mathf.Max(0, child.MaxWidth);
                widths[i].outputSize = Mathf.Max(widths[i].minSize, Mathf.Min(child.PreferredWidth, widths[i].maxSize));
            }

            Array.Sort(widths, 0, children.Count);

            float adjustedWidth = allocatedWidth - PaddingHorizontal - BorderHorizontal;

            FillTracks(widths, children.Count, adjustedWidth);
            float largestTrackSize = 0;
            float paddingBorderLeft = PaddingLeft + BorderLeft;

            for (int i = 0; i < tracks.Count; i++) {
                FlexTrack track = tracks[i];
                float remainingSpace = adjustedWidth - track.mainSize;

                if (remainingSpace > 0) {
                    GrowTrack(track, widths);
                }
                else if (remainingSpace < 0) {
                    ShrinkTrack(track, widths);
                }

                if (track.mainSize > largestTrackSize) {
                    largestTrackSize = track.mainSize;
                }

                AlignMainAxis(track, widths, style.FlexLayoutMainAxisAlignment, paddingBorderLeft);

                for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                    children[widths[j].childIndex].SetAllocatedXAndWidth(widths[j].axisStart, widths[j].outputSize);
                }
            }
        }

        private float RunColumnLayoutContentCheck(float targetWidth) {
            float cachedAllocatedWidth = allocatedWidth;
            float cachedAllocatedHeight = allocatedHeight;
            allocatedWidth = targetWidth;
            allocatedHeight = 0f;

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                widths[i] = new FlexItem();
                widths[i].childIndex = i;
                widths[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                widths[i].growthFactor = child.style.FlexItemGrowthFactor;
                widths[i].shrinkFactor = child.style.FlexItemShrinkFactor;
                widths[i].minSize = Mathf.Max(0, child.MinWidth);
                widths[i].maxSize = Mathf.Max(0, child.MaxWidth);
                widths[i].outputSize = Mathf.Max(widths[i].minSize, Mathf.Min(child.PreferredWidth, widths[i].maxSize));
            }

            Array.Sort(widths, 0, children.Count);

            float adjustedWidth = targetWidth - PaddingHorizontal - BorderHorizontal;

            FillTracks(widths, children.Count, adjustedWidth);

            float largestTrackSize = 0;
            float maxHeight = 0;

            for (int i = 0; i < tracks.Count; i++) {
                FlexTrack track = tracks[i];
                float height = 0;
                float remainingSpace = adjustedWidth - track.mainSize;

                if (remainingSpace > 0) {
                    GrowTrack(track, widths);
                }
                else if (remainingSpace < 0) {
                    ShrinkTrack(track, widths);
                }

                if (track.mainSize > largestTrackSize) {
                    largestTrackSize = track.mainSize;
                }

                for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                    LayoutBox child = children[widths[j].childIndex];
                    float minSize = Mathf.Max(0, child.MinHeight);
                    float maxSize = Mathf.Max(0, child.MaxHeight);
                    height += Mathf.Max(minSize, Mathf.Min(child.GetPreferredHeightForWidth(widths[i].outputSize), maxSize));
                }

                if (height > maxHeight) {
                    maxHeight = height;
                }
            }

            allocatedWidth = cachedAllocatedWidth;
            actualHeight = cachedAllocatedHeight;
            return maxHeight;
        }

        private void RunHeightColumnLayout() {
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                heights[i] = new FlexItem();
                heights[i].childIndex = i;
                heights[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                float minSize = Mathf.Max(0, child.MinHeight);
                float maxSize = Mathf.Max(0, child.MaxHeight);
                heights[i].outputSize = Mathf.Max(
                    minSize,
                    Mathf.Min(
                        child.GetPreferredHeightForWidth(widths[i].outputSize),
                        maxSize
                    )
                );
                heights[i].crossAxisAlignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
                    ? child.style.FlexItemSelfAlignment
                    : style.FlexLayoutCrossAxisAlignment;
            }

            Array.Sort(heights, 0, children.Count);

            float maxHeight = 0;
            float trackCrossAxisStart = 0;
            float targetHeight = allocatedHeight - PaddingVertical - BorderVertical;

            for (int i = 0; i < tracks.Count; i++) {
                FlexTrack track = tracks[i];
                trackCrossAxisStart = PositionCrossAxis(trackCrossAxisStart, track, heights, targetHeight);

                for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                    children[heights[j].childIndex].SetAllocatedYAndHeight(heights[j].axisStart, heights[j].outputSize);

                    if (heights[j].AxisEnd > maxHeight) {
                        maxHeight = heights[j].AxisEnd;
                    }
                }
            }

            actualHeight = maxHeight;
        }

        private void RunWidthRowLayout() {
            float max = 0;
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                widths[i] = new FlexItem();
                widths[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                widths[i].childIndex = i;
                widths[i].outputSize = Mathf.Max(child.MinWidth, Mathf.Min(child.PreferredWidth, child.MaxWidth));

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
                trackCrossAxisStart = PositionCrossAxis(trackCrossAxisStart, track, widths, adjustedWidth);

                for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                    children[widths[j].childIndex].SetAllocatedXAndWidth(widths[j].axisStart, widths[j].outputSize);
                }
            }

            FlexTrack lastTrack = tracks[tracks.Count - 1];
            for (int j = lastTrack.startItem; j < lastTrack.startItem + lastTrack.itemCount; j++) {
                if (widths[j].AxisEnd > maxWidth) {
                    maxWidth = widths[j].AxisEnd;
                }
            }


            actualWidth = maxWidth;
        }

        private void RunHeightRowLayout() {
            float largestTrackSize = 0;
            for (int i = 0; i < tracks.Count; i++) {
                FlexTrack track = tracks[i];

                if (track.mainSize > largestTrackSize) {
                    largestTrackSize = track.mainSize;
                }

                for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                    children[heights[j].childIndex].SetAllocatedYAndHeight(heights[j].axisStart, heights[j].outputSize);
                }
            }

            actualHeight = largestTrackSize;
        }


        public override float GetPreferredHeightForWidth(float width) {
            if (style.PreferredHeight.unit == UIUnit.Content) {
                // row stacks and we prefer not to wrap so this is fine I think
                width -= PaddingHorizontal - BorderHorizontal;
                if (style.FlexLayoutDirection == LayoutDirection.Row) {
                    float retn = 0;
                    for (int i = 0; i < children.Count; i++) {
                        float minSize = children[i].MinHeight;
                        float maxSize = children[i].MaxHeight;
                        float prfSize = children[i].GetPreferredHeightForWidth(width);
                        retn += Mathf.Max(minSize, Mathf.Min(prfSize, maxSize));
                    }

                    return retn + PaddingVertical + BorderVertical;
                }

                return RunColumnLayoutContentCheck(width) + PaddingVertical + BorderVertical;
            }

            return PreferredHeight;
        }

        private void FillTracks(FlexItem[] items, int itemCount, float targetSize) {
            FlexTrack currentTrack = new FlexTrack();
            if (style.FlexLayoutWrap != LayoutWrap.Wrap) {
                for (int i = 0; i < itemCount; i++) {
                    currentTrack.mainSize += items[i].outputSize;
                }

                currentTrack.startItem = 0;
                currentTrack.itemCount = itemCount;
                currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                tracks.Add(currentTrack);
                return;
            }

            for (int i = 0; i < itemCount; i++) {
                float size = items[i].outputSize;

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
                    if (TryShrinkTrack(currentTrack, items)) {
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
            FlexTrack currentTrack = new FlexTrack();

            if (style.FlexLayoutWrap != LayoutWrap.Wrap) {
                for (int i = 0; i < children.Count; i++) {
                    LayoutBox child = children[i];
                    heights[i] = new FlexItem();
                    heights[i].childIndex = i;
                    heights[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                    heights[i].growthFactor = child.style.FlexItemGrowthFactor;
                    heights[i].shrinkFactor = child.style.FlexItemShrinkFactor;
                    heights[i].minSize = Mathf.Max(0, child.MinHeight);
                    heights[i].maxSize = Mathf.Max(0, child.MaxHeight);
                    heights[i].outputSize =
                        Mathf.Max(heights[i].minSize, Mathf.Min(child.GetPreferredHeightForWidth(widths[i].outputSize), heights[i].maxSize));
                    currentTrack.mainSize += heights[i].outputSize;
                    currentTrack.crossSize = Mathf.Max(widths[i].outputSize, currentTrack.crossSize);
                }

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
                heights[i].minSize = Mathf.Max(0, child.MinHeight);
                heights[i].maxSize = Mathf.Max(0, child.MaxHeight);
                float currentItemHeight = Mathf.Max(heights[i].minSize, Mathf.Min(child.GetPreferredHeightForWidth(widths[i].outputSize), heights[i].maxSize));
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
                            heights[j].outputSize = Mathf.Max(heights[j].minSize, Mathf.Min(child.GetPreferredHeightForWidth(currentItemWidth), heights[j].maxSize));
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
                else if (TryShrinkTrack(currentTrack, heights)) {
                    currentTrack.mainSize += currentItemHeight;
                    currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                    currentTrack.itemCount++;
                }
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

            ArrayPool<float>.Release(tmpWidths);
            ArrayPool<float>.Release(tmpHeights);
        }

        public override void OnChildAdded(LayoutBox child) {
            if ((child.style.LayoutBehavior & LayoutBehavior.Ignored) != 0) {
                ignoredChildren.Add(child);
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
                RequestParentLayoutIfContentBased();
                RequestLayout();
            }
        }

        private static int FindLayoutSiblingIndex(UIElement element) {
            // if parent is not in layout
            // we want to replace it
            // so find parent's sibling index
            // spin through laid out children until finding target
            // use parent index + child index
            if (element.parent == null) return 0;

            int idx = 0;
            for (int i = 0; i < element.parent.ownChildren.Length; i++) {
                UIElement sibling = element.parent.ownChildren[i];
                if (sibling == element) {
                    break;
                }

                if ((sibling.flags & UIElementFlags.RequiresLayout) != 0 && (sibling.style.computedStyle.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
                    idx++;
                }
            }

            if ((element.parent.flags & UIElementFlags.RequiresLayout) == 0) {
                idx += FindLayoutSiblingIndex(element.parent);
            }

            return idx;
        }

        public override void OnChildRemoved(LayoutBox child) {
            ignoredChildren.Remove(child);
            if (children.Remove(child)) {
                RequestLayout();
            }
        }

        private static float PositionCrossAxis(float axisOffset, FlexTrack track, FlexItem[] items, float targetSize) {
            float crossSize = 0;
            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
                switch (items[i].crossAxisAlignment) {
                    case CrossAxisAlignment.Center:
                        items[i].axisStart = (targetSize * 0.5f) - (items[i].outputSize * 0.5f);
                        break;

                    case CrossAxisAlignment.End:
                        items[i].axisStart = targetSize - items[i].outputSize;
                        break;

                    case CrossAxisAlignment.Start:
                        items[i].axisStart = 0;
                        break;

                    case CrossAxisAlignment.Stretch:
                        items[i].axisStart = 0;
                        items[i].outputSize = targetSize;
                        break;
                    default:
                        items[i].axisStart = 0;
                        break;
                }

                items[i].axisStart += axisOffset;
                crossSize += items[i].outputSize;
            }

            return axisOffset + crossSize;
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
            float[] outputs = ArrayPool<float>.GetMinSize(children.Count);
            track.remainingSpace = DoShrinkTrack(track, items, outputs);
            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
                items[i].outputSize = outputs[i];
            }

            ArrayPool<float>.Release(outputs);
        }

        private bool TryShrinkTrack(FlexTrack track, FlexItem[] items) {
            if (track.remainingSpace >= 0) return false;
            float[] outputs = ArrayPool<float>.GetMinSize(children.Count);
            float overflow = DoShrinkTrack(track, items, outputs);
            if (overflow <= 0) {
                track.remainingSpace = overflow;
                track.mainSize = 0;
                for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
                    items[i].outputSize = outputs[i];
                    track.mainSize += outputs[i];
                }

                ArrayPool<float>.Release(outputs);

                return true;
            }

            ArrayPool<float>.Release(outputs);

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

            public void Clear() {
                startItem = 0;
                itemCount = 0;
                mainSize = 0;
                remainingSpace = 0;
            }

        }

    }

}
//
//private void RunFullColumnLayout() {
//            for (int i = 0; i < children.Count; i++) {
//                LayoutBox child = children[i];
//
//                widths[i] = new FlexItem();
//                widths[i].childIndex = i;
//                widths[i].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
//                widths[i].growthFactor = child.style.FlexItemGrowthFactor;
//                widths[i].shrinkFactor = child.style.FlexItemShrinkFactor;
//                widths[i].minSize = Mathf.Max(0, child.MinWidth);
//                widths[i].maxSize = Mathf.Max(0, child.MaxWidth);
//                widths[i].outputSize = Mathf.Max(widths[i].minSize, Mathf.Min(child.PreferredWidth, widths[i].maxSize));
//            }
//
//            Array.Sort(widths, 0, children.Count);
//            Array.Sort(heights, 0, children.Count);
//
//            float adjustedWidth = allocatedWidth - PaddingHorizontal - BorderHorizontal;
//            float adjustedHeight = allocatedHeight - PaddingVertical - BorderVertical;
//
//            Vector2 size = Run(children.Count, widths, heights, adjustedWidth, adjustedHeight, PaddingLeft + BorderLeft, PaddingTop + BorderTop);
//            actualWidth = size.x;
//            actualHeight = size.y;
//        }
//
//        private void RunFullRowLayout() {
//            int inFlowItemCount = 0;
//
//            // for each item
//            // find max item preferred width
//            // if we are content width based 
//            // track width = max width of items in track
//            // using what width?
//            // width = max content size, min content size, preferred size, max size, min size
//            // if preferred width is content size
//            // track width = max of items in track
//
//            float max = 0;
//            float adjustedHeight = allocatedHeight - PaddingVertical - BorderVertical;
//            float adjustedWidth = allocatedWidth - PaddingHorizontal - BorderHorizontal;
//
//            for (int i = 0; i < children.Count; i++) {
//                LayoutBox child = children[i];
//
//                if (child.element.isEnabled) {
//                    widths[inFlowItemCount] = new FlexItem();
//                    widths[inFlowItemCount].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
//                    widths[inFlowItemCount].childIndex = inFlowItemCount;
//                    widths[inFlowItemCount].outputSize = Mathf.Max(child.MinWidth, Mathf.Min(child.PreferredWidth, child.MaxWidth));
//
//                    if (widths[inFlowItemCount].outputSize > max) max = widths[inFlowItemCount].outputSize;
//
//                    widths[inFlowItemCount].crossAxisAlignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
//                        ? child.style.FlexItemSelfAlignment
//                        : style.FlexLayoutCrossAxisAlignment;
//
//                    inFlowItemCount++;
//                }
//            }
//
//            inFlowItemCount = 0;
//            for (int i = 0; i < children.Count; i++) {
//                LayoutBox child = children[i];
//
//                if (child.element.isEnabled) {
//                    float width = widths[inFlowItemCount].outputSize;
//                    if (widths[inFlowItemCount].crossAxisAlignment == CrossAxisAlignment.Stretch) {
//                        width = adjustedWidth;
//                    }
//
//                    heights[inFlowItemCount] = new FlexItem();
//                    heights[inFlowItemCount].childIndex = inFlowItemCount;
//                    heights[inFlowItemCount].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
//                    heights[inFlowItemCount].growthFactor = child.style.FlexItemGrowthFactor;
//                    heights[inFlowItemCount].shrinkFactor = child.style.FlexItemShrinkFactor;
//                    heights[inFlowItemCount].minSize = Mathf.Max(0, child.MinHeight);
//                    heights[inFlowItemCount].maxSize = Mathf.Max(0, child.MaxHeight);
//                    heights[inFlowItemCount].outputSize =
//                        Mathf.Max(heights[inFlowItemCount].minSize, Mathf.Min(child.GetPreferredHeightForWidth(width), heights[inFlowItemCount].maxSize));
//                    inFlowItemCount++;
//                }
//            }
//
//            Array.Sort(widths, 0, inFlowItemCount);
//            Array.Sort(heights, 0, inFlowItemCount);
//
//            FillTracks(heights, inFlowItemCount, adjustedHeight);
//
//            float largestTrackSize = 0;
//            float trackCrossAxisStart = 0;
//            Vector2 retn = Vector2.zero;
//
//            for (int i = 0; i < tracks.Count; i++) {
//                FlexTrack track = tracks[i];
//                float remainingSpace = adjustedHeight - track.mainSize;
//
//                if (remainingSpace > 0) {
//                    GrowTrack(track, heights);
//                }
//                else if (remainingSpace < 0) {
//                    ShrinkTrack(track, heights);
//                }
//
//                if (track.mainSize > largestTrackSize) {
//                    largestTrackSize = track.mainSize;
//                }
//
//                AlignMainAxis(track, heights, style.FlexLayoutMainAxisAlignment, PaddingTop + BorderTop);
//                trackCrossAxisStart = PositionCrossAxis(trackCrossAxisStart, track, widths, adjustedWidth);
//
//                for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
//                    children[widths[j].childIndex].SetAllocatedRect(
//                        widths[j].axisStart,
//                        heights[j].axisStart,
//                        widths[j].outputSize,
//                        heights[j].outputSize
//                    );
//
//                    if (widths[j].axisStart + widths[j].outputSize > retn.y) {
//                        retn.y = widths[j].axisStart + widths[j].outputSize;
//                    }
//                }
//            }
//
//            retn.x = largestTrackSize;
//
//            //Vector2 size = Run(inFlowItemCount, heights, widths, adjustedHeight, adjustedWidth, PaddingTop + BorderTop, PaddingLeft + BorderLeft);
//
//            actualWidth = retn.y;
//            actualHeight = largestTrackSize;
////        }
//        private Vector2 Run(int inFlowItemCount, FlexItem[] mainAxisItems, FlexItem[] crossAxisItems, float mainAxisTargetSize, float crossAxisTargetSize,
//            float mainAxisOffset, float crossAxisOffset) {
//            FillTracks(mainAxisItems, inFlowItemCount, mainAxisTargetSize);
//
//            float trackCrossAxisStart = crossAxisOffset;
//            float largestTrackSize = 0;
//            Vector2 retn = Vector2.zero;
//
//            for (int i = 0; i < tracks.Count; i++) {
//                FlexTrack track = tracks[i];
//                float remainingSpace = mainAxisTargetSize - track.mainSize;
//
//                if (remainingSpace > 0) {
//                    GrowTrack(track, mainAxisItems);
//                }
//                else if (remainingSpace < 0) {
//                    ShrinkTrack(track, mainAxisItems);
//                }
//
//                if (track.mainSize > largestTrackSize) {
//                    largestTrackSize = track.mainSize;
//                }
//
//                AlignMainAxis(track, mainAxisItems, style.FlexLayoutMainAxisAlignment, mainAxisOffset);
//
//                if (crossAxisItems == heights) {
//                    inFlowItemCount = 0;
//                    for (int h = 0; h < children.Count; h++) {
//                        LayoutBox child = children[h];
//
//                        if (child.element.isEnabled) {
//                            child.allocatedWidth = widths[inFlowItemCount].outputSize;
//                            heights[inFlowItemCount] = new FlexItem();
//                            heights[inFlowItemCount].childIndex = inFlowItemCount;
//                            heights[inFlowItemCount].outputSize = Mathf.Max(
//                                child.MinHeight,
//                                Mathf.Min(
//                                    child.GetPreferredHeightForWidth(widths[inFlowItemCount].outputSize),
//                                    child.MaxHeight
//                                )
//                            );
//
//                            heights[inFlowItemCount].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, h);
//
//                            heights[inFlowItemCount].crossAxisAlignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
//                                ? child.style.FlexItemSelfAlignment
//                                : style.FlexLayoutCrossAxisAlignment;
//
//                            inFlowItemCount++;
//                        }
//                    }
//                }
//
//                trackCrossAxisStart = PositionCrossAxis(trackCrossAxisStart, track, crossAxisItems, crossAxisTargetSize);
//
//                for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
//                    children[widths[j].childIndex].SetAllocatedRect(
//                        widths[j].axisStart,
//                        heights[j].axisStart,
//                        widths[j].outputSize,
//                        heights[j].outputSize
//                    );
//
//                    if (crossAxisItems[j].axisStart + crossAxisItems[j].outputSize > retn.y) {
//                        retn.y = crossAxisItems[j].axisStart + crossAxisItems[j].outputSize;
//                    }
//                }
//            }
//
//            retn.x = largestTrackSize;
//            return retn;
//        }