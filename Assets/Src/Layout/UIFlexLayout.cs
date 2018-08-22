using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Layout {

    public class FlexLayout : UILayout {

        private int itemCount;
        private FlexItemAxis[] widthItems;
        private FlexItemAxis[] heightItems;

        public FlexLayout() {
            widthItems = new FlexItemAxis[16];
            heightItems = new FlexItemAxis[16];
        }

        public override void Run(Rect viewport, LayoutDataSet dataSet, Rect[] results) {
            Rect size = dataSet.result;
            LayoutData data = dataSet.data;

            if (widthItems.Length < data.children.Count) {
                Array.Resize(ref widthItems, data.children.Count * 2);
                Array.Resize(ref heightItems, data.children.Count * 2);
            }

            float contentStartX = data.ContentStartOffsetX;
            float contentStartY = data.ContentStartOffsetY;
            float contentEndX = size.xMax - size.x - data.ContentEndOffsetX;
            float contentEndY = size.yMax - size.y - data.ContentEndOffsetY;
            float contentAreaWidth = contentEndX - contentStartX;
            float contentAreaHeight = contentEndY - contentStartY;

            List<LayoutData> children = data.children;

            float remainingWidth = contentAreaWidth;
            float remainingHeight = contentAreaHeight;

            itemCount = 0;

            for (int i = 0; i < children.Count; i++) {
                LayoutData child = children[i];
                if (!child.isInFlow) continue;

                FlexItemAxis widthItem = new FlexItemAxis();
                FlexItemAxis heightItem = new FlexItemAxis();

                widthItem.axisStart = contentStartX;
                widthItem.preferredSize =
                    child.GetPreferredWidth(data.rect.width.unit, contentAreaWidth, viewport.width);
                widthItem.minSize = child.GetMinWidth(data.rect.width.unit, contentAreaWidth, viewport.width);
                widthItem.maxSize = child.GetMaxWidth(data.rect.width.unit, contentAreaWidth, viewport.width);
                widthItem.outputSize = widthItem.MinDefined && widthItem.preferredSize < widthItem.minSize ? widthItem.minSize : widthItem.preferredSize;

                widthItem.growthFactor = child.constraints.growthFactor;
                widthItem.shrinkFactor = child.constraints.shrinkFactor;

                heightItem.axisStart = contentStartY;
                heightItem.preferredSize =
                    child.GetPreferredHeight(data.rect.height.unit, contentAreaHeight, viewport.height);
                heightItem.minSize = child.GetMinHeight(data.rect.height.unit, contentAreaHeight, viewport.height);
                heightItem.maxSize = child.GetMaxHeight(data.rect.height.unit, contentAreaHeight, viewport.height);
                heightItem.outputSize = heightItem.MinDefined && heightItem.preferredSize < heightItem.minSize ? heightItem.minSize : heightItem.preferredSize;

                heightItem.growthFactor = child.constraints.growthFactor;
                heightItem.shrinkFactor = child.constraints.shrinkFactor;

                widthItems[itemCount] = widthItem;
                heightItems[itemCount] = heightItem;

                remainingHeight -= heightItem.outputSize;
                remainingWidth -= widthItem.outputSize;

                itemCount++;
            }

            if (data.parameters.direction == LayoutDirection.Row) {

                if (remainingWidth > 0) {
                    remainingWidth = GrowAxis(widthItems, itemCount, remainingWidth);
                }
                else if (remainingWidth < 0) {
                    ShrinkAxis(widthItems, itemCount, remainingWidth);
                }

                AlignMainAxis(widthItems, itemCount, contentStartX, remainingWidth, data.parameters.mainAxisAlignment);
                AlignCrossAxis(heightItems, itemCount, contentStartY, data.parameters.crossAxisAlignment, contentAreaHeight);
            }
            else {

                if (remainingHeight > 0) {
                    remainingHeight = GrowAxis(heightItems, itemCount, remainingHeight);
                }
                else if (remainingHeight < 0) {
                    ShrinkAxis(heightItems, itemCount, remainingHeight);
                }

                AlignMainAxis(heightItems, itemCount, contentStartY, remainingHeight, data.parameters.mainAxisAlignment);
                AlignCrossAxis(widthItems, itemCount, contentStartX, data.parameters.crossAxisAlignment, contentAreaWidth);
            }

            int itemTracker = 0;
            for (int i = 0; i < data.children.Count; i++) {
                if (data.children[i].isInFlow) {
                    results[i] = new Rect(
                        widthItems[itemTracker].axisStart,
                        heightItems[itemTracker].axisStart,
                        widthItems[itemTracker].outputSize,
                        heightItems[itemTracker].outputSize
                    );
                    itemTracker++;
                }
                else {
                    results[i] = new Rect(); // todo -- sizing for non flow children
                }
            }
        }

        private static float GrowAxis(FlexItemAxis[] items, int itemCount, float remainingSpace) {
            int pieces = 0;

            for (int i = 0; i < itemCount; i++) {
                pieces += items[i].growthFactor;
            }

            bool allocate = pieces > 0;

            while (allocate && (int) remainingSpace > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remainingSpace / pieces;

                for (int i = 0; i < itemCount; i++) {
                    float max = items[i].maxSize;
                    float output = items[i].outputSize;
                    bool maxDefined = items[i].MaxDefined;
                    int growthFactor = items[i].growthFactor;

                    if (growthFactor == 0) {
                        continue;
                    }

                    if ((maxDefined && (int) output == (int) max)) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    output = (maxDefined && totalGrowth > max) ? max : totalGrowth;

                    remainingSpace -= output - start;

                    items[i].outputSize = output;
                }
            }
            return remainingSpace;
        }

        private static void ShrinkAxis(FlexItemAxis[] items, int itemCount, float overflow) {
            int pieces = 0;

            for (int i = 0; i < itemCount; i++) {
                pieces += items[i].shrinkFactor;
            }

            overflow *= -1;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;

                for (int i = 0; i < itemCount; i++) {
                    float min = items[i].minSize;
                    float output = items[i].outputSize;
                    bool minDefined = items[i].MinDefined;
                    int shrinkFactor = items[i].shrinkFactor;

                    if (shrinkFactor == 0) {
                        continue;
                    }

                    if ((minDefined && (int) output == (int) min) || output == 0f) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    output = (minDefined && totalShrink < min) ? min : totalShrink;
                    output = output < 0 ? 0 : output;
                    overflow += output - start;

                    items[i].outputSize = output;
                }
            }
        }

        private static void AlignMainAxis(FlexItemAxis[] items, int itemCount, float axisStart, float space, MainAxisAlignment mainAxisAlignment) {
            float offset = 0;
            float spacerSize = 0;

            if (itemCount == 0) return;

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
                items[i].axisStart = axisStart + offset;
                offset += items[i].outputSize + spacerSize;
            }
        }

        private static void AlignCrossAxis(FlexItemAxis[] items, int itemCount, float axisStart, CrossAxisAlignment crossAxisAlignment,
            float contentSize) {
            // todo -- respect individual align-cross-axis-self settings on children

            for (int i = 0; i < itemCount; i++) {
                switch (crossAxisAlignment) {
                    case CrossAxisAlignment.Center:
                        items[i].axisStart = (contentSize * 0.5f) - (items[i].outputSize * 0.5f);
                        break;

                    case CrossAxisAlignment.End:
                        items[i].outputSize = (contentSize - items[i].outputSize);
                        break;

                    case CrossAxisAlignment.Start:
                        items[i].axisStart = 0;
                        break;
                    
                    case CrossAxisAlignment.Unset:
                    case CrossAxisAlignment.Default:
                    case CrossAxisAlignment.Stretch:
                    default:
                        items[i].outputSize = contentSize;
                        items[i].axisStart = 0;
                        break;
                }
                items[i].axisStart += axisStart;
            }
        }

        [DebuggerDisplay("{outputSize}")]
        private struct FlexItemAxis {

            public float axisStart;
            public float outputSize;
            public float preferredSize;
            public float maxSize;
            public float minSize;
            public int growthFactor;
            public int shrinkFactor;

            public bool MaxDefined => maxSize <= UIStyle.UnsetFloatThreshold;
            public bool MinDefined => minSize <= UIStyle.UnsetFloatThreshold;

        }

    }

//        private Rect rect;
//        
//        public List<UITransform> children;
//        public MainAxisAlignment mainAxisAlignment;
//        public CrossAxisAlignment crossAxisAlignment;
//               
//        public void DoLayout(UITransform transform, Rect rect) {

//            
//            this.rect = rect;
//            children = transform.children;
//            float remainingSpace = rect.width;
//            
//            for (int i = 0; i < children.Count; i++) {
//                UITransform flexItem = children[i];
//                if (!flexItem.isInFlow) {
//                    continue;
//                }
//                LayoutParameters layoutParams = flexItem.element.style.layoutParameters;
//                
//                flexItem.width = layoutParams.basisWidth;
//                remainingSpace -= flexItem.preferred;
//            }
//
//            if (remainingSpace > 0) {
//                GrowWidth(remainingSpace);
//            }
//            else if (remainingSpace < 0) {
//                ShrinkWidth(remainingSpace);
//            }
//            else {
//                AllocateFreeSpaceHorizontal(0);
//            }       

}
//
//        private  float SafeDivide(float numerator, float denominator) {
//            return denominator == 0 ? 0 : numerator / denominator;
//        }
//
//        private void GrowWidth(float remainingSpace) {
//            int pieces = 0;
//            
//            for (int i = 0; i < children.Count; i++) {
//                pieces += children[i].growthFactor;
//            }
//            
//            bool didAllocate = pieces > 0;
//            while (didAllocate && remainingSpace > 0) {
//                didAllocate = false;
//                float pieceSize = SafeDivide(remainingSpace, pieces);
//                for (int i = 0; i < children.Count; i++) {
//                    FlexItem child = children[i];
//                    float maxWidth = child.max;
//                    int growthFactor = child.growthFactor;
//                    float outputWidth = child.width;
//                    if (growthFactor == 0 || outputWidth == maxWidth) {
//                        continue;
//                    }
//                    didAllocate = true;
//                    float startWidth = outputWidth;
//                    float growSize = growthFactor * pieceSize;
//                    float totalGrowWidth = startWidth + growSize;
//                    outputWidth = totalGrowWidth > maxWidth ? maxWidth : totalGrowWidth;
//                    remainingSpace -= outputWidth - startWidth;
//                    child.width = outputWidth;
//                }
//            }
//        }
//
//        private void ShrinkWidth(float remainingSpace) {
//            int pieces = 0;
//            for (int i = 0; i < children.Count; i++) {
//                pieces += children[i].shrinkFactor;
//            }
//
//            bool didAllocate = pieces > 0;
//
//            remainingSpace *= -1;
//
//            while (didAllocate && remainingSpace > 0) {
//                didAllocate = false;
//                float pieceSize = SafeDivide(remainingSpace, pieces);
//                for (int i = 0; i < children.Count; i++) {
//                    FlexItem item = children[i];
//                    float minWidth = item.min;
//                    int shrinkFactor = item.shrinkFactor;
//
//                    if (shrinkFactor == 0 || item.width == minWidth) {
//                        continue;
//                    }
//
//                    didAllocate = true;
//                    float startWidth = item.width;
//                    float shrinkSize = shrinkFactor * pieceSize;
//                    float totalShrinkWidth = startWidth - shrinkSize;
//                    item.width = totalShrinkWidth < minWidth ? minWidth : totalShrinkWidth;
//                    remainingSpace -= item.width - startWidth;
//                }
//            }
//            AllocateFreeSpaceHorizontal(0);
//        }
//
//        private void AllocateFreeSpaceHorizontal(float gutterSize) {
//            switch (mainAxisAlignment) {
//                case MainAxisAlignment.Start:
//                case MainAxisAlignment.Default:
//                    AlignMainAxis(gutterSize, 0, 0);
//                    break;
//                case MainAxisAlignment.Center:
//                    AlignMainAxis(gutterSize, 0, gutterSize * 0.5f);
//                    break;
//                case MainAxisAlignment.End:
//                    AlignMainAxis(gutterSize, 0, gutterSize);
//                    break;
//                case MainAxisAlignment.SpaceBetween: {
//                    int segments = children.Count - 1;
//                    AlignMainAxis(gutterSize, segments, 0);
//                    break;
//                }
//                case MainAxisAlignment.SpaceAround: {
//                    int segments = children.Count;
//                    AlignMainAxis(gutterSize, segments, (gutterSize / segments) * 0.5f);
//                    break;
//                }
//            }
//        }
//
//        private void AlignMainAxis(float space, int segmentCount, float offset) {
//            float x = offset;
//            float spacerSize = SafeDivide(space, segmentCount);
//            float height = rect.height;
//            for (int i = 0; i < children.Count; i++) {
//                FlexItem item = children[i];
//                item.y = AlignCrossAxis(item, height);
//                item.x = x;
//                x += item.width + spacerSize;
//            }
//        }
//
//        private float AlignCrossAxis(FlexItem item, float height) {
//            CrossAxisAlignment alignment = item.alignment;
//            if (alignment == CrossAxisAlignment.Default) {
//                alignment = crossAxisAlignment;
//            }
//            item.height = item.preferred; //todo wrong!
//            switch (alignment) {
//                case CrossAxisAlignment.Center:
//                    return (rect.height * 0.5f) - (item.height * 0.5f);
//                case CrossAxisAlignment.End:
//                    return (height - item.height);
//                case CrossAxisAlignment.Stretch:
//                    item.height = height;
//                    return 0;
//                default:
//                    return 0;
//            }
//        }   
//    }