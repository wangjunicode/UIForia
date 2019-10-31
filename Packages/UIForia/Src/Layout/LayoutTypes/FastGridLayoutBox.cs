using System;
using System.Collections.Generic;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout.LayoutTypes {

    public class FastGridLayoutBox : FastLayoutBox {

        private bool placementDirty;

        private readonly StructList<GridTrack> colTrackList;
        private readonly StructList<GridTrack> rowTrackList;
        private readonly StructList<GridPlacement> placementList;

        private static readonly StructList<GridRegion> s_OccupiedAreas = new StructList<GridRegion>(32);

        public FastGridLayoutBox() {
            this.placementDirty = true;
            this.colTrackList = new StructList<GridTrack>();
            this.rowTrackList = new StructList<GridTrack>();
            this.placementList = new StructList<GridPlacement>();
        }

        internal int RowCount => rowTrackList.size;

        internal int ColCount => colTrackList.size;

        public override float GetIntrinsicMinWidth() {
            return 0;
        }

        public override float GetIntrinsicMinHeight() {
            return 0;
        }

        protected override float ComputeIntrinsicPreferredWidth() {
            return 0;
        }

        public override float GetIntrinsicPreferredHeight() {
            return 0;
        }

        public override float ComputeContentWidth(BlockSize widthBlock) {
            if (firstChild == null) {
                return 0;
            }

            BlockSize heightBlock = containingBoxHeight;

            AdjustBlockSizes(ref widthBlock, ref heightBlock);

            Place();

            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetWidth(widthBlock, ref placement.size);
                placement.layoutBox.GetMarginHorizontal(placement.size.prefWidth, ref placement.margin);
                placement.outputWidth = placement.size.prefWidth + placement.margin.left + placement.margin.right;
            }

            ResolveHorizontalTrackWidths(widthBlock, size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right);

            PositionTracks(colTrackList, element.style.GridLayoutColGap);

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetHeight(placement.layoutBox.size.width, widthBlock, heightBlock, ref placement.size);
                placement.layoutBox.GetMarginVertical(placement.size.prefHeight, ref placement.margin);
                placement.outputHeight = placement.size.prefHeight + placement.margin.top + placement.margin.bottom;
            }

            ResolveVerticalTrackHeights(heightBlock, size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom);

            PositionTracks(rowTrackList, element.style.GridLayoutRowGap);

            return colTrackList.array[colTrackList.size - 1].position + colTrackList.array[colTrackList.size - 1].size;
        }

        public override float ComputeContentHeight(float width, BlockSize widthBlock, BlockSize heightBlock) {
            if (firstChild == null) {
                return 0;
            }

            AdjustBlockSizes(ref widthBlock, ref heightBlock);
            
            Place();

            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetWidth(widthBlock, ref placement.size);
                placement.layoutBox.GetMarginHorizontal(placement.size.prefWidth, ref placement.margin);
                placement.outputWidth = placement.size.prefWidth + placement.margin.left + placement.margin.right;
            }

            ResolveHorizontalTrackWidths(widthBlock, size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right);

            PositionTracks(colTrackList, element.style.GridLayoutColGap);

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetHeight(placement.layoutBox.size.width, widthBlock, heightBlock, ref placement.size);
                placement.layoutBox.GetMarginVertical(placement.size.prefHeight, ref placement.margin);
                placement.outputHeight = placement.size.prefHeight + placement.margin.top + placement.margin.bottom;
            }

            ResolveVerticalTrackHeights(heightBlock, size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom);

            PositionTracks(rowTrackList, element.style.GridLayoutRowGap);

            return rowTrackList.array[rowTrackList.size - 1].position + rowTrackList.array[rowTrackList.size - 1].size;
        }

        protected override void PerformLayout() {
            if (firstChild == null) {
                return;
            }

            BlockSize widthBlock = containingBoxWidth;
            BlockSize heightBlock = containingBoxHeight;

            AdjustBlockSizes(ref widthBlock, ref heightBlock);
            
            Place();

            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetWidth(widthBlock, ref placement.size);
                placement.layoutBox.GetMarginHorizontal(placement.size.prefWidth, ref placement.margin);
                placement.outputWidth = placement.size.prefWidth + placement.margin.left + placement.margin.right;
            }

            ResolveHorizontalTrackWidths(widthBlock, size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right);

            PositionTracks(colTrackList, element.style.GridLayoutColGap);

            ApplyHorizontalSizes();

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetHeight(placement.layoutBox.size.width, widthBlock, heightBlock, ref placement.size);
                placement.layoutBox.GetMarginVertical(placement.size.prefHeight, ref placement.margin);
                placement.outputHeight = placement.size.prefHeight + placement.margin.top + placement.margin.bottom;
            }

            ResolveVerticalTrackHeights(heightBlock, size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom);

            PositionTracks(rowTrackList, element.style.GridLayoutRowGap);

            ApplyVerticalSizes();
        }

        private void ApplyHorizontalSizes() {
            GridAxisAlignment colAlignment = element.style.GridLayoutColAlignment;

            float paddingBorderLeft = paddingBox.left + borderBox.left;

            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;
            GridTrack[] tracks = colTrackList.array;

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                ref GridTrack spanEnd = ref tracks[placement.x + placement.width - 1];

                FastLayoutBox child = placement.layoutBox;

                float x = tracks[placement.x].position;
                float spannedTracksWidth = (spanEnd.position + spanEnd.size) - x;

                float finalX = x + placement.margin.left;
                float finalWidth = spannedTracksWidth;

                LayoutFit layoutFit = default;

                BlockSize blockSize = new BlockSize();
                blockSize.size = finalWidth;
                blockSize.contentAreaSize = finalWidth;
                float colPosition = 0;

                switch (colAlignment) {
                    case GridAxisAlignment.Center:
                        colPosition = 0.5f;
                        break;

                    case GridAxisAlignment.End:
                        colPosition = 1f;
                        break;

                    case GridAxisAlignment.Start:
                        colPosition = 0f;
                        break;

                    case GridAxisAlignment.Grow:
                        layoutFit = LayoutFit.Grow;
                        break;
                    case GridAxisAlignment.Shrink:
                        layoutFit = LayoutFit.Shrink;
                        break;
                    case GridAxisAlignment.Fit:
                        layoutFit = LayoutFit.Fill;
                        break;
                }

                child.ApplyHorizontalLayout(finalX + paddingBorderLeft, blockSize, blockSize.contentAreaSize - placement.margin.left - placement.margin.right, placement.size.prefWidth, colPosition, layoutFit);
            }
        }

        private void ApplyVerticalSizes() {
            GridAxisAlignment colAlignment = element.style.GridLayoutColAlignment;

            float paddingBorderTop = paddingBox.top + borderBox.top;

            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;
            GridTrack[] tracks = rowTrackList.array;

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                ref GridTrack spanEnd = ref tracks[placement.y + placement.height - 1];

                FastLayoutBox child = placement.layoutBox;

                float y = tracks[placement.y].position;
                float spannedTrackHeights = (spanEnd.position + spanEnd.size) - y;

                float finalY = y + placement.margin.top;
                float finalHeight = spannedTrackHeights;

                LayoutFit layoutFit = default;
                float alignment = 0;

                BlockSize blockSize = new BlockSize();
                blockSize.size = finalHeight - placement.margin.top - placement.margin.bottom;
                blockSize.contentAreaSize = finalHeight - placement.margin.top - placement.margin.bottom;

                switch (colAlignment) {
                    case GridAxisAlignment.Center:
                        alignment = 0.5f;
                        break;

                    case GridAxisAlignment.End:
                        alignment = 1f;
                        break;

                    case GridAxisAlignment.Start:
                        break;

                    case GridAxisAlignment.Grow:
                        layoutFit = LayoutFit.Grow;
                        break;
                    case GridAxisAlignment.Shrink:
                        layoutFit = LayoutFit.Shrink;
                        break;
                    case GridAxisAlignment.Fit:
                        layoutFit = LayoutFit.Fill;
                        break;
                }

                child.ApplyVerticalLayout(finalY + paddingBorderTop, blockSize, blockSize.contentAreaSize, placement.size.prefHeight, alignment, layoutFit);
            }
        }

        private void ResolveHorizontalTrackWidths(in BlockSize blockWidth, float remaining) {
            int flexPieces = 0;

            StructList<int> intrinsics = StructList<int>.Get();
            StructList<int> flexes = StructList<int>.Get();

            GridTrack[] colTracks = colTrackList.array;
            int colTrackCount = colTrackList.size;

            for (int i = 0; i < colTrackCount; i++) {
                ref GridTrack track = ref colTracks[i];

                if ((track.minUnit & GridTemplateUnit.Fixed) != 0) {
                    track.size = ResolveFixedGridMeasurement(blockWidth, track.minValue, track.minUnit);
                    remaining -= track.size;
                }
                else if (track.minUnit == GridTemplateUnit.FractionalRemaining) {
                    flexes.Add(i);
                    flexPieces += (int) track.minValue; // move flex to max only?
                }
                else {
                    intrinsics.Add(i);
                }
            }

            for (int i = 0; i < intrinsics.size; i++) {
                ref GridTrack track = ref colTracks[intrinsics.array[i]];

                if (track.minUnit == GridTemplateUnit.MinContent) {
                    track.size = ResolveContentMinWidth(track, intrinsics.array[i]);
                }
                else if (track.minUnit == GridTemplateUnit.MaxContent) {
                    track.size = ResolveContentMaxWidth(track, intrinsics.array[i]);
                }

                remaining -= track.size;
            }

            // now grow the track using maxes of min-max

            float colGap = element.style.GridLayoutColGap;
            remaining -= colGap * (colTrackCount - 1);

            if ((int) remaining > 0 && flexes.size > 0) {
                float pieceSize = remaining / flexPieces;
                for (int i = 0; i < flexes.size; i++) {
                    ref GridTrack track = ref colTracks[flexes.array[i]];
                    track.size = (int) track.minValue * pieceSize;
                }
            }


            intrinsics.Release();
            flexes.Release();
        }

        private void ResolveVerticalTrackHeights(in BlockSize blockHeight, float remaining) {
            int flexPieces = 0;

            StructList<int> intrinsics = StructList<int>.Get();
            StructList<int> flexes = StructList<int>.Get();

            GridTrack[] rowTracks = rowTrackList.array;
            int rowTrackCount = rowTrackList.size;

            // min values are never fr
            // we alias 1fr to minmax(0, 1fr)
            // clamp(base, min, max)

            for (int i = 0; i < rowTrackCount; i++) {
                ref GridTrack track = ref rowTracks[i];

                if ((track.minUnit & GridTemplateUnit.Fixed) != 0) {
                    track.size = ResolveFixedGridMeasurement(blockHeight, track.minValue, track.minUnit);
                    remaining -= track.size;
                }
                else if (track.minUnit == GridTemplateUnit.FractionalRemaining) {
                    flexes.Add(i);
                    flexPieces += (int) track.minValue; // move flex to max only?
                }
                else {
                    intrinsics.Add(i);
                }
            }

            for (int i = 0; i < intrinsics.Count; i++) {
                ref GridTrack track = ref rowTrackList.array[intrinsics.array[i]];

                if (track.minUnit == GridTemplateUnit.MinContent) {
                    track.size = ResolveContentMinHeight(track, intrinsics.array[i]);
                }
                else if (track.minUnit == GridTemplateUnit.MaxContent) {
                    track.size = ResolveContentMaxHeight(track, intrinsics.array[i]);
                }

                remaining -= track.size;
            }

            // everything has a base value now

            // find non flexed maxes

            // grow all non flexed things to their max treat as flexbox with grow factor 1

            // find maxed sizes of non flexed things

            // remove minmax flex when min > flex would have been

            // distribute fr as in flexbox

            // now grow the track using maxes of min-max

            float rowGap = element.style.GridLayoutRowGap;
            remaining -= rowGap * (rowTrackCount - 1);

            if ((int) remaining > 0 && flexes.size > 0) {
                float pieceSize = remaining / flexPieces;
                for (int i = 0; i < flexes.size; i++) {
                    ref GridTrack track = ref rowTracks[flexes.array[i]];
                    track.size = (int) track.minValue * pieceSize;
                }
            }

            // for minmax ->
            // for each track
            // if max size not reached & space remaining
            // treat everything as though it had grow factor = 1
            // grow until max size reached
            // fr has no max size but has more grow factor
            // maybe grow everything except frs first
            
            intrinsics.Release();
            flexes.Release();
        }

        private float GrowHeight(int pieces, float remainingSpace) {
            bool allocate = pieces > 0;

            GridTrack[] rowTracks = rowTrackList.array;
            int trackCount = rowTrackList.size;

            while (allocate && (int) remainingSpace > 0 && pieces > 0) {
                allocate = false;

                float pieceSize = remainingSpace / pieces;

                for (int i = 0; i < trackCount; i++) {
                    ref GridTrack track = ref rowTracks[i];
                    float max = track.maxSize;
                    float output = track.size;
                    int growthFactor = 1; //track.growFactor;

                    if (growthFactor == 0 || (int) output == (int) max) {
                        continue;
                    }

                    allocate = true;
                    float start = output;
                    float growSize = growthFactor * pieceSize;
                    float totalGrowth = start + growSize;
                    output = totalGrowth > max ? max : totalGrowth;

                    remainingSpace -= output - start;

                    track.size = output;
                }
            }

            return remainingSpace;
        }

        private static void PositionTracks(StructList<GridTrack> trackList, float gap) {
            for (int i = 1; i < trackList.size; i++) {
                ref GridTrack track = ref trackList.array[i];
                ref GridTrack previous = ref trackList.array[i - 1];
                track.position = gap + previous.position + previous.size;
            }
        }

        private float ResolveContentMinWidth(in GridTrack track, int index) {
            float minSize = 0;

            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            // only need to check 1 dimension
            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];

                int xMax = placement.x + placement.width;

                // if not spanning this cell, continue
                if (!(placement.x <= index && xMax > index)) {
                    continue;
                }

                int pieces = 0; // never 0 because we only call this function for intrinsic sized tracks
                float baseWidth = placement.outputWidth;

                for (int j = placement.x; j < xMax; j++) {
                    GridTrack spanned = colTrackList.array[j];
                    if ((track.minUnit & GridTemplateUnit.Fixed) != 0) {
                        baseWidth -= spanned.size;
                        if (baseWidth <= 0) {
                            break;
                        }
                    }
                    else if (track.minUnit != GridTemplateUnit.FractionalRemaining) {
                        pieces++;
                    }
                }

                if (minSize == 0) {
                    minSize = baseWidth;
                }

                if (baseWidth > 0) {
                    minSize = Mathf.Min(minSize, track.minValue * (baseWidth / pieces));
                }
            }

            return minSize;
        }

        private float ResolveContentMaxWidth(in GridTrack track, int index) {
            float maxSize = 0;

            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            // only need to check 1 dimension
            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];

                int xMax = placement.x + placement.width;

                // if not spanning this cell, continue
                if (!(placement.x <= index && xMax > index)) {
                    continue;
                }

                int pieces = 0; // never 0 because we only call this function for intrinsic sized tracks
                float baseWidth = placement.outputWidth;

                for (int j = placement.x; j < xMax; j++) {
                    GridTrack spanned = colTrackList.array[j];
                    if ((track.maxUnit & GridTemplateUnit.Fixed) != 0) {
                        baseWidth -= spanned.size;
                        if (baseWidth <= 0) {
                            break;
                        }
                    }
                    else if (track.maxUnit != GridTemplateUnit.FractionalRemaining) {
                        pieces++;
                    }
                }

                if (maxSize == 0) {
                    maxSize = baseWidth;
                }

                if (baseWidth > 0) {
                    maxSize = Mathf.Max(maxSize, track.maxValue * (baseWidth / pieces));
                }
            }

            return maxSize;
        }

        private float ResolveContentMinHeight(in GridTrack track, int index) {
            float minSize = 0;

            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            // only need to check 1 dimension
            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];

                int yMax = placement.y + placement.height;

                // if not spanning this cell, continue
                if (!(placement.y <= index && yMax > index)) {
                    continue;
                }

                int pieces = 0; // never 0 because we only call this function for intrinsic sized tracks
                float baseHeight = placement.outputHeight;

                for (int j = placement.y; j < yMax; j++) {
                    GridTrack spanned = rowTrackList.array[j];
                    if ((track.minUnit & GridTemplateUnit.Fixed) != 0) {
                        baseHeight -= spanned.size;
                        if (baseHeight <= 0) {
                            break;
                        }
                    }
                    else if (track.minUnit != GridTemplateUnit.FractionalRemaining) {
                        pieces++;
                    }
                }

                if (minSize == 0) {
                    minSize = baseHeight;
                }

                if (baseHeight > 0) {
                    minSize = Mathf.Min(minSize, track.minValue * (baseHeight / pieces));
                }
            }

            return minSize;
        }

        private float ResolveContentMaxHeight(in GridTrack track, int index) {
            float maxSize = 0;

            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            // only need to check 1 dimension
            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];

                int yMax = placement.y + placement.height;

                // if not spanning this cell, continue
                if (!(placement.y <= index && yMax > index)) {
                    continue;
                }

                int pieces = 0; // never 0 because we only call this function for intrinsic sized tracks
                float baseHeight = placement.outputHeight;

                for (int j = placement.y; j < yMax; j++) {
                    GridTrack spanned = rowTrackList.array[j];
                    if ((track.minUnit & GridTemplateUnit.Fixed) != 0) {
                        baseHeight -= spanned.size;
                        if (baseHeight <= 0) {
                            break;
                        }
                    }
                    else if (track.minUnit != GridTemplateUnit.FractionalRemaining) {
                        pieces++;
                    }
                }

                if (maxSize == 0) {
                    maxSize = baseHeight;
                }

                if (baseHeight > 0) {
                    maxSize = Mathf.Max(maxSize, track.minValue * (baseHeight / pieces));
                }
            }

            return maxSize;
        }

        private float ResolveFixedGridMeasurement(in BlockSize blockWidth, float value, GridTemplateUnit unit) {
            switch (unit) {
                case GridTemplateUnit.Pixel:
                    return value;

                case GridTemplateUnit.Em:
                    return value * element.style.GetResolvedFontSize() * value;

                case GridTemplateUnit.ViewportWidth:
                    return value * element.View.Viewport.width;

                case GridTemplateUnit.ViewportHeight:
                    return value * element.View.Viewport.height;

                case GridTemplateUnit.ParentSize:
                    return blockWidth.size * value;

                case GridTemplateUnit.ParentContentArea:
                    return blockWidth.contentAreaSize * value;

                case GridTemplateUnit.FractionalRemaining:
                case GridTemplateUnit.MinContent:
                case GridTemplateUnit.MaxContent:
                    return 0f;

                default:
                    return 0f;
            }
        }

        private void CreateNamedGridAreas() {
            // retokenize & parse if needed
        }

        private int ResolveHorizontalStart(string name) {
            return -1;
        }

        private int ResolveHorizontalWidth(int resolvedColStart, string name) {
            return 1;
        }

        private int ResolveVerticalStart(string name) {
            return -1;
        }

        private int ResolveVerticalHeight(int resolvedRowStart, string name) {
            return 1;
        }

        private void CreateOrUpdatePlacement(FastLayoutBox child) {
            GridPlacement[] placements = placementList.array;

            GridItemPlacement x = child.element.style.GridItemX;
            GridItemPlacement y = child.element.style.GridItemY;
            GridItemPlacement width = child.element.style.GridItemWidth;
            GridItemPlacement height = child.element.style.GridItemHeight;

            GridPlacement placement = default;

            placement.layoutBox = child;
            placement.x = x.name != null ? ResolveHorizontalStart(x.name) : x.index;
            placement.y = y.name != null ? ResolveVerticalStart(y.name) : y.index;
            placement.width = width.name != null ? ResolveHorizontalWidth(placement.x, width.name) : width.index;
            placement.height = height.name != null ? ResolveVerticalHeight(placement.y, height.name) : height.index;

            for (int i = 0; i < placementList.size; i++) {
                if (placements[i].layoutBox == child) {
                    placements[i] = placement;
                    return;
                }
            }

            placementList.Add(placement);
        }

        private float ResolveMin(in BlockSize blockSize, float value, GridTemplateUnit unit) {
            switch (unit) {
                case GridTemplateUnit.Unset:
                case GridTemplateUnit.Pixel:
                    return value;

                case GridTemplateUnit.Em:
                    return element.style.GetResolvedFontSize() * value;

                case GridTemplateUnit.ViewportWidth:
                    return element.View.Viewport.width * value;

                case GridTemplateUnit.ViewportHeight:
                    return element.View.Viewport.height * value;

                case GridTemplateUnit.ParentSize:
                    return blockSize.size * value;

                case GridTemplateUnit.ParentContentArea:
                    return blockSize.contentAreaSize * value;

                default:
                    return 0;
            }
        }

        private void GenerateExplicitTracksForAxis(IReadOnlyList<GridTrackSize> templateList, in BlockSize blockSize, float contentSize, StructList<GridTrack> trackList, float gutter) {
            int fillRepeatIndex = -1;
            int idx = 0;

            GridTrackSize repeatFill = default;

            for (int i = 0; i < templateList.Count; i++) {
                GridTrackSize template = templateList[i];

                switch (template.type) {
                    case GridTrackSizeType.MinMax:
                    case GridTrackSizeType.Value:
                        trackList.array[idx++] = new GridTrack(template);
                        break;

                    case GridTrackSizeType.Repeat:

                        trackList.EnsureAdditionalCapacity(template.pattern.Length);

                        for (int j = 0; j < template.pattern.Length; j++) {
                            trackList.array[idx] = new GridTrack(template.pattern[j]);
                        }

                        break;

                    case GridTrackSizeType.RepeatFit:
                    case GridTrackSizeType.RepeatFill:
                        fillRepeatIndex = idx;
                        repeatFill = template;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            trackList.size = idx;

            if (fillRepeatIndex != -1) {
                float totalMin = 0;

                for (int i = 0; i < trackList.size; i++) {
                    totalMin += ResolveMin(blockSize, trackList.array[i].minValue, trackList.array[i].minUnit);
                    totalMin += gutter;
                }

                float remaining = contentSize - totalMin;

                StructList<GridTrack> repeats = StructList<GridTrack>.Get();

                do {
                    repeats.EnsureAdditionalCapacity(repeatFill.pattern.Length);
                    for (int i = 0; i < repeatFill.pattern.Length; i++) {
                        repeats.array[idx] = new GridTrack(repeatFill.pattern[i]);
                        float val = ResolveMin(blockSize, trackList.array[i].minValue, trackList.array[i].minUnit);
                        if (val <= 0) val = 1;
                        remaining -= val; // this is a pretty crap way to handle this
                        remaining -= gutter;
                    }
                } while (remaining > 0);

                trackList.InsertRange(fillRepeatIndex, repeats);

                repeats.Release();
            }
        }

        private void GenerateExplicitTracks(BlockSize widthSize, BlockSize heightSize) {
            colTrackList.size = 0;
            rowTrackList.size = 0;

            IReadOnlyList<GridTrackSize> rowTemplate = element.style.GridLayoutRowTemplate;
            IReadOnlyList<GridTrackSize> colTemplate = element.style.GridLayoutColTemplate;

            if (rowTemplate.Count == 0 && colTemplate.Count == 0) {
                return;
            }

            float contentAreaWidth = size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right;
            float contentAreaHeight = size.height - paddingBox.top - paddingBox.bottom - borderBox.top - borderBox.bottom;

            float horizontalGutter = element.style.GridLayoutColGap;
            float verticalGutter = element.style.GridLayoutRowGap;

            GenerateExplicitTracksForAxis(colTemplate, widthSize, contentAreaWidth, colTrackList, horizontalGutter);
            GenerateExplicitTracksForAxis(rowTemplate, heightSize, contentAreaHeight, rowTrackList, verticalGutter);
        }

        private void Place() {
            if (!placementDirty) {
                return;
            }

            placementDirty = false;

            CreateNamedGridAreas();

            placementList.size = 0;
            FastLayoutBox child = firstChild;

            while (child != null) {
                GridItemPlacement x = child.element.style.GridItemX;
                GridItemPlacement y = child.element.style.GridItemY;
                GridItemPlacement width = child.element.style.GridItemWidth;
                GridItemPlacement height = child.element.style.GridItemHeight;

                GridPlacement placement = default;

                placement.layoutBox = child;
                placement.x = x.name != null ? ResolveHorizontalStart(x.name) : x.index;
                placement.y = y.name != null ? ResolveVerticalStart(y.name) : y.index;
                placement.width = width.name != null ? ResolveHorizontalWidth(placement.x, width.name) : width.index;
                placement.height = height.name != null ? ResolveVerticalHeight(placement.y, height.name) : height.index;

                placementList.Add(placement);

                child = child.nextSibling;
            }

            rowTrackList.Clear();
            colTrackList.Clear();

            GenerateExplicitTracks(default, default);

            s_OccupiedAreas.size = 0;

            IReadOnlyList<GridTrackSize> autoColSizePattern = element.style.GridLayoutColAutoSize;
            IReadOnlyList<GridTrackSize> autoRowSizePattern = element.style.GridLayoutRowAutoSize;

            int rowSizeAutoPtr = 0;
            int colSizeAutoPtr = 0;

            PreAllocateRowAndColumns(ref colSizeAutoPtr, ref rowSizeAutoPtr, autoColSizePattern, autoRowSizePattern);
            PlaceBothAxisLocked();
            PlaceSingleAxisLocked(ref colSizeAutoPtr, autoColSizePattern, ref rowSizeAutoPtr, autoRowSizePattern);
            PlaceRemainingItems(ref colSizeAutoPtr, autoColSizePattern, ref rowSizeAutoPtr, autoRowSizePattern);
            placementDirty = false;
        }

        private void PlaceBothAxisLocked() {
            for (int i = 0; i < placementList.Count; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                if (placement.y >= 0 && placement.x >= 0) {
                    GridRegion region = new GridRegion();
                    region.xMin = placement.x;
                    region.yMin = placement.y;
                    region.xMax = placement.x + placement.width;
                    region.yMax = placement.y + placement.height;
                    s_OccupiedAreas.Add(region);
                }
            }
        }

        private void PlaceSingleAxisLocked(ref int colSizeAutoPtr, IReadOnlyList<GridTrackSize> autoColSizePattern, ref int rowSizeAutoPtr, IReadOnlyList<GridTrackSize> autoRowSizePattern) {
            bool dense = element.style.GridLayoutDensity == GridLayoutDensity.Dense;

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                int x = placement.x;
                int y = placement.y;
                int width = placement.width;
                int height = placement.height;

                // x axis is in a fixed position, we need to find a valid Y
                if (y < 0 && x >= 0) {
                    int cursorY = dense ? 0 : colTrackList.array[x].autoPlacementCursor;

                    while (!IsGridAreaAvailable(x, cursorY, width, height)) {
                        cursorY++;
                    }

                    EnsureImplicitTrackCapacity(rowTrackList, cursorY + height, ref rowSizeAutoPtr, autoRowSizePattern);
                    EnsureImplicitTrackCapacity(colTrackList, x + width, ref rowSizeAutoPtr, autoRowSizePattern);

                    colTrackList.array[x].autoPlacementCursor = cursorY;

                    placement.y = cursorY;

                    s_OccupiedAreas.Add(new GridRegion() {
                        xMin = placement.x,
                        yMin = placement.y,
                        xMax = placement.x + placement.width,
                        yMax = placement.y + placement.height
                    });
                }

                // if row was fixed we definitely created it in an earlier step
                else if (x < 0 && y >= 0) {
                    int cursorX = dense ? 0 : rowTrackList[y].autoPlacementCursor;

                    while (!IsGridAreaAvailable(cursorX, y, width, height)) {
                        cursorX++;
                    }

                    EnsureImplicitTrackCapacity(colTrackList, cursorX + width, ref colSizeAutoPtr, autoColSizePattern);
                    EnsureImplicitTrackCapacity(rowTrackList, y + height, ref rowSizeAutoPtr, autoRowSizePattern);

                    rowTrackList.array[y].autoPlacementCursor = cursorX;

                    placement.x = cursorX;

                    s_OccupiedAreas.Add(new GridRegion() {
                        xMin = placement.x,
                        yMin = placement.y,
                        xMax = placement.x + placement.width,
                        yMax = placement.y + placement.height
                    });
                }
            }
        }

        private void PreAllocateRowAndColumns(ref int colPtr, ref int rowPtr, IReadOnlyList<GridTrackSize> colPattern, IReadOnlyList<GridTrackSize> rowPattern) {
            int maxColStartAndSpan = 0;
            int maxRowStartAndSpan = 0;

            GridPlacement[] placements = placementList.array;

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placements[i];
                int colStart = placement.x;
                int rowStart = placement.y;
                int colSpan = placement.width;
                int rowSpan = placement.height;


                if (colStart < 1) {
                    colStart = 0;
                }

                if (rowStart < 1) {
                    rowStart = 0;
                }

                maxColStartAndSpan = maxColStartAndSpan > colStart + colSpan ? maxColStartAndSpan : colStart + colSpan;
                maxRowStartAndSpan = maxRowStartAndSpan > rowStart + rowSpan ? maxRowStartAndSpan : rowStart + rowSpan;
            }

            EnsureImplicitTrackCapacity(colTrackList, maxColStartAndSpan, ref colPtr, colPattern);
            EnsureImplicitTrackCapacity(rowTrackList, maxRowStartAndSpan, ref rowPtr, rowPattern);
        }

        private void PlaceRemainingItems(ref int colSizeAutoPtr, IReadOnlyList<GridTrackSize> autoColSizePattern, ref int rowSizeAutoPtr, IReadOnlyList<GridTrackSize> autoRowSizePattern) {
            if (placementList.size == 0) {
                return;
            }

            bool flowHorizontal = element.style.GridLayoutDirection == LayoutDirection.Horizontal;
            bool dense = element.style.GridLayoutDensity == GridLayoutDensity.Dense;

            int sparseStartX = 0;
            int sparseStartY = 0;

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                int width = placement.width;
                int height = placement.height;

                int cursorX = 0;
                int cursorY = 0;

                if (placement.x >= 0 || placement.y >= 0) {
                    continue;
                }

                if (flowHorizontal) {
                    if (dense) {
                        cursorX = 0;
                        cursorY = 0;
                    }
                    else {
                        cursorX = sparseStartX;
                        cursorY = sparseStartY;
                    }

                    while (true) {
                        if (cursorX + width > colTrackList.size) {
                            cursorY++;
                            // make sure enough rows exist to contain the entire vertical span
                            EnsureImplicitTrackCapacity(rowTrackList, cursorY + height, ref rowSizeAutoPtr, autoRowSizePattern);
                            cursorX = !dense ? rowTrackList.array[cursorY].autoPlacementCursor : 0;
                            continue;
                        }

                        if (IsGridAreaAvailable(cursorX, cursorY, width, height)) {
                            break;
                        }

                        cursorX++;
                    }

                    sparseStartX = cursorX + width;
                    sparseStartY = cursorY;
                    placement.x = cursorX;
                    placement.y = cursorY;
                    EnsureImplicitTrackCapacity(colTrackList, cursorX + width, ref colSizeAutoPtr, autoColSizePattern);
                    EnsureImplicitTrackCapacity(rowTrackList, cursorY + height, ref rowSizeAutoPtr, autoRowSizePattern);
                    rowTrackList.array[cursorY].autoPlacementCursor = cursorX + width;
                    colTrackList.array[cursorX].autoPlacementCursor = cursorY;
                }
                else {
                    if (dense) {
                        cursorX = 0;
                        cursorY = 0;
                    }
                    else {
                        cursorX = sparseStartX;
                        cursorY = sparseStartY;
                    }

                    while (true) {
                        if (cursorY + height > rowTrackList.size) {
                            cursorX++;
                            EnsureImplicitTrackCapacity(colTrackList, cursorX + width, ref colSizeAutoPtr, autoColSizePattern);
                            cursorY = !dense ? colTrackList.array[cursorX].autoPlacementCursor : 0;
                            continue;
                        }

                        if (IsGridAreaAvailable(cursorX, cursorY, width, height)) {
                            break;
                        }

                        cursorY++;
                    }

                    sparseStartX = cursorX;
                    sparseStartY = cursorY;
                    placement.x = cursorX;
                    placement.y = cursorY;
                    colTrackList.array[cursorX].autoPlacementCursor = cursorY;
                    rowTrackList.array[cursorY].autoPlacementCursor = cursorX;
                    EnsureImplicitTrackCapacity(colTrackList, cursorX + width, ref colSizeAutoPtr, autoColSizePattern);
                    EnsureImplicitTrackCapacity(rowTrackList, cursorY + height, ref rowSizeAutoPtr, autoRowSizePattern);
                }

                s_OccupiedAreas.Add(new GridRegion() {
                    xMin = placement.x,
                    yMin = placement.y,
                    xMax = placement.x + placement.width,
                    yMax = placement.y + placement.height
                });
            }
        }

        private static bool IsGridAreaAvailable(int x, int y, int width, int height) {
            int xMax = x + width;
            int yMax = y + height;

            GridRegion[] array = s_OccupiedAreas.array;
            int count = s_OccupiedAreas.size;

            for (int i = 0; i < count; i++) {
                ref GridRegion check = ref array[i];
                if (!(y >= check.yMax || yMax <= check.yMin || xMax <= check.xMin || x >= check.xMax)) {
                    return false;
                }
            }

            return true;
        }

        private static void EnsureImplicitTrackCapacity(StructList<GridTrack> tracksList, int count, ref int autoSize, IReadOnlyList<GridTrackSize> autoSizes) {
            if (count >= tracksList.size) {
                tracksList.EnsureCapacity(count);

                int idx = tracksList.size;
                int toCreate = count - tracksList.size;

                for (int i = 0; i < toCreate; i++) {
                    ref GridTrack track = ref tracksList.array[idx++];
                    GridTrackSize size = autoSizes[autoSize];
                    track.minValue = size.minValue;
                    track.minUnit = size.maxUnit;
                    track.maxValue = size.maxValue;
                    track.maxUnit = size.maxUnit;
                    track.autoPlacementCursor = 0;
                    track.size = 0;
                    track.position = 0;
                    autoSize = (autoSize + 1) % autoSizes.Count;
                }

                tracksList.size = idx;
            }
        }

        public override void AddChild(FastLayoutBox child) {
            base.AddChild(child);
            placementDirty = true;
            CreateOrUpdatePlacement(child);
        }

        public override void RemoveChild(FastLayoutBox child) {
            for (int i = 0; i < placementList.size; i++) {
                if (placementList.array[i].layoutBox == child) {
                    placementList.RemoveAt(i);
                    return;
                }
            }
        }

        protected override void OnChildStyleChanged(FastLayoutBox child, StructList<StyleProperty> properties) {
            bool markedForLayout = false;
            for (int i = 0; i < properties.Count; i++) {
                StyleProperty property = properties[i];
                switch (property.propertyId) {
                    case StylePropertyId.PreferredHeight:
                    case StylePropertyId.PreferredWidth:
                    case StylePropertyId.MaxHeight:
                    case StylePropertyId.MaxWidth:
                    case StylePropertyId.MarginTop:
                    case StylePropertyId.MarginRight:
                    case StylePropertyId.MarginBottom:
                    case StylePropertyId.MarginLeft:
                    case StylePropertyId.PaddingTop:
                    case StylePropertyId.PaddingRight:
                    case StylePropertyId.PaddingBottom:
                    case StylePropertyId.PaddingLeft:
                    case StylePropertyId.GridLayoutColAutoSize:
                    case StylePropertyId.GridLayoutRowAutoSize:
                    case StylePropertyId.GridLayoutColTemplate:
                    case StylePropertyId.GridLayoutRowTemplate:
                    case StylePropertyId.LayoutBehavior:
                    case StylePropertyId.GridItemHeight:
                    case StylePropertyId.GridItemWidth:
                    case StylePropertyId.GridItemY:
                    case StylePropertyId.GridItemX:
                    case StylePropertyId.GridLayoutColGap:
                    case StylePropertyId.GridLayoutRowGap:
                        placementDirty = true;
                        markedForLayout = true;
                        CreateOrUpdatePlacement(child);
                        break;
                }
            }

            if (markedForLayout) {
                placementDirty = true;
                MarkForLayout();
            }
        }

        internal struct GridRegion {

            public int xMin;
            public int yMin;
            public int xMax;
            public int yMax;

        }

        internal struct GridPlacement {

            public int x;
            public int y;
            public int width;
            public int height;
            public FastLayoutBox layoutBox;
            public OffsetRect margin;
            public SizeConstraints size;
            public float outputWidth;
            public float outputHeight;

        }

        internal struct GridTrack {

            public float position;
            public float size;
            public int autoPlacementCursor;
            public float minValue;
            public float maxValue;
            public GridTemplateUnit minUnit;
            public GridTemplateUnit maxUnit;
            public float maxSize;

            public GridTrack(in GridTrackSize template) {
                this.minValue = template.minValue;
                this.minUnit = template.minUnit;
                this.maxValue = template.maxValue;
                this.maxUnit = template.maxUnit;
                this.position = 0;
                this.size = 0;
                this.autoPlacementCursor = 0;
                this.maxSize = 0;
            }

        }

    }

}