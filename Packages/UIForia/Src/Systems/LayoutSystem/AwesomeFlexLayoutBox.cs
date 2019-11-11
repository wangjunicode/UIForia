using System;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class AwesomeFlexLayoutBox : AwesomeLayoutBox {

        private StructList<FlexItem> items;
        private LayoutDirection direction;
        private MainAxisAlignment mainAxisAlignment;
        private CrossAxisAlignment crossAxisAlignment;

        protected override void OnInitialize() {
            direction = element.style.FlexLayoutDirection;
            mainAxisAlignment = element.style.FlexLayoutMainAxisAlignment;
            crossAxisAlignment = element.style.FlexLayoutCrossAxisAlignment;
        }

        protected override float ComputeContentWidth() {
            if (childCount == 0) return 0;
            return direction == LayoutDirection.Horizontal
                ? ComputeContentWidthHorizontal()
                : ComputeContentWidthVertical();
        }

        protected override float ComputeContentHeight() {
            if (childCount == 0) return 0;
            return direction == LayoutDirection.Horizontal
                ? ComputeContentHeightHorizontal()
                : ComputeContentHeightVertical();
        }

        private float ComputeContentWidthHorizontal() {
            float totalSize = 0;

            for (int i = 0; i < items.size; i++) {
                LayoutSize widths = default;
                items.array[i].layoutBox.GetWidths(ref widths);
                float baseSize = Mathf.Max(widths.minimum, Mathf.Min(widths.preferred, widths.maximum));
                totalSize += baseSize + widths.marginStart + widths.marginEnd;
            }

            return totalSize;
        }

        private float ComputeContentWidthVertical() {
            float maxSize = 0;

            for (int i = 0; i < items.size; i++) {
                LayoutSize widths = default;
                items.array[i].layoutBox.GetWidths(ref widths);
                float baseSize = Mathf.Max(widths.minimum, Mathf.Min(widths.preferred, widths.maximum));
                float totalSize = baseSize + widths.marginStart + widths.marginEnd;
                if (totalSize > maxSize) {
                    maxSize = totalSize;
                }
            }

            return maxSize;
        }

        private float ComputeContentHeightHorizontal() {
            float maxSize = 0;

            for (int i = 0; i < items.size; i++) {
                LayoutSize heights = default;
                items.array[i].layoutBox.GetHeights(ref heights);
                float baseSize = Mathf.Max(heights.minimum, Mathf.Min(heights.preferred, heights.maximum));
                float totalSize = baseSize + heights.marginStart + heights.marginEnd;
                if (totalSize > maxSize) {
                    maxSize = totalSize;
                }
            }

            return maxSize;
        }

        private float ComputeContentHeightVertical() {
            float totalSize = 0;

            for (int i = 0; i < items.size; i++) {
                LayoutSize heights = default;
                items.array[i].layoutBox.GetHeights(ref heights);
                float baseSize = Mathf.Max(heights.minimum, Mathf.Min(heights.preferred, heights.maximum));
                totalSize += baseSize + heights.marginStart + heights.marginEnd;
            }

            return totalSize;
        }

        public override void OnStyleChanged(StructList<StyleProperty> propertyList) {
            for (int i = 0; i < propertyList.size; i++) {
                ref StyleProperty property = ref propertyList.array[i];
                switch (property.propertyId) {
                    case StylePropertyId.FlexLayoutWrap:
                    case StylePropertyId.FlexLayoutDirection:
                    case StylePropertyId.FlexLayoutMainAxisAlignment:
                    case StylePropertyId.FlexLayoutCrossAxisAlignment:
                        MarkForLayout(LayoutDirtyFlag.All); // new LayoutReason("FlexStyleChanged");
                        return;
                }
            }
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) {
            items?.Clear();

            if (childList.size == 0) {
                return;
            }

            items = items ?? new StructList<FlexItem>(childCount);
            items.EnsureCapacity(childCount);
            items.size = childCount;
            for (int i = 0; i < childList.size; i++) {
                items.array[i] = new FlexItem() {
                    layoutBox = childList.array[i],
                    growPieces = childList.array[i].element.style.FlexItemGrow,
                    shrinkPieces = childList.array[i].element.style.FlexItemShrink
                };
            }
        }

        public override void RunLayoutHorizontal(int frameId) {
            if (childCount == 0) return;
            if (direction == LayoutDirection.Horizontal) {
                RunLayoutHorizontalStep_HorizontalDirection(frameId);
            }
            else {
                RunLayoutHorizontalStep_VerticalDirection(frameId);
            }
        }

        private void RunLayoutHorizontalStep_HorizontalDirection(int frameId) {
            Track track = new Track();
            track.remaining = finalWidth - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);
            track.size = track.remaining;
            track.startIndex = 0;
            track.endIndex = childCount;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.layoutBox.GetWidths(ref item.widthData);
                item.baseWidth = Mathf.Max(item.widthData.minimum, Mathf.Min(item.widthData.preferred, item.widthData.maximum));
                item.availableSize = item.baseWidth;
                track.remaining -= item.baseWidth + item.widthData.marginStart + item.widthData.marginEnd;
            }

            if (track.remaining > 0) {
                GrowHorizontal(ref track);
            }
            else if (track.remaining < 0) {
                ShrinkHorizontal(ref track);
            }

            float offset = 0;
            float inset = paddingBorderHorizontalStart;
            float spacerSize = 0;

            if (track.remaining > 0) {
                GetMainAxisOffsets(track.remaining, inset, out offset, out spacerSize);
            }

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                float x = inset + item.widthData.marginStart + offset;
                offset += item.availableSize + spacerSize;
                LayoutFit fit = LayoutFit.None;

                if (item.growPieces > 0 || item.shrinkPieces > 0) {
                    fit = LayoutFit.Fill;
                }

                item.layoutBox.ApplyLayoutHorizontal(x, x, item.baseWidth, item.availableSize, fit, frameId);
            }
        }

        private void GetMainAxisOffsets(float remaining, float inset, out float offset, out float spacerSize) {
            offset = 0;
            spacerSize = 0;
            switch (mainAxisAlignment) {
                case MainAxisAlignment.Unset:
                case MainAxisAlignment.Start:
                    break;
                case MainAxisAlignment.Center:
                    offset = inset * 0.5f + remaining * 0.5f;
                    break;
                case MainAxisAlignment.End:
                    offset = remaining;
                    break;
                case MainAxisAlignment.SpaceBetween: {
                    if (childCount == 1) {
                        offset = remaining * 0.5f;
                        break;
                    }

                    spacerSize = remaining / (childCount - 1);
                    offset = 0;
                    break;
                }

                case MainAxisAlignment.SpaceAround: {
                    if (childCount == 1) {
                        offset = remaining * 0.5f;
                        break;
                    }

                    spacerSize = (remaining / childCount);
                    offset = spacerSize * 0.5f;
                    break;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(mainAxisAlignment), mainAxisAlignment, null);
            }
        }

        private void GrowHorizontal(ref Track track) {
            int pieces = 0;

            for (int i = track.startIndex; i < track.endIndex; i++) {
                pieces += items.array[i].growPieces;
            }

            bool allocate = pieces > 0;
            float toAllocate = track.remaining;
            while (allocate && (int) track.remaining > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = track.remaining / pieces;

                for (int i = track.startIndex; i < track.endIndex; i++) {
                    ref FlexItem item = ref items.array[i];
                    float max = item.widthData.maximum;
                    float output = item.availableSize;
                    int growthFactor = item.growPieces;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    output = totalGrowth > max ? max : totalGrowth;

                    track.remaining -= output - start;
                    item.availableSize = output;
                }
            }

            track.size += toAllocate - track.remaining;
        }

        private float GrowVertical(float remaining) {
            int pieces = 0;

            // todo -- do on item creation
            for (int i = 0; i < items.size; i++) {
                pieces += items.array[i].growPieces;
            }

            bool allocate = pieces > 0;
            while (allocate && (int) remaining > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remaining / pieces;

                for (int i = 0; i < items.size; i++) {
                    ref FlexItem item = ref items.array[i];
                    float max = item.heightData.maximum;
                    float output = item.availableSize;
                    int growthFactor = item.growPieces;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    output = totalGrowth > max ? max : totalGrowth;

                    remaining -= output - start;
                    item.availableSize = output;
                }
            }

            return remaining;
        }

        private void ShrinkHorizontal(ref Track track) {
            
            int startIndex = track.startIndex;
            int endIndex = track.endIndex;
            int pieces = 0;
            
            // todo -- do on item creation
            for (int i = startIndex; i < endIndex; i++) {
                pieces += items.array[i].shrinkPieces;
            }
            
            float overflow = -track.remaining;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;

                for (int i = startIndex; i < endIndex; i++) {
                    ref FlexItem item = ref items.array[i];
                    float min = item.widthData.minimum;
                    float output = item.availableSize;
                    int shrinkFactor = item.shrinkPieces;

                    if (shrinkFactor == 0 || (int) output == (int) min || (int) output == 0) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    output = (totalShrink < min) ? min : totalShrink;
                    overflow += output - start;

                    item.availableSize = output;
                }
            }

            track.remaining = overflow;
        }

        private float ShrinkVertical(float remaining) {
            int pieces = 0;
            
            // todo -- do on item creation
            for (int i = 0; i < items.size; i++) {
                pieces += items.array[i].shrinkPieces;
            }
            
            float overflow = -remaining;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;

                for (int i = 0; i < items.size; i++) {
                    ref FlexItem item = ref items.array[i];
                    float min = item.heightData.minimum;
                    float output = item.availableSize;
                    int shrinkFactor = item.shrinkPieces;

                    if (shrinkFactor == 0 || (int) output == (int) min || (int) output == 0) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    output = (totalShrink < min) ? min : totalShrink;
                    overflow += output - start;

                    item.availableSize = output;
                }
            }

            return overflow;
        }

        public override void RunLayoutVertical(int frameId) {
            if (childCount == 0) return;
            if (direction == LayoutDirection.Horizontal) {
                RunLayoutVerticalStep_HorizontalDirection(frameId);
            }
            else {
                RunLayoutVerticalStep_VerticalDirection(frameId);
            }
        }

        private void RunLayoutVerticalStep_HorizontalDirection(int frameId) {
            float contentStartY = paddingBorderVerticalStart;
            float adjustedHeight = finalHeight - (paddingBorderVerticalStart + paddingBorderVerticalEnd);
            LayoutFit verticalLayoutFit = LayoutFit.None;
            float verticalAlignment = 0;

            switch (crossAxisAlignment) {
                case CrossAxisAlignment.Unset:
                case CrossAxisAlignment.Start:
                    verticalAlignment = 0;
                    break;
                case CrossAxisAlignment.Center:
                    verticalAlignment = 0.5f;
                    break;
                case CrossAxisAlignment.End:
                    verticalAlignment = 1f;
                    break;
                case CrossAxisAlignment.Stretch:
                    verticalLayoutFit = LayoutFit.Fill;
                    break;
            }

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];

                item.layoutBox.GetHeights(ref item.heightData);
                item.baseHeight = Mathf.Max(item.heightData.minimum, Mathf.Min(item.heightData.preferred, item.heightData.maximum));

                float allocatedHeight = adjustedHeight - (item.heightData.marginStart + item.heightData.marginEnd);
                float originBase = contentStartY + item.heightData.marginStart;
                float originOffset = allocatedHeight * verticalAlignment;
                float offset = item.baseHeight * -verticalAlignment;

                item.layoutBox.ApplyLayoutVertical(
                    contentStartY + item.heightData.marginStart,
                    originBase + originOffset + offset,
                    item.baseHeight,
                    allocatedHeight,
                    verticalLayoutFit,
                    frameId
                );
            }
        }

        private void RunLayoutVerticalStep_VerticalDirection(int frameId) {
            float adjustedHeight = finalHeight - (paddingBorderVerticalStart + paddingBorderVerticalEnd);

            float remaining = adjustedHeight;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.layoutBox.GetHeights(ref item.heightData);
                item.baseHeight = Mathf.Max(item.heightData.minimum, Mathf.Min(item.heightData.preferred, item.heightData.maximum));
                item.availableSize = item.baseHeight;
                remaining -= item.baseHeight;
            }

            if (remaining > 0) {
                remaining = GrowVertical(remaining);
            }
            else if (remaining < 0) {
                remaining = ShrinkVertical(remaining);
            }

            float offset = 0;
            float inset = paddingBorderVerticalStart;
            float spacerSize = 0;

            if (remaining > 0) {
                GetMainAxisOffsets(remaining, inset, out offset, out spacerSize);
            }

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                float y = inset + item.heightData.marginStart + offset;
                offset += item.availableSize + spacerSize;
                LayoutFit fit = LayoutFit.None;

                if (item.growPieces > 0 || item.shrinkPieces > 0) {
                    fit = LayoutFit.Fill;
                }

                item.layoutBox.ApplyLayoutVertical(y, y, item.baseHeight, item.availableSize, fit, frameId);
            }
        }

        private void RunLayoutHorizontalStep_VerticalDirection(int frameId) {
            float contentStartX = paddingBorderHorizontalStart;
            float horizontalAlignment = 0;
            LayoutFit fit = LayoutFit.None;

            float adjustedWidth = finalWidth - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);

            switch (crossAxisAlignment) {
                case CrossAxisAlignment.Unset:
                case CrossAxisAlignment.Start:
                    break;
                case CrossAxisAlignment.Center:
                    horizontalAlignment = 0.5f;
                    break;
                case CrossAxisAlignment.End:
                    horizontalAlignment = 1f;
                    break;
                case CrossAxisAlignment.Stretch:
                    horizontalAlignment = 0;
                    fit = LayoutFit.Fill;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.layoutBox.GetWidths(ref item.widthData);
                item.baseWidth = Mathf.Max(item.widthData.minimum, Mathf.Min(item.widthData.preferred, item.widthData.maximum));

                float availableWidth = adjustedWidth - (item.widthData.marginStart + item.widthData.marginEnd);
                float originBase = contentStartX + item.widthData.marginStart;
                float originOffset = availableWidth * horizontalAlignment;
                float offset = item.baseWidth * -horizontalAlignment;

                item.layoutBox.ApplyLayoutHorizontal(
                    originBase,
                    originBase + originOffset + offset,
                    item.baseWidth,
                    availableWidth,
                    fit,
                    frameId
                );
            }
        }


        private struct FlexItem {

            public AwesomeLayoutBox layoutBox;
            public LayoutSize widthData;
            public LayoutSize heightData;
            public int growPieces;
            public int shrinkPieces;
            public float baseWidth;
            public float baseHeight;
            public float availableSize;

        }

        private struct Track {

            public float size;
            public int endIndex;
            public int startIndex;
            public float remaining;

        }

    }

}