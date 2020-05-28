using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Rendering;
using UIForia.Systems;
using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Mathematics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia.Layout {

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct FlexLayoutBoxBurst : ILayoutHandler {

        public LayoutDirection direction;
        public LayoutWrap layoutWrap;

        public float alignHorizontal;
        public LayoutFit fitHorizontal;
        public SpaceDistribution horizontalDistribution;

        public LayoutFit fitVertical;
        public float alignVertical;
        public SpaceDistribution verticalDistribution;

        public ElementId elementId;
        public List_FlexItem items;

        public void RunHorizontal(BurstLayoutRunner* burstLayoutSystem) {
            // if (flexItems.size == 0) return;
            if (layoutWrap == LayoutWrap.WrapHorizontal) {
                // RunLayoutHorizontalStep_HorizontalDirection_Wrapped(frameId);
            }
            else {
                RunLayoutHorizontalStep_HorizontalDirection(burstLayoutSystem);
            }

        }

        // where is shared layout info stored? layout system makes sense to own this

        public void RunLayoutHorizontalStep_HorizontalDirection(BurstLayoutRunner* burstLayoutSystem) {

            ref LayoutSizeInfo sizeInfo = ref burstLayoutSystem->GetHorizontalSizeInfo(elementId);
            ref LayoutMetaData metaData = ref burstLayoutSystem->GetLayoutMetaData(elementId);
            ref LayoutPassResult passResult = ref burstLayoutSystem->GetHorizontalLayoutResult(elementId);

            Track track = new Track {
                remaining = passResult.actualSize - (sizeInfo.paddingBorderStart + sizeInfo.paddingBorderEnd),
                startIndex = 0,
                endIndex = items.size
            };

            BlockSize blockSize = sizeInfo.parentBlockSize;

            if (metaData.widthBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = passResult.actualSize,
                    insetSize = passResult.actualSize - (sizeInfo.paddingBorderStart + sizeInfo.paddingBorderEnd)
                };
            }

            for (int i = 0; i < items.size; i++) {
                ref FlexItem item = ref items.array[i];
                item.horizontalFit = LayoutFit.None;
                burstLayoutSystem->GetWidths(this, blockSize, item.elementId, ref item.widthData);
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

            float inset = sizeInfo.paddingBorderStart;

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

                burstLayoutSystem->ApplyLayoutHorizontal(
                    item.elementId,
                    x,
                    alignedPosition,
                    item.baseWidth,
                    item.availableSize,
                    blockSize,
                    item.horizontalFit,
                    sizeInfo.finalSize
                );

                offset += item.widthData.marginStart + item.widthData.marginEnd + gap;
            }
        }

        private void GrowHorizontal(ref Track track) {
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
                        if (items.array[j].availableSize != items.array[j].widthData.maximum) {
                            pieces += items.array[j].growPieces;
                        }
                    }

                    if (pieces == 0) {
                        return;
                    }
                }
            }
        }

        private void ShrinkHorizontal(ref Track track) {
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
                        if (item.availableSize != item.widthData.minimum) {
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

        public float ComputeContentWidth(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            if (items.size == 0) return 0;
            return direction == LayoutDirection.Horizontal
                ? ComputeContentWidthHorizontal(ref layoutRunner, blockSize)
                : ComputeContentWidthVertical(ref layoutRunner, blockSize);
        }

        private float ComputeContentWidthHorizontal(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            float totalSize = 0;

            for (int i = 0; i < items.size; i++) {
                LayoutSize widths = default;
                layoutRunner.GetWidths(this, blockSize, items.array[i].elementId, ref widths);
                float baseSize = math.max(widths.minimum, math.min(widths.preferred, widths.maximum));
                totalSize += baseSize + widths.marginStart + widths.marginEnd;
            }

            return totalSize;
        }

        private float ComputeContentWidthVertical(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            float maxSize = 0;

            for (int i = 0; i < items.size; i++) {
                LayoutSize widths = default;
                layoutRunner.GetWidths(this, blockSize, items.array[i].elementId, ref widths);
                float baseSize = math.max(widths.minimum, math.min(widths.preferred, widths.maximum));
                float totalSize = baseSize + widths.marginStart + widths.marginEnd;
                if (totalSize > maxSize) {
                    maxSize = totalSize;
                }
            }

            return maxSize;
        }

        public static void Initialize_Managed(ref FlexLayoutBoxBurst layoutInfo, UIElement element) {
            layoutInfo.elementId = element.id;
            layoutInfo.direction = element.style.FlexLayoutDirection;
            layoutInfo.layoutWrap = element.style.FlexLayoutWrap;
            layoutInfo.alignHorizontal = element.style.AlignItemsHorizontal;
            layoutInfo.alignVertical = element.style.AlignItemsVertical;
            layoutInfo.fitHorizontal = element.style.FitItemsHorizontal;
            layoutInfo.fitVertical = element.style.FitItemsVertical;
            layoutInfo.horizontalDistribution = element.style.DistributeExtraSpaceHorizontal;
            layoutInfo.verticalDistribution = element.style.DistributeExtraSpaceVertical;
        }

        public static void OnChildrenChanged_Managed(ref FlexLayoutBoxBurst layoutInfo, ElementId elementId, LayoutSystem layoutSystem) {

            ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutSystem.elementSystem.layoutHierarchyTable[elementId];

            int childCount = layoutHierarchyInfo.childCount;

            if (childCount == 0) {
                layoutInfo.items.size = 0;
                return;
            }

            // todo -- use an allocator that makes sense for this and not persistent

            if (layoutInfo.items.capacity < childCount) {
                TypedUnsafe.Dispose(layoutInfo.items.array, Allocator.Persistent);
                layoutInfo.items.array = TypedUnsafe.Malloc<FlexItem>(layoutHierarchyInfo.childCount, Allocator.Persistent);
                layoutInfo.items.capacity = childCount;
            }

            layoutInfo.items.size = childCount;
            ElementId ptr = layoutHierarchyInfo.firstChildId;

            int idx = 0;
            while (ptr != default) {
                UIElement child = layoutSystem.elementSystem.instanceTable[ptr.index];
                layoutInfo.items.array[idx++] = new FlexItem() {
                    growPieces = child.style.FlexItemGrow,
                    shrinkPieces = child.style.FlexItemShrink,
                    elementId = child.id,
                };
                ptr = layoutSystem.elementSystem.layoutHierarchyTable[ptr].nextSiblingId;
            }

        }

        private struct Track {

            public int endIndex;
            public int startIndex;
            public float remaining;
            public float height;

            public Track(float remaining, int startIndex) {
                this.remaining = remaining;
                this.startIndex = startIndex;
                this.endIndex = startIndex;
                this.height = 0;
            }

            public bool IsEmpty => startIndex == endIndex;

        }

        public float ResolveAutoWidth(ElementId elementId, UIMeasurement measurement) {
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

    }

}