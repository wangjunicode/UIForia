using System;
using System.Collections.Generic;
using Rendering;
using UnityEngine;
using UnityEngine.UI;

namespace Src.Layout {

    public class FlexLayout : UILayout {

        private Rect contentRect;
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

            float contentStartX = size.x + data.ContentStartOffsetX;
            float contentStartY = size.y + data.ContentStartOffsetY;
            float contentEndX = size.xMax - data.ContentEndOffsetX;
            float contentEndY = size.yMax - data.ContentEndOffsetY;
            float contentAreaWidth = contentEndX - contentStartX;
            float contentAreaHeight = contentEndY - contentStartY;

            contentRect = new Rect(contentStartX, contentStartY, contentAreaWidth, contentAreaHeight);

            List<LayoutData> children = data.children;

            float remainingWidth = contentRect.width;
            float remainingHeight = contentRect.height;

            itemCount = 0;

            for (int i = 0; i < children.Count; i++) {
                LayoutData child = children[i];
                if (!child.isInFlow) continue;

                FlexItemAxis widthItem = new FlexItemAxis();
                FlexItemAxis heightItem = new FlexItemAxis();

                widthItem.axisStart = 0;
                widthItem.preferredSize = child.GetPreferredWidth(data.preferredWidth.unit, contentRect.width, viewport.width);
                widthItem.minSize = child.minWidth.GetFixedPixelValue(contentRect.width, viewport.width);
                widthItem.maxSize = float.MaxValue;//child.maxWidth.GetFixedPixelValue(contentRect.width, viewport.width);

                widthItem.growthFactor = child.growthFactor;
                widthItem.shrinkFactor = child.shrinkFactor;

                heightItem.axisStart = 0;
                heightItem.preferredSize = child.GetPreferredHeight(data.preferredHeight.unit, contentRect.height, viewport.height);
                heightItem.minSize = child.minHeight.GetFixedPixelValue(contentRect.height, viewport.height);
                heightItem.maxSize = float.MaxValue;//child.maxHeight.GetFixedPixelValue(contentRect.height, viewport.height);

                heightItem.growthFactor = child.growthFactor;
                heightItem.shrinkFactor = child.shrinkFactor;

                widthItems[itemCount] = widthItem;
                heightItems[itemCount] = heightItem;

                remainingHeight -= heightItem.preferredSize;
                remainingWidth -= widthItem.preferredSize;

                itemCount++;
            }

            if (data.layoutDirection == LayoutDirection.Row) {
                if (remainingWidth > 0) {
                    GrowAxis(widthItems, itemCount, remainingWidth);
                }
                else if (remainingWidth < 0) {
                    ShrinkAxis(widthItems, itemCount, remainingWidth);
                }

                AlignMainAxis(widthItems, itemCount, data.mainAxisAlignment);
                AlignCrossAxis(heightItems, itemCount, data.crossAxisAlignment, contentRect.height);
            }
            else {
                if (remainingHeight > 0) {
                    GrowAxis(heightItems, itemCount, remainingHeight);
                }
                else if (remainingHeight < 0) {
                    ShrinkAxis(heightItems, itemCount, remainingHeight);
                }
                
                AlignMainAxis(heightItems, itemCount, data.mainAxisAlignment);
                AlignCrossAxis(widthItems, itemCount, data.crossAxisAlignment, contentRect.width);
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

        private static void GrowAxis(FlexItemAxis[] items, int itemCount, float remainingSpace) {
            int pieces = 0;
            int growBonus = 0;
            for (int i = 0; i < itemCount; i++) {
                pieces += items[i].growthFactor;
            }

            if (pieces == 0) {
                growBonus = 1;
                pieces = itemCount;
            }

            bool didAllocate = true;
            while (didAllocate && remainingSpace > 0) {
                didAllocate = false;

                float pieceSize = SafeDivide(remainingSpace, pieces);

                for (int i = 0; i < itemCount; i++) {

                    float max = items[i].maxSize;
                    float output = items[i].outputSize;
                    int growthFactor = items[i].growthFactor + growBonus;

                    if (growthFactor == 0 || output == max) {
                        continue;
                    }

                    didAllocate = true;
                    float start = items[i].preferredSize;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    output = totalGrowth > max ? max : totalGrowth;
                    remainingSpace -= output - start;

                    items[i].outputSize = output;
                }
            }
        }

        // needs preferred, min, shrinkFactor
        private static void ShrinkAxis(FlexItemAxis[] items, int itemCount, float overflow) {
            int pieces = 0;

            for (int i = 0; i < itemCount; i++) {
                pieces += items[i].shrinkFactor;
            }

            bool didAllocate = pieces > 0;

            overflow *= -1;

            while (didAllocate && overflow > 0) {
                didAllocate = false;
                float pieceSize = SafeDivide(overflow, pieces);
                for (int i = 0; i < itemCount; i++) {

                    float min = items[i].minSize;
                    float size = items[i].outputSize;
                    int shrinkFactor = items[i].shrinkFactor;

                    if (shrinkFactor == 0 || size == min) {
                        continue;
                    }

                    didAllocate = true;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = size - shrinkSize;
                    float output = totalShrink < min ? min : totalShrink;
                    overflow -= output - size;

                    items[i].outputSize = output;
                }
            }
        }

        // todo I don't think this needs to be specific per axis
        private static void AlignMainAxis(FlexItemAxis[] items, int itemCount, MainAxisAlignment mainAxisAlignment) {
            float gutterSize = 0;
            int segmentCount = 0;
            float offset = 0;

            switch (mainAxisAlignment) {
                case MainAxisAlignment.Start:
                case MainAxisAlignment.Default:
                    gutterSize = 0;
                    segmentCount = 0;
                    break;
                case MainAxisAlignment.Center:
                    gutterSize = 0;
                    segmentCount = 0;
                    offset = gutterSize * 0.5f;
                    break;
                case MainAxisAlignment.End:
                    gutterSize = 0f;
                    segmentCount = 0;
                    offset = 0;
                    break;
                case MainAxisAlignment.SpaceBetween: {
                    gutterSize = 0;
                    segmentCount = itemCount - 1;
                    offset = 0;
                    break;
                }
                case MainAxisAlignment.SpaceAround: {
                    gutterSize = 0;
                    segmentCount = itemCount;
                    offset = (gutterSize / segmentCount * 0.5f);
                    break;
                }
                default:
                    throw new ArgumentOutOfRangeException(nameof(mainAxisAlignment), mainAxisAlignment, null);
            }

            float spacerSize = SafeDivide(gutterSize, segmentCount);

            for (int i = 0; i < itemCount; i++) {
                items[i].axisStart = offset;
                offset += items[i].outputSize + spacerSize;
            }
        }

        private static void AlignCrossAxis(FlexItemAxis[] items, int itemCount, CrossAxisAlignment crossAxisAlignment, float contentSize) {
            // todo -- respect individual align-cross-axis-self settings on children

            for (int i = 0; i < itemCount; i++) {
                items[i].outputSize = items[i].preferredSize;
                switch (crossAxisAlignment) {
                    case CrossAxisAlignment.Center:
                        items[i].axisStart = (contentSize * 0.5f) - (items[i].outputSize * 0.5f);
                        break;

                    case CrossAxisAlignment.End:
                        items[i].outputSize = (contentSize - items[i].outputSize);
                        break;

                    case CrossAxisAlignment.Stretch:
                        items[i].axisStart = 0;
                        items[i].outputSize = contentSize;
                        break;
                    case CrossAxisAlignment.Default:
                    case CrossAxisAlignment.Start:
                        items[i].axisStart = 0;
                        break;
                    default:
                        items[i].axisStart = 0;
                        break;
                }
            }
        }

        private static float SafeDivide(float numerator, float denominator) {
            return denominator == 0 ? 0 : numerator / denominator;
        }

        private struct FlexItemAxis {

            public float axisStart;
            public float outputSize;
            public float preferredSize;
            public float maxSize;
            public float minSize;
            public int growthFactor;
            public int shrinkFactor;

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