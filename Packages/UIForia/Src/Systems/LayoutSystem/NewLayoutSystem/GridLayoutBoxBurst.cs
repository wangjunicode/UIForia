using System.Runtime.InteropServices;
using UIForia.Elements;
using UIForia.Layout.LayoutTypes;
using UIForia.ListTypes;
using UIForia.Rendering;
using UIForia.Systems;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using Unity.Mathematics;

namespace UIForia.Layout {

    [StructLayout(LayoutKind.Sequential)]
    internal unsafe struct GridLayoutBoxBurst : ILayoutBox {

        public ElementId elementId;

        // technically could merge into 1 buffer w/ track info
        public List_GridPlacement placementList;

        // can still be merged into 1 buffer
        public List_GridTrack rowTrackList;
        public List_GridTrack colTrackList;

        public float colGap;
        public float rowGap;
        public float horizontalAlignment;
        public float verticalAlignment;
        public LayoutFit fitHorizontal;
        public LayoutFit fitVertical;

        public float ResolveAutoWidth(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return 0;
        }

        public float ResolveAutoHeight(ref BurstLayoutRunner runner, ElementId elementId, in BlockSize blockSize) {
            return 0;
        }

        public void RunHorizontal(BurstLayoutRunner* prunner) {
            if (placementList.size == 0) {
                return;
            }

            ref BurstLayoutRunner runner = ref UnsafeUtilityEx.AsRef<BurstLayoutRunner>(prunner);

            ref LayoutInfo info = ref runner.GetHorizontalLayoutInfo(elementId);

            BlockSize blockSize = info.parentBlockSize;

            if (info.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = info.finalSize,
                    insetSize = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd)
                };
            }

            ComputeHorizontalFixedTrackSizes(ref runner, blockSize);

            ComputeItemWidths(ref runner, blockSize);

            // now compute the intrinsic sizes
            ComputeContentWidthContributionSizes();

            ResolveTrackSizes(colTrackList);

            float contentWidth = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd);

            float retn = 0;

            // this ignores fr sizes on purpose, fr size is always 0 for content size
            for (int i = 0; i < colTrackList.size; i++) {
                ref GridTrack track = ref colTrackList[i];
                track.size = track.resolvedBaseSize;
                retn += track.size;
            }

            retn += colGap * (colTrackList.size - 1);

            float remaining = contentWidth - retn;

            // todo -- do something with 'remaining'
            if (remaining > 0) {
                remaining = Grow(ref colTrackList, remaining);
            }
            else if (remaining < 0) {
                remaining = Shrink(ref colTrackList, remaining);
            }

            PositionTracks(ref colTrackList, colGap, info.paddingBorderStart);

            float alignment = horizontalAlignment;

            for (int i = 0; i < placementList.size; i++) {

                ref GridPlacement placement = ref placementList[i];
                ref GridTrack startTrack = ref colTrackList[placement.x];
                ref GridTrack endTrack = ref colTrackList[placement.x + placement.width - 1];

                float elementWidth = placement.outputWidth - placement.widthData.marginStart - placement.widthData.marginEnd;
                float x = startTrack.position;
                float layoutBoxWidth = (endTrack.position + endTrack.size) - x;

                //+ (placement.widthData.marginStart - placement.widthData.marginEnd);

                float originBase = x + placement.widthData.marginStart;
                float originOffset = layoutBoxWidth * alignment;
                float offset = elementWidth * -alignment;
                float alignedPosition = originBase + originOffset + offset;

                runner.ApplyLayoutHorizontal(
                    placement.elementId,
                    x,
                    alignedPosition,
                    elementWidth,
                    layoutBoxWidth,
                    ComputeHorizontalBlockSize(blockSize, i),
                    fitHorizontal,
                    info.finalSize
                );
            }
        }

        public void RunVertical(BurstLayoutRunner* prunner) {
            if (placementList.size == 0) {
                return;
            }

            ref BurstLayoutRunner runner = ref UnsafeUtilityEx.AsRef<BurstLayoutRunner>(prunner);

            ref LayoutInfo info = ref runner.GetVerticalLayoutInfo(elementId);

            BlockSize blockSize = info.parentBlockSize;

            if (info.isBlockProvider) { // or layout is constrained via fit 
                blockSize = new BlockSize() {
                    outerSize = info.finalSize,
                    insetSize = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd)
                };
            }

            ComputeVerticalFixedTrackSizes(ref runner, blockSize);

            ComputeItemHeights(ref runner, blockSize);

            // now compute the intrinsic sizes
            ComputeContentHeightContributionSizes();

            ResolveTrackSizes(rowTrackList);

            float contentHeight = info.finalSize - (info.paddingBorderStart + info.paddingBorderEnd);

            float retn = 0;

            // this ignores fr sizes on purpose, fr size is always 0 for content size
            for (int i = 0; i < rowTrackList.size; i++) {
                ref GridTrack track = ref rowTrackList[i];
                track.size = track.resolvedBaseSize;
                retn += track.size;
            }

            retn += rowGap * (rowTrackList.size - 1);

            float remaining = contentHeight - retn;

            if (remaining > 0) {
                remaining = Grow(ref rowTrackList, remaining);
            }
            else if (remaining < 0) {
                remaining = Shrink(ref rowTrackList, remaining);
            }

            PositionTracks(ref rowTrackList, rowGap, info.paddingBorderStart);

            float alignment = verticalAlignment;

            for (int i = 0; i < placementList.size; i++) {

                ref GridPlacement placement = ref placementList[i];
                ref GridTrack startTrack = ref rowTrackList[placement.y];
                ref GridTrack endTrack = ref rowTrackList[placement.y + placement.height - 1];

                float elementHeight = placement.outputHeight - placement.heightData.marginStart - placement.heightData.marginEnd;
                float y = startTrack.position;
                float layoutBoxHeight = (endTrack.position + endTrack.size) - y;

                float originBase = y + placement.heightData.marginStart;
                float originOffset = layoutBoxHeight * alignment;
                float offset = elementHeight * -alignment;
                float alignedPosition = originBase + originOffset + offset;

                runner.ApplyLayoutVertical(
                    placement.elementId,
                    y,
                    alignedPosition,
                    elementHeight,
                    layoutBoxHeight,
                    ComputeVerticalBlockSize(blockSize, i),
                    fitVertical,
                    info.finalSize
                );

            }

        }

        private BlockSize ComputeHorizontalBlockSize(in BlockSize originalBlockSize, int placementIndex) {
            ref GridPlacement placement = ref placementList[placementIndex];

            float blockSize = 0;
            for (int x = placement.x; x < placement.x + placement.width; x++) {
                ref GridTrack track = ref colTrackList[x];

                GridTemplateUnit flags = track.cellDefinition.baseSize.unit | track.cellDefinition.shrinkLimit.unit | track.cellDefinition.growLimit.unit;
                if ((flags & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                    return originalBlockSize;
                }

                blockSize += track.size;
            }

            return new BlockSize(blockSize, blockSize);
        }

        private BlockSize ComputeVerticalBlockSize(in BlockSize originalBlockSize, int placementIndex) {
            ref GridPlacement placement = ref placementList[placementIndex];

            float blockSize = 0;
            for (int y = placement.y; y < placement.y + placement.height; y++) {
                ref GridTrack track = ref rowTrackList[y];

                GridTemplateUnit flags = track.cellDefinition.baseSize.unit | track.cellDefinition.shrinkLimit.unit | track.cellDefinition.growLimit.unit;
                if ((flags & (GridTemplateUnit.MaxContent | GridTemplateUnit.MinContent)) != 0) {
                    return originalBlockSize;
                }

                blockSize += track.size;
            }

            return new BlockSize(blockSize, blockSize);
        }

        public float ComputeContentWidth(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            // first pass, get fixed sizes
            ComputeHorizontalFixedTrackSizes(ref layoutRunner, blockSize);

            ComputeItemWidths(ref layoutRunner, blockSize);

            // now compute the intrinsic sizes
            ComputeContentWidthContributionSizes();

            ResolveTrackSizes(colTrackList);

            float retn = 0;
            // this ignores fr sizes on purpose, fr size is always 0 for content size
            for (int i = 0; i < colTrackList.size; i++) {
                retn += colTrackList[i].resolvedBaseSize;
            }

            retn += colGap * (colTrackList.size - 1);

            return retn;
        }

        public float ComputeContentHeight(ref BurstLayoutRunner layoutRunner, in BlockSize blockSize) {
            // get a height for all children
            ComputeItemHeights(ref layoutRunner, blockSize);

            // first pass, get fixed sizes
            ComputeVerticalFixedTrackSizes(ref layoutRunner, blockSize);

            // now compute the intrinsic sizes for all tracks so we can resolve mn and mx
            ComputeContentHeightContributionSizes();

            // now we have all the size data, figure out actual pixel sizes for all vertical tracks
            ResolveTrackSizes(rowTrackList);

            // at this point we are done because we never grow and we never allocate to fr because by definition content height
            // is only the base size of the content without any growth applied
            // this ignores fr sizes on purpose, fr size is always 0 for content size

            // last step is to sum the base heights and add the row gap size
            float retn = 0;

            for (int i = 0; i < rowTrackList.size; i++) {
                retn += rowTrackList.array[i].resolvedBaseSize;
            }

            retn += rowGap * (rowTrackList.size - 1);

            return retn;
        }

        public void OnInitialize(LayoutSystem layoutSystem, UIElement element) {
            elementId = element.id;
            horizontalAlignment = element.style.AlignItemsHorizontal;
            verticalAlignment = element.style.AlignItemsVertical;
            fitVertical = element.style.FitItemsVertical;
            fitHorizontal = element.style.FitItemsHorizontal;
            colGap = element.style.GridLayoutColGap;
            rowGap = element.style.GridLayoutRowGap;
        }

        private float ResolveFixedHorizontalCellSize(ref BurstLayoutRunner runner, in GridCellSize cellSize, in BlockSize blockSize) {
            switch (cellSize.unit) {
                default:
                case GridTemplateUnit.Unset:
                case GridTemplateUnit.Pixel:
                    return cellSize.value;

                case GridTemplateUnit.ParentSize:
                    return math.max(0, blockSize.outerSize * cellSize.value);

                case GridTemplateUnit.Percent:
                case GridTemplateUnit.ParentContentArea:
                    return math.max(0, blockSize.insetSize * cellSize.value);

                case GridTemplateUnit.Em:
                    return runner.GetResolvedFontSize(elementId) * cellSize.value;

                case GridTemplateUnit.ViewportWidth:
                    return runner.viewParameters.viewWidth * cellSize.value;

                case GridTemplateUnit.ViewportHeight:
                    return runner.viewParameters.viewHeight * cellSize.value;

                case GridTemplateUnit.MinContent:
                case GridTemplateUnit.MaxContent:
                    // not fixed size, never hits
                    return -1;
            }

        }

        private float ResolveFixedVerticalCellSize(ref BurstLayoutRunner runner, in GridCellSize cellSize, in BlockSize blockSize) {
            switch (cellSize.unit) {
                default:
                case GridTemplateUnit.Unset:
                case GridTemplateUnit.Pixel:
                    return cellSize.value;

                case GridTemplateUnit.ParentSize:
                    return math.max(0, blockSize.outerSize * cellSize.value);

                case GridTemplateUnit.Percent:
                case GridTemplateUnit.ParentContentArea:
                    return math.max(0, blockSize.insetSize * cellSize.value);

                case GridTemplateUnit.Em:
                    return runner.GetResolvedFontSize(elementId) * cellSize.value;

                case GridTemplateUnit.ViewportWidth:
                    return runner.viewParameters.viewWidth * cellSize.value;

                case GridTemplateUnit.ViewportHeight:
                    return runner.viewParameters.viewHeight * cellSize.value;

                case GridTemplateUnit.MinContent:
                case GridTemplateUnit.MaxContent:
                    return -1;

            }
        }

        private void ComputeHorizontalFixedTrackSizes(ref BurstLayoutRunner runner, in BlockSize blockSize) {
            for (int i = 0; i < colTrackList.size; i++) {
                ref GridTrack track = ref colTrackList[i];
                track.resolvedBaseSize = ResolveFixedHorizontalCellSize(ref runner, track.cellDefinition.baseSize, blockSize);
                track.resolvedGrowLimit = ResolveFixedHorizontalCellSize(ref runner, track.cellDefinition.growLimit, blockSize);
                track.resolvedShrinkLimit = ResolveFixedHorizontalCellSize(ref runner, track.cellDefinition.shrinkLimit, blockSize);
            }
        }

        /// <summary>
        /// Compute the fixed sizes of all grid tracks on the vertical axis. This will resolve all units except fr, mn, and mx into pixel values
        /// it will also set the fixedSizeMin and fixedSizeMax values on each track. If the unit is fr, mn, or mx, the fixed size is always 0
        /// </summary>
        private void ComputeVerticalFixedTrackSizes(ref BurstLayoutRunner runner, in BlockSize blockSize) {
            for (int i = 0; i < rowTrackList.size; i++) {
                ref GridTrack track = ref rowTrackList[i];
                track.resolvedBaseSize = ResolveFixedVerticalCellSize(ref runner, track.cellDefinition.baseSize, blockSize);
                track.resolvedGrowLimit = ResolveFixedVerticalCellSize(ref runner, track.cellDefinition.growLimit, blockSize);
                track.resolvedShrinkLimit = ResolveFixedVerticalCellSize(ref runner, track.cellDefinition.shrinkLimit, blockSize);
            }
        }

        /// <summary>
        /// Some sizes in the grid track definition are not immediately resolvable to pixels sizes. For these cases (MaxContent, MinContent)
        /// we need to look at every item in the children and figure out what the min and max content contributions are. This is not a 1-1 with elements
        /// since an element might span multiple tracks. The resolution algorithm is as follows:
        /// First we find the sum of all the fixed sized tracks that this element spans (horizontal axis in this case)
        /// While doing this, keep track of the number of intrinsically sized tracks the element spans.
        /// if we have any intrinsically sized spanned tracks, we compute the content contribution size as
        /// (element width + horizontal margins - spanned fixed track sizes) / number of spanned intrinsic tracks
        /// this value will be used later to resolve the sizes of mx and mn tracks 
        /// </summary>
        private void ComputeContentWidthContributionSizes() {
            for (int i = 0; i < colTrackList.size; i++) {
                ref GridTrack track = ref colTrackList[i];
                track.minContentContribution = -1;
                track.maxContentContribution = 0;
            }

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList[i];

                int spannedIntrinsics = 0;
                float width = placement.outputWidth;

                for (int k = placement.x; k < placement.x + placement.width; k++) {
                    ref GridTrack track = ref colTrackList[k];
                    if (track.IsIntrinsic) {
                        spannedIntrinsics++;
                    }
                    else {
                        width -= track.resolvedBaseSize;
                    }
                }

                width = math.max(0, width);

                if (spannedIntrinsics > 0) {
                    placement.widthContributionSize = width / spannedIntrinsics;
                }
                else {
                    placement.widthContributionSize = 0;
                }

                for (int k = placement.x; k < placement.x + placement.width; k++) {
                    ref GridTrack track = ref colTrackList[k];

                    if (track.maxContentContribution < placement.widthContributionSize) {
                        track.maxContentContribution = placement.widthContributionSize;
                    }

                    if (track.minContentContribution == -1 || track.minContentContribution > placement.widthContributionSize) {
                        track.minContentContribution = placement.widthContributionSize;
                    }
                }
            }

        }

        /// See ComputeContentWidthContributionSizes, use height instead of width 
        private void ComputeContentHeightContributionSizes() {
            for (int i = 0; i < rowTrackList.size; i++) {
                ref GridTrack track = ref rowTrackList[i];
                track.minContentContribution = -1;
                track.maxContentContribution = 0;
            }

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList[i];

                int spannedIntrinsics = 0;
                float height = placement.outputHeight;

                for (int k = placement.y; k < placement.y + placement.height; k++) {
                    ref GridTrack track = ref rowTrackList[k];
                    if (track.IsIntrinsic) {
                        spannedIntrinsics++;
                    }
                    else {
                        height -= track.resolvedBaseSize;
                    }
                }

                height = math.max(0, height);

                if (spannedIntrinsics > 0) {
                    placement.heightContributionSize = height / spannedIntrinsics;
                }
                else {
                    placement.heightContributionSize = 0;
                }

                for (int k = placement.y; k < placement.y + placement.height; k++) {
                    ref GridTrack track = ref rowTrackList[k];

                    if (track.maxContentContribution < placement.heightContributionSize) {
                        track.maxContentContribution = placement.heightContributionSize;
                    }

                    if (track.minContentContribution == -1 || track.minContentContribution > placement.heightContributionSize) {
                        track.minContentContribution = placement.heightContributionSize;
                    }
                }
            }
        }

        private static void ResolveTrackSizes(in List_GridTrack tracks) {
            for (int i = 0; i < tracks.size; i++) {
                ref GridTrack track = ref tracks.array[i];

                if (track.IsIntrinsic) {
                    // fixed sizes will have already been figured out by now
                    if (track.cellDefinition.baseSize.unit == GridTemplateUnit.MaxContent) {
                        track.resolvedBaseSize = track.cellDefinition.baseSize.value * track.maxContentContribution;
                    }

                    if (track.cellDefinition.baseSize.unit == GridTemplateUnit.MinContent) {
                        track.resolvedBaseSize = track.cellDefinition.baseSize.value * track.minContentContribution;
                    }

                    if (track.cellDefinition.growLimit.unit == GridTemplateUnit.MaxContent) {
                        track.resolvedGrowLimit = track.cellDefinition.growLimit.value * track.maxContentContribution;
                    }

                    if (track.cellDefinition.growLimit.unit == GridTemplateUnit.MinContent) {
                        track.resolvedGrowLimit = track.cellDefinition.growLimit.value * track.minContentContribution;
                    }

                    if (track.cellDefinition.shrinkLimit.unit == GridTemplateUnit.MaxContent) {
                        track.resolvedShrinkLimit = track.cellDefinition.shrinkLimit.value * track.maxContentContribution;
                    }

                    if (track.cellDefinition.shrinkLimit.unit == GridTemplateUnit.MinContent) {
                        track.resolvedShrinkLimit = track.cellDefinition.shrinkLimit.value * track.minContentContribution;
                    }
                }
            }
        }

        private void ComputeItemWidths(ref BurstLayoutRunner runner, in BlockSize blockSize) {
            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList[i];
                runner.GetWidths(this, blockSize, placement.elementId, out placement.widthData);
                placement.outputWidth = placement.widthData.Clamped + placement.widthData.marginStart + placement.widthData.marginEnd;
            }
        }

        private void ComputeItemHeights(ref BurstLayoutRunner runner, in BlockSize blockSize) {
            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList[i];
                runner.GetHeights(this, blockSize, placement.elementId, out placement.heightData);
                placement.outputHeight = placement.heightData.Clamped + placement.heightData.marginStart + placement.heightData.marginEnd;
            }
        }

        private static void PositionTracks(ref List_GridTrack trackList, float gap, float inset) {
            float offset = inset;
            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList[i];
                track.position = offset;
                offset += gap + track.size;
            }
        }

        private static float Grow(ref List_GridTrack trackList, float remaining) {
            float allocated = 0;
            do {
                allocated = GrowStep(ref trackList, remaining);
                remaining -= allocated;
            } while (allocated != 0);

            return remaining;
        }

        private static float Shrink(ref List_GridTrack trackList, float remaining) {
            float allocated = 0;
            do {
                allocated = ShrinkStep(ref trackList, remaining);
                remaining += allocated;
            } while (allocated != 0);

            return remaining;
        }

        private static float GrowStep(ref List_GridTrack trackList, float remaining) {
            // get grow limits for each track
            // distribute space in 'virtual' fr units across things that still want to grow
            float desiredSpace = 0;
            int pieces = 0;

            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList[i];

                if (track.size < track.resolvedGrowLimit && track.cellDefinition.growFactor > 0) {
                    desiredSpace += track.resolvedGrowLimit - track.size;
                    pieces += track.cellDefinition.growFactor;
                }
            }

            if (pieces == 0) {
                return 0;
            }

            float allocatedSpace = 0;

            if (desiredSpace > remaining) {
                desiredSpace = remaining;
            }

            float pieceSize = desiredSpace / pieces;

            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList[i];

                if (track.size < track.resolvedGrowLimit) {
                    track.size += pieceSize;
                    allocatedSpace += pieceSize;
                    if (track.size > track.resolvedGrowLimit) {
                        allocatedSpace -= (track.size - track.resolvedGrowLimit);
                        track.size = track.resolvedGrowLimit;
                        return allocatedSpace;
                    }
                }
            }

            return allocatedSpace;
        }

        private static float ShrinkStep(ref List_GridTrack trackList, float overflow) {
            int pieces = 0;

            if (overflow >= 0) return 0;

            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList[i];

                if (track.size > track.resolvedShrinkLimit && track.cellDefinition.shrinkFactor > 0) {
                    pieces += track.cellDefinition.shrinkFactor;
                }
            }

            if (pieces == 0) {
                return 0;
            }

            float allocatedSpace = 0;

            float pieceSize = -overflow / pieces;

            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList[i];

                if (track.size > track.resolvedShrinkLimit) {
                    track.size -= pieceSize;
                    allocatedSpace += pieceSize;
                    if (track.size < track.resolvedShrinkLimit) {
                        allocatedSpace -= (track.size - track.resolvedShrinkLimit);
                        track.size = track.resolvedShrinkLimit;
                        return allocatedSpace;
                    }
                }
            }

            return allocatedSpace;
        }

        public void OnChildrenChanged(LayoutSystem layoutSystem) {
            layoutSystem.EnqueueGridForPlacement(elementId);
        }

        public void RunPlacement(LayoutSystem layoutSystem) {
            UIElement element = layoutSystem.elementSystem.instanceTable[elementId.index];
            ref LayoutHierarchyInfo layoutHierarchyInfo = ref layoutSystem.layoutHierarchyTable[elementId];

            placementList.SetSize(layoutHierarchyInfo.childCount, Allocator.Persistent);
            ElementId ptr = layoutHierarchyInfo.firstChildId;

            int idx = 0;

            while (ptr != default) {
                UIElement child = layoutSystem.elementSystem.instanceTable[ptr.index];

                placementList[idx++] = new GridPlacement() {
                    elementId = child.id,
                    x = child.style.GridItemX.index,
                    y = child.style.GridItemY.index,
                    width = child.style.GridItemWidth.index,
                    height = child.style.GridItemHeight.index,
                };

                ptr = layoutSystem.layoutHierarchyTable[ptr].nextSiblingId;
            }

            GridPlacementJob job = new GridPlacementJob() {
                placementList = placementList,
                layoutDirection = element.style.GridLayoutDirection,
                density = element.style.GridLayoutDensity,
                colTemplate = layoutSystem.BufferGridColTemplate(element.style.GridLayoutColTemplate),
                rowTemplate = layoutSystem.BufferGridRowTemplate(element.style.GridLayoutRowTemplate),
                autoColSizePattern = layoutSystem.BufferGridColAutoSize(element.style.GridLayoutColAutoSize),
                autoRowSizePattern = layoutSystem.BufferGridRowAutoSize(element.style.GridLayoutRowAutoSize),
                colTrackList = layoutSystem.GetGridColTrackBuffer(),
                rowTrackList = layoutSystem.GetGridRowTrackBuffer()
            };

            job.Run();

            colTrackList.CopyFrom(job.colTrackList.GetArrayPointer(), job.colTrackList.size, Allocator.Persistent);
            rowTrackList.CopyFrom(job.rowTrackList.GetArrayPointer(), job.rowTrackList.size, Allocator.Persistent);

        }

        public void Dispose() {
            placementList.Dispose();
            colTrackList.Dispose();
            rowTrackList.Dispose();
        }

        public void OnStylePropertiesChanged(LayoutSystem layoutSystem, UIElement element, StyleProperty[] properties, int propertyCount) {

            bool placementDirty = false;

            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref properties[i];

                switch (property.propertyId) {

                    case StylePropertyId.GridLayoutDensity:
                    case StylePropertyId.GridLayoutDirection:
                    case StylePropertyId.GridLayoutColAutoSize:
                    case StylePropertyId.GridLayoutColTemplate:
                    case StylePropertyId.GridLayoutRowAutoSize:
                    case StylePropertyId.GridLayoutRowTemplate:
                        placementDirty = true;
                        break;

                    case StylePropertyId.GridLayoutColGap:
                        colGap = property.AsFloat;
                        break;

                    case StylePropertyId.GridLayoutRowGap:
                        rowGap = property.AsFloat;
                        break;

                    case StylePropertyId.FitItemsHorizontal:
                        fitHorizontal = property.AsLayoutFit;
                        break;

                    case StylePropertyId.FitItemsVertical:
                        fitVertical = property.AsLayoutFit;
                        break;

                    case StylePropertyId.AlignItemsHorizontal:
                        horizontalAlignment = property.AsFloat;
                        break;

                    case StylePropertyId.AlignItemsVertical:
                        verticalAlignment = property.AsFloat;
                        break;
                }
            }

            if (placementDirty) {
                layoutSystem.EnqueueGridForPlacement(elementId);
            }

        }

        public void OnChildStyleChanged(LayoutSystem layoutSystem, ElementId childId, StyleProperty[] propertyList, int propertyCount) {
            for (int i = 0; i < propertyCount; i++) {
                ref StyleProperty property = ref propertyList[i];
                switch (property.propertyId) {
                    case StylePropertyId.GridItemX:
                    case StylePropertyId.GridItemY:
                    case StylePropertyId.GridItemWidth:
                    case StylePropertyId.GridItemHeight:
                        layoutSystem.EnqueueGridForPlacement(elementId);
                        return;
                }
            }
        }

        public float GetActualContentWidth(ref BurstLayoutRunner runner) {
            return 0;
        }

        public float GetActualContentHeight(ref BurstLayoutRunner runner) {
            return 0;
        }

    }

    // todo -- probably want a split buffer for this
    public struct GridPlacement {

        public int x;
        public int y;
        public int width;
        public int height;
        public ElementId elementId;
        public LayoutSize widthData;
        public LayoutSize heightData;
        public float outputWidth;
        public float outputHeight;
        public float widthContributionSize;
        public float heightContributionSize;

    }

}