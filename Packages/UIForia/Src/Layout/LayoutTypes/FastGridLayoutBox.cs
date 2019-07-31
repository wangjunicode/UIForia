using System;
using System.Collections.Generic;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout.LayoutTypes {

    public class FastGridLayoutBox : FastLayoutBox {

        private bool placementDirty;

        private readonly StructList<GridTrack> rowTrackList;
        private readonly StructList<GridTrack> colTrackList;
        private readonly StructList<GridPlacement> placementList;

        private static readonly StructList<GridRegion> s_OccupiedAreas = new StructList<GridRegion>(32);

        public FastGridLayoutBox() {
            this.placementDirty = true;
            this.rowTrackList = new StructList<GridTrack>();
            this.colTrackList = new StructList<GridTrack>();
            this.placementList = new StructList<GridPlacement>();
        }

        public override float GetIntrinsicMinWidth() {
            return 0;
        }

        public override float GetIntrinsicMinHeight() {
            return 0;
        }

        public override float GetIntrinsicPreferredWidth() {
            return 0;
        }

        public override float GetIntrinsicPreferredHeight() {
            return 0;
        }

        public override float ComputeContentWidth(BlockSize blockWidth) {
            return 0;
        }

        public override float ComputeContentHeight(float width, BlockSize blockWidth, BlockSize blockHeight) {
            return 0;
        }

        protected override void PerformLayout() {
            if (firstChild == null) {
                return;
            }

            BlockSize widthBlock = containingBoxWidth;
            BlockSize heightBlock = containingBoxHeight;

            Place();

            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetWidth(widthBlock, ref placement.size);
                placement.layoutBox.GetMarginHorizontal(widthBlock, ref placement.margin);
                placement.outputWidth = placement.size.prefWidth + placement.margin.left + placement.margin.right;
            }
            
            ResolveColumnTrackWidths(size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right);
            

        }

        private void ResolveColumnTrackWidths(in BlockSize blockWidth, float remaining) {
            int flexPieces = 0;

            List<ValueTuple<int, GridTrack>> intrinsics = ListPool<ValueTuple<int, GridTrack>>.Get();
            List<ValueTuple<int, GridTrack>> flexes = ListPool<ValueTuple<int, GridTrack>>.Get();

            GridTrack[] colTracks = colTrackList.array;
            int colTrackCount = colTrackList.size;
            
            for (int i = 0; i < colTrackCount; i++) {
                ref GridTrack track = ref  colTracks[i];

                if ((track.minUnit & GridTemplateUnit.Fixed) != 0) {
                    track.outputSize = ResolveFixedWidthMeasurement(blockWidth, track.size, SizeType.Min);
                    remaining -= track.outputSize;
                }
                else if (track.minUnit == GridTemplateUnit.FractionalRemaining) {
                    flexes.Add(ValueTuple.Create(i, track));
                    flexPieces += (int) track.minValue; // move flex to max only?
                }
                else {
                    intrinsics.Add(ValueTuple.Create(i, track));
                }
            }

            for (int i = 0; i < intrinsics.Count; i++) {
                GridTrack track = intrinsics[i].Item2;

                if (track.size.minUnit == GridTemplateUnit.MinContent) {
                    track.outputSize = ResolveContentMinWidth(track, SizeType.Min);
                }
                else if (track.size.minUnit == GridTemplateUnit.MaxContent) {
                    track.outputSize = ResolveContentMaxWidth(track, SizeType.Min);
                }

                remaining -= track.outputSize;
                m_ColTracks[intrinsics[i].Item1] = track;
            }

            float colGap = style.GridLayoutColGap;
            remaining -= colGap * (m_ColTracks.Count - 1);

            if ((int) remaining > 0 && flexes.Count > 0) {
                float pieceSize = remaining / flexPieces;
                for (int i = 0; i < flexes.Count; i++) {
                    GridTrack track = flexes[i].Item2;
                    track.outputSize = track.size.minValue * pieceSize;
                    m_ColTracks[flexes[i].Item1] = track;
                }
            }

            PositionColumnTracks();
            ListPool<ValueTuple<int, GridTrack>>.Release(ref intrinsics);
            ListPool<ValueTuple<int, GridTrack>>.Release(ref flexes);
        }
        
        private enum SizeType {

            Min,
            Max

        }
        
        private float ResolveContentMinWidth(GridTrack track, SizeType sizeType) {
            float minSize = 0;

            GridTemplateUnit unit = track.size.minUnit;
            float value = track.size.minValue;

            if (sizeType == SizeType.Max) {
                value = track.size.maxValue;
            }

            for (int i = 0; i < track.spanningItems.Count; i++) {
                int childIndex = track.spanningItems[i];
                GridItem colItem = m_Placements[childIndex].colItem;
                int pieces = 0; // never 0 because we only call this function for intrinsic sized tracks
                GridItemSizes gridItemSizes = m_Widths[childIndex];
                float baseWidth = gridItemSizes.outputSize;

                for (int j = colItem.trackStart; j < colItem.trackStart + colItem.trackSpan; j++) {
                    GridTrack spanned = m_ColTracks[j];
                    if ((unit & GridTemplateUnit.Fixed) != 0) {
                        baseWidth -= spanned.outputSize;
                        if (baseWidth <= 0) {
                            break;
                        }
                    }
                    else if (unit != GridTemplateUnit.FractionalRemaining) {
                        pieces++;
                    }
                }

                if (minSize == 0) {
                    minSize = baseWidth;
                }

                if (baseWidth > 0) {
                    minSize = Mathf.Min(minSize, value * (baseWidth / pieces));
                }
            }

            return minSize;
        }
        
        private float ResolveFixedWidthMeasurement(in BlockSize blockWidth, GridTrackSize size, SizeType sizeType) {
            GridTemplateUnit unit = size.maxUnit;
            float value = size.maxValue;

            if (sizeType == SizeType.Min) {
                unit = size.minUnit;
                value = size.minValue;
            }

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

        private int ResolveColumnStart(string name) {
            return -1;
        }

        private int ResolveColumnSpan(int resolvedColStart, string name) {
            return 1;
        }

        private int ResolveRowStart(string name) {
            return -1;
        }

        private int ResolveRowSpan(int resolvedRowStart, string name) {
            return 1;
        }

        private void CreateOrUpdatePlacement(FastLayoutBox child) {
            GridPlacement[] placements = placementList.array;

            GridItemPlacement colStart = child.element.style.GridItemColStart;
            GridItemPlacement colSpan = child.element.style.GridItemColSpan;
            GridItemPlacement rowStart = child.element.style.GridItemRowStart;
            GridItemPlacement rowSpan = child.element.style.GridItemRowSpan;

            GridPlacement placement = default;

            placement.layoutBox = child;
            placement.colStart = colStart.name != null ? ResolveColumnStart(colStart.name) : colStart.index;
            placement.colSpan = colSpan.name != null ? ResolveColumnSpan(placement.colStart, colSpan.name) : colSpan.index;
            placement.rowStart = rowStart.name != null ? ResolveRowStart(rowStart.name) : rowStart.index;
            placement.rowSpan = rowSpan.name != null ? ResolveRowSpan(placement.rowStart, rowSpan.name) : rowSpan.index;

            for (int i = 0; i < placementList.size; i++) {
                if (placements[i].layoutBox == child) {
                    placements[i] = placement;
                    break;
                }
            }

            placementList.Add(placement);
        }

        private bool ValidateTrackSizes() {
            IReadOnlyList<GridTrackSize> rowTemplate = element.style.GridLayoutRowTemplate;
            bool usingFitOrFill = false;
            bool hasIntrinsics = false;

            for (int i = 0; i < rowTemplate.Count; i++) {
                GridTrackSize template = rowTemplate[i];

                switch (template.type) {
                    case GridTrackSizeType.Value:
                        break;
                    case GridTrackSizeType.Repeat:
                        break;
                    case GridTrackSizeType.MinMax:
                        break;
                    case GridTrackSizeType.RepeatFill:
                    case GridTrackSizeType.RepeatFit:
                        usingFitOrFill = true;
                        for (int j = 0; j < template.pattern.Length; j++) {
                            GridTrackSize innerTemplate = template.pattern[j];

                            switch (innerTemplate.type) {
                                case GridTrackSizeType.Value:
                                    break;
                                case GridTrackSizeType.MinMax:
                                    break;
                                case GridTrackSizeType.Repeat:
                                case GridTrackSizeType.RepeatFit:
                                case GridTrackSizeType.RepeatFill:
                                    break;
                                default:
                                    throw new ArgumentOutOfRangeException();
                            }
                        }

                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            return true;
            // minmax(400, 500);
            // grow(start, max);
            // shrink(start, min);
            // clamp(fixed, min, max);
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

        private void GenerateExplicitTracks(BlockSize widthSize, BlockSize height) {

            rowTrackList.size = 0;
            colTrackList.size = 0;

            IReadOnlyList<GridTrackSize> rowTemplate = element.style.GridLayoutRowTemplate;
            IReadOnlyList<GridTrackSize> colTemplate = element.style.GridLayoutColTemplate;

            int idx = 0;

            rowTrackList.EnsureCapacity(rowTemplate.Count);
            colTrackList.EnsureCapacity(colTemplate.Count);

            float totalMin = 0;

            // find auto fill repeats
            // add fixed sizes

            // do one iteration

            for (int i = 0; i < rowTemplate.Count; i++) {
                GridTrackSize template = rowTemplate[i];
                switch (template.type) {
                    
                    case GridTrackSizeType.MinMax:
                    case GridTrackSizeType.Value:
                        totalMin += ResolveMin(widthSize, template.minValue, template.minUnit);
                        break;
                    
                    case GridTrackSizeType.Repeat:
                    
                        for (int j = 0; j < template.pattern.Length; j++) {
                            totalMin += ResolveMin(widthSize, template.pattern[j].minValue, template.pattern[j].minUnit);
                        }
                        
                        break;
                    
                    case GridTrackSizeType.RepeatFit:
                    case GridTrackSizeType.RepeatFill:
                        throw new NotImplementedException();
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            for (int i = 0; i < rowTemplate.Count; i++) {
                GridTrackSize template = rowTemplate[i];

                switch (template.type) {
                    case GridTrackSizeType.MinMax:
                    case GridTrackSizeType.Value: {
                        GridTrack track = default;
                        track.minValue = template.minValue;
                        track.minUnit = template.minUnit;
                        track.maxValue = template.maxValue;
                        track.maxUnit = template.maxUnit;
                        rowTrackList.array[idx++] = track;
                        break;
                    }

                    case GridTrackSizeType.Repeat: {
                        rowTrackList.EnsureAdditionalCapacity(template.pattern.Length);

                        for (int j = 0; j < template.pattern.Length; j++) {
                            GridTrack track = default;
                            track.minValue = template.pattern[j].minValue;
                            track.minUnit = template.pattern[j].minUnit;
                            track.maxValue = template.pattern[j].maxValue;
                            track.maxUnit = template.pattern[j].maxUnit;
                            rowTrackList.array[idx++] = track;
                        }

                        break;
                    }

                    case GridTrackSizeType.RepeatFill:
                    case GridTrackSizeType.RepeatFit:
                        throw new NotImplementedException();
                    
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            rowTrackList.size = idx;
            
        }

        private void Place() {
            if (!placementDirty) {
                return;
            }

            CreateNamedGridAreas();

            GenerateExplicitTracks(default, default);

            StructList<GridPlacement> remainingItems = StructList<GridPlacement>.GetMinSize(placementList.size);

            FastLayoutBox child = firstChild;

            // todo -- move out of here
            while (child != null) {
                CreateOrUpdatePlacement(child);
                child = child.nextSibling;
            }

            s_OccupiedAreas.size = 0;

            int colTrackCount = 0;
            int rowTrackCount = 0;

            int idx = 0;

            for (int i = 0; i < placementList.Count; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                if (placement.colStart >= 0 && placement.rowStart >= 0) {
                    GridRegion region = new GridRegion();
                    region.xMin = placement.rowStart;
                    region.yMin = placement.colStart;
                    region.xMax = placement.rowStart + placement.rowSpan;
                    region.yMax = placement.colStart + placement.colSpan;
                    s_OccupiedAreas.Add(region);
                }
                else {
                    remainingItems[idx++] = placement;
                }
            }

            remainingItems.size = idx;

            PlaceRemainingItems(remainingItems);
            placementDirty = false;

            StructList<GridPlacement>.Release(ref remainingItems);
        }

        private void PlaceRemainingItems(StructList<GridPlacement> placementList) {
            bool flowHorizontal = element.style.GridLayoutDirection == LayoutDirection.Horizontal;
            bool dense = element.style.GridLayoutDensity == GridLayoutDensity.Dense;

            LayoutDirection direction = element.style.GridLayoutDirection;

            GridTrackSize[] autoColSizePattern;
            GridTrackSize[] autoRowSizePattern;

            // todo -- change auto size to accept a list
            if (direction == LayoutDirection.Horizontal) {
                autoColSizePattern = new[] {element.style.GridLayoutMainAxisAutoSize};
                autoRowSizePattern = new[] {element.style.GridLayoutCrossAxisAutoSize};
            }
            else {
                autoColSizePattern = new[] {element.style.GridLayoutCrossAxisAutoSize};
                autoRowSizePattern = new[] {element.style.GridLayoutMainAxisAutoSize};
            }

            int sparseStartX = 0;
            int sparseStartY = 0;

            int rowSizeAutoPtr = 0;
            int colSizeAutoPtr = 0;

            GridTrack[] rowTracks = rowTrackList.array;
            GridTrack[] colTracks = colTrackList.array;

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                int rowStart = placement.rowStart;
                int rowSpan = placement.rowSpan;
                int colStart = placement.colStart;
                int colSpan = placement.colSpan;

                int colCount = colTrackList.Count;
                int rowCount = rowTrackList.Count;

                int cursorX = 0;
                int cursorY = 0;

                // if row was fixed we definitely created it in an earlier step
                if (rowStart >= 0) {
                    cursorX = dense ? 0 : rowTrackList[rowStart].autoPlacementCursor;

                    while (!IsGridAreaAvailable(cursorX, colSpan, rowStart, rowSpan)) {
                        cursorX++;
                    }

                    CreateTracks(colTrackList, cursorX + colSpan, ref colSizeAutoPtr, autoColSizePattern);

                    rowTracks[rowStart].autoPlacementCursor = cursorX;
                    placement.colStart = cursorX;
                }
                else if (colStart >= 0) {
                    cursorY = dense ? 0 : colTracks[colStart].autoPlacementCursor;

                    while (!IsGridAreaAvailable(colStart, colSpan, cursorY, rowSpan)) {
                        cursorY++;
                    }

                    CreateTracks(rowTrackList, cursorY + rowSpan, ref rowSizeAutoPtr, autoRowSizePattern);

                    colTracks[colStart].autoPlacementCursor = cursorY;

                    placement.rowStart = cursorY;
                }
                else if (flowHorizontal) {
                    if (dense) {
                        cursorX = 0;
                        cursorY = 0;
                    }
                    else {
                        cursorX = sparseStartX;
                        cursorY = sparseStartY;
                    }

                    while (true) {
                        if (cursorX + colSpan > colCount) {
                            cursorY++;
                            CreateTracks(rowTrackList, cursorY + rowSpan, ref rowSizeAutoPtr, autoRowSizePattern);
                            cursorX = !dense ? rowTracks[cursorY].autoPlacementCursor : 0;
                            continue;
                        }

                        if (IsGridAreaAvailable(cursorX, colSpan, cursorY, rowSpan)) {
                            break;
                        }

                        cursorX++;
                    }

                    sparseStartX = cursorX + colSpan;
                    sparseStartY = cursorY;
                    placement.colStart = cursorX;
                    placement.rowStart = cursorY;
                    CreateTracks(colTrackList, cursorX + colSpan, ref colSizeAutoPtr, autoColSizePattern);
                    colTracks[cursorX].autoPlacementCursor = cursorY;
                    rowTracks[cursorY].autoPlacementCursor = cursorX;
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
                        if (cursorY + rowSpan > rowCount) {
                            cursorX++;
                            CreateTracks(colTrackList, cursorX + colSpan, ref colSizeAutoPtr, autoColSizePattern);
                            cursorY = !dense ? colTracks[cursorX].autoPlacementCursor : 0;
                            continue;
                        }

                        if (IsGridAreaAvailable(cursorX, colSpan, cursorY, rowSpan)) {
                            break;
                        }

                        cursorY++;
                    }

                    sparseStartX = cursorX;
                    sparseStartY = cursorY + rowSpan;
                    placement.colStart = cursorX;
                    placement.rowStart = cursorY;
                    colTracks[cursorX].autoPlacementCursor = cursorY;
                    rowTracks[cursorY].autoPlacementCursor = cursorX;
                    CreateTracks(colTrackList, cursorX + colSpan, ref colSizeAutoPtr, autoColSizePattern);
                    CreateTracks(rowTrackList, cursorY + rowSpan, ref rowSizeAutoPtr, autoRowSizePattern);
                }

                s_OccupiedAreas.Add(new GridRegion() {
                    xMin = placement.rowStart,
                    yMin = placement.colStart,
                    xMax = placement.rowStart + placement.rowSpan,
                    yMax = placement.colStart + placement.colSpan
                });
            }
        }

        private static bool IsGridAreaAvailable(int colStart, int colSpan, int rowStart, int rowSpan) {
            int xMax = rowStart + rowSpan;
            int yMax = colStart + colSpan;

            GridRegion[] array = s_OccupiedAreas.array;
            int count = s_OccupiedAreas.size;

            for (int i = 0; i < count; i++) {
                ref GridRegion check = ref array[i];
                if (colStart >= check.yMax || yMax <= check.yMin || xMax <= check.xMin || rowStart >= check.xMax) {
                    return false;
                }
            }

            return true;
        }

        private static int CreateTracks(StructList<GridTrack> tracksList, int count, ref int autoSize, GridTrackSize[] autoSizes) {
            int total = count - tracksList.size;
            if (total > 0) {
                tracksList.EnsureAdditionalCapacity(total);
            }

            int idx = tracksList.size;

            for (int i = 0; i < total; i++) {
                ref GridTrack track = ref tracksList.array[idx++];
                track.minValue = autoSizes[autoSize].minValue;
                track.minUnit = autoSizes[autoSize].maxUnit;
                track.maxValue = autoSizes[autoSize].maxValue;
                track.maxUnit = autoSizes[autoSize].maxUnit;
                autoSize = (autoSize + 1) % autoSizes.Length;
            }

            return total;
        }

        public struct GridRegion {

            public int xMin;
            public int yMin;
            public int xMax;
            public int yMax;

        }

        public struct GridPlacement {

            public int colStart;
            public int colSpan;
            public int rowStart;
            public int rowSpan;
            public FastLayoutBox layoutBox;
            public OffsetRect margin;
            public SizeConstraints size;
            public float outputWidth;
            public float outputHeight;

        }

        public struct GridTrack {

            public float position;
            public float outputSize;
            public int autoPlacementCursor;
            public float minValue;
            public float maxValue;
            public GridTemplateUnit minUnit;
            public GridTemplateUnit maxUnit;

        }

    }

}