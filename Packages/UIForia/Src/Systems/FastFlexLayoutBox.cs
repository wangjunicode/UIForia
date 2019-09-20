using System.Diagnostics;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    [DebuggerDisplay("FlexBox : {element.ToString()}")]
    public class FastFlexLayoutBox : FastLayoutBox {

        private StructList<Item> itemList;
        private LayoutDirection direction;
        private MainAxisAlignment mainAxisAlignment;
        private CrossAxisAlignment crossAxisAlignment;

        public override void UpdateStyleData() {
            base.UpdateStyleData();
            direction = element.style.FlexLayoutDirection;
            mainAxisAlignment = element.style.FlexLayoutMainAxisAlignment;
            crossAxisAlignment = element.style.FlexLayoutCrossAxisAlignment;
        }

        protected override void PerformLayout() {
            if (firstChild == null) {
                contentSize = default;
                return;
            }

            if (direction == LayoutDirection.Horizontal) {
                contentSize = PerformLayoutHorizontal(containingBoxWidth, containingBoxHeight, false);
            }
            else {
                contentSize = PerformLayoutVertical();
            }

        }

        public override float ComputeContentWidth(BlockSize blockWidth) {
            if (firstChild == null) {
                return 0;
            }

            if (direction == LayoutDirection.Horizontal) {
                return ComputeHorizontalContentWidth(blockWidth);
            }

            return ComputeVerticalContentWidth(blockWidth);
        }

        private float ComputeHorizontalContentWidth(BlockSize blockWidth) {
            float retn = 0;
            Item[] items = itemList.array;

            // todo -- doesn't account for wrapping
            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetWidth(blockWidth, ref item.size);
                child.GetMarginHorizontal(item.size.prefWidth, ref item.margin);
                item.outputWidth = item.size.prefWidth;
                retn += item.size.prefWidth + item.margin.left + item.margin.right;
            }

            return retn;
        }

        private float ComputeVerticalContentWidth(BlockSize blockWidth) {
            float retn = 0;
            Item[] items = itemList.array;

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetWidth(blockWidth, ref item.size);
                child.GetMarginHorizontal(item.size.prefWidth, ref item.margin);
                float value = item.size.prefWidth + item.margin.left + item.margin.right;
                if (value > retn) {
                    retn = value;
                }
            }

            return retn;
        }

        private float ComputeHorizontalContentHeight(float contentAreaWidth, BlockSize blockWidth, BlockSize blockHeight) {
            return PerformLayoutHorizontal(blockWidth, blockHeight, true).height;
        }

        public override float ComputeContentHeight(float width, BlockSize blockWidth, BlockSize blockHeight) {
            if (firstChild == null) {
                return 0;
            }

            if (direction == LayoutDirection.Horizontal) {
                return ComputeHorizontalContentHeight(width, blockWidth, blockHeight);
            }

            return ComputeVerticalContentHeight(width, blockWidth, blockHeight);
        }

        private float ComputeVerticalContentHeight(float width, BlockSize blockWidth, BlockSize blockHeight) {
            Item[] items = itemList.array;

            blockWidth.size = width;
            blockWidth.contentAreaSize = width
                                         - ResolveFixedSize(width, paddingBox.left)
                                         - ResolveFixedSize(width, paddingBox.right)
                                         - ResolveFixedSize(width, borderBox.left)
                                         - ResolveFixedSize(width, borderBox.right); 
            
            float crossSize = 0;
            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetWidth(blockWidth, ref item.size);
                child.GetMarginHorizontal(item.size.prefWidth, ref item.margin);
                item.outputWidth = item.size.prefWidth;
                float totalWidth = item.margin.left + item.size.prefWidth + item.margin.right;
                if (crossSize < totalWidth) {
                    crossSize = totalWidth;
                }
            }

            crossSize = Mathf.Max(width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right, crossSize);

            float retn = 0;
            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetHeight(item.outputWidth, blockWidth, blockHeight, ref item.size);
                child.GetMarginVertical(item.size.prefHeight, ref item.margin);
                retn += item.margin.top + item.size.prefHeight + item.margin.bottom;
            }

            return retn;
        }

        public override float GetIntrinsicMinWidth() {
            FastLayoutBox child = firstChild;

            if (direction == LayoutDirection.Horizontal) {
                float min = 0;

                while (child != null) {
                    min += child.GetIntrinsicMinWidth();
                    child = child.nextSibling;
                }

                return min;
            }

            // intrinsic min for vertical direction is the max child intrinsic width
            float max = 0;

            while (child != null) {
                float childMin = child.GetIntrinsicMinWidth();
                if (childMin > max) {
                    max = childMin;
                }

                child = child.nextSibling;
            }

            return max;
        }

        public override float GetIntrinsicMinHeight() {
            FastLayoutBox child = firstChild;

            if (direction == LayoutDirection.Vertical) {
                float min = 0;

                while (child != null) {
                    min += child.GetIntrinsicMinHeight();
                    child = child.nextSibling;
                }

                return min;
            }

            // intrinsic min for horizontal direction is the max child intrinsic width
            float max = 0;

            while (child != null) {
                float childMin = child.GetIntrinsicMinHeight();
                if (childMin > max) {
                    max = childMin;
                }

                child = child.nextSibling;
            }

            return max;
        }

        protected override float ComputeIntrinsicPreferredWidth() {
            FastLayoutBox child = firstChild;

            if (direction == LayoutDirection.Horizontal) {
                float retn = 0;

                while (child != null) {
                    retn += child.GetIntrinsicPreferredWidth();
                    child = child.nextSibling;
                }

                return retn;
            }
            else {
                // intrinsic max for vertical direction is the max child intrinsic width
                float retn = 0;

                while (child != null) {
                    float childMax = child.GetIntrinsicPreferredWidth(); //(element.View.Viewport.width);
                    if (childMax > retn) {
                        retn = childMax;
                    }

                    child = child.nextSibling;
                }
                
                return retn;
            }
        }

        public override float GetIntrinsicPreferredHeight() {
            FastLayoutBox child = firstChild;

            if (direction == LayoutDirection.Vertical) {
                float retn = 0;

                while (child != null) {
                    retn += child.GetIntrinsicPreferredHeight();
                    child = child.nextSibling;
                }

                float contentWidth = retn;

                retn += ResolveFixedSize(contentWidth, element.style.PaddingTop);
                retn += ResolveFixedSize(contentWidth, element.style.PaddingBottom);
                retn += ResolveFixedSize(contentWidth, element.style.BorderTop);
                retn += ResolveFixedSize(contentWidth, element.style.BorderBottom);

                return retn;
            }
            else {
                // intrinsic max for horizontal direction is the max child intrinsic height
                float retn = 0;

                while (child != null) {
                    float childMax = child.GetIntrinsicPreferredHeight(); //(element.View.Viewport.width);
                    if (childMax > retn) {
                        retn = childMax;
                    }

                    child = child.nextSibling;
                }

                float contentWidth = retn;

                retn += ResolveFixedSize(contentWidth, element.style.PaddingTop);
                retn += ResolveFixedSize(contentWidth, element.style.PaddingBottom);
                retn += ResolveFixedSize(contentWidth, element.style.BorderTop);
                retn += ResolveFixedSize(contentWidth, element.style.BorderBottom);
                return retn;
            }
        }

        private Size PerformLayoutHorizontal(BlockSize blockWidth, BlockSize blockHeight, bool isDryRun) {
            Item[] items = itemList.array;

            AdjustBlockSizes(ref blockWidth, ref blockHeight);

            Track track = default;
            track.startItemIndex = 0;
            track.endItemIndex = itemList.size;

            float totalContentHeight = 0;

            Size retn = default;

            bool applyWrapping = element.style.FlexLayoutWrap == LayoutWrap.Wrap;
            float contentAreaWidth = size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right;
            float largestTrackWidth = 0;

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetWidth(blockWidth, ref item.size);
                child.GetMarginHorizontal(item.size.prefWidth, ref item.margin);
                item.outputWidth = item.size.prefWidth;
                track.growPieces += item.growFactor;
                track.shrinkPieces += item.shrinkFactor;
                float totalItemWidth = item.size.prefWidth + item.margin.left + item.margin.right;
                if (applyWrapping) {
                    track.endItemIndex = i + 1;
                    if (totalItemWidth + track.mainSize > contentAreaWidth) {
                        // break track case
                        if (track.startItemIndex == i) {
                            // single item track
                            track.endItemIndex -= 1;
                            totalContentHeight += LayoutTrackHorizontal(totalContentHeight, blockWidth, blockHeight, track, isDryRun, applyWrapping);
                            track = default;
                            track.startItemIndex = i;
                            track.endItemIndex = i;
                        }
                        else {
                            // multi item track
                            totalContentHeight += LayoutTrackHorizontal(totalContentHeight, blockWidth, blockHeight, track, isDryRun, applyWrapping);
                            track = default;
                            track.startItemIndex = i;
                            track.endItemIndex = i;
                        }
                    }
                }

                track.mainSize += totalItemWidth;
                track.remainingSpace = contentAreaWidth - track.mainSize;
            }

            if (track.endItemIndex - track.startItemIndex > 0) {
                totalContentHeight += LayoutTrackHorizontal(totalContentHeight, blockWidth, blockHeight, track, isDryRun, applyWrapping);
            }

            retn.height = totalContentHeight;
            retn.width = contentAreaWidth; // todo compute this!
            return retn;
        }

        private Size PerformLayoutVertical() {
            Item[] items = itemList.array;

            BlockSize blockWidth = containingBoxWidth;
            BlockSize blockHeight = containingBoxHeight;
            AdjustBlockSizes(ref blockWidth, ref blockHeight);

            Track track = default;
            int growPieces = 0;
            int shrinkPieces = 0;

            float axisOffset = paddingBox.left + borderBox.left;

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                growPieces += item.growFactor;
                shrinkPieces += item.shrinkFactor;
                FastLayoutBox child = item.layoutBox;
                child.GetWidth(blockWidth, ref item.size);
                child.GetMarginHorizontal(item.size.prefWidth, ref item.margin);
                item.outputWidth = item.size.prefWidth;
            }

            track.startItemIndex = 0;
            track.endItemIndex = itemList.size;
            track.crossSize = size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right;

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetHeight(track.crossSize - item.margin.left - item.margin.right, blockWidth, blockHeight, ref item.size);
                child.GetMarginVertical(item.size.prefHeight, ref item.margin);
                item.outputHeight = item.size.prefHeight;
                track.mainSize += item.margin.top + item.size.prefHeight + item.margin.bottom;
                float horizontalAlignment = 0;
                LayoutFit horizontalLayoutFit = LayoutFit.None;

                switch (crossAxisAlignment) {
                    case CrossAxisAlignment.Unset:
                    case CrossAxisAlignment.Start:
                        horizontalAlignment = 0f;
                        break;
                    case CrossAxisAlignment.Center:
                        horizontalAlignment = 0.5f;
                        break;
                    case CrossAxisAlignment.End:
                        horizontalAlignment = 1f;
                        break;
                    case CrossAxisAlignment.Stretch:
                        horizontalLayoutFit = LayoutFit.Fill;
                        break;
                }

                child.ApplyHorizontalLayout(axisOffset + item.margin.left, blockWidth, track.crossSize - item.margin.left - item.margin.right, item.size.prefWidth, horizontalAlignment, horizontalLayoutFit);
            }

            track.remainingSpace = size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom - track.mainSize;

            if (growPieces > 0 && track.remainingSpace > 0) {
                GrowHeight(ref track, growPieces);
            }
            else if (shrinkPieces > 0 && track.remainingSpace < 0) {
                ShrinkHeight(ref track, shrinkPieces);
            }

            ApplyMainAxisVerticalPositioning(track, paddingBox.top + borderBox.top);

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                LayoutFit layoutFit = LayoutFit.Unset;

                if (item.outputHeight > item.size.prefHeight && item.growFactor > 0) {
                    layoutFit = LayoutFit.Grow;
                }
                else if (item.outputHeight < item.size.prefHeight && item.shrinkFactor > 0) {
                    layoutFit = LayoutFit.Shrink;
                }

                item.layoutBox.ApplyVerticalLayout(item.mainAxisStart, blockHeight, item.outputHeight, item.size.prefHeight, 0, layoutFit);
            }

            return new Size(track.crossSize, track.mainSize);
        }

        private float LayoutTrackHorizontal(float yOffset, in BlockSize blockWidth, in BlockSize blockHeight, Track track, bool isDryRun, bool isWrapping = false) {
            Item[] items = itemList.array;

            if (track.remainingSpace > 0) {
                GrowWidth(ref track, track.growPieces);
            }
            else if (track.remainingSpace < 0) {
                ShrinkWidth(ref track, track.shrinkPieces);
            }

            ApplyMainAxisHorizontalPositioning(track, paddingBox.left + borderBox.left);

            if (isDryRun) {
                for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                    ref Item item = ref items[i];
                    FastLayoutBox child = item.layoutBox;
                    child.GetHeight(item.outputWidth, blockWidth, blockHeight, ref item.size);
                    child.GetMarginVertical(item.size.prefHeight, ref item.margin);
                    item.outputHeight = item.size.prefHeight;
                    float heightAndMargin = item.outputHeight + item.margin.top + item.margin.bottom;
                    track.maxItemHeight = track.maxItemHeight > heightAndMargin ? track.maxItemHeight : heightAndMargin;
                }

                // todo -- this doesn't work for min or max height
                track.crossSize = isWrapping || element.style.PreferredHeight.unit == UIMeasurementUnit.Content ? track.maxItemHeight : size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom;

                return track.crossSize;
            }

            for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;

                LayoutFit layoutFit = LayoutFit.Unset;

                if (item.outputWidth > item.size.prefWidth && item.growFactor > 0) {
                    layoutFit = LayoutFit.Grow;
                }
                else if (item.outputWidth < item.size.prefWidth && item.shrinkFactor > 0) {
                    layoutFit = LayoutFit.Shrink;
                }

                float horizontalAlignment = 0;

                switch (mainAxisAlignment) {
                    case MainAxisAlignment.Unset:
                    case MainAxisAlignment.Start:
                        horizontalAlignment = 0f;
                        break;
                    case MainAxisAlignment.Center:
                        horizontalAlignment = 0.5f;
                        break;
                    case MainAxisAlignment.End:
                        horizontalAlignment = 1f;
                        break;
                }
                
                child.ApplyHorizontalLayout(item.mainAxisStart, blockWidth, item.outputWidth, item.size.prefWidth, horizontalAlignment, layoutFit);

                child.GetHeight(item.outputWidth, blockWidth, blockHeight, ref item.size);
                child.GetMarginVertical(item.size.prefHeight, ref item.margin);
                item.outputHeight = item.size.prefHeight;
                float heightAndMargin = item.outputHeight + item.margin.top + item.margin.bottom;
                track.maxItemHeight = track.maxItemHeight > heightAndMargin ? track.maxItemHeight : heightAndMargin;
            }

            // if track is first or last do padding & border
            float paddingBorderTop = track.startItemIndex == 0 ? paddingBox.top + borderBox.top : 0;
            // float paddingBorderBottom = track.endItemIndex == itemList.size ? paddingBox.bottom + borderBox.bottom : 0;
            float axisOffset = yOffset + paddingBorderTop; // paddingBox.top + borderBox.top;

            track.crossSize = isWrapping || element.style.PreferredHeight.unit == UIMeasurementUnit.Content ? track.maxItemHeight : size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom;

            for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                ref Item item = ref items[i];
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
                        verticalLayoutFit = LayoutFit.Grow;
                        break;
                }

                item.layoutBox.ApplyVerticalLayout(axisOffset + item.margin.top, blockHeight, track.crossSize - item.margin.top - item.margin.bottom, item.size.prefHeight, verticalAlignment, verticalLayoutFit);
            }

            return track.crossSize;
        }

        private void ApplyMainAxisHorizontalPositioning(Track track, float mainAxisOffset) {
            Item[] items = itemList.array;
            int itemCount = track.endItemIndex - track.startItemIndex;
            float spacerSize = 0;
            float offset = 0;

            if (track.remainingSpace > 0) {
                switch (mainAxisAlignment) {
                    case MainAxisAlignment.Unset:
                    case MainAxisAlignment.Start:
                        break;

                    case MainAxisAlignment.Center:
                        mainAxisOffset *= 0.5f;
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
                }
            }

            for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                items[i].mainAxisStart = items[i].margin.left + mainAxisOffset + offset;
                offset += items[i].outputWidth + items[i].margin.left + items[i].margin.right + spacerSize;
            }
        }

        private void ApplyMainAxisVerticalPositioning(Track track, float mainAxisOffset) {
            Item[] items = itemList.array;
            int itemCount = track.endItemIndex - track.startItemIndex;
            float spacerSize = 0;
            float offset = 0;

            if (track.remainingSpace > 0) {
                switch (mainAxisAlignment) {
                    case MainAxisAlignment.Unset:
                    case MainAxisAlignment.Start:
                        break;

                    case MainAxisAlignment.Center:
                        mainAxisOffset *= 0.5f;
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
                }
            }

            for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                items[i].mainAxisStart = items[i].margin.top + mainAxisOffset + offset;
                offset += items[i].outputHeight + items[i].margin.top + items[i].margin.bottom + spacerSize;
            }
        }

        private void GrowWidth(ref Track track, int pieces) {
            Item[] items = itemList.array;

            bool allocate = pieces > 0;
            float toAllocate = track.remainingSpace;
            float remainingSpace = track.remainingSpace;

            while (allocate && (int) remainingSpace > 0) {
                allocate = false;

                float pieceSize = remainingSpace / pieces;

                for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                    ref Item item = ref items[i];
                    float max = item.size.maxWidth;
                    float output = item.outputWidth;
                    int growthFactor = item.growFactor;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    output = totalGrowth > max ? max : totalGrowth;

                    remainingSpace -= output - start;

                    item.outputWidth = output;
                }
            }

            track.mainSize += toAllocate - remainingSpace;
            track.remainingSpace = remainingSpace;
        }

        private void GrowHeight(ref Track track, int pieces) {
            Item[] items = itemList.array;

            bool allocate = pieces > 0;
            float toAllocate = track.remainingSpace;
            float remainingSpace = track.remainingSpace;

            while (allocate && (int) remainingSpace > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remainingSpace / pieces;

                for (int i = track.startItemIndex; i < track.endItemIndex; i++) {
                    ref Item item = ref items[i];
                    float max = item.size.maxHeight;
                    float output = item.outputHeight;
                    int growthFactor = item.growFactor;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    output = totalGrowth > max ? max : totalGrowth;

                    remainingSpace -= output - start;

                    item.outputHeight = output;
                }
            }

            track.mainSize += toAllocate - remainingSpace;
            track.remainingSpace = remainingSpace;
        }

        private void ShrinkWidth(ref Track track, int pieces) {
            int startIndex = track.startItemIndex;
            int endIndex = track.endItemIndex;

            Item[] items = itemList.array;

            float overflow = -track.remainingSpace;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;

                for (int i = startIndex; i < endIndex; i++) {
                    ref Item item = ref items[i];
                    float min = item.size.minWidth;
                    float output = item.size.prefWidth;
                    int shrinkFactor = item.shrinkFactor;

                    if (shrinkFactor == 0 || (int) output == (int) min || (int) output == 0) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    output = (totalShrink < min) ? min : totalShrink;
                    overflow += output - start;

                    item.outputWidth = output;
                }
            }

            track.remainingSpace = overflow;
        }

        private void ShrinkHeight(ref Track track, int pieces) {
            int startIndex = track.startItemIndex;
            int endIndex = track.endItemIndex;

            Item[] items = itemList.array;

            float overflow = -track.remainingSpace;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;

                for (int i = startIndex; i < endIndex; i++) {
                    ref Item item = ref items[i];
                    float min = item.size.minHeight;
                    float output = item.size.prefHeight;
                    int shrinkFactor = item.shrinkFactor;

                    if (shrinkFactor == 0 || (int) output == (int) min || (int) output == 0) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    output = (totalShrink < min) ? min : totalShrink;
                    overflow += output - start;

                    item.outputHeight = output;
                }
            }

            track.remainingSpace = overflow;
        }

        public override void SetChildren(LightList<FastLayoutBox> children) {
            base.SetChildren(children);

            if (itemList == null) {
                itemList = new StructList<Item>(children.size);
            }
            else {
                itemList.EnsureCapacity(children.size);
            }

            itemList.size = children.size;
            Item[] items = itemList.array;

            if (children.size == 0) {
                return;
            }

            firstChild = children[0];

            for (int i = 0; i < children.size; i++) {
                ref Item item = ref items[i];
                item.layoutBox = children[i];
                item.growFactor = children[i].element.style.FlexItemGrow;
                item.shrinkFactor = children[i].element.style.FlexItemShrink;
            }

            MarkForLayout();
        }

        protected override void OnChildAdded(FastLayoutBox child, int index) {
            itemList = itemList ?? new StructList<Item>();

            if (firstChild == null) {
                firstChild = child;
                MarkForLayout();
                return;
            }

            itemList.Insert(index, new Item() {layoutBox = child, growFactor = child.element.style.FlexItemGrow, shrinkFactor = child.element.style.FlexItemShrink,});

            MarkForLayout();
        }

        protected override void OnChildRemoved(FastLayoutBox child, int index) {
            itemList.RemoveAt(index);
        }

        public override void OnStyleChanged(StructList<StyleProperty> changeList) {
            base.OnStyleChanged(changeList);

            bool changed = false;
            StyleProperty[] properties = changeList.array;

            for (int i = 0; i < changeList.size; i++) {
                ref StyleProperty property = ref properties[i];

                switch (property.propertyId) {
                    case StylePropertyId.FlexLayoutDirection:
                        changed = true;
                        direction = property.AsLayoutDirection;
                        break;

                    case StylePropertyId.FlexLayoutMainAxisAlignment:
                        changed = true;
                        mainAxisAlignment = property.AsMainAxisAlignment;
                        break;

                    case StylePropertyId.FlexLayoutCrossAxisAlignment:
                        changed = true;
                        crossAxisAlignment = property.AsCrossAxisAlignment;
                        break;
                }
            }

            if (changed) {
                MarkForLayout();
            }
        }

        protected override void OnChildStyleChanged(FastLayoutBox child, StructList<StyleProperty> changeList) {
            base.OnChildStyleChanged(child, changeList);

            bool changed = false;
            StyleProperty[] properties = changeList.array;

            for (int i = 0; i < changeList.size; i++) {
                ref StyleProperty property = ref properties[i];

                switch (property.propertyId) {
                    case StylePropertyId.FlexItemGrow:
                        for (int j = 0; j < itemList.size; j++) {
                            if (itemList.array[j].layoutBox == child) {
                                itemList.array[j].growFactor = property.AsInt;
                                break;
                            }
                        }

                        break;

                    case StylePropertyId.FlexItemShrink:
                        changed = true;
                        for (int j = 0; j < itemList.size; j++) {
                            if (itemList.array[j].layoutBox == child) {
                                itemList.array[j].shrinkFactor = property.AsInt;
                                break;
                            }
                        }

                        break;
                }
            }

            if (changed) {
                MarkForLayout();
            }
        }

        public struct Track {

            public float mainSize;
            public float crossSize;
            public float remainingSpace;
            public int startItemIndex;
            public int endItemIndex;
            public int growPieces;
            public int shrinkPieces;
            public float maxItemWidth;
            public float maxItemHeight;

        }

        public struct Item {

            public float mainAxisStart;
            public float crossAxisStart;
            public float outputWidth;
            public float outputHeight;
            public int growFactor;
            public int shrinkFactor;
            public OffsetRect margin;
            public SizeConstraints size;
            public FastLayoutBox layoutBox;

        }

    }

}