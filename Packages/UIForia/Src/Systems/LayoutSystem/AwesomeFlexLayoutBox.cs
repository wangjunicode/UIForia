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
            return direction == LayoutDirection.Horizontal
                ? ComputeContentWidthHorizontal()
                : ComputeContentWidthVertical();
        }

        protected override float ComputeContentHeight() {
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
                LayoutSize heights = items.array[i].layoutBox.GetHeights();
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
                LayoutSize heights = items.array[i].layoutBox.GetHeights();
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
            if (direction == LayoutDirection.Horizontal) {
                RunLayoutWidth_Horizontal(frameId);
            }
            else {
                RunLayoutWidth_Vertical(frameId);
            }
        }

        private void RunLayoutWidth_Horizontal(int frameId) {
            Track track = new Track();
            track.remaining = finalWidth - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);
            track.size = track.remaining;
            track.startIndex = 0;
            track.endIndex = childCount;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.layoutBox.GetWidths(ref item.widthData);
                item.baseWidth = Mathf.Max(item.widthData.minimum, Mathf.Min(item.widthData.preferred, item.widthData.maximum));
                item.availableWidth = item.baseWidth;
                track.remaining -= item.baseWidth + item.widthData.marginStart + item.widthData.marginEnd;
            }

            if (track.remaining > 0) {
                GrowHorizontal(ref track);
            }
            else if (track.remaining < 0) {
                Shrink(ref track);
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
                offset += item.availableWidth + spacerSize;
                LayoutFit fit = LayoutFit.None;

                if (item.growPieces > 0 || item.shrinkPieces > 0) {
                    fit = LayoutFit.Fill;
                }

                item.layoutBox.ApplyLayoutHorizontal(x, item.baseWidth, item.availableWidth, fit, frameId);
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
                    float output = item.availableWidth;
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
                    item.availableWidth = output;
                }
            }

            track.size += toAllocate - track.remaining;
        }

        private void Shrink(ref Track track) { }

        public override void RunLayoutVertical() { }

        private void RunLayoutWidth_Vertical(int frameId) {
            float availableWidth = finalWidth;
            float axisOffset = paddingBorderHorizontalStart;
            float alignment = 0;
            LayoutFit fit = LayoutFit.None;

            switch (crossAxisAlignment) {
                case CrossAxisAlignment.Unset:
                case CrossAxisAlignment.Start:
                    break;
                case CrossAxisAlignment.Center:
                    alignment = 0.5f;
                    break;
                case CrossAxisAlignment.End:
                    alignment = 1f;
                    break;
                case CrossAxisAlignment.Stretch:
                    alignment = 0;
                    fit = LayoutFit.Fill;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.layoutBox.GetWidths(ref item.widthData);
                item.baseWidth = Mathf.Max(item.widthData.minimum, Mathf.Min(item.widthData.preferred, item.widthData.maximum));
                float originBase = axisOffset + item.widthData.marginStart;
                float originOffset = (availableWidth - (item.widthData.marginStart + item.widthData.marginEnd)) * alignment;
                float offset = availableWidth * -alignment;
                float localX = originBase + originOffset + offset;
                item.layoutBox.ApplyLayoutHorizontal(localX, item.baseWidth, availableWidth, fit, frameId);
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
            public float availableWidth;
            public float availableHeight;

        }

        private struct Track {

            public float size;
            public int endIndex;
            public int startIndex;
            public float remaining;

        }

    }

}