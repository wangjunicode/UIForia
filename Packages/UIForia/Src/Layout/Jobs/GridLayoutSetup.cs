using System;
using UIForia.Layout.LayoutTypes;
using UIForia.Style;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;

namespace UIForia.Layout {

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct GridLayoutSetup : IJob {

        public float appWidth;
        public float appHeight;

        public DataList<GridTemplate> styleDatabaseTemplateTable; // from style db
        public DataList<GridCellDefinition> styleDatabaseCellTable; // from style db

        [NativeDisableUnsafePtrRestriction] public float** emTablePtr;
        [NativeDisableUnsafePtrRestriction] public LayoutTree* layoutTree;
        [NativeDisableUnsafePtrRestriction] public StyleTables* styleTables;
        [NativeDisableUnsafePtrRestriction] public LockedBumpAllocator* perFrameBumpAllocator;

        [NativeDisableUnsafePtrRestriction] public GridLayoutData* mainAxisInfo;
        [NativeDisableUnsafePtrRestriction] public GridLayoutData* crossAxisInfo;
        public LayoutBoxType targetBoxType;
        public CheckedArray<Rect> viewRects;
        public CheckedArray<ushort> elementIdToViewId;

        public void Execute() {

            CheckedArray<float> emTable = new CheckedArray<float>(*emTablePtr, layoutTree->elementCount);

            int cnt = 0;

            for (int i = 0; i < layoutTree->elementCount; i++) {
                if (layoutTree->nodeList[i].layoutBoxType == targetBoxType) cnt++;
            }

            if (cnt == 0) {
                return;
            }

            TempList<GridInfo> gridInfoList = TypedUnsafe.MallocUnsizedTempList<GridInfo>(cnt, Allocator.Temp);

            for (int i = 0; i < layoutTree->elementCount; i++) {
                if (layoutTree->nodeList[i].layoutBoxType == targetBoxType) {
                    gridInfoList[gridInfoList.size++] = new GridInfo() {
                        boxId = i,
                        elementId = layoutTree->nodeList[i].elementId,
                        childRange = layoutTree->nodeList[i].childRange
                    };
                    if (gridInfoList.size == cnt) break;
                }
            }

            int placementCount = 0;

            if (targetBoxType == LayoutBoxType.GridHorizontal) {

                for (int i = 0; i < gridInfoList.size; i++) {
                    ref GridInfo gridInfo = ref gridInfoList.Get(i);

                    int elementIndex = gridInfo.elementId.index;

                    ushort colTemplateId = styleTables->GridLayoutColTemplate[elementIndex].templateId;
                    ushort rowTemplateId = styleTables->GridLayoutRowTemplate[elementIndex].templateId;

                    gridInfo.isDense = styleTables->GridLayoutDensity[elementIndex] == GridLayoutDensity.Dense;
                    gridInfo.mainAxisTemplate = styleDatabaseTemplateTable[colTemplateId];
                    gridInfo.crossAxisTemplate = styleDatabaseTemplateTable[rowTemplateId];

                    placementCount += gridInfo.childRange.length;
                }
            }
            else {
                for (int i = 0; i < gridInfoList.size; i++) {
                    ref GridInfo gridInfo = ref gridInfoList.Get(i);

                    int elementIndex = gridInfo.elementId.index;

                    ushort colTemplateId = styleTables->GridLayoutColTemplate[elementIndex].templateId;
                    ushort rowTemplateId = styleTables->GridLayoutRowTemplate[elementIndex].templateId;

                    gridInfo.isDense = styleTables->GridLayoutDensity[elementIndex] == GridLayoutDensity.Dense;
                    gridInfo.mainAxisTemplate = styleDatabaseTemplateTable[rowTemplateId];
                    gridInfo.crossAxisTemplate = styleDatabaseTemplateTable[colTemplateId];

                    placementCount += gridInfo.childRange.length;
                }
            }

            GridItemPlacement* placementBuffer = perFrameBumpAllocator->Allocate<GridItemPlacement>(placementCount * 2);

            CheckedArray<GridItemPlacement> placementListX = new CheckedArray<GridItemPlacement>(placementBuffer, placementCount);
            CheckedArray<GridItemPlacement> placementListY = new CheckedArray<GridItemPlacement>(placementBuffer + placementCount, placementCount);

            placementCount = 0;
            for (int i = 0; i < gridInfoList.size; i++) {
                ref GridInfo gridInfo = ref gridInfoList.Get(i);

                int start = placementCount;

                for (int cIdx = gridInfo.childRange.start; cIdx < gridInfo.childRange.end; cIdx++) {
                    ElementId elementId = layoutTree->elementIdList[cIdx];

                    placementListX[placementCount] = styleTables->GridItemX[elementId.index];
                    placementListY[placementCount] = styleTables->GridItemY[elementId.index];
                    placementCount++;
                }

                gridInfo.placementRange = new RangeInt(start, gridInfo.childRange.length);

            }

            // min span must be 1
            for (int i = 0; i < placementCount; i++) {
                if (placementBuffer[i].span <= 0) placementBuffer[i].span = 1;
            }

            CheckedArray<GridItemPlacement> mainAxisList = placementListX;
            CheckedArray<GridItemPlacement> crossAxisList = placementListY;

            if (targetBoxType == LayoutBoxType.GridVertical) {
                mainAxisList = placementListY;
                crossAxisList = placementListX;
            }

            ClampMainAxisPlacements(gridInfoList, mainAxisList);

            int crossAxisCellCount = Place(gridInfoList, mainAxisList, crossAxisList);

            // init content contributions

            int mainAxisCellCount = 0;
            for (int i = 0; i < gridInfoList.size; i++) {
                ref GridInfo gridInfo = ref gridInfoList.Get(i);
                mainAxisCellCount += gridInfo.mainAxisTemplate.cellCount;
            }

            GridCell* mainAxisCellBuffer = perFrameBumpAllocator->Allocate<GridCell>(mainAxisCellCount);
            GridCell* crossAxisCellBuffer = perFrameBumpAllocator->Allocate<GridCell>(crossAxisCellCount);

            GridAxisInfo* mainAxisGridInfos = perFrameBumpAllocator->Allocate<GridAxisInfo>(gridInfoList.size * 2);

            mainAxisInfo->placements = mainAxisList;
            mainAxisInfo->gridCells = new CheckedArray<GridCell>(mainAxisCellBuffer, mainAxisCellCount);
            mainAxisInfo->gridAxisInfos = new CheckedArray<GridAxisInfo>(mainAxisGridInfos, gridInfoList.size);

            crossAxisInfo->placements = crossAxisList;
            crossAxisInfo->gridCells = new CheckedArray<GridCell>(crossAxisCellBuffer, crossAxisCellCount);
            crossAxisInfo->gridAxisInfos = new CheckedArray<GridAxisInfo>(mainAxisGridInfos + gridInfoList.size, gridInfoList.size);

            TempList<ViewIndexEm> viewIndexEms = TypedUnsafe.MallocSizedTempList<ViewIndexEm>(gridInfoList.size, Allocator.Temp);

            for (int i = 0; i < gridInfoList.size; i++) {
                int boxId = gridInfoList[i].boxId;
                
                float emSize = emTable[boxId];
                int viewIdx = elementIdToViewId[gridInfoList[i].elementId.index];
                viewIndexEms[i] = new ViewIndexEm() {
                    emSize = emSize,
                    viewIndex = viewIdx
                };
            }

            ResolveSizesAndIntrinsicsMainAxis(gridInfoList, mainAxisInfo, viewIndexEms);
            ResolveSizesAndIntrinsicsCrossAxis(gridInfoList, crossAxisInfo, viewIndexEms);

            viewIndexEms.Dispose();
            gridInfoList.Dispose();

        }

        private void ResolveSizesAndIntrinsicsMainAxis(TempList<GridInfo> gridInfoList, GridLayoutData* layoutData, TempList<ViewIndexEm> viewIndexEmSizes) {

            int writeIndex = 0;

            CheckedArray<GridCell> cellList = layoutData->GetCellList();

#if DEBUG
            if (layoutData->gridAxisInfos.size != gridInfoList.size) {
                throw new Exception("Invalid allocation of grid sizes");
            }
#endif

            for (int gridInfoIndex = 0; gridInfoIndex < gridInfoList.size; gridInfoIndex++) {

                ref GridInfo gridInfo = ref gridInfoList.Get(gridInfoIndex);
                int start = writeIndex;
                bool hasIntrinsicSize = false;

                int viewIdx = viewIndexEmSizes[gridInfoIndex].viewIndex;
                float emSize = viewIndexEmSizes[gridInfoIndex].emSize;

                GridTemplate template = gridInfo.mainAxisTemplate;

                for (int i = 0; i < template.cellCount; i++) {

                    GridCellDefinition cell = styleDatabaseCellTable[template.offset + i]; // copy on purpose! Don't manipulate style database information

                    bool isIntrinsic = (cell.unit == GridTemplateUnit.MaxContent || cell.unit == GridTemplateUnit.MinContent);
                    hasIntrinsicSize ^= isIntrinsic;

                    ResolvedGridCellSize sizeDef = MeasurementUtil.ResolveGridCellSize(viewRects, appWidth, appHeight, viewIdx, cell, emSize);

                    cellList[writeIndex++] = new GridCell() {
                        sizeDef = sizeDef,
                        maxContentContribution = 0,
                        minContentContribution = -1,
                        resolvedBaseSize = isIntrinsic ? 0 : sizeDef.value
                    };

                }

                layoutData->gridAxisInfos[gridInfoIndex] = new GridAxisInfo() {
                    boxId = gridInfo.boxId,
                    cellOffset = start,
                    childRange = gridInfo.childRange,
                    cellCount = (ushort) (writeIndex - start),
                    placementRange = gridInfo.placementRange,
                    hasIntrinsicSizes = hasIntrinsicSize,
                };

            }

        }

        private void ResolveSizesAndIntrinsicsCrossAxis(TempList<GridInfo> gridInfoList, GridLayoutData* layoutData, TempList<ViewIndexEm> viewIndexEmSizes) {

            CheckedArray<GridCell> cellList = layoutData->GetCellList();

#if DEBUG
            if (layoutData->gridAxisInfos.size != gridInfoList.size) {
                throw new Exception("Invalid allocation of grid sizes");
            }
#endif

            for (int gridInfoIndex = 0; gridInfoIndex < gridInfoList.size; gridInfoIndex++) {

                ref GridInfo gridInfo = ref gridInfoList.Get(gridInfoIndex);

                bool hasIntrinsicSize = false;

                int viewIdx = viewIndexEmSizes[gridInfoIndex].viewIndex;
                float emSize = viewIndexEmSizes[gridInfoIndex].emSize;

                GridTemplate template = gridInfo.crossAxisTemplate;

                int totalCellCount = gridInfo.crossAxisCellCount;

                for (int i = 0; i < totalCellCount; i++) {

                    int dbCellIndex = template.offset + (i % template.cellCount);

                    GridCellDefinition cell = styleDatabaseCellTable[dbCellIndex]; // copy on purpose! Don't manipulate style database information

                    bool isIntrinsic = (cell.unit == GridTemplateUnit.MaxContent || cell.unit == GridTemplateUnit.MinContent);
                    hasIntrinsicSize ^= isIntrinsic;

                    ResolvedGridCellSize sizeDef = MeasurementUtil.ResolveGridCellSize(viewRects, appWidth, appHeight, viewIdx, cell, emSize);

                    cellList[gridInfo.crossAxisTemplateStart + i] = new GridCell() {
                        sizeDef = sizeDef,
                        maxContentContribution = 0,
                        minContentContribution = -1,
                        resolvedBaseSize = isIntrinsic ? 0 : sizeDef.value
                    };

                }

                layoutData->gridAxisInfos[gridInfoIndex] = new GridAxisInfo() {
                    boxId = gridInfo.boxId,
                    childRange = gridInfo.childRange,
                    cellOffset = gridInfo.crossAxisTemplateStart,
                    cellCount = (ushort) gridInfo.crossAxisCellCount,
                    placementRange = gridInfo.placementRange,
                    hasIntrinsicSizes = hasIntrinsicSize,
                };

            }
        }

        private static int Place(TempList<GridInfo> gridInfoList, CheckedArray<GridItemPlacement> mainAxisPlacements, CheckedArray<GridItemPlacement> crossAxisPlacements) {

            int totalCrossAxisCellCount = 0;

            DataList<BitSet> occupiedMap = new DataList<BitSet>(8, Allocator.Temp);

            for (int gridIndex = 0; gridIndex < gridInfoList.size; gridIndex++) {

                ref GridInfo gridInfo = ref gridInfoList.Get(gridIndex);

                int placementStart = gridInfo.placementRange.start;
                int placementEnd = gridInfo.placementRange.end;

                int maxCross = 1;

                for (int p = placementStart; p < placementEnd; p++) {
                    ref GridItemPlacement placementCross = ref crossAxisPlacements.array[p];
                    if (placementCross.span > maxCross) {
                        maxCross = placementCross.span;
                    }

                    if (placementCross.place + placementCross.span > maxCross) {
                        maxCross = placementCross.place + placementCross.span;
                    }
                }

                occupiedMap.EnsureCapacity(maxCross);
                occupiedMap.SetSize(maxCross);
                occupiedMap.Clear();

                // place everything that ws explicitly positioned
                for (int p = placementStart; p < placementEnd; p++) {
                    ref GridItemPlacement placementMain = ref mainAxisPlacements.Get(p);
                    ref GridItemPlacement placementCross = ref crossAxisPlacements.Get(p);

                    if (placementCross.place >= 0 && placementMain.place >= 0) {
                        int crossEnd = placementCross.place + placementCross.span;
                        BitSet mainArea = BitSet.SetRange(placementMain.place, placementMain.span);

                        for (int o = placementCross.place; o < crossEnd; o++) {
                            occupiedMap[o].value |= mainArea.value;
                        }

                    }
                }

                int mainAxisCursor = 0;
                int crossAxisCursor = 0;
                int denseMultiplier = gridInfo.isDense ? 0 : 1;

                // place single axis locked
                for (int p = placementStart; p < placementEnd; p++) {

                    ref GridItemPlacement placementMain = ref mainAxisPlacements.array[p];
                    ref GridItemPlacement placementCross = ref crossAxisPlacements.array[p];

                    if (placementCross.place >= 0 && placementMain.place >= 0) {
                        continue;
                    }

                    // main axis is fixed, find a valid cross axis position 
                    if (placementMain.place >= 0 && placementCross.place < 0) {

                        BitSet mainAxis = BitSet.SetRange(placementMain.place, placementMain.span);

                        while (true) {

                            bool valid = true;

                            int end = crossAxisCursor + placementCross.span;

                            if (occupiedMap.size <= end) {
                                occupiedMap.SetSize(end, NativeArrayOptions.ClearMemory);
                            }

                            for (int c = crossAxisCursor; c < end; c++) {
                                if ((occupiedMap[c].value & mainAxis.value) != 0) valid = false;
                            }

                            if (valid) {

                                for (int c = crossAxisCursor; c < end; c++) {
                                    occupiedMap[c].value |= mainAxis.value;
                                }

                                placementCross.place = (short) crossAxisCursor;
                                mainAxisCursor = placementMain.place + placementMain.span;
                                if (mainAxisCursor == gridInfo.mainAxisTemplate.cellCount) {
                                    mainAxisCursor = 0;
                                    crossAxisCursor += placementCross.span; 
                                }

                                break;
                            }

                            crossAxisCursor++;

                        }

                    }
                    else {
                        // single axis lock where locked axis == cross axis is not supported and treated as if it were unplaced 
                        mainAxisCursor *= denseMultiplier;
                        crossAxisCursor *= denseMultiplier;
                        
                        if (occupiedMap.size < crossAxisCursor + placementCross.span) {
                            occupiedMap.SetSize(crossAxisCursor + placementCross.span, NativeArrayOptions.ClearMemory);
                        }

                        while (true) {

                            bool valid = true;

                            if (mainAxisCursor + placementMain.span > gridInfo.mainAxisTemplate.cellCount) {
                                crossAxisCursor++;
                                mainAxisCursor = 0;
                                if (occupiedMap.size <= crossAxisCursor + placementCross.span) {
                                    occupiedMap.SetSize(crossAxisCursor + placementCross.span, NativeArrayOptions.ClearMemory);
                                }
                            }

                            BitSet mainAxis = BitSet.SetRange(mainAxisCursor, placementMain.span);

                            for (int c = crossAxisCursor; c < crossAxisCursor + placementCross.span; c++) {
                                if ((occupiedMap[c].value & mainAxis.value) != 0) valid = false;
                            }

                            if (valid) {

                                for (int c = crossAxisCursor; c < crossAxisCursor + placementCross.span; c++) {
                                    occupiedMap[c].value |= mainAxis.value;
                                }

                                placementMain.place = (short) mainAxisCursor;
                                placementCross.place = (short) crossAxisCursor;
                                mainAxisCursor = placementMain.place + placementMain.span;
                                break;
                            }

                            mainAxisCursor++;

                        }
                    }

                }

                // place all unplaced items
                for (int p = placementStart; p < placementEnd; p++) {
                    ref GridItemPlacement placementMain = ref mainAxisPlacements.array[p];
                    ref GridItemPlacement placementCross = ref crossAxisPlacements.array[p];

                    if (placementCross.place >= 0 || placementMain.place >= 0) {
                        continue;
                    }

                    // place < 0 if not given a spot yet

                    mainAxisCursor *= denseMultiplier;
                    crossAxisCursor *= denseMultiplier;
                    
                    if (occupiedMap.size < crossAxisCursor + placementCross.span) {
                        occupiedMap.SetSize(crossAxisCursor + placementCross.span, NativeArrayOptions.ClearMemory);
                    }

                }

                int maxCrossCursor = 0;
                for (int p = placementStart; p < placementEnd; p++) {
                    ref GridItemPlacement placementCross = ref crossAxisPlacements.array[p];
                    if (placementCross.place + placementCross.span > maxCrossCursor) {
                        maxCrossCursor = placementCross.place + placementCross.span;
                    }
                }

                gridInfo.crossAxisTemplateStart = totalCrossAxisCellCount;
                gridInfo.crossAxisCellCount = maxCrossCursor;
                totalCrossAxisCellCount += maxCrossCursor;

            }

            occupiedMap.Dispose();

            return totalCrossAxisCellCount;
        }

        private static void ClampMainAxisPlacements(TempList<GridInfo> list, CheckedArray<GridItemPlacement> placementList) {
            for (int i = 0; i < list.size; i++) {
                ref GridInfo gridInfo = ref list.Get(i);

                int cellCount = gridInfo.mainAxisTemplate.cellCount;

                for (int p = 0; p < placementList.size; p++) {

                    ref GridItemPlacement placement = ref placementList.array[i];

                    if (placement.place >= cellCount) {
                        placement.place = (short) (cellCount - 1);
                    }

                    if (placement.place < 0) {
                        if (placement.span >= cellCount) placement.span = (ushort) cellCount;
                    }
                    else if (placement.place + placement.span > cellCount) {
                        placement.span = (ushort) (cellCount - placement.place); // max span we can have without overflowing the grid 
                    }

                }

            }
        }

        private struct ViewIndexEm {

            public float emSize;
            public int viewIndex;

        }

        private struct GridInfo {

            public int boxId;
            public RangeInt childRange;
            public GridTemplate mainAxisTemplate;
            public GridTemplate crossAxisTemplate;
            public RangeInt placementRange;
            public bool isDense;
            public ElementId elementId;
            public int crossAxisCellCount;
            public int crossAxisTemplateStart;

        }

    }

    internal enum ResolveGridCellSizeUnit : byte {

        Pixel = Unit.Pixel,
        Percent = Unit.Percent,
        MaxContent = Unit.MaxContent,
        MinContent = Unit.MinContent,

    }

    internal struct GridCell {

        public float resolvedBaseSize;
        public float minContentContribution;
        public float maxContentContribution;
        public ResolvedGridCellSize sizeDef;

    }

    internal struct ResolvedGridCellSize {

        public float value;
        public ushort stretch;
        public ResolveGridCellSizeUnit unit;
        
    }

}