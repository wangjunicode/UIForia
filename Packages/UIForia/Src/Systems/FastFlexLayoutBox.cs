using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Systems {

    public class FastFlexLayoutBox : FastLayoutBox {

        private StructList<Item> itemList;
        private StructList<Track> trackList;
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
                contentSize.width = 0;
                contentSize.height = 0;
                return;
            }

            if (direction == LayoutDirection.Horizontal) {
                PerformLayoutHorizontal();
            }
            else {
                PerformLayoutVertical();
            }

            for (int i = 0; i < itemList.size; i++) {
                itemList.array[i].layoutBox.Layout();
            }
        }

        protected override float ComputeContentWidth(float blockWidth) {
            // no growing,
            // just get size for all children,
            // add them up.
            // return it
            if (firstChild == null) return 0;

            if (direction == LayoutDirection.Horizontal) {
                return ComputeHorizontalContentWidth(blockWidth);
            }

            return ComputeVerticalContentWidth(blockWidth);
        }

        private float ComputeHorizontalContentWidth(float blockWidth) {
            float retn = 0;
            Item[] items = itemList.array;

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetWidth(blockWidth, ref item.size);
                child.GetMarginHorizontal(blockWidth, ref item.margin);
                item.outputWidth = item.size.prefWidth;
                retn += item.size.prefWidth + item.margin.left + item.margin.right;
            }

            return retn;
        }

        private float ComputeVerticalContentWidth(float blockWidth) {
            float retn = 0;
            Item[] items = itemList.array;

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetWidth(blockWidth, ref item.size);
                child.GetMarginHorizontal(blockWidth, ref item.margin);
                float value = item.size.prefWidth + item.margin.left + item.margin.right;
                if (value > retn) {
                    retn = value;
                }
            }

            return retn;
        }

        private float ComputeHorizontalContentHeight(float width, float blockWidth, float blockHeight) {
            Item[] items = itemList.array;
            Track track = default;

            int growPieces = 0;
            int shrinkPieces = 0;

            width -= paddingBox.left - paddingBox.right - borderBox.right - borderBox.left;

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetWidth(blockWidth, ref item.size);
                child.GetMarginHorizontal(blockWidth, ref item.margin);
                item.outputWidth = item.size.prefWidth;
                growPieces += item.growFactor;
                shrinkPieces += item.shrinkFactor;
                track.mainSize += item.size.prefWidth + item.margin.left + item.margin.right;
            }

            track.remainingSpace = width - track.mainSize;

            if (growPieces > 0 && track.remainingSpace > 0) {
                GrowWidth(ref track, growPieces);
            }
            else if (shrinkPieces > 0 && track.remainingSpace < 0) {
                ShrinkWidth(ref track, shrinkPieces);
            }

            float retn = 0;

            // do i need to account for children growing on width and take their actual sizes? probably :(

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetHeight(item.outputWidth, blockWidth, blockHeight, ref item.size);
                child.GetMarginVertical(blockWidth, ref item.margin);
                float childHeight = item.size.prefHeight + item.margin.top + item.margin.bottom;

                if (childHeight > retn) {
                    retn = childHeight;
                }
            }

            return retn;
        }

        protected override float ComputeContentHeight(float width, float blockWidth, float blockHeight) {
            if (firstChild == null) return 0;

            if (direction == LayoutDirection.Horizontal) {
                return ComputeHorizontalContentHeight(width, blockWidth, blockHeight);
            }

            return ComputeVerticalContentHeight(width, blockWidth, blockHeight);
        }

        private float ComputeVerticalContentHeight(float width, float blockWidth, float blockHeight) {
            Item[] items = itemList.array;

            float crossSize = 0;
            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetWidth(blockWidth, ref item.size);
                child.GetMarginHorizontal(blockWidth, ref item.margin);
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
                child.GetHeight(crossSize - item.margin.left - item.margin.right, blockWidth, blockHeight, ref item.size);
                child.GetMarginVertical(blockHeight, ref item.margin);
                retn += item.margin.top + item.size.prefHeight + item.margin.bottom;
            }

            return retn;
        }

        public override float GetIntrinsicMinWidth() {
            Item[] items = itemList.array;
            FastLayoutBox child = firstChild;

            if (direction == LayoutDirection.Horizontal) {
                float min = 0;

                while (child != null) {
                    min += child.GetIntrinsicMinWidth();
                    child = child.nextSibling;
                }

                return min;
            }
            else {
                // intrinsic min for vertical direction is the max child intrinsic width
                float max = 0;

                while (child != null) {
                    float childMin = child.GetIntrinsicMinWidth();
                    if (childMin > max) max = childMin;
                    child = child.nextSibling;
                }

                return max;
            }
        }

        private void PerformLayoutHorizontalWrap() {
            trackList = trackList ?? new StructList<Track>(4); // todo -- see if tracks can be stack alloced or reused instead
        }

        private void PerformLayoutHorizontal() {
            Item[] items = itemList.array;

            Size block = containingBox;

            if (prefWidth.unit == UIMeasurementUnit.Content) {
                block.width = containingBox.width;
            }
            else {
                block.width = size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right;
            }

            if (prefHeight.unit == UIMeasurementUnit.Content) {
                block.height = containingBox.height;
            }
            else {
                block.height = size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom;
            }

            Track track = default;
            track.startItemIndex = 0;
            track.endItemIndex = itemList.size;

            int growPieces = 0;
            int shrinkPieces = 0;

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetWidth(block.width, ref item.size);
                child.GetMarginHorizontal(block.width, ref item.margin);
                item.outputWidth = item.size.prefWidth;
                growPieces += item.growFactor;
                shrinkPieces += item.shrinkFactor;
                track.mainSize += item.size.prefWidth + item.margin.left + item.margin.right;
            }

            // todo - padding / border ?
            track.remainingSpace = size.width - track.mainSize;

            if (track.remainingSpace > 0) {
                GrowWidth(ref track, growPieces);
            }
            else if (track.remainingSpace < 0) {
                ShrinkWidth(ref track, shrinkPieces);
            }

            ApplyMainAxisHorizontalPositioning(track, paddingBox.left + borderBox.left);

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;

                Fit fit = Fit.Unset;

                if (item.outputWidth > item.size.prefWidth && item.growFactor > 0) {
                    fit = Fit.Grow;
                }
                else if (item.outputWidth < item.size.prefWidth && item.shrinkFactor > 0) {
                    fit = Fit.Shrink;
                }

                child.ApplyHorizontalLayout(item.mainAxisStart, block.width, item.outputWidth, item.size.prefWidth, default, fit);

                child.GetHeight(child.size.width, block.width, block.height, ref item.size);
                child.GetMarginVertical(block.height, ref item.margin);

                if (item.size.prefHeight > track.crossSize) {
                    track.crossSize = item.size.prefHeight;
                }
            }

            float axisOffset = paddingBox.top + borderBox.top;

            Alignment verticalAlignment = default;
            Fit verticalFit = Fit.None;

            switch (crossAxisAlignment) {
                case CrossAxisAlignment.Unset:
                case CrossAxisAlignment.Start:
                    verticalAlignment = new Alignment(0, 0, AlignmentTarget.AllocatedBox);
                    break;
                case CrossAxisAlignment.Center:
                    verticalAlignment = new Alignment(0.5f, 0.5f, AlignmentTarget.AllocatedBox);
                    break;
                case CrossAxisAlignment.End:
                    verticalAlignment = new Alignment(1f, 1f, AlignmentTarget.AllocatedBox);
                    break;
                case CrossAxisAlignment.Stretch:
                    verticalFit = Fit.Grow;
                    break;
            }

            track.crossSize = Mathf.Max(size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom, track.crossSize);

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                item.layoutBox.ApplyVerticalLayout(axisOffset + item.margin.top, block.height, track.crossSize - item.margin.top - item.margin.bottom, item.size.prefHeight, verticalAlignment, verticalFit);
                item.outputHeight = item.layoutBox.size.height;
            }

            contentSize.width = 0;
            contentSize.height = 0;
        }

        private void PerformLayoutVertical() {
            Item[] items = itemList.array;

            Size block = containingBox;

            if (prefWidth.unit != UIMeasurementUnit.Content) {
                block.width = size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right;
            }

            if (prefHeight.unit != UIMeasurementUnit.Content) {
                block.height = size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom;
            }

            Track track = default;
            int growPieces = 0;
            int shrinkPieces = 0;

            float axisOffset = paddingBox.left + borderBox.left;

            Alignment horizontalAlignment = default;
            Fit horizontalFit = Fit.None;

            switch (crossAxisAlignment) {
                case CrossAxisAlignment.Unset:
                case CrossAxisAlignment.Start:
                    horizontalAlignment = new Alignment(0, 0, AlignmentTarget.AllocatedBox);
                    break;
                case CrossAxisAlignment.Center:
                    horizontalAlignment = new Alignment(0.5f, 0.5f, AlignmentTarget.AllocatedBox);
                    break;
                case CrossAxisAlignment.End:
                    horizontalAlignment = new Alignment(1f, 1f, AlignmentTarget.AllocatedBox);
                    break;
                case CrossAxisAlignment.Stretch:
                    horizontalFit = Fit.Grow;
                    break;
            }

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                growPieces += item.growFactor;
                shrinkPieces += item.shrinkFactor;
                FastLayoutBox child = item.layoutBox;
                child.GetWidth(block.width, ref item.size);
                child.GetMarginHorizontal(block.width, ref item.margin);
                item.outputWidth = item.size.prefWidth;
                float totalWidth = item.margin.left + item.size.prefWidth + item.margin.right;
                if (track.crossSize < totalWidth) {
                    track.crossSize = totalWidth;
                }
            }

            track.startItemIndex = 0;
            track.endItemIndex = itemList.size;
            track.crossSize = Mathf.Max(size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right, track.crossSize);

            for (int i = 0; i < itemList.size; i++) {
                ref Item item = ref items[i];
                FastLayoutBox child = item.layoutBox;
                child.GetHeight(track.crossSize - item.margin.left - item.margin.right, block.width, block.height, ref item.size);
                child.GetMarginVertical(block.height, ref item.margin);
                item.outputHeight = item.size.prefHeight;
                track.mainSize += item.margin.top + item.size.prefHeight + item.margin.bottom;
                child.ApplyHorizontalLayout(axisOffset + item.margin.left, block.width, track.crossSize - item.margin.left - item.margin.right, item.size.prefWidth, horizontalAlignment, horizontalFit);
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
                Fit fit = Fit.Unset;

                if (item.outputHeight > item.size.prefHeight && item.growFactor > 0) {
                    fit = Fit.Grow;
                }
                else if (item.outputHeight < item.size.prefHeight && item.shrinkFactor > 0) {
                    fit = Fit.Shrink;
                }

                item.layoutBox.ApplyVerticalLayout(item.mainAxisStart, block.height, item.outputHeight, item.size.prefHeight, default, fit);
            }

            contentSize.width = 0;
            contentSize.height = 0;
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

            while (allocate && (int) remainingSpace > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remainingSpace / pieces;

                for (int i = 0; i < itemList.size; i++) {
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

                for (int i = 0; i < itemList.size; i++) {
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

            MarkNeedsLayout();
        }

        protected override void OnChildAdded(FastLayoutBox child, int index) {
            if (firstChild == null) {
                firstChild = child;
                MarkNeedsLayout();
                return;
            }

            itemList = itemList ?? new StructList<Item>();

            itemList.Insert(index, new Item() {
                layoutBox = child,
                growFactor = child.element.style.FlexItemGrow,
                shrinkFactor = child.element.style.FlexItemShrink,
            });

            MarkNeedsLayout();
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
                MarkNeedsLayout();
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
                MarkNeedsLayout();
            }
        }

        public struct Track {

            public float mainSize;
            public float crossSize;
            public float remainingSpace;
            public int startItemIndex;
            public int endItemIndex;

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