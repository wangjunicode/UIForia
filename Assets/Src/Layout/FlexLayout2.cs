//using System;
//using System.Collections.Generic;
//using Rendering;
//using Src.Util;
//using UnityEngine;
//
//namespace Src.Layout {
//
//    public class FlexLayout2 : Layout2 {
//
//        private struct FlexItemAxis {
//
//            public float axisStart;
//            public float outputSize;
//            public float preferredSize;
//            public float maxSize;
//            public float minSize;
//            public int growthFactor;
//            public int shrinkFactor;
//
//        }
//
//        public class FlexLine {
//
//            public int startItem;
//            public int itemCount;
//            public float mainSize;
//            public float crossSize;
//            public float remainingSpace;
//
//            public void Clear() {
//                startItem = 0;
//                itemCount = 0;
//                mainSize = 0;
//                crossSize = 0;
//                remainingSpace = 0;
//            }
//
//        }
//
//        private FlexItemAxis[] widths;
//        private FlexItemAxis[] heights;
//        
//        private readonly List<LayoutBox> activeBoxes;
//        private static readonly ObjectPool<FlexLine> s_FlexLinePool = new ObjectPool<FlexLine>(null, (line) => line.Clear());
//
//        public FlexLayout2() {
//            activeBoxes = new List<LayoutBox>(16);
//            widths = new FlexItemAxis[16];
//            heights = new FlexItemAxis[16];
//        }
//
//        /*
//         * layout to get preferred width
//         *     (assume width is content based)
//         * if height is content based & width is content based
//         * if there is no max height
//         * assume single column regardless of style settings
//         * ignore ordering
//         * layout to get preferred height
//         * 
//         */
//
//        public float GetContentPreferredWidth(LayoutBox container, UIUnit unit) {
//            // assume unconstrained
//            // don't grow
//            // don't shrink
//            // every item gets it's preferred width
//            // if columns take max width of children
//            // if wrap then we have to apply ordering maybe
//            // if row do single line layout and return extents
//            if (container.activeChildCount > widths.Length) {
//                Array.Resize(ref widths, container.activeChildCount * 2);
//                Array.Resize(ref heights, container.activeChildCount * 2);
//            }
//
//            return 0;
//        }
//
//        public override void Run(LayoutBox container, LayoutUpdateType layoutUpdateType) {
//            RunFullLayout(container);
//            return; 
//            // todo -- make partial updates work as expected
//            switch (layoutUpdateType) {
//                case LayoutUpdateType.None:
//                    return;
//                case LayoutUpdateType.Full:
//                    RunFullLayout(container);
//                    break;
//                case LayoutUpdateType.Alignment:
//                    // if wrapped we need to re-compute the track lines :(
//                    // or save them in (layoutBox.cachedLayoutData)
//                    // else find the largest item in direction and align others based on that
//                    break;
//                case LayoutUpdateType.Ordering:
//                    break;
//                case LayoutUpdateType.Positioning:
//                    break;
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(layoutUpdateType), layoutUpdateType, null);
//            }
//
//            // out of flow? ignore or take into sizing consideration? --> not handled here
//
//            // if required layout type == alignment ->
//            // if required layout type == full -> 
//            // if required layout type == position
//        }
//
//        private void RunFullLayout(LayoutBox container) {
//            LayoutBox ptr = container.firstChild;
//            if (ptr == null) return;
//
//            if (container.activeChildCount > widths.Length) {
//                Array.Resize(ref widths, container.activeChildCount * 2);
//                Array.Resize(ref heights, container.activeChildCount * 2);
//            }
//
//            int i = 0;
//
//            while (ptr != null) {
//                ptr.UpdateWidthValues();
//                widths[i] = new FlexItemAxis();
//                widths[i].minSize = ptr.minWidth;
//                widths[i].maxSize = ptr.maxWidth;
//                widths[i].preferredSize = ptr.preferredWidth;
//                widths[i].outputSize = Mathf.Max(ptr.minWidth, Mathf.Min(ptr.preferredWidth, ptr.maxWidth));
//                widths[i].growthFactor = ptr.element.style.growthFactor;
//                widths[i].shrinkFactor = ptr.element.style.shrinkFactor;
//
//                activeBoxes.Add(ptr);
//                ptr = ptr.nextSibling;
//                i++;
//            }
//
//            UIStyleSet style = container.element.style;
//
//            if (style.layoutDirection == LayoutDirection.Column) {
//                RunFullColumnLayout(container);
//            }
//
//            // if direction == row
//            // if wrap && width is not content based how to handle min / max that are fixed? try single line until we break max
//            //RowLayoutWrap();
//            // else
//            //RowLayoutNoWrap()
//
//            // if height unit is content
//            //     and min height is not fixed
//            if (style.height.unit != UIUnit.Content && style.layoutWrap == LayoutWrap.Wrap) {
//                if (style.layoutDirection == LayoutDirection.Column) {
//                    // multi wrap columns
//                    // for each width item
//                    // compute heights using that width
//                    // place in column
//                    // if overflowing column ->
//                    //     -> start new column
//                    //     -> apply alignment to placed widths
//                    // after all placed
//                    // apply alignment to columns themselves
//                }
//                else { }
//            }
//            else {
//                if (style.layoutDirection == LayoutDirection.Column) { }
//            }
//
//            // get preferred height using(actual width)
//            // 
//            activeBoxes.Clear();
//        }
//
//        public override float GetContentPreferredHeight(LayoutBox box, float width) {
//            if (box.style.layoutParameters.direction == LayoutDirection.Column) {
//                LayoutBox ptr = box.firstChild;
//                float max = 0;
//                while (ptr != null) {
//                    ptr.UpdateWidthValues();
//                    ptr.UpdateHeightValuesUsingWidth(ptr.preferredWidth);
//                    if (ptr.preferredHeight > max) {
//                        max = ptr.preferredHeight;
//                    }
//
//                    ptr = ptr.nextSibling;
//                }
//
//                return max;
//            }
//            else {
//                LayoutBox ptr = box.firstChild;
//                float max = 0;
//                while (ptr != null) {
//                    ptr.UpdateWidthValues();
//                    ptr.GetPreferredHeightForWidth(width);
//                    if (ptr.preferredHeight > max) {
//                        max = ptr.preferredHeight;
//                    }
//
//                    ptr = ptr.nextSibling;
//                }
//
//                return max;
//                // if no wrap
//                // return sum of preferred heights
//                // else
//                // figure out tracks
//                // return longest track size
//            }
//
//            return 0f;
//        }
//
//
//        public override float GetContentMinWidth(LayoutBox box) {
//            float total = 0;
//            LayoutBox ptr = box.firstChild;
//            while (ptr != null) {
//                total += ptr.preferredWidth;
//                ptr = ptr.nextSibling;
//            }
//
//            return total * box.style.layoutConstraints.minWidth.value;
//        }
//
//        public override float GetContentMaxWidth(LayoutBox box) {
//            float total = 0;
//            LayoutBox ptr = box.firstChild;
//            while (ptr != null) {
//                total += ptr.preferredWidth;
//                ptr = ptr.nextSibling;
//            }
//
//            return total * box.style.layoutConstraints.maxWidth.value;
//        }
//
//        private void RunFullColumnLayout(LayoutBox container) {
//            ApplyOrdering(container.style);
//
//            float targetWidth = container.allocatedWidth;
//
//            List<FlexLine> tracks = ListPool<FlexLine>.Get();
//
//            if (container.style.layoutParameters.wrap == LayoutWrap.Wrap) {
//                FlexLine currentTrack = new FlexLine();
//                for (int i = 0; i < activeBoxes.Count; i++) {
//                    float size = widths[i].preferredSize;
//
//                    if (currentTrack.mainSize + size < targetWidth) {
//                        currentTrack.mainSize += size;
//                    }
//                    else if (size >= targetWidth) {
//                        currentTrack.itemCount++;
//                        tracks.Add(currentTrack);
//
//                        currentTrack = new FlexLine();
//                        currentTrack.startItem = i;
//                        currentTrack.mainSize = size;
//                        currentTrack.itemCount = 1;
//                        tracks.Add(currentTrack);
//
//                        currentTrack = new FlexLine();
//                        currentTrack.startItem = i;
//                        currentTrack.itemCount = 1;
//                        currentTrack.mainSize = size;
//                    }
//                    else {
//                        currentTrack.itemCount++;
//                        tracks.Add(currentTrack);
//
//                        currentTrack = new FlexLine();
//                        currentTrack.startItem = i;
//                        currentTrack.itemCount = 1;
//                        currentTrack.mainSize = size;
//                    }
//                }
//
//                tracks.Add(currentTrack);
//            }
//
//            else {
//                FlexLine currentTrack = new FlexLine();
//                currentTrack.itemCount = activeBoxes.Count;
//                for (int i = 0; i < activeBoxes.Count; i++) {
//                    currentTrack.mainSize += activeBoxes[i].preferredWidth;
//                }
//
//                tracks.Add(currentTrack);
//            }
//
//            float trackCrossAxisStart = 0;
//            for (int i = 0; i < tracks.Count; i++) {
//                FlexLine track = tracks[i];
//                float remainingSpace = targetWidth - track.mainSize;
//
//                if (remainingSpace > 0) {
//                    GrowAxis(track, widths, targetWidth);
//                }
//                else if (remainingSpace < 0) {
//                    ShrinkAxis(track, widths, targetWidth);
//                }
//
//                AlignMainAxis(track, widths, remainingSpace, container.style.layoutParameters.mainAxisAlignment);
//                CreateHeightItems(track);
//                trackCrossAxisStart = PositionCrossAxis(trackCrossAxisStart, track, heights, container.style.layoutParameters.crossAxisAlignment);
//                s_FlexLinePool.Release(track);
//            }
//
//            for (int i = 0; i < activeBoxes.Count; i++) {
//                activeBoxes[i].SetRectFromParentLayout(widths[i].axisStart, heights[i].axisStart, widths[i].outputSize, heights[i].outputSize);
//            }
//
//            ListPool<FlexLine>.Release(tracks);
//        }
//        // when doing a column layout we try not to overflow width if possible -- css even shrinks overflowing wrapped elements to fit :(
//        // maybe introduce a 'compress' option to mimic this as an opt-in feature
//        // assume overflowing height is ok
//        private void CreateHeightItems(FlexLine track) {
//
//            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
//                heights[i] = new FlexItemAxis();
//                activeBoxes[i].UpdateHeightValuesUsingWidth(widths[i].outputSize);
//                heights[i].outputSize = heights[i].preferredSize;
//            }
//            // allocated size vs actual size
//        }
//
//        private static float PositionCrossAxis(float axisOffset, FlexLine track, FlexItemAxis[] items, CrossAxisAlignment crossAxisAlignment) {
//            // todo -- respect individual align-cross-axis-self settings on children
//            float maxOutputSize = 0;
//
//            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
//                if (items[i].outputSize > maxOutputSize) {
//                    maxOutputSize = items[i].outputSize;
//                }
//            }
//
//            for (int i = track.startItem; i < track.startItem + track.itemCount; i++) {
//                switch (crossAxisAlignment) {
//                    case CrossAxisAlignment.Center:
//                        items[i].axisStart = (maxOutputSize * 0.5f) - (items[i].outputSize * 0.5f);
//                        break;
//
//                    case CrossAxisAlignment.End:
//                        items[i].axisStart = (maxOutputSize - items[i].outputSize);
//                        break;
//
//                    case CrossAxisAlignment.Start:
//                        items[i].axisStart = 0;
//                        break;
//
//                    case CrossAxisAlignment.Stretch:
//                        items[i].axisStart = 0;
//                        items[i].outputSize = maxOutputSize;
//                        break;
//                    default:
//                        items[i].axisStart = 0;
//                        break;
//                }
//
//                items[i].axisStart += axisOffset;
//            }
//
//            return axisOffset + maxOutputSize;
//        }
//
//        private static void AlignMainAxis(FlexLine track, FlexItemAxis[] items, float space, MainAxisAlignment mainAxisAlignment) {
//            float offset = 0;
//            float spacerSize = 0;
//            int itemCount = track.itemCount;
//
//            switch (mainAxisAlignment) {
//                case MainAxisAlignment.Unset:
//                case MainAxisAlignment.Start:
//                case MainAxisAlignment.Default:
//                    break;
//                case MainAxisAlignment.Center:
//                    offset = space * 0.5f;
//                    break;
//                case MainAxisAlignment.End:
//                    offset = space;
//                    break;
//                case MainAxisAlignment.SpaceBetween: {
//                    if (itemCount == 1) {
//                        offset = space * 0.5f;
//                        break;
//                    }
//
//                    spacerSize = space / (itemCount - 1);
//                    offset = 0;
//                    break;
//                }
//                case MainAxisAlignment.SpaceAround: {
//                    if (itemCount == 1) {
//                        offset = space * 0.5f;
//                        break;
//                    }
//
//                    spacerSize = (space / itemCount);
//                    offset = spacerSize * 0.5f;
//                    break;
//                }
//                default:
//                    throw new ArgumentOutOfRangeException(nameof(mainAxisAlignment), mainAxisAlignment, null);
//            }
//
//            for (int i = 0; i < itemCount; i++) {
//                items[i].axisStart = offset;
//                offset += items[i].outputSize + spacerSize;
//            }
//        }
//
//        private static void GrowAxis(FlexLine track, FlexItemAxis[] items, float targetWidth) {
//            int pieces = 0;
//
//            int startIndex = track.startItem;
//            int endIndex = startIndex + track.itemCount;
//
//            for (int i = startIndex; i < endIndex; i++) {
//                pieces += items[i].growthFactor;
//            }
//
//            bool allocate = pieces > 0;
//            float remainingSpace = targetWidth - track.mainSize;
//            while (allocate && (int) remainingSpace > 0 && pieces > 0) {
//                allocate = false;
//
//                float pieceSize = remainingSpace / pieces;
//
//                for (int i = startIndex; i < endIndex; i++) {
//                    float max = items[i].maxSize;
//                    float output = items[i].outputSize;
//                    int growthFactor = items[i].growthFactor;
//
//                    if (growthFactor == 0) {
//                        continue;
//                    }
//
//                    if ((int) output == (int) max) {
//                        continue;
//                    }
//
//                    allocate = true;
//                    float start = output;
//                    float growSize = growthFactor * pieceSize;
//                    float totalGrowth = start + growSize;
//                    output = totalGrowth > max ? max : totalGrowth;
//
//                    remainingSpace -= output - start;
//
//                    items[i].outputSize = output;
//                }
//            }
//
//            track.remainingSpace = remainingSpace;
//        }
//
//        private static void ShrinkAxis(FlexLine track, FlexItemAxis[] items, float overflow) {
//            int pieces = 0;
//            int startIndex = track.startItem;
//            int endIndex = startIndex + track.itemCount;
//            
//            for (int i = startIndex; i < endIndex; i++) {
//                pieces += items[i].shrinkFactor;
//            }
//
//            // need to constrain things to max width here
//
//            overflow *= -1;
//
//            for (int i = startIndex; i < endIndex; i++) {
//                if (items[i].maxSize < items[i].outputSize) {
//                    float diff = items[i].outputSize - items[i].maxSize;
//                    items[i].outputSize = items[i].maxSize;
//                    overflow -= diff;
//                }
//            }
//
//            bool allocate = pieces > 0;
//            while (allocate && (int) overflow > 0) {
//                allocate = false;
//
//                float pieceSize = overflow / pieces;
//
//                for (int i = startIndex; i < endIndex; i++) {
//                    float min = items[i].minSize;
//                    float output = items[i].outputSize;
//                    int shrinkFactor = items[i].shrinkFactor;
//
//                    if (shrinkFactor == 0 || (int) output == (int) min || output == 0f) {
//                        continue;
//                    }
//
//                    allocate = true;
//                    float start = output;
//                    float shrinkSize = shrinkFactor * pieceSize;
//                    float totalShrink = output - shrinkSize;
//                    output = (totalShrink < min) ? min : totalShrink;
//                    output = output < 0 ? 0 : output;
//                    overflow += output - start;
//
//                    items[i].outputSize = output;
//                }
//            }
//        }
//
//        private void ApplyOrdering(UIStyle style) {
//            // sort items by style group
//            // if no group defined use source order
//            // if reverse, apply reverse here
//            // todo -- separate this enum
//            if (style.layoutParameters.wrap == LayoutWrap.Reverse) {
//                ArrayUtil.ReverseInPlace(widths);
//                ArrayUtil.ReverseInPlace(heights);
//            }
//
//        }
//
//    }
//
//}