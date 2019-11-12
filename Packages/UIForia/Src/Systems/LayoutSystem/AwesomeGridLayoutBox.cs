using System;
using System.Collections.Generic;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Util;

namespace UIForia.Systems {

    public class AwesomeGridLayoutBox : AwesomeLayoutBox {

        private bool placementDirty;
        private readonly StructList<GridTrack> colTrackList;
        private readonly StructList<GridTrack> rowTrackList;
        private readonly StructList<GridPlacement> placementList;
        private static readonly StructList<GridRegion> s_OccupiedAreas = new StructList<GridRegion>(32);

        protected override float ComputeContentWidth() {
            if (firstChild == null) {
                return 0;
            }

            Place();

            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetWidths(ref placement.widthData);
                placement.outputWidth = placement.widthData.Clamped + placement.widthData.marginStart + placement.widthData.marginEnd;
            }

            ResolveHorizontalTrackWidths(widthBlock, size.width - paddingBox.left - paddingBox.right - borderBox.left - borderBox.right);

            PositionTracks(colTrackList, element.style.GridLayoutColGap);
            return 0;
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

        protected override float ComputeContentHeight() {
            // placement will never be dirty here

            return 0;
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) { }

        public override void RunLayoutHorizontal(int frameId) { }

        public override void RunLayoutVertical(int frameId) { }

        private void Place() {
            if (!placementDirty) {
                return;
            }

            placementDirty = false;

            placementList.size = 0;
            AwesomeLayoutBox child = firstChild;

            while (child != null) {
                if (child.element.isDisabled) {
                    child = child.nextSibling;
                    continue;
                }

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

            float contentAreaWidth = finalWidth - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);
            float contentAreaHeight = finalHeight - (paddingBorderVerticalStart + paddingBorderVerticalEnd);

            float horizontalGutter = element.style.GridLayoutColGap;
            float verticalGutter = element.style.GridLayoutRowGap;

            GenerateExplicitTracksForAxis(colTemplate, widthSize, contentAreaWidth, colTrackList, horizontalGutter);
            GenerateExplicitTracksForAxis(rowTemplate, heightSize, contentAreaHeight, rowTrackList, verticalGutter);
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

        private void PlaceBothAxisLocked() {
            for (int i = 0; i < placementList.Count; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                if (placement.y >= 0 && placement.x >= 0) {
                    FastGridLayoutBox.GridRegion region = new FastGridLayoutBox.GridRegion();
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

                    s_OccupiedAreas.Add(new FastGridLayoutBox.GridRegion() {
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

                    s_OccupiedAreas.Add(new FastGridLayoutBox.GridRegion() {
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

                s_OccupiedAreas.Add(new FastGridLayoutBox.GridRegion() {
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

            FastGridLayoutBox.GridRegion[] array = s_OccupiedAreas.array;
            int count = s_OccupiedAreas.size;

            for (int i = 0; i < count; i++) {
                ref FastGridLayoutBox.GridRegion check = ref array[i];
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

        private float ResolveFixedGridMeasurementWidth(float value, GridTemplateUnit unit) {
            switch (unit) {
                case GridTemplateUnit.Pixel:
                    return value;

                case GridTemplateUnit.Em:
                    return value * element.style.GetResolvedFontSize() * value;

                case GridTemplateUnit.ViewportWidth:
                    return value * element.View.Viewport.width;

                case GridTemplateUnit.ViewportHeight:
                    return value * element.View.Viewport.height;

                case GridTemplateUnit.ParentSize: {
                    if ((flags & AwesomeLayoutBoxFlags.Ignored) != 0) {
                        LayoutResult parentResult = element.layoutResult.layoutParent;
                        return Math.Max(0, parentResult.actualSize.width * value);
                    }

                    AwesomeLayoutBox ptr = parent;
                    while (ptr != null) {
                        if ((ptr.flags & AwesomeLayoutBoxFlags.WidthBlockProvider) != 0) {
                            return Math.Max(0, ptr.finalWidth * value);
                        }

                        ptr = ptr.parent;
                    }

                    return Math.Max(0, element.View.Viewport.width * value);
                }

                case GridTemplateUnit.ParentContentArea: {
                    return blockWidth.contentAreaSize * value;
                }

                case GridTemplateUnit.FractionalRemaining:
                case GridTemplateUnit.MinContent:
                case GridTemplateUnit.MaxContent:
                    return 0f;

                default:
                    return 0f;
            }
        }


        internal struct GridPlacement {

            public int x;
            public int y;
            public int width;
            public int height;
            public AwesomeLayoutBox layoutBox;
            public LayoutSize widthData;
            public float outputWidth;
            public float outputHeight;

        }

        internal struct GridRegion {

            public int xMin;
            public int yMin;
            public int xMax;
            public int yMax;

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