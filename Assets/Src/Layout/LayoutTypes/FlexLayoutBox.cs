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
        }

        public override void RunLayout() {
            ApplyOrdering(style);
            tracks.Clear();

            if (style.layoutParameters.direction == LayoutDirection.Column) {
                RunFullColumnLayout();
            }
            else {
                RunFullRowLayout();
            }
        }
        
        private void RunFullColumnLayout() {
            int inFlowItemCount = 0;
            // allocated width / height is known at this point
            for (int i = 0; i < children.Count; i++) {
                if (children[i].element.isEnabled && children[i].style.layoutParameters.flow != LayoutFlowType.OutOfFlow) {
                    widths[inFlowItemCount] = new FlexItemAxis();
                    widths[inFlowItemCount].minSize = children[i].minWidth;
                    widths[inFlowItemCount].maxSize = children[i].maxWidth;
                    widths[inFlowItemCount].preferredSize = children[i].preferredWidth;
                    widths[inFlowItemCount].outputSize = Mathf.Max(children[i].minWidth, Mathf.Min(children[i].preferredWidth, children[i].maxWidth));
                    widths[inFlowItemCount].growthFactor = children[i].element.style.growthFactor;
                    widths[inFlowItemCount].shrinkFactor = children[i].element.style.shrinkFactor;
                    inFlowItemCount++;
                }
            }

            if (style.layoutParameters.wrap == LayoutWrap.Wrap) {
                FillTracksWrapped(widths, inFlowItemCount, allocatedWidth);
            }
            else {
                FillColumnTracksUnwrapped(widths, inFlowItemCount, allocatedWidth);
            }
        }

        public float GetPreferredHeightForWidth(float width) {
            if (style.layoutParameters.direction == LayoutDirection.Column) {
                
                if (style.layoutParameters.wrap == LayoutWrap.Wrap) {
                    // fill width tracks 
                    // return total height after wrap
                }
                else {
                    // if approximate == true
                    // don't bother growing / shrinking
                    // return max preferred height using width and clamped to min / max
                }
            }

            return 0;
        }
        
        private void RunFullRowLayout() {
            int inFlowItemCount = 0;
            for (int i = 0; i < children.Count; i++) {
                if (children[i].element.isEnabled && children[i].style.layoutParameters.flow != LayoutFlowType.OutOfFlow) {
                    widths[inFlowItemCount] = new FlexItemAxis();
                    widths[inFlowItemCount].childIndex = inFlowItemCount;
                    widths[inFlowItemCount].outputSize = Mathf.Max(children[i].minWidth, Mathf.Min(children[i].preferredWidth, children[i].maxWidth));

                    heights[inFlowItemCount] = new FlexItemAxis();
                    heights[inFlowItemCount].childIndex = inFlowItemCount;
                    heights[inFlowItemCount].preferredSize = children[i].preferredHeight;
                    heights[inFlowItemCount].minSize = children[i].minHeight;
                    heights[inFlowItemCount].maxSize = children[i].maxHeight;
                    heights[inFlowItemCount].outputSize = Mathf.Max(children[i].minHeight, Mathf.Min(children[i].preferredHeight, children[i].maxHeight));

                    inFlowItemCount++;
                }
            }

            float targetSize = allocatedHeight;
            
            FlexTrack currentTrack = new FlexTrack();
            for (int i = 0; i < inFlowItemCount; i++) {
                float size = heights[i].preferredSize;

                if (currentTrack.mainSize + size < targetSize) {
                    currentTrack.mainSize += size;
                }
                else if (size >= targetSize) {
                    currentTrack.itemCount++;
                    tracks.Add(currentTrack);

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i;
                    currentTrack.mainSize = size;
                    currentTrack.itemCount = 1;
                    tracks.Add(currentTrack);

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i;
                    currentTrack.itemCount = 1;
                    currentTrack.mainSize = size;
                }
                else {
                    currentTrack.itemCount++;
                    tracks.Add(currentTrack);

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i;
                    currentTrack.itemCount = 1;
                    currentTrack.mainSize = size;
                }
            }

            tracks.Add(currentTrack);

            //get preferred widths
            //get initial heights
            //place in track until overflow
            //stretch width if needed
            //using final width, get min / max / preferred height
            //set heights
            // for each track grow/shrink to try to fit allocated height
            // after grow / shrink 
            
            // if width grows height should shrink (for text at least)
            
            // get max track height
            // place next item in row at preferred height for width
            // when doing row layout we never shrink widths so we can safely use largest preferred width in track as sizing
            // means we need to 
            /*
             * for each width item
             *     get height using preferred width
             *     place in track
             *     if exceeds target height, new track
             *
             * find tallest track
             * tracks shrink individually
             * if content sized and wrapping, don't wrap
             * wrap only works with a fixed size
             *
             * track width = largest width
             * need to getPreferredHeight first without a width constraint, then with the width constraint if allocated width != preferred width & alignment stretches
             *
             * if height shrinks && additional width available, can width grow to fill?
             *
             * when content changes -> mark for preferred size update if needed
             *
             * if parent is content sized
             *     mark parent for update
             *         recurse upwards
             * 
             * and also GetMinRequiredHeightForWidth() 2 calls but only when content sized & stretch depending on axis
             *
             *     in recursive layout we don't have an allocated parent size when depth > 1
             *     content based height things 
             *     
             *     if predicted allocated width != actual allocated width -> get new height value
             *        width == allocated width || cachedWidths.Find()
             * 
             * some tracks could grow while others shrink in the same layout if wrapped
             *
             * place item
             *     if track size > targetSize
             *         start new track
             *         for each width get min height
             *             
             *     try to to shrink current track
             *         
             *     
             */

            if (style.layoutParameters.wrap == LayoutWrap.Wrap) {
                FillTracksWrapped(widths, inFlowItemCount, allocatedWidth);
            }
            else { }
        }


        private void FillTracksWrapped(FlexItemAxis[] items, int itemCount, float targetSize) {
            FlexTrack currentTrack = new FlexTrack();
            for (int i = 0; i < itemCount; i++) {
                float size = items[i].preferredSize;

                if (currentTrack.mainSize + size < targetSize) {
                    currentTrack.mainSize += size;
                }
                else if (size >= targetSize) {
                    currentTrack.itemCount++;
                    tracks.Add(currentTrack);

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i;
                    currentTrack.mainSize = size;
                    currentTrack.itemCount = 1;
                    tracks.Add(currentTrack);

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i;
                    currentTrack.itemCount = 1;
                    currentTrack.mainSize = size;
                }
                else {
                    currentTrack.itemCount++;
                    tracks.Add(currentTrack);

                    currentTrack = new FlexTrack();
                    currentTrack.startItem = i;
                    currentTrack.itemCount = 1;
                    currentTrack.mainSize = size;
                }
            }

            tracks.Add(currentTrack);
        }

        private void FillColumnTracksUnwrapped(FlexItemAxis[] items, int itemCount, float targetSize) {
            FlexTrack currentTrack = new FlexTrack();
            currentTrack.itemCount = itemCount;
            for (int i = 0; i < itemCount; i++) {
                currentTrack.mainSize += items[i].outputSize;
            }

            tracks.Add(currentTrack);
        }


        private static float PositionCrossAxis(float axisOffset, FlexTrack track, FlexItemAxis[] items, CrossAxisAlignment crossAxisAlignment) {
            // todo -- respect individual align-cross-axis-self settings on children
            float maxOutputSize = 0;

            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
                if (items[i].outputSize > maxOutputSize) {
                    maxOutputSize = items[i].outputSize;
                }
            }

            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
                switch (crossAxisAlignment) {
                    case CrossAxisAlignment.Center:
                        items[i].axisStart = (maxOutputSize * 0.5f) - (items[i].outputSize * 0.5f);
                        break;

                    case CrossAxisAlignment.End:
                        items[i].axisStart = (maxOutputSize - items[i].outputSize);
                        break;

                    case CrossAxisAlignment.Start:
                        items[i].axisStart = 0;
                        break;

                    case CrossAxisAlignment.Stretch:
                        items[i].axisStart = 0;
                        items[i].outputSize = maxOutputSize;
                        break;
                    default:
                        items[i].axisStart = 0;
                        break;
                }

                items[i].axisStart += axisOffset;
            }

            return axisOffset + maxOutputSize;
        }

        private static void AlignMainAxis(FlexTrack track, FlexItemAxis[] items, float space, MainAxisAlignment mainAxisAlignment) {
            float offset = 0;
            float spacerSize = 0;
            int itemCount = track.itemCount;

            switch (mainAxisAlignment) {
                case MainAxisAlignment.Unset:
                case MainAxisAlignment.Start:
                case MainAxisAlignment.Default:
                    break;
                case MainAxisAlignment.Center:
                    offset = space * 0.5f;
                    break;
                case MainAxisAlignment.End:
                    offset = space;
                    break;
                case MainAxisAlignment.SpaceBetween: {
                    if (itemCount == 1) {
                        offset = space * 0.5f;
                        break;
                    }

                    spacerSize = space / (itemCount - 1);
                    offset = 0;
                    break;
                }
                case MainAxisAlignment.SpaceAround: {
                    if (itemCount == 1) {
                        offset = space * 0.5f;
                        break;
                    }

                    spacerSize = (space / itemCount);
                    offset = spacerSize * 0.5f;
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(mainAxisAlignment), mainAxisAlignment, null);
            }

            for (int i = 0; i < itemCount; i++) {
                items[i].axisStart = offset;
                offset += items[i].outputSize + spacerSize;
            }
        }

        private static void GrowAxis(FlexTrack track, FlexItemAxis[] items, float targetWidth) {
            int pieces = 0;

            int startIndex = track.startItem;
            int endIndex = startIndex + track.itemCount;

            for (int i = startIndex; i < endIndex; i++) {
                pieces += items[i].growthFactor;
            }

            bool allocate = pieces > 0;
            float remainingSpace = targetWidth - track.mainSize;
            while (allocate && (int) remainingSpace > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remainingSpace / pieces;

                for (int i = startIndex; i < endIndex; i++) {
                    float max = items[i].maxSize;
                    float output = items[i].outputSize;
                    int growthFactor = items[i].growthFactor;

                    if (growthFactor == 0) {
                        continue;
                    }

                    if ((int) output == (int) max) {
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

        private static void ShrinkAxis(FlexTrack track, FlexItemAxis[] items, float overflow) {
            int pieces = 0;
            int startIndex = track.startItem;
            int endIndex = startIndex + track.itemCount;

            for (int i = startIndex; i < endIndex; i++) {
                pieces += items[i].shrinkFactor;
            }

            // need to constrain things to max width here

            overflow *= -1;

            for (int i = startIndex; i < endIndex; i++) {
                if (items[i].maxSize < items[i].outputSize) {
                    float diff = items[i].outputSize - items[i].maxSize;
                    items[i].outputSize = items[i].maxSize;
                    overflow -= diff;
                }
            }

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
                    output = output < 0 ? 0 : output;
                    overflow += output - start;

                    items[i].outputSize = output;
                }
            }
        }

        private void ApplyOrdering(UIStyle style) {
            // sort items by style group
            // if no group defined use source order
            // if reverse, apply reverse here
            // todo -- separate this enum
            if (style.layoutParameters.wrap == LayoutWrap.Reverse) {
                ArrayUtil.ReverseInPlace(widths);
                ArrayUtil.ReverseInPlace(heights);
            }
        }

        private struct FlexItemAxis {

            public int childIndex;
            public float axisStart;
            public float outputSize;
            public float preferredSize;
            public float maxSize;
            public float minSize;
            public int growthFactor;
            public int shrinkFactor;

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