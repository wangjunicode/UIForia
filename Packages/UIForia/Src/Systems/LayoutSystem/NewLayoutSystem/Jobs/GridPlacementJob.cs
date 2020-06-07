using UIForia.Layout.LayoutTypes;
using UIForia.ListTypes;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Jobs;

namespace UIForia.Layout {

    [BurstCompile]
    public struct GridPlacementJob : IJob {

        public List_GridTrack.Shared colTrackList;
        public List_GridTrack.Shared rowTrackList;
        public List_GridPlacement placementList;
        public LayoutDirection layoutDirection;
        public GridLayoutDensity density;

        private DataList<GridRegion> occupiedAreas;

        public DataList<GridTrackSize> colTemplate;
        public DataList<GridTrackSize> rowTemplate;
        public DataList<GridTrackSize> autoColSizePattern;
        public DataList<GridTrackSize> autoRowSizePattern;

        public void Execute() {
            occupiedAreas = new DataList<GridRegion>(128, Allocator.Temp);

            Place();

            occupiedAreas.Dispose();
        }

        private void Place() {

            GenerateExplicitTracks();

            occupiedAreas.size = 0;

            int rowSizeAutoPtr = 0;
            int colSizeAutoPtr = 0;

            PreAllocateRowAndColumns(ref colSizeAutoPtr, ref rowSizeAutoPtr);

            PlaceBothAxisLocked();

            PlaceSingleAxisLocked(ref colSizeAutoPtr, ref rowSizeAutoPtr);

            PlaceRemainingItems(ref colSizeAutoPtr, ref rowSizeAutoPtr);
        }

        private static void GenerateExplicitTracksForAxis(ref DataList<GridTrackSize> templateList, List_GridTrack.Shared trackList) {
            int idx = 0;

            trackList.size = 0;

            trackList.EnsureCapacity(templateList.size);

            for (int i = 0; i < templateList.size; i++) {
                trackList[idx++] = new GridTrack(templateList[i].cell);
            }

            trackList.size = idx;
        }

        private void GenerateExplicitTracks() {
            colTrackList.size = 0;
            rowTrackList.size = 0;

            if (rowTemplate.size == 0 && colTemplate.size == 0) {
                return;
            }

            GenerateExplicitTracksForAxis(ref colTemplate, colTrackList);
            GenerateExplicitTracksForAxis(ref rowTemplate, rowTrackList);
        }

        private void PlaceBothAxisLocked() {
            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList[i];

                if (placement.y >= 0 && placement.x >= 0) {
                    occupiedAreas.Add(new GridRegion {
                        xMin = placement.x,
                        yMin = placement.y,
                        xMax = placement.x + placement.width,
                        yMax = placement.y + placement.height
                    });
                }
            }
        }

        private void PlaceSingleAxisLocked(ref int colSizeAutoPtr, ref int rowSizeAutoPtr) {
            bool dense = density == GridLayoutDensity.Dense;

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList[i];

                int x = placement.x;
                int y = placement.y;
                int width = placement.width;
                int height = placement.height;

                // x axis is in a fixed position, we need to find a valid Y
                if (y < 0 && x >= 0) {
                    int cursorY = dense ? 0 : colTrackList[x].autoPlacementCursor;

                    while (!IsGridAreaAvailable(x, cursorY, width, height)) {
                        cursorY++;
                    }

                    EnsureImplicitTrackCapacity(rowTrackList, cursorY + height, ref rowSizeAutoPtr, autoRowSizePattern);
                    EnsureImplicitTrackCapacity(colTrackList, x + width, ref rowSizeAutoPtr, autoRowSizePattern);

                    colTrackList[x].autoPlacementCursor = cursorY;

                    placement.y = cursorY;

                    occupiedAreas.Add(new GridRegion() {
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

                    rowTrackList[y].autoPlacementCursor = cursorX;

                    placement.x = cursorX;

                    occupiedAreas.Add(new GridRegion() {
                        xMin = placement.x,
                        yMin = placement.y,
                        xMax = placement.x + placement.width,
                        yMax = placement.y + placement.height
                    });
                }
            }
        }

        private void PreAllocateRowAndColumns(ref int colPtr, ref int rowPtr) {
            int maxColStartAndSpan = 0;
            int maxRowStartAndSpan = 0;

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList[i];
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

            EnsureImplicitTrackCapacity(colTrackList, maxColStartAndSpan, ref colPtr, autoColSizePattern);
            EnsureImplicitTrackCapacity(rowTrackList, maxRowStartAndSpan, ref rowPtr, autoRowSizePattern);
        }

        private void PlaceRemainingItems(ref int colSizeAutoPtr, ref int rowSizeAutoPtr) {
            if (placementList.size == 0) {
                return;
            }

            bool flowHorizontal = layoutDirection == LayoutDirection.Horizontal;
            bool dense = density == GridLayoutDensity.Dense;

            int sparseStartX = 0; // purposefully not reading autoCursor value because that results in weird behavior for sparse grids (this is not the same as css!)
            int sparseStartY = 0; // purposefully not reading autoCursor value because that results in weird behavior for sparse grids (this is not the same as css!)

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList[i];

                int width = placement.width;
                int height = placement.height;

                int cursorX;
                int cursorY;

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
                            cursorX = !dense ? rowTrackList[cursorY].autoPlacementCursor : 0;
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
                    colTrackList[cursorX].autoPlacementCursor = cursorY;
                    for (int j = cursorY; j < cursorY + height; j++) {
                        rowTrackList[j].autoPlacementCursor = cursorX + width;
                    }
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
                            cursorY = !dense ? colTrackList[cursorX].autoPlacementCursor : 0;
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
                    rowTrackList[cursorY].autoPlacementCursor = cursorX;
                    for (int j = cursorX; j < cursorX + width; j++) {
                        colTrackList[j].autoPlacementCursor = cursorY + height;
                    }

                    EnsureImplicitTrackCapacity(colTrackList, cursorX + width, ref colSizeAutoPtr, autoColSizePattern);
                    EnsureImplicitTrackCapacity(rowTrackList, cursorY + height, ref rowSizeAutoPtr, autoRowSizePattern);
                }

                occupiedAreas.Add(new GridRegion() {
                    xMin = placement.x,
                    yMin = placement.y,
                    xMax = placement.x + placement.width,
                    yMax = placement.y + placement.height
                });
            }
        }

        private bool IsGridAreaAvailable(int x, int y, int width, int height) {
            int xMax = x + width;
            int yMax = y + height;

            for (int i = 0; i < occupiedAreas.size; i++) {

                ref GridRegion check = ref occupiedAreas[i];

                if (!(y >= check.yMax || yMax <= check.yMin || xMax <= check.xMin || x >= check.xMax)) {
                    return false;
                }
            }

            return true;
        }

        private static void EnsureImplicitTrackCapacity(List_GridTrack.Shared tracksList, int count, ref int autoSize, in DataList<GridTrackSize> autoSizes) {
            if (count >= tracksList.size) {
                tracksList.EnsureCapacity(count);

                int idx = tracksList.size;
                int toCreate = count - tracksList.size;

                for (int i = 0; i < toCreate; i++) {
                    tracksList[idx++] = new GridTrack(autoSizes[autoSize].cell);
                    autoSize = (autoSize + 1) % autoSizes.size;
                }

                tracksList.size = idx;
            }
        }

        private struct GridRegion {

            public int xMin;
            public int yMin;
            public int xMax;
            public int yMax;

        }

    }

}