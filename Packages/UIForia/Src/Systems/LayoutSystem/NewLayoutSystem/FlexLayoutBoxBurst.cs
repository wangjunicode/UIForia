using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;

namespace UIForia.Layout {

    internal unsafe struct FlexLayoutBoxBurst : ILayoutBox {

        public ElementId elementId;

        public LayoutDirection direction;
        public LayoutWrap layoutWrap;

        public float alignHorizontal;
        public float alignVertical;

        public LayoutFit fitHorizontal;
        public LayoutFit fitVertical;

        public SpaceDistribution horizontalDistribution;
        public SpaceDistribution verticalDistribution;

        public List_FlexItem items;
        public List_FlexTrack wrappedTracks;
        
        private float finalContentWidth;
        private float finalContentHeight;
        
        public void RunHorizontal(BurstLayoutRunner* runner) {
            if (items.size == 0) {
                return;
            }

            if (direction == LayoutDirection.Horizontal) {
                if (layoutWrap == LayoutWrap.WrapHorizontal) {
                    RunLayoutHorizontalStep_HorizontalDirection_Wrapped(runner);
                }
                else {
                    RunLayoutHorizontalStep_HorizontalDirection(runner);
                }
            }
            else {
                RunLayoutHorizontalStep_VerticalDirection(runner);
            }

        }

        public void RunVertical(BurstLayoutRunner* runner) {
            if (items.size == 0) return;

            if (direction == LayoutDirection.Horizontal) {
                if (layoutWrap == LayoutWrap.WrapHorizontal) {
                    RunLayoutVerticalStep_HorizontalDirection_Wrapped(runner);
                }
                else {
                    RunLayoutVerticalStep_HorizontalDirection(runner);
                }
            }
            else {
                RunLayoutVerticalStep_VerticalDirection(runner);
            }
        }

        private void RunLayoutHorizontalStep_HorizontalDirection_Wrapped(BurstLayoutRunner* runner) {

            ref LayoutInfo info = ref runner->GetHorizontalLayoutInfo(elementId);

            BlockSize blockSize = info.parentBlockSize;

            if (info.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = info.finalSize,
                    insetSize = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd)
                };
            }

            float contentAreaWidth = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd);

            FlexTrack currentTrack = new FlexTrack(contentAreaWidth, 0);
            // wrappedTracks = wrappedTracks ?? new StructList<FlexTrack>(4);
            wrappedTracks.size = 0;
            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.verticalFit = LayoutFit.None;
                runner->GetWidths(this, blockSize, item.elementId, out item.widthData);
                item.baseWidth = item.widthData.Clamped;
                item.availableSize = item.baseWidth;
                // available size is equal to base size but we need to take up base size + margin in the layout
                float itemSize = item.baseWidth + item.widthData.marginStart + item.widthData.marginEnd;

                // if item is bigger than content area, stop current track if not empty
                if (itemSize >= contentAreaWidth) {
                    // stop current track
                    if (!currentTrack.IsEmpty) {
                        currentTrack.endIndex = i;
                        wrappedTracks.Add(currentTrack);
                        currentTrack = new FlexTrack(contentAreaWidth, i);
                        currentTrack.remaining -= itemSize;
                        currentTrack.endIndex = i + 1;
                        wrappedTracks.Add(currentTrack);
                        currentTrack = new FlexTrack(contentAreaWidth, i + 1);
                    }
                    else {
                        currentTrack.endIndex = i + 1;
                        currentTrack.remaining -= itemSize;
                        wrappedTracks.Add(currentTrack);
                        currentTrack = new FlexTrack(contentAreaWidth, i + 1);
                    }
                }
                else if (currentTrack.remaining - itemSize == 0) {
                    currentTrack.remaining -= itemSize;
                    currentTrack.endIndex = i + 1;
                    wrappedTracks.Add(currentTrack);
                    currentTrack = new FlexTrack(contentAreaWidth, i + 1);
                }
                else if (currentTrack.remaining - itemSize < 0) {
                    currentTrack.endIndex = i;
                    wrappedTracks.Add(currentTrack);
                    currentTrack = new FlexTrack(contentAreaWidth, i);
                    currentTrack.remaining -= itemSize;
                    currentTrack.endIndex = i + 1;
                }
                else {
                    currentTrack.remaining -= itemSize;
                    currentTrack.endIndex = i + 1;
                }
            }

            if (!currentTrack.IsEmpty) {
                wrappedTracks.Add(currentTrack);
            }

            float itemAlignment = alignHorizontal;
            float gap = 0; // something like element.style.GridLayoutColGap might be interesting here
            SpaceDistribution distribution = horizontalDistribution;

            if (distribution == SpaceDistribution.Default) {
                distribution = SpaceDistribution.AfterContent;
            }

            for (int trackIndex = 0; trackIndex < wrappedTracks.size; trackIndex++) {
                ref FlexTrack track = ref wrappedTracks.array[trackIndex];

                if (track.remaining > 0) {
                    GrowHorizontal(ref track);
                }
                else if (track.remaining < 0) {
                    ShrinkHorizontal(ref track);
                }

                SpaceDistributionUtil.GetAlignmentOffsets(track.remaining, track.endIndex - track.startIndex, distribution, out float offset, out float spacerSize);

                int startIdx = track.startIndex;
                int endIdx = track.endIndex;

                for (int i = startIdx; i < endIdx; i++) {
                    ref FlexItem item = ref items.array[i];
                    float x = info.paddingBorderStart + offset + item.widthData.marginStart;
                    offset += item.availableSize + spacerSize;

                    float elementWidth = item.baseWidth;
                    float originOffset = item.availableSize * itemAlignment;
                    float alignedPosition = x + originOffset + (elementWidth * -itemAlignment);

                    runner->ApplyLayoutHorizontal(
                        item.elementId,
                        x,
                        alignedPosition,
                        item.baseWidth,
                        item.availableSize,
                        blockSize,
                        item.horizontalFit,
                        info.finalSize
                    );
                    offset += item.widthData.marginStart + item.widthData.marginEnd + gap;
                }

            }

            // MarkForLayoutVertical();
        }

        private void RunLayoutVerticalStep_HorizontalDirection_Wrapped(BurstLayoutRunner* runner) {
            ref LayoutInfo info = ref runner->GetVerticalLayoutInfo(elementId);

            BlockSize blockSize = info.parentBlockSize;

            if (info.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = info.finalSize,
                    insetSize = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd)
                };
            }

            float contentStartY = info.paddingBorderStart;

            float verticalAlignment = alignVertical;

            float remainingHeight = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd);
            SpaceDistribution spaceDistribution = verticalDistribution;

            for (int i = 0; i < wrappedTracks.size; i++) {
                ref FlexTrack track = ref wrappedTracks.array[i];

                float maxHeight = 0;

                for (int j = track.startIndex; j < track.endIndex; j++) {
                    ref FlexItem item = ref items.array[j];

                    runner->GetHeights(this, blockSize, item.elementId, out item.heightData);

                    item.baseHeight = item.heightData.Clamped;

                    float height = item.baseHeight + item.heightData.marginStart + item.heightData.marginEnd;

                    if (height > maxHeight) {
                        maxHeight = height;
                    }
                }

                track.height = maxHeight;
                remainingHeight -= maxHeight;
            }

            SpaceDistributionUtil.GetAlignmentOffsets(remainingHeight, wrappedTracks.size, spaceDistribution, out float offset, out float spacerSize);

            for (int i = 0; i < wrappedTracks.size; i++) {
                ref FlexTrack track = ref wrappedTracks.array[i];

                float height = track.height;
                float y = contentStartY + offset;
                offset += height + spacerSize;

                for (int j = track.startIndex; j < track.endIndex; j++) {
                    ref FlexItem item = ref items.array[j];

                    float allocatedHeight = height - (item.heightData.marginStart + item.heightData.marginEnd);
                    float originBase = y + item.heightData.marginStart;
                    float originOffset = allocatedHeight * verticalAlignment;

                    runner->ApplyLayoutVertical(
                        item.elementId,
                        y + item.heightData.marginStart,
                        originBase + originOffset + (item.baseHeight * -verticalAlignment),
                        item.baseHeight,
                        allocatedHeight,
                        blockSize,
                        fitVertical,
                        info.finalSize
                    );

                }
            }
        }

        private void RunLayoutHorizontalStep_HorizontalDirection(BurstLayoutRunner* runner) {

            ref LayoutInfo info = ref runner->GetHorizontalLayoutInfo(elementId);

            FlexTrack track = new FlexTrack {
                remaining = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd),
                startIndex = 0,
                endIndex = items.size
            };

            BlockSize blockSize = info.parentBlockSize;

            if (info.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = info.finalSize,
                    insetSize = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd)
                };
            }

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.horizontalFit = LayoutFit.None;
                runner->GetWidths(this, blockSize, item.elementId, out item.widthData);
                item.baseWidth = item.widthData.Clamped;
                item.availableSize = item.baseWidth;
                // available size is equal to base size but we need to take up base size + margin in the layout
                track.remaining -= item.baseWidth + item.widthData.marginStart + item.widthData.marginEnd;
            }

            if (track.remaining > 0) {
                GrowHorizontal(ref track);
            }
            else if (track.remaining < 0) {
                ShrinkHorizontal(ref track);
            }

            float inset = info.paddingBorderStart;

            SpaceDistribution alignment = horizontalDistribution;

            if (alignment == SpaceDistribution.Default) {
                alignment = SpaceDistribution.AfterContent;
            }

            SpaceDistributionUtil.GetAlignmentOffsets(track.remaining, track.endIndex - track.startIndex, alignment, out float offset, out float spacerSize);

            float gap = 0; //element.style.GridLayoutColGap;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                float x = inset + offset + item.widthData.marginStart;
                offset += item.availableSize + spacerSize;

                float elementWidth = item.baseWidth;
                float originOffset = item.availableSize * alignHorizontal;
                float alignedPosition = x + originOffset + (elementWidth * -alignHorizontal);

                runner->ApplyLayoutHorizontal(
                    item.elementId,
                    x,
                    alignedPosition,
                    item.baseWidth,
                    item.availableSize,
                    blockSize,
                    item.horizontalFit,
                    info.finalSize
                );

                offset += item.widthData.marginStart + item.widthData.marginEnd + gap;
            }
        }

        private void RunLayoutHorizontalStep_VerticalDirection(BurstLayoutRunner* runner) {

            ref LayoutInfo info = ref runner->GetHorizontalLayoutInfo(elementId);

            BlockSize blockSize = info.parentBlockSize;
            float contentStartX = info.paddingBorderStart;

            float adjustedWidth = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd);

            float inset = info.paddingBorderStart;

            SpaceDistribution alignment = horizontalDistribution;

            if (alignment == SpaceDistribution.Default) {
                alignment = SpaceDistribution.AfterContent;
            }

            if (info.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = info.finalSize,
                    insetSize = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd)
                };
            }

            float itemAlignment = alignVertical;
            //float gap = 0; //element.style.GridLayoutColGap;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                runner->GetWidths(this, blockSize, item.elementId, out item.widthData);
                item.baseWidth = item.widthData.Clamped;

                float remaining = adjustedWidth - item.baseWidth - (item.widthData.marginStart + item.widthData.marginEnd);

                SpaceDistributionUtil.GetAlignmentOffsets(remaining, 1, alignment, out float offset, out float _);

                float x = inset + offset;

                float availableWidth = adjustedWidth - (item.widthData.marginStart + item.widthData.marginEnd);
                float originBase = contentStartX + item.widthData.marginStart;
                float originOffset = (x - inset) + availableWidth * itemAlignment;
                float alignedPosition = originBase + originOffset + (item.baseWidth * -itemAlignment);

                runner->ApplyLayoutHorizontal(
                    item.elementId,
                    x + item.widthData.marginStart,
                    alignedPosition,
                    item.baseWidth,
                    availableWidth,
                    blockSize,
                    fitHorizontal,
                    info.finalSize
                );
            }
        }

        private void RunLayoutVerticalStep_HorizontalDirection(BurstLayoutRunner* runner) {

            ref LayoutInfo info = ref runner->GetVerticalLayoutInfo(elementId);
            float adjustedHeight = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd);

            BlockSize blockSize = info.parentBlockSize;

            if (info.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = info.finalSize,
                    insetSize = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd)
                };
            }

            // todo -- once tracks get implemented we want to be able to use space-between and space-around to vertically align the tracks
            // track heights are equal to the max height of all contents in that track
            // when we only have 1 track the allocated height for every box is the full height of this element's content area and align content vertical does nothing

            // MainAxisAlignment contentAlignment = element.style.AlignContentVertical;

            // 2 options for horizontal height sizing:
            // 1. use the full content area height of this element
            // 2. use the tallest child's height + margin
            // if using the tallest child then we can center/start/end as normal
            // however it might be awkward if AlignContentVertical = Center is used and that just centers the allocated rects, not the actual content
            // so for now I have implemented option 1 for the case where we are not wrapping / only have 1 track

            float itemAlignment = alignVertical;
            LayoutFit verticalLayoutFit = fitVertical;

            SpaceDistribution distribution = verticalDistribution;

            if (distribution == SpaceDistribution.Default) {
                distribution = SpaceDistribution.AfterContent;
            }

            float inset = info.paddingBorderStart;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                runner->GetHeights(this, blockSize, item.elementId, out item.heightData);
                item.baseHeight = item.heightData.Clamped;
                float clampedWithMargin = item.baseHeight + item.heightData.marginStart + item.heightData.marginEnd;

                SpaceDistributionUtil.GetAlignmentOffsets(adjustedHeight - (clampedWithMargin), 1, distribution, out float offset, out _);

                float y = inset + offset + item.heightData.marginStart;
                float allocatedHeight = adjustedHeight - (item.heightData.marginStart + item.heightData.marginEnd);
                float originOffset = allocatedHeight * itemAlignment;
                float alignedPosition = y + originOffset + (item.baseHeight * -itemAlignment);

                runner->ApplyLayoutVertical(
                    item.elementId,
                    y,
                    alignedPosition,
                    item.baseHeight,
                    allocatedHeight,
                    blockSize,
                    verticalLayoutFit,
                    info.finalSize
                );
            }
        }

        private void RunLayoutVerticalStep_VerticalDirection(BurstLayoutRunner* runner) {
            ref LayoutInfo info = ref runner->GetVerticalLayoutInfo(elementId);
            float adjustedHeight = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd);

            BlockSize blockSize = info.parentBlockSize;

            if (info.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = info.finalSize,
                    insetSize = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd)
                };
            }

            float remaining = adjustedHeight;
            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                runner->GetHeights(this, blockSize, item.elementId, out item.heightData);
                item.baseHeight = item.heightData.Clamped;
                item.availableSize = item.baseHeight;
                remaining -= item.baseHeight + item.heightData.marginStart + item.heightData.marginEnd;
            }

            if (remaining > 0) {
                remaining = GrowVertical(remaining);
            }
            else if (remaining < 0) {
                remaining = ShrinkVertical(remaining);
            }

            float inset = info.paddingBorderStart;

            SpaceDistribution distribution = verticalDistribution;
            if (distribution == default) {
                distribution = SpaceDistribution.AfterContent;
            }

            SpaceDistributionUtil.GetAlignmentOffsets(remaining, items.size, distribution, out float offset, out float spacerSize);
            float itemAlignment = alignVertical;
            float gap = 0; //element.style.GridLayoutColGap;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                float y = inset + item.heightData.marginStart + offset;
                offset += item.availableSize + spacerSize;

                float elementHeight = item.baseHeight;
                float originOffset = item.availableSize * itemAlignment;
                float alignedPosition = y + originOffset + (elementHeight * -itemAlignment);

                runner->ApplyLayoutVertical(
                    item.elementId,
                    y,
                    alignedPosition,
                    item.baseHeight,
                    item.availableSize,
                    blockSize,
                    item.verticalFit,
                    info.finalSize
                );
                offset += item.heightData.marginStart + item.heightData.marginEnd + gap;
            }

            finalContentHeight = offset - gap;

        }

        public float GetActualContentWidth(ref BurstLayoutRunner runner) {
            return 0;
        }

        public float GetActualContentHeight(ref BurstLayoutRunner runner) {
            return finalContentHeight;
        }
        
        private void GrowHorizontal(ref FlexTrack track) {
            int pieces = 0;

            for (int i = track.startIndex; i < track.endIndex; i++) {
                pieces += items.array[i].growPieces;
                items.array[i].horizontalFit = LayoutFit.None;
            }

            bool allocate = pieces > 0;
            while (allocate && (int) track.remaining > 0) {
                allocate = false;

                float pieceSize = track.remaining / pieces;

                bool recomputePieces = false;
                for (int i = track.startIndex; i < track.endIndex; i++) {
                    ref FlexItem item = ref items.array[i];
                    float max = item.widthData.maximum;
                    float output = item.availableSize;
                    int growthFactor = item.growPieces;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    item.horizontalFit = LayoutFit.Grow;
                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    if (totalGrowth >= max) {
                        output = max;
                        recomputePieces = true;
                    }
                    else {
                        output = totalGrowth;
                    }

                    track.remaining -= output - start;
                    item.availableSize = output;
                }

                if (recomputePieces) {
                    pieces = 0;
                    for (int j = track.startIndex; j < track.endIndex; j++) {
                        if (items.array[j].availableSize < items.array[j].widthData.maximum) {
                            pieces += items.array[j].growPieces;
                        }
                    }

                    if (pieces == 0) {
                        return;
                    }
                }
            }
        }

        private void ShrinkHorizontal(ref FlexTrack track) {
            int startIndex = track.startIndex;
            int endIndex = track.endIndex;
            int pieces = 0;

            for (int i = startIndex; i < endIndex; i++) {
                pieces += items.array[i].shrinkPieces;
            }

            float overflow = -track.remaining;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;
                bool recomputePieces = false;

                for (int i = startIndex; i < endIndex; i++) {
                    ref FlexItem item = ref items.array[i];
                    float min = item.widthData.minimum;
                    float output = item.availableSize;
                    int shrinkFactor = item.shrinkPieces;

                    if (shrinkFactor == 0 || (int) output == (int) min || (int) output == 0) {
                        continue;
                    }

                    allocate = true;
                    item.horizontalFit = LayoutFit.Shrink;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    if (totalShrink <= min) {
                        output = min;
                        recomputePieces = true;
                    }
                    else {
                        output = totalShrink;
                    }

                    overflow += output - start;
                    item.availableSize = output;
                }

                if (recomputePieces) {
                    pieces = 0;
                    for (int j = track.startIndex; j < track.endIndex; j++) {
                        ref FlexItem item = ref items.array[j];
                        if (item.availableSize > item.widthData.minimum) {
                            pieces += item.shrinkPieces;
                        }
                    }

                    if (pieces == 0) {
                        break;
                    }
                }
            }

            track.remaining = -overflow;
        }

        private float GrowVertical(float remaining) {
            int pieces = 0;

            for (int i = 0; i < items.size; i++) {
                pieces += items.array[i].growPieces;
                items.array[i].verticalFit = LayoutFit.None;
            }

            bool allocate = pieces > 0;
            while (allocate && (int) remaining > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remaining / pieces;
                bool recomputePieces = false;

                for (int i = 0; i < items.size; i++) {
                    ref FlexItem item = ref items.array[i];
                    float max = item.heightData.maximum;
                    float output = item.availableSize;
                    int growthFactor = item.growPieces;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    item.verticalFit = LayoutFit.Grow;
                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    if (totalGrowth >= max) {
                        output = max;
                        recomputePieces = true;
                    }
                    else {
                        output = totalGrowth;
                    }

                    remaining -= output - start;
                    item.availableSize = output;
                }

                if (recomputePieces) {
                    pieces = 0;
                    for (int j = 0; j < items.size; j++) {
                        if (items.array[j].availableSize != items.array[j].heightData.maximum) {
                            pieces += items.array[j].growPieces;
                        }
                    }

                    if (pieces == 0) {
                        return remaining;
                    }
                }
            }

            return remaining;
        }

        private float ShrinkVertical(float remaining) {
            int pieces = 0;

            for (int i = 0; i < items.size; i++) {
                pieces += items.array[i].shrinkPieces;
                items.array[i].verticalFit = LayoutFit.None;
            }

            float overflow = -remaining;

            bool allocate = pieces > 0;
            while (allocate && (int) overflow > 0) {
                allocate = false;

                float pieceSize = overflow / pieces;
                bool recomputePieces = false;

                for (int i = 0; i < items.size; i++) {
                    ref FlexItem item = ref items.array[i];
                    float min = item.heightData.minimum;
                    float output = item.availableSize;
                    int shrinkFactor = item.shrinkPieces;

                    if (shrinkFactor == 0 || (int) output == (int) min || (int) output == 0) {
                        continue;
                    }

                    allocate = true;
                    item.verticalFit = LayoutFit.Shrink;
                    float start = output;
                    float shrinkSize = shrinkFactor * pieceSize;
                    float totalShrink = output - shrinkSize;
                    if (totalShrink <= min) {
                        output = min;
                        recomputePieces = true;
                    }
                    else {
                        output = totalShrink;
                    }

                    overflow += output - start;
                    item.availableSize = output;
                }

                if (recomputePieces) {
                    pieces = 0;
                    for (int j = 0; j < items.size; j++) {
                        ref FlexItem item = ref items.array[j];
                        if (item.availableSize != item.heightData.minimum) {
                            pieces += item.shrinkPieces;
                        }
                    }

                    if (pieces == 0) {
                        break;
                    }
                }
            }

            return -overflow;
        }

        public float ComputeContentWidth(ref BurstLayoutRunner runner, in BlockSize blockSize) {
            if (items.size == 0) return 0;
            return direction == LayoutDirection.Horizontal
                ? ComputeContentWidthHorizontal(ref runner, blockSize)
                : ComputeContentWidthVertical(ref runner, blockSize);
        }

        private float ComputeContentWidthHorizontal(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            float totalSize = 0;

            for (int i = 0; i < items.size; i++) {
                layoutRunner.GetWidths(this, blockSize, items.array[i].elementId, out LayoutSize widths);
                float baseSize = math.max(widths.minimum, math.min(widths.preferred, widths.maximum));
                totalSize += baseSize + widths.marginStart + widths.marginEnd;
            }

            return totalSize;
        }

        private float ComputeContentWidthVertical(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            float maxSize = 0;

            for (int i = 0; i < items.size; i++) {
                layoutRunner.GetWidths(this, blockSize, items.array[i].elementId, out LayoutSize widths);
                float baseSize = math.max(widths.minimum, math.min(widths.preferred, widths.maximum));
                float totalSize = baseSize + widths.marginStart + widths.marginEnd;
                if (totalSize > maxSize) {
                    maxSize = totalSize;
                }
            }

            return maxSize;
        }

        public float ComputeContentHeight(ref BurstLayoutRunner runner, in BlockSize blockSize) {
            if (items.size == 0) return 0;
            return direction == LayoutDirection.Horizontal
                ? ComputeContentHeightHorizontal(ref runner, blockSize)
                : ComputeContentHeightVertical(ref runner, blockSize);
        }

        private float ComputeContentHeightHorizontal(ref BurstLayoutRunner runner, BlockSize blockSize) {

            if (wrappedTracks.array != null && wrappedTracks.size != 0) {
                float retn = 0;
                for (int i = 0; i < wrappedTracks.size; i++) {
                    ref FlexTrack track = ref wrappedTracks.array[i];

                    float maxHeight = 0;

                    for (int j = track.startIndex; j < track.endIndex; j++) {
                        ref FlexItem item = ref items.array[j];
                        runner.GetHeights(this, blockSize, item.elementId, out item.heightData);
                        item.baseHeight = item.heightData.Clamped;

                        if (item.baseHeight + item.heightData.marginStart + item.heightData.marginEnd > maxHeight) {
                            maxHeight = item.baseHeight + item.heightData.marginStart + item.heightData.marginEnd;
                        }
                    }

                    retn += maxHeight;
                }

                return retn;
            }

            float maxSize = 0;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                runner.GetHeights(this, blockSize, item.elementId, out item.heightData);
                float totalSize = item.heightData.ClampedWithMargin;
                if (totalSize > maxSize) {
                    maxSize = totalSize;
                }
            }

            return maxSize;
        }

        private float ComputeContentHeightVertical(ref BurstLayoutRunner runner, BlockSize blockSize) {
            float totalSize = 0;

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                runner.GetHeights(this, blockSize, item.elementId, out item.heightData);
                totalSize += item.heightData.ClampedWithMargin;
            }

            return totalSize;
        }

        public void OnInitialize(LayoutSystem layoutSystem, UIElement element) {
            elementId = element.id;
            direction = element.style.FlexLayoutDirection;
            layoutWrap = element.style.FlexLayoutWrap;
            alignHorizontal = element.style.AlignItemsHorizontal;
            alignVertical = element.style.AlignItemsVertical;
            fitHorizontal = element.style.FitItemsHorizontal;
            fitVertical = element.style.FitItemsVertical;
            horizontalDistribution = element.style.DistributeExtraSpaceHorizontal;
            verticalDistribution = element.style.DistributeExtraSpaceVertical;

            if (layoutWrap == LayoutWrap.WrapHorizontal) {
                wrappedTracks = new List_FlexTrack(8, Allocator.Persistent);
            }
        }

        public void OnChildrenChanged(LayoutSystem layoutSystem) {

            ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutSystem.layoutHierarchyTable[elementId];

            int childCount = layoutHierarchyInfo.childCount;

            if (childCount == 0) {
                items.size = 0;
                return;
            }

            // todo -- use an allocator that makes sense for this and not persistent

            if (items.capacity < childCount) {
                TypedUnsafe.Dispose(items.array, Allocator.Persistent);
                items.array = TypedUnsafe.Malloc<FlexItem>(layoutHierarchyInfo.childCount * 2, Allocator.Persistent);
                items.capacity = childCount * 2;
            }

            items.size = childCount;
            ElementId ptr = layoutHierarchyInfo.firstChildId;

            int idx = 0;
            while (ptr != default) {
                UIElement child = layoutSystem.elementSystem.instanceTable[ptr.index];
                items.array[idx++] = new FlexItem() {
                    growPieces = child.style.FlexItemGrow,
                    shrinkPieces = child.style.FlexItemShrink,
                    elementId = child.id,
                };
                ptr = layoutSystem.layoutHierarchyTable[ptr].nextSiblingId;
            }

        }

        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, UIMeasurement measurement, in BlockSize blockSize) {
            return 0;
        }

        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, UIMeasurement measurement, in BlockSize blockSize) {
            return 0;
        }

        [AssertSize(16)]
        [StructLayout(LayoutKind.Explicit)]
        [DebuggerTypeProxy(typeof(ListDebugView<FlexItem>))]
        public unsafe struct List_FlexItem : IListInterface {

            [FieldOffset(0)] public FlexItem* array;
            [FieldOffset(8)] public int size;
            [FieldOffset(12)] public int capacity;

            public ref ListInterface GetListInterface() {
                void* x = UnsafeUtility.AddressOf(ref this);
                return ref UnsafeUtilityEx.AsRef<ListInterface>(x);
            }

            public int ItemSize {
                get => sizeof(FlexItem);
            }

        }

        // todo --
        // if I was really desperate to optimize something I could split the buffer holding items
        // into width & height and compute a pointer to use as the items array.
        public struct FlexItem {

            public LayoutSize widthData;
            public LayoutSize heightData;
            public int growPieces;
            public int shrinkPieces;
            public float baseWidth;
            public float baseHeight;
            public float availableSize;
            public ElementId elementId;
            public LayoutFit horizontalFit;
            public LayoutFit verticalFit;

        }

        public void Dispose() {
            TypedUnsafe.Dispose(items.array, Allocator.Persistent);
            TypedUnsafe.Dispose(wrappedTracks.array, Allocator.Persistent);
            this = default;
        }

        public void OnStylePropertiesChanged(LayoutSystem layoutSystem, UIElement element, StyleProperty[] properties, int propertyCount) {
            // todo -- a lot of these won't require a full layout, optimize this later to just do alignment / etc
            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref properties[i];
                switch (property.propertyId) {
                    case StylePropertyId.FlexLayoutWrap: {
                        layoutWrap = property.AsLayoutWrap;
                        if (layoutWrap == LayoutWrap.WrapHorizontal && wrappedTracks.array == null) {
                            wrappedTracks = new List_FlexTrack(8, Allocator.Persistent);
                        }

                        break;
                    }

                    case StylePropertyId.FlexLayoutDirection:
                        direction = property.AsLayoutDirection;
                        break;

                    case StylePropertyId.DistributeExtraSpaceHorizontal:
                        horizontalDistribution = property.AsSpaceDistribution;
                        break;

                    case StylePropertyId.DistributeExtraSpaceVertical:
                        verticalDistribution = property.AsSpaceDistribution;
                        break;

                    case StylePropertyId.AlignItemsHorizontal:
                        alignHorizontal = property.AsFloat;
                        break;

                    case StylePropertyId.AlignItemsVertical:
                        alignVertical = property.AsFloat;
                        break;

                    case StylePropertyId.FitItemsHorizontal:
                        fitHorizontal = property.AsLayoutFit;
                        break;

                    case StylePropertyId.FitItemsVertical:
                        fitVertical = property.AsLayoutFit;
                        break;
                }
            }
        }

        public void OnChildStyleChanged(LayoutSystem layoutSystem, ElementId childId, StyleProperty[] properties, int propertyCount) {
            int idx = -1;
            for (int i = 0; i < items.size; i++) {
                if (items.array[i].elementId == childId) {
                    idx = i;
                    break;

                }
            }

            if (idx < 0) {
                return;
            }

            ref FlexItem item = ref items.array[idx];
            for (int i = 0; i < propertyCount; i++) {
                if (properties[i].propertyId == StylePropertyId.FlexItemGrow) {
                    item.growPieces = properties[i].AsInt;
                }
                else if (properties[i].propertyId == StylePropertyId.FlexItemShrink) {
                    item.shrinkPieces = properties[i].AsInt;
                }
            }
        }

        
    }

    public struct FlexTrack {

        public int endIndex;
        public int startIndex;
        public float remaining;
        public float height;

        public FlexTrack(float remaining, int startIndex) {
            this.remaining = remaining;
            this.startIndex = startIndex;
            this.endIndex = startIndex;
            this.height = 0;
        }

        public bool IsEmpty {
            get => startIndex == endIndex;
        }

    }

}