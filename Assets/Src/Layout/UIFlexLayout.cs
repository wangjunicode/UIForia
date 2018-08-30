using System;
using System.Collections.Generic;
using System.Diagnostics;
using Rendering;
using UnityEngine;

namespace Src.Layout {

    public class FlexLayout : UILayout {

        private FlexItemAxis[] widthItems;
        private FlexItemAxis[] heightItems;

        public FlexLayout(ITextSizeCalculator textSizeCalculator) : base(textSizeCalculator) {
            widthItems = new FlexItemAxis[16];
            heightItems = new FlexItemAxis[16];
        }

        private void DoLayoutRow(Rect viewport, LayoutNode currentNode, Rect contentArea) {
            int itemCount = 0;
            List<LayoutNode> children = currentNode.children;

            float remainingWidth = contentArea.width;

            for (int i = 0; i < children.Count; i++) {
                LayoutNode child = children[i];
                if (!child.isInFlow || child.element.isDisabled) continue;

                FlexItemAxis widthItem = new FlexItemAxis();

                widthItem.axisStart = contentArea.x;
                widthItem.minSize = child.GetMinWidth(currentNode.rect.width.unit, contentArea.width, viewport.width);
                widthItem.maxSize = child.GetMaxWidth(currentNode.rect.width.unit, contentArea.width, viewport.width);
                widthItem.preferredSize = child.GetPreferredWidth(currentNode.rect.width.unit, contentArea.width, viewport.width);
                widthItem.outputSize = widthItem.MinDefined && widthItem.preferredSize < widthItem.minSize ? widthItem.minSize : widthItem.preferredSize;
                widthItem.outputSize = widthItem.MaxDefined && widthItem.outputSize > widthItem.maxSize ? widthItem.maxSize : widthItem.outputSize;

                widthItem.growthFactor = child.constraints.growthFactor;
                widthItem.shrinkFactor = child.constraints.shrinkFactor;

                widthItems[itemCount] = widthItem;

                remainingWidth -= widthItem.outputSize;

                itemCount++;
            }

            if (remainingWidth > 0) {
                remainingWidth = GrowAxis(widthItems, itemCount, remainingWidth);
            }
            else if (remainingWidth < 0) {
                ShrinkAxis(widthItems, itemCount, remainingWidth);
            }

            if (remainingWidth < 0) {
                remainingWidth = 0;
            }

            itemCount = 0; // need to reset so we safely skip non flow children
            for (int i = 0; i < children.Count; i++) {
                LayoutNode child = children[i];
                if (!child.isInFlow || child.element.isDisabled) continue;

                FlexItemAxis heightItem = new FlexItemAxis();

                heightItem.axisStart = contentArea.y;
                heightItem.minSize = child.GetMinHeight(currentNode.rect.height.unit, contentArea.height, viewport.height);
                heightItem.maxSize = child.GetMaxHeight(currentNode.rect.height.unit, contentArea.height, viewport.height);
                // now we have the final width and can compute preferred height accordingly
                // this restriction doesn't exist in the column layout case
                heightItem.preferredSize = child.GetPreferredHeight(currentNode.rect.height.unit, widthItems[itemCount].outputSize - child.horizontalOffset, contentArea.height, viewport.height);

                heightItem.outputSize = heightItem.MinDefined && heightItem.preferredSize < heightItem.minSize ? heightItem.minSize : heightItem.preferredSize;
                heightItem.outputSize = heightItem.MaxDefined && heightItem.outputSize > heightItem.maxSize ? heightItem.maxSize : heightItem.outputSize;

                heightItems[itemCount++] = heightItem;
            }

            AlignMainAxis(widthItems, itemCount, contentArea.x, remainingWidth, currentNode.parameters.mainAxisAlignment);
            AlignCrossAxis(heightItems, itemCount, contentArea.y, currentNode.parameters.crossAxisAlignment, contentArea.height);
        }

        private void DoLayoutColumn(Rect viewport, LayoutNode currentNode, Rect contentArea) {
            float remainingHeight = contentArea.height;
            List<LayoutNode> children = currentNode.children;

            int itemCount = 0;

            for (int i = 0; i < children.Count; i++) {
                LayoutNode child = children[i];
                if (!child.isInFlow || child.element.isDisabled) continue;

                FlexItemAxis heightItem = new FlexItemAxis();
                FlexItemAxis widthItem = new FlexItemAxis();

                widthItem.axisStart = contentArea.x;
                widthItem.minSize = child.GetMinWidth(currentNode.rect.width.unit, contentArea.width, viewport.width);
                widthItem.maxSize = child.GetMaxWidth(currentNode.rect.width.unit, contentArea.width, viewport.width);
                widthItem.preferredSize = child.GetPreferredWidth(currentNode.rect.width.unit, contentArea.width, viewport.width);
                widthItem.outputSize = widthItem.MinDefined && widthItem.preferredSize < widthItem.minSize ? widthItem.minSize : widthItem.preferredSize;
                widthItem.outputSize = widthItem.MaxDefined && widthItem.outputSize > widthItem.maxSize ? widthItem.maxSize : widthItem.outputSize;

                widthItem.growthFactor = child.constraints.growthFactor;
                widthItem.shrinkFactor = child.constraints.shrinkFactor;

                heightItem.axisStart = contentArea.y;
                heightItem.minSize = child.GetMinHeight(currentNode.rect.height.unit, contentArea.height, viewport.height);
                heightItem.maxSize = child.GetMaxHeight(currentNode.rect.height.unit, contentArea.height, viewport.height);

                // need to un-offset the output size because nested calls to GetPreferredHeight() will add the offset back.
                heightItem.preferredSize = child.GetPreferredHeight(currentNode.rect.height.unit, widthItem.outputSize - child.horizontalOffset, contentArea.height, viewport.height);

                heightItem.outputSize = heightItem.MinDefined && heightItem.preferredSize < heightItem.minSize ? heightItem.minSize : heightItem.preferredSize;
                heightItem.outputSize = heightItem.MaxDefined && heightItem.outputSize > heightItem.maxSize ? heightItem.maxSize : heightItem.outputSize;

                heightItem.growthFactor = child.constraints.growthFactor;
                heightItem.shrinkFactor = child.constraints.shrinkFactor;

                widthItems[itemCount] = widthItem;
                heightItems[itemCount] = heightItem;

                remainingHeight -= heightItem.outputSize;

                itemCount++;
            }

            if (remainingHeight > 0) {
                remainingHeight = GrowAxis(heightItems, itemCount, remainingHeight);
            }
            else if (remainingHeight < 0) {
                ShrinkAxis(heightItems, itemCount, remainingHeight);
            }

            if (remainingHeight < 0) {
                remainingHeight = 0;
            }

            AlignMainAxis(heightItems, itemCount, contentArea.y, remainingHeight, currentNode.parameters.mainAxisAlignment);
            AlignCrossAxis(widthItems, itemCount, contentArea.x, currentNode.parameters.crossAxisAlignment, contentArea.width);
        }

        public override void Run(Rect viewport, LayoutNode currentNode, Rect[] results) {
            Rect size = currentNode.outputRect;

            float contentStartX = currentNode.contentStartOffsetX;
            float contentStartY = currentNode.contentStartOffsetY;
            float contentEndX = size.xMax - size.x - currentNode.contentEndOffsetX;
            float contentEndY = size.yMax - size.y - currentNode.contentEndOffsetY;
            float contentAreaWidth = contentEndX - contentStartX;
            float contentAreaHeight = contentEndY - contentStartY;

            Rect contentArea = new Rect(contentStartX, contentStartY, contentAreaWidth, contentAreaHeight);

            if (widthItems.Length < currentNode.children.Count) {
                Array.Resize(ref widthItems, currentNode.children.Count * 2);
                Array.Resize(ref heightItems, currentNode.children.Count * 2);
            }

            if (currentNode.parameters.direction == LayoutDirection.Row) {
                DoLayoutRow(viewport, currentNode, contentArea);
            }
            else {
                DoLayoutColumn(viewport, currentNode, contentArea);
            }

            int itemTracker = 0;
            for (int i = 0; i < currentNode.children.Count; i++) {
                if (currentNode.children[i].isInFlow && currentNode.children[i].element.isEnabled) {
                    // todo -- this is weeeeird
                    currentNode.children[i].localPosition = new Vector2(
                        widthItems[itemTracker].axisStart - (currentNode.element.style.marginLeft),
                        heightItems[itemTracker].axisStart - (currentNode.element.style.marginTop)
                    );
                    currentNode.children[i].outputRect = new Rect(
                        widthItems[itemTracker].axisStart + size.x,
                        heightItems[itemTracker].axisStart + size.y,
                        widthItems[itemTracker].outputSize,
                        heightItems[itemTracker].outputSize
                    );
                    itemTracker++;
                }
                else {
                    //results[i] = new Rect(); // todo -- sizing for non flow children
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

            // need to constrain things to max width here

            overflow *= -1;

            for (int i = 0; i < itemCount; i++) {
                if (items[i].MaxDefined && items[i].maxSize < items[i].outputSize) {
                    float diff = items[i].outputSize - items[i].maxSize;
                    items[i].outputSize = items[i].maxSize;
                    overflow -= diff;
                }
            }

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

        private static void AlignCrossAxis(FlexItemAxis[] items, int itemCount, float axisStart, CrossAxisAlignment crossAxisAlignment, float contentSize) {
            // todo -- respect individual align-cross-axis-self settings on children

            for (int i = 0; i < itemCount; i++) {
                switch (crossAxisAlignment) {
                    case CrossAxisAlignment.Center:
                        items[i].axisStart = (contentSize * 0.5f) - (items[i].outputSize * 0.5f);
                        break;

                    case CrossAxisAlignment.End:
                        items[i].axisStart = (contentSize - items[i].outputSize);
                        break;

                    case CrossAxisAlignment.Start:
                        items[i].axisStart = 0;
                        break;

                    case CrossAxisAlignment.Stretch:
                        items[i].axisStart = 0;
                        items[i].outputSize = contentSize;
                        break;
                    default:
                        items[i].axisStart = 0;
                        break;
                }

                items[i].axisStart += axisStart;
            }
        }

        [DebuggerDisplay("{" + nameof(outputSize) + "}")]
        private struct FlexItemAxis {

            public float axisStart;
            public float outputSize;
            public float preferredSize;
            public float maxSize;
            public float minSize;
            public int growthFactor;
            public int shrinkFactor;

            public bool MaxDefined => FloatUtil.IsDefined(maxSize);
            public bool MinDefined => FloatUtil.IsDefined(minSize);

        }

    }

}