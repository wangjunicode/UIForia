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

        private FlexItemAxis[] widths;
        private FlexItemAxis[] heights;
        private List<LayoutBox> ignoredChildren;

        public FlexLayoutBox(LayoutSystem2 layoutSystem, UIElement element)
            : base(layoutSystem, element) {
            ignoredChildren = ListPool<LayoutBox>.Get();
            tracks = ListPool<FlexTrack>.Get();
            widths = ArrayPool<FlexItemAxis>.GetMinSize(4);
            heights = ArrayPool<FlexItemAxis>.GetMinSize(4);
        }

        public override void RunLayout() {
            if (children.Count == 0) return;
            tracks.Clear();

            if (style.FlexLayoutDirection == LayoutDirection.Column) {
                RunFullColumnLayout();
            }
            else {
                RunFullRowLayout();
            }

            for (int i = 0; i < ignoredChildren.Count; i++) {
                LayoutBox child = ignoredChildren[i];
                if (!child.element.isEnabled) continue;
                float width = Mathf.Max(child.MinWidth, Mathf.Min(child.PreferredWidth, child.MaxWidth));
                float height = Mathf.Max(child.MinHeight, Mathf.Min(child.PreferredHeight, child.MaxHeight));
                child.SetAllocatedRect(child.TransformX, child.TransformY, width, height);
            }
        }

        protected override Size RunContentSizeLayout() {
            if (style.FlexLayoutDirection == LayoutDirection.Row) {
                float maxWidth = 0;
                float totalHeight = 0;
                for (int i = 0; i < children.Count; i++) {
                    LayoutBox child = children[i];
                    if (child.element.isEnabled && (child.style.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
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
                if (child.element.isEnabled && (child.style.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
                    maxHeight = Mathf.Max(maxHeight, Mathf.Max(child.MinHeight, Mathf.Min(child.PreferredHeight, child.MaxHeight)));
                    totalWidth += Mathf.Max(child.MinWidth, Mathf.Min(child.PreferredWidth, child.MaxWidth));
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

        private void RunFullColumnLayout() {
            int inFlowItemCount = 0;

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                if (child.element.isEnabled && (child.style.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
                    widths[inFlowItemCount] = new FlexItemAxis();
                    widths[inFlowItemCount].childIndex = inFlowItemCount;
                    widths[inFlowItemCount].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                    widths[inFlowItemCount].growthFactor = child.style.FlexItemGrowthFactor;
                    widths[inFlowItemCount].shrinkFactor = child.style.FlexItemShrinkFactor;
                    widths[inFlowItemCount].minSize = Mathf.Max(0, child.MinWidth);
                    widths[inFlowItemCount].maxSize = Mathf.Max(0, child.MaxWidth);
                    widths[inFlowItemCount].outputSize = Mathf.Max(widths[inFlowItemCount].minSize, Mathf.Min(child.PreferredWidth, widths[inFlowItemCount].maxSize));

                    float prevAllocated = child.allocatedWidth;
                    // will be overwritten later but needs to be set for proper content sized height...I think
                    child.allocatedWidth = widths[inFlowItemCount].outputSize;

                    heights[inFlowItemCount] = new FlexItemAxis();
                    heights[inFlowItemCount].childIndex = inFlowItemCount;
                    heights[inFlowItemCount].outputSize = Mathf.Max(child.MinHeight, Mathf.Min(child.PreferredHeight, child.MaxHeight));
                    heights[inFlowItemCount].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);

                    heights[inFlowItemCount].crossAxisAlignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
                        ? child.style.FlexItemSelfAlignment
                        : style.FlexLayoutCrossAxisAlignment;

                    child.allocatedWidth = prevAllocated;

                    inFlowItemCount++;
                }
            }

            Array.Sort(widths, 0, inFlowItemCount);
            Array.Sort(heights, 0, inFlowItemCount);

            float adjustedWidth = allocatedWidth - PaddingHorizontal - BorderHorizontal;

            Vector2 size = Run(inFlowItemCount, widths, heights, adjustedWidth, allocatedHeight, PaddingLeft + BorderLeft, PaddingTop + BorderTop);
            actualWidth = size.x;
            actualHeight = size.y;
        }

        private void RunFullRowLayout() {
            int inFlowItemCount = 0;

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                if (child.element.isEnabled && (child.style.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
                    widths[inFlowItemCount] = new FlexItemAxis();
                    widths[inFlowItemCount].childIndex = inFlowItemCount;
                    widths[inFlowItemCount].outputSize = Mathf.Max(child.MinWidth, Mathf.Min(child.PreferredWidth, child.MaxWidth));
                    widths[inFlowItemCount].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);

                    widths[inFlowItemCount].crossAxisAlignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
                        ? child.style.FlexItemSelfAlignment
                        : style.FlexLayoutCrossAxisAlignment;

                    heights[inFlowItemCount] = new FlexItemAxis();
                    heights[inFlowItemCount].childIndex = inFlowItemCount;
                    heights[inFlowItemCount].order = BitUtil.SetHighLowBits(child.style.FlexItemOrder, i);
                    heights[inFlowItemCount].growthFactor = child.style.FlexItemGrowthFactor;
                    heights[inFlowItemCount].shrinkFactor = child.style.FlexItemShrinkFactor;
                    heights[inFlowItemCount].minSize = Mathf.Max(0, child.MinHeight);
                    heights[inFlowItemCount].maxSize = Mathf.Max(0, child.MaxHeight);
                    heights[inFlowItemCount].outputSize = Mathf.Max(heights[inFlowItemCount].minSize, Mathf.Min(child.PreferredHeight, heights[inFlowItemCount].maxSize));

                    inFlowItemCount++;
                }
            }

            Array.Sort(widths, 0, inFlowItemCount);
            Array.Sort(heights, 0, inFlowItemCount);

            float adjustedHeight = allocatedHeight - PaddingVertical - BorderVertical;

            Vector2 size = Run(inFlowItemCount, heights, widths, adjustedHeight, allocatedWidth, PaddingTop + BorderTop, PaddingLeft + BorderLeft);
            actualWidth = size.y;
            actualHeight = size.x;
        }

        private Vector2 Run(int inFlowItemCount, FlexItemAxis[] mainAxisItems, FlexItemAxis[] crossAxisItems, float mainAxisTargetSize, float crossAxisTargetSize,
            float mainAxisOffset, float crossAxisOffset) {
            FillTracks(mainAxisItems, inFlowItemCount, mainAxisTargetSize);

            float trackCrossAxisStart = crossAxisOffset;
            float largestTrackSize = 0;
            Vector2 retn = Vector2.zero;

            for (int i = 0; i < tracks.Count; i++) {
                FlexTrack track = tracks[i];
                float remainingSpace = mainAxisTargetSize - track.mainSize;

                if (remainingSpace > 0) {
                    GrowTrack(track, mainAxisItems);
                }
                else if (remainingSpace < 0) {
                    ShrinkTrack(track, mainAxisItems);
                }

                if (track.mainSize > largestTrackSize) {
                    largestTrackSize = track.mainSize;
                }

                AlignMainAxis(track, mainAxisItems, style.FlexLayoutMainAxisAlignment, mainAxisOffset);
                trackCrossAxisStart = PositionCrossAxis(trackCrossAxisStart, track, crossAxisItems, crossAxisTargetSize);

                for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                    children[widths[j].childIndex].SetAllocatedRect(
                        widths[j].axisStart,
                        heights[j].axisStart,
                        widths[j].outputSize,
                        heights[j].outputSize
                    );

                    if (crossAxisItems[j].axisStart + crossAxisItems[j].outputSize > retn.y) {
                        retn.y = crossAxisItems[j].axisStart + crossAxisItems[j].outputSize;
                    }
                }
            }

            retn.x = largestTrackSize;
            return retn;
        }

        private void FillTracks(FlexItemAxis[] items, int itemCount, float targetSize) {
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
                ArrayPool<FlexItemAxis>.Resize(ref widths, children.Count);
            }

            if (heights.Length <= children.Count) {
                ArrayPool<FlexItemAxis>.Resize(ref heights, children.Count);
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

        private static float PositionCrossAxis(float axisOffset, FlexTrack track, FlexItemAxis[] items, float targetSize) {
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

        private static void AlignMainAxis(FlexTrack track, FlexItemAxis[] items, MainAxisAlignment mainAxisAlignment, float mainAxisOffset) {
            float spacerSize = 0;
            float offset = 0;
            int itemCount = track.itemCount;

            if (track.remainingSpace > 0) {
                switch (mainAxisAlignment) {
                    case MainAxisAlignment.Unset:
                    case MainAxisAlignment.Start:
                        break;
                    case MainAxisAlignment.Center:
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

        private static void GrowTrack(FlexTrack track, FlexItemAxis[] items) {
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

        private static float DoShrinkTrack(FlexTrack track, FlexItemAxis[] items, float[] outputs) {
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

        private void ShrinkTrack(FlexTrack track, FlexItemAxis[] items) {
            float[] outputs = ArrayPool<float>.GetMinSize(children.Count);
            track.remainingSpace = DoShrinkTrack(track, items, outputs);
            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
                items[i].outputSize = outputs[i];
            }

            ArrayPool<float>.Release(outputs);
        }

        private bool TryShrinkTrack(FlexTrack track, FlexItemAxis[] items) {
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

        private struct FlexItemAxis : IComparable<FlexItemAxis> {

            public int childIndex;
            public float axisStart;
            public float outputSize;
            public float maxSize;
            public float minSize;
            public int growthFactor;
            public int shrinkFactor;
            public CrossAxisAlignment crossAxisAlignment;
            public int order;

            public int CompareTo(FlexItemAxis other) {
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

            public void Clear() {
                startItem = 0;
                itemCount = 0;
                mainSize = 0;
                remainingSpace = 0;
            }

        }

    }

}