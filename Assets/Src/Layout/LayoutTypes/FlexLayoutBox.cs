using System;
using System.Collections.Generic;
using Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    public class FlexLayoutBox : LayoutBox {

        private List<FlexTrack> tracks;

        private FlexItemAxis[] widths;
        private FlexItemAxis[] heights;

        public FlexLayoutBox(LayoutSystem2 layoutSystem, UIElement element)
            : base(layoutSystem, element) {
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
        }

        protected override Size RunContentSizeLayout() {
            if (style.FlexLayoutDirection == LayoutDirection.Row) {
                float maxWidth = 0;
                float totalHeight = 0;
                for (int i = 0; i < children.Count; i++) {
                    LayoutBox child = children[i];
                    if (child.element.isEnabled) { // }&& child.style.flow != LayoutFlowType.OutOfFlow) {
                        maxWidth = Mathf.Max(maxWidth, Mathf.Max(child.MinWidth, Mathf.Min(child.PreferredWidth, child.MaxWidth)));
                        totalHeight += Mathf.Max(child.MinHeight, Mathf.Min(child.PreferredHeight, child.MaxHeight));
                    }
                }

                return new Size(maxWidth, totalHeight);
            }

            return new Size();
        }

        private void RunFullColumnLayout() { }

        private void RunFullRowLayout() {
            int inFlowItemCount = 0;

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];

                if (child.element.isEnabled) { // }&& child.style.flow != LayoutFlowType.OutOfFlow) {
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


            float targetSize = allocatedHeight;

            FillTracks(heights, inFlowItemCount, targetSize);

            float trackCrossAxisStart = 0;
            float maxTrackHeight = 0;

            for (int i = 0; i < tracks.Count; i++) {
                FlexTrack track = tracks[i];
                float remainingSpace = targetSize - track.mainSize;

                if (remainingSpace > 0) {
                    GrowTrack(track, heights, remainingSpace);
                }
                else if (remainingSpace < 0) {
                    ShrinkTrack(track, heights, targetSize);
                }

                if (track.mainSize > maxTrackHeight) {
                    maxTrackHeight = track.mainSize;
                }

                AlignMainAxis(track, heights, style.FlexLayoutMainAxisAlignment);
                trackCrossAxisStart = PositionCrossAxis(trackCrossAxisStart, track, widths, allocatedWidth);

                for (int j = track.startItem; j < track.startItem + track.itemCount; j++) {
                    children[widths[j].childIndex].SetAllocatedRect(new Rect(
                        widths[j].axisStart,
                        heights[j].axisStart,
                        widths[j].outputSize,
                        heights[j].outputSize)
                    );

                    if (widths[j].axisStart + widths[j].outputSize > actualWidth) {
                        actualWidth = widths[j].axisStart + widths[j].outputSize;
                    }
                }
            }

            actualWidth = trackCrossAxisStart + tracks[tracks.Count - 1].crossSize;
            actualHeight = maxTrackHeight;
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
                currentTrack.crossSize = 0;
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
                    currentTrack.itemCount++;
                    currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                    tracks.Add(currentTrack);

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i;
                    currentTrack.mainSize = size;
                    currentTrack.itemCount = 1;
                    currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                    tracks.Add(currentTrack);

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i;
                    currentTrack.itemCount = 1;
                    currentTrack.mainSize = size;
                }
                else {
                    currentTrack.itemCount++;
                    currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
                    tracks.Add(currentTrack);

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i;
                    currentTrack.itemCount = 1;
                    currentTrack.mainSize = size;
                }
            }

            currentTrack.remainingSpace = targetSize - currentTrack.mainSize;
            tracks.Add(currentTrack);
        }

        public override void OnChildAddedChild(LayoutBox child) {
            children.Add(child);
            if (widths.Length <= children.Count) {
                ArrayPool<FlexItemAxis>.Resize(ref widths, children.Count);
            }

            if (heights.Length <= children.Count) {
                ArrayPool<FlexItemAxis>.Resize(ref heights, children.Count);
            }
        }

        private static float PositionCrossAxis(float axisOffset, FlexTrack track, FlexItemAxis[] items, float targetSize) {
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
            }

            return axisOffset + track.crossSize;
        }

        private static void AlignMainAxis(FlexTrack track, FlexItemAxis[] items, MainAxisAlignment mainAxisAlignment) {
            float offset = 0;
            float spacerSize = 0;
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

            for (int i = 0; i < itemCount; i++) {
                items[i].axisStart = offset;
                offset += items[i].outputSize + spacerSize;
            }
        }

        private static void GrowTrack(FlexTrack track, FlexItemAxis[] items, float targetSize) {
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

        private static void ShrinkTrack(FlexTrack track, FlexItemAxis[] items, float targetSize) {
            int pieces = 0;
            int startIndex = track.startItem;
            int endIndex = startIndex + track.itemCount;

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
                    float output = items[i].outputSize;
                    int shrinkFactor = items[i].shrinkFactor;

                    if (shrinkFactor == 0 || (int) output == (int) min || output == 0f) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    output = (totalShrink < min) ? min : totalShrink;
                    overflow += output - start;

                    items[i].outputSize = output;
                }
            }
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
            public float crossSize;
            public float remainingSpace;

            public void Clear() {
                startItem = 0;
                itemCount = 0;
                mainSize = 0;
                crossSize = 0;
                remainingSpace = 0;
            }

        }

    }

}