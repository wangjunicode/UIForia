using System;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout.LayoutTypes {

    public class FlowLayoutBox : LayoutBox {

        private readonly LightList<FlexItem> items;

        public FlowLayoutBox(UIElement element) : base(element) {
            items = new LightList<FlexItem>();
        }

        public override void RunLayout() {
            if (children.Count == 0) {
                actualWidth = allocatedWidth;
                actualHeight = allocatedHeight;
                return;
            }

            if (style.FlexLayoutDirection == LayoutDirection.Column) {
                Size size = RunColumnLayout();
                actualWidth = size.width;
                actualHeight = size.height;
                for (int i = 0; i < children.Count; i++) {
                    FlexItem item = items[i];

                    children[i].SetAllocatedRect(
                        item.mainAxisStart,
                        item.crossAxisStart,
                        Mathf.Max(0, item.mainSize - item.margin.left - item.margin.right),
                        Mathf.Max(0, item.crossSize - item.margin.top - item.margin.bottom)
                    );
                }
            }
            else {
                Size size = RunRowLayout();
                actualWidth = size.width;
                actualHeight = size.height;
                for (int i = 0; i < children.Count; i++) {
                    FlexItem item = items[i];

                    children[i].SetAllocatedRect(
                        item.crossAxisStart,
                        item.mainAxisStart,
                        Mathf.Max(0, item.crossSize - item.margin.left - item.margin.right),
                        Mathf.Max(0, item.mainSize - item.margin.top - item.margin.bottom)
                    );
                }
            }
        }

        protected override float ComputeContentWidth() {
            if (children.Count == 0) {
                return 0;
            }

            if (style.FlexLayoutDirection == LayoutDirection.Column) {
                Size size = RunColumnLayout();
                return size.width;
            }
            else {
                Size size = RunRowLayout();
                return size.width;
            }
        }

        protected override float ComputeContentHeight(float width) {
            if (children.Count == 0) {
                return 0;
            }
            float cachedWidth = allocatedWidth;
            allocatedWidth = width;
            Size size;
            if (style.FlexLayoutDirection == LayoutDirection.Column) {
                size = RunColumnLayout();
            }
            else {
                size = RunRowLayout();
            }
            allocatedWidth = cachedWidth;
            return size.height;
        }

        protected Size RunColumnLayout() {
            float paddingBorderTop = PaddingTop + BorderTop;
            float paddingBorderBottom = PaddingBottom + BorderBottom;

            float paddingBorderLeft = PaddingLeft + BorderLeft;
            float adjustedWidth = allocatedWidth - paddingBorderLeft - PaddingRight - BorderRight;

            CrossAxisAlignment crossAxisAlignment = style.FlexLayoutCrossAxisAlignment;

            items.Clear();

            FlexTrack track = new FlexTrack(adjustedWidth);

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                FlexItem item = new FlexItem();
                LayoutBoxSize widthSize = child.GetWidths();
                CrossAxisAlignment childAlignment = child.style.FlexItemSelfAlignment;

                item.margin = child.GetMargin(widthSize.clampedSize);
                item.mainSize = widthSize.clampedSize + item.margin.left + item.margin.right;
                item.crossSize = child.GetHeights(item.mainSize).clampedSize + item.margin.top + item.margin.bottom;
                item.crossAxisAlignment = childAlignment != CrossAxisAlignment.Unset
                    ? childAlignment
                    : crossAxisAlignment;

                items.Add(item);
                track.remainingSpace -= item.mainSize;
                track.mainSize += item.mainSize;
            }

            AlignMainAxis(track, paddingBorderLeft);
            track.crossSize = PositionCrossAxis(paddingBorderTop, paddingBorderBottom, allocatedHeight);

            return new Size(
                track.mainSize,
                track.crossSize
            );
        }

        protected Size RunRowLayout() {
            float paddingBorderTop = PaddingTop + BorderTop;
            float paddingBorderBottom = PaddingBottom + BorderBottom;

            float paddingBorderLeft = PaddingLeft + BorderLeft;
            float adjustedWidth = allocatedWidth - paddingBorderLeft - PaddingRight - BorderRight;

            CrossAxisAlignment crossAxisAlignment = style.FlexLayoutCrossAxisAlignment;
            items.Clear();

            FlexTrack track = new FlexTrack(adjustedWidth);
            float maxWidth = 0f;

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                FlexItem item = new FlexItem();
                LayoutBoxSize widthSize = child.GetWidths();
                CrossAxisAlignment childAlignment = child.style.FlexItemSelfAlignment;

                OffsetRect margin = new OffsetRect(
                    0, child.GetMarginRight(),
                    0, child.GetMarginLeft()
                );
                item.margin = margin;
                item.crossSize = widthSize.clampedSize + margin.left + margin.right;
                item.crossAxisAlignment = childAlignment != CrossAxisAlignment.Unset
                    ? childAlignment
                    : crossAxisAlignment;

                items.Add(item);
                maxWidth = Mathf.Max(maxWidth, item.crossSize);
            }

            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                FlexItem item = items[i];

                if (item.crossAxisAlignment == CrossAxisAlignment.Stretch) {
                    item.crossSize = maxWidth;
                }

                OffsetRect margin = new OffsetRect(
                    child.GetMarginTop(item.crossSize),
                    item.margin.right,
                    child.GetMarginBottom(item.crossSize),
                    item.margin.left
                );

                item.margin = margin;
                item.mainSize = child.GetHeights(item.crossSize).clampedSize + margin.top + margin.bottom;
                items.Add(item);
            }
           
            return new Size(
                AlignMainAxis(track, paddingBorderTop),
                PositionCrossAxis(paddingBorderTop, paddingBorderBottom, allocatedHeight)
            );
        }

        private float PositionCrossAxis(float offsetTop, float offsetBottom, float targetSize) {
            float maxHeight = 0;
            targetSize -= (offsetTop + offsetBottom);
            FlexItem[] itemArray = items.Array;
            for (int i = 0; i < items.Count; i++) {
                FlexItem item = itemArray[i];
                switch (itemArray[i].crossAxisAlignment) {
                    case CrossAxisAlignment.Center:
                        item.crossAxisStart = (targetSize * 0.5f) - (item.crossSize * 0.5f);
                        break;

                    case CrossAxisAlignment.End:
                        item.crossAxisStart = targetSize - item.crossSize;
                        break;

                    case CrossAxisAlignment.Start:
                        item.crossAxisStart = 0;
                        break;

                    case CrossAxisAlignment.Stretch:
                        item.crossAxisStart = 0;
                        item.crossSize = targetSize;
                        break;

                    default:
                        item.crossAxisStart = 0;
                        break;
                }

                item.crossAxisStart += offsetTop + item.margin.top;

                if (item.crossSize > maxHeight) {
                    maxHeight = item.crossSize;
                }

                items[i] = item;
            }

            return maxHeight;
        }

        private float AlignMainAxis(FlexTrack track, float mainAxisOffset) {
            MainAxisAlignment mainAxisAlignment = style.FlexLayoutMainAxisAlignment;
            float spacerSize = 0;
            float offset = items[0].margin.left;
            int itemCount = items.Count;

            if (track.remainingSpace > 0) {
                switch (mainAxisAlignment) {
                    case MainAxisAlignment.Unset:
                    case MainAxisAlignment.Start:
                        break;
                    case MainAxisAlignment.Center:
                        mainAxisOffset *= 0.5f; //todo seems wrong to add this twice
                        offset = mainAxisOffset + track.remainingSpace * 0.5f;
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

            FlexItem[] itemArray = items.Array;
            for (int i = 0; i < itemCount; i++) {
                itemArray[i].mainAxisStart = mainAxisOffset + offset;
                offset += itemArray[i].mainSize + spacerSize;
            }

            return track.mainSize;
        }

        protected override void OnChildAdded(LayoutBox child) {
            if (!child.element.isEnabled || child.IsIgnored) {
                return;
            }

            int idx = FindLayoutSiblingIndex(child.element);

            if (idx <= children.Count) {
                children.Insert(idx, child);
            }
            else {
                children.Add(child);
            }

            if (child.element.isEnabled) {
                RequestContentSizeChangeLayout();
            }
        }

        public override void OnChildEnabled(LayoutBox child) {
            OnChildAdded(child);
        }

        public override void OnChildDisabled(LayoutBox child) {
            children.Remove(child);
            RequestContentSizeChangeLayout();
        }
        
        private struct FlexTrack {

            public float mainSize;
            public float crossSize;
            public float remainingSpace;

            public FlexTrack(float totalSize) {
                this.remainingSpace = totalSize;
                this.mainSize = 0;
                this.crossSize = 0;
            }

        }

        private struct FlexItem {

            public float mainAxisStart;
            public float crossAxisStart;
            public float mainSize;
            public float crossSize;
            public OffsetRect margin;

            public CrossAxisAlignment crossAxisAlignment;

        }

    }

}