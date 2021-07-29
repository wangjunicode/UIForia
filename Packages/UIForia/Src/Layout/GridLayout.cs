using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Layout {

    internal unsafe struct GridLayout {

        /* ideas:

            A grid is a 2d layout box that works by taking a template definition on the row and column axis
            Items can be placed explicitly in numbered grid cells or placed implicitly using one of two placement
            algorithms(explained later)           
            
        */

        public static void HandleMarginCollapse(ref MarginCollapseInfo marginCollapseInfo, ref GridLayoutData layoutData) {

            if (layoutData.gridAxisInfos.size == 0) return;

            CheckedArray<GridItemPlacement> globalPlacementList = layoutData.GetPlacementList();

            for (int layoutIndex = 0; layoutIndex < layoutData.gridAxisInfos.size; layoutIndex++) {

                GridAxisInfo gridInfo = layoutData.gridAxisInfos[layoutIndex];
                int boxId = gridInfo.boxId;
                RangeInt childRange = gridInfo.childRange;

                SpaceCollapse spaceCollapse = marginCollapseInfo.spaceCollapse[boxId];

                if (spaceCollapse == 0 || childRange.length == 0) {
                    continue;
                }

                ResolvedSpacerSize paddingStart = marginCollapseInfo.paddingStart[boxId];
                ResolvedSpacerSize paddingEnd = marginCollapseInfo.paddingEnd[boxId];

                float removeInnerMult = 1;
                float removeOuterMult = 1;

                bool shouldRemove = false;
                bool shouldCollapse = (spaceCollapse & SpaceCollapse.CollapseOuter) != 0;

                // for grids CollapseInner == RemoveInner
                if ((spaceCollapse & SpaceCollapse.RemoveInner) != 0 || (spaceCollapse & SpaceCollapse.CollapseInner) != 0) {
                    removeInnerMult = childRange.length == 1 ? 1 : 0;
                    shouldRemove = true;
                }

                if ((spaceCollapse & SpaceCollapse.RemoveOuter) != 0) {
                    shouldRemove = true;
                    removeOuterMult = 0;
                }

                CheckedArray<GridItemPlacement> placements = globalPlacementList.Slice(gridInfo.placementRange);

                int cellCount = gridInfo.cellCount;

                if (shouldRemove) {
                    for (int childIdx = childRange.start, placementIdx = 0; placementIdx < placements.size; placementIdx++, childIdx++) {
                        GridItemPlacement placement = placements[placementIdx];

                        ref ResolvedSpacerSize startMargin = ref marginCollapseInfo.marginStart.array[childIdx];
                        ref ResolvedSpacerSize endMargin = ref marginCollapseInfo.marginEnd.array[childIdx];

                        if (placement.place == 0 && placement.end == cellCount) {
                            startMargin.value *= removeOuterMult;
                            startMargin.stretch *= (int) removeOuterMult;
                            endMargin.value *= removeOuterMult;
                            endMargin.stretch *= (int) removeOuterMult;
                        }
                        else if (placement.place == 0) {
                            startMargin.value *= removeOuterMult;
                            startMargin.stretch *= (int) removeOuterMult;
                            endMargin.value *= removeInnerMult;
                            endMargin.stretch *= (int) removeInnerMult;
                        }
                        else if (placement.end == cellCount) {
                            startMargin.value *= removeInnerMult;
                            startMargin.stretch *= (int) removeInnerMult;

                            endMargin.value *= removeOuterMult;
                            endMargin.stretch *= (int) removeOuterMult;
                        }
                        else {
                            startMargin.value *= removeInnerMult;
                            startMargin.stretch *= (int) removeInnerMult;

                            endMargin.value *= removeInnerMult;
                            endMargin.stretch *= (int) removeInnerMult;
                        }
                    }
                }

                if (shouldCollapse) {
                    for (int childIdx = childRange.start, placementIdx = 0; placementIdx < placements.size; placementIdx++, childIdx++) {

                        GridItemPlacement placement = placements[placementIdx];

                        ref ResolvedSpacerSize startMargin = ref marginCollapseInfo.marginStart.array[childIdx];
                        ref ResolvedSpacerSize endMargin = ref marginCollapseInfo.marginEnd.array[childIdx];

                        if (placement.place == 0 && placement.end == cellCount) {
                            startMargin.value = paddingStart.value >= startMargin.value ? 0 : startMargin.value - paddingStart.value;
                            startMargin.stretch = paddingStart.stretch >= startMargin.stretch ? 0 : startMargin.stretch - paddingStart.stretch;

                            endMargin.value = paddingEnd.value >= endMargin.value ? 0 : endMargin.value - paddingEnd.value;
                            endMargin.stretch = paddingEnd.stretch >= endMargin.stretch ? 0 : endMargin.stretch - paddingEnd.stretch;
                        }
                        else if (placement.place == 0) {
                            startMargin.value = paddingStart.value >= startMargin.value ? 0 : startMargin.value - paddingStart.value;
                            startMargin.stretch = paddingStart.stretch >= startMargin.stretch ? 0 : startMargin.stretch - paddingStart.stretch;
                        }
                        else if (placement.end == cellCount) {
                            endMargin.value = paddingEnd.value >= endMargin.value ? 0 : endMargin.value - paddingEnd.value;
                            endMargin.stretch = paddingEnd.stretch >= endMargin.stretch ? 0 : endMargin.stretch - paddingEnd.stretch;
                        }

                    }

                }

            }

        }

        private static void ResolveIntrinsicSizes(in LayoutAxis layoutInfo, CheckedArray<GridItemPlacement> placements, CheckedArray<GridCell> cellList, in GridAxisInfo gridAxisInfo, int childRangeStart) {
            RangeInt placementRange = gridAxisInfo.placementRange;

            for (int childIdx = childRangeStart, placementIdx = placementRange.start; placementIdx < placementRange.end; placementIdx++, childIdx++) {

                GridItemPlacement placement = placements[placementIdx];

                float spansIntrinsicCount = 0; // fp division is faster than int division 

                for (int i = placement.place; i < placement.end; i++) {
                    ResolveGridCellSizeUnit unit = cellList[i].sizeDef.unit;

                    if (unit == ResolveGridCellSizeUnit.MaxContent || unit == ResolveGridCellSizeUnit.MinContent) {
                        spansIntrinsicCount++;
                    }

                }

                if (spansIntrinsicCount == 0) {
                    continue;
                }

                float size = layoutInfo.sizes[childIdx].baseSize;
                size += layoutInfo.marginSize[childIdx];

                for (int i = placement.place; i < placement.end; i++) {
                    size -= cellList[i].resolvedBaseSize;
                }

                float sizeContribution = math.max(0, size) / spansIntrinsicCount;

                for (int i = placement.place; i < placement.end; i++) {
                    ref GridCell cell = ref cellList.Get(i);

                    if (cell.maxContentContribution < sizeContribution) {
                        cell.maxContentContribution = sizeContribution;
                    }

                    if (cell.minContentContribution == -1 || cell.minContentContribution > sizeContribution) {
                        cell.minContentContribution = sizeContribution;
                    }
                }
            }

            for (int i = 0; i < cellList.size; i++) {

                ref GridCell cell = ref cellList.Get(i);

                if (cell.sizeDef.unit == ResolveGridCellSizeUnit.MinContent) {
                    cell.resolvedBaseSize = cell.sizeDef.value * cell.minContentContribution;
                }
                else if (cell.sizeDef.unit == ResolveGridCellSizeUnit.MaxContent) {
                    cell.resolvedBaseSize = cell.sizeDef.value * cell.maxContentContribution;
                }

            }

        }

        private static void PlaceAndSizeChildren(TempList<LayoutBoxInfo> layoutInfos, ref LayoutAxis axis, TempList<GridLayoutPassInfo> passList, TempList<ChildInfo> childInfoList, TempList<GridStretchInfo> cellSizes, CheckedArray<GridItemPlacement> globalPlacementList) {

            for (int passIndex = 0; passIndex < passList.size; passIndex++) {

                ref GridLayoutPassInfo pass = ref passList.Get(passIndex);

                RangeInt placementRange = pass.gridInfo.placementRange;

                int childIdx = layoutInfos[passIndex].childRange.start;

                axis.readyFlags[passIndex] |= ReadyFlags.LayoutComplete;

                for (int p = placementRange.start; p < placementRange.end; p++, childIdx++) {
                    ref ChildInfo childInfo = ref childInfoList.Get(p);
                    childInfo.boxIndex = childIdx;
                    axis.readyFlags[childIdx] |= ReadyFlags.FinalSizeResolved | ReadyFlags.ParentReady | ReadyFlags.StretchReady;
                }

            }

            for (int i = 0; i < childInfoList.size; i++) {
                ref ChildInfo child = ref childInfoList.Get(i);
                int boxId = child.boxIndex;
                ResolvedSpacerSize marginStart = axis.marginStart[boxId];
                ResolvedSpacerSize marginEnd = axis.marginEnd[boxId];
                LayoutSizes size = axis.sizes[boxId];
                child.stretch = (ushort) size.stretch;
                child.baseSize = size.baseSize;
                child.marginStartStretch = (ushort) marginStart.stretch;
                child.marginEndStretch = (ushort) marginEnd.stretch;
                child.marginStartValue = marginStart.value;
                child.marginEndValue = marginEnd.value;
                child.minSize = size.min;
                child.maxSize = size.max;
            }

            // for locality spin the other way this time 
            for (int passIndex = passList.size - 1; passIndex >= 0; passIndex--) {

                ref GridLayoutPassInfo pass = ref passList.Get(passIndex);

                RangeInt placementRange = pass.gridInfo.placementRange;

                for (int p = placementRange.start; p < placementRange.end; p++) {

                    GridItemPlacement placement = globalPlacementList[p];

                    int placementEnd = placement.end;

                    ref ChildInfo childInfo = ref childInfoList.Get(p);

                    int startIdx = placement.place + placement.place + 1;
                    int endIdx = placement.span == 1 ? startIdx + 1 : placementEnd + placementEnd;

                    float size = 0;
                    for (int k = startIdx; k < endIdx; k++) {
                        size += cellSizes[pass.passRange.start + k].currentSize;
                    }

                    childInfo.spannedCellSize = size;
                    childInfo.cellOrigin = cellSizes[pass.passRange.start + startIdx].position + pass.borderStart; // todo if we fill from the other way i guess we take border end 

                    childInfo.fillFromEnd = false;

                }

                StretchChildrenInCells(ref axis, childInfoList);
            }
        }

        private static void StretchChildrenInCells(ref LayoutAxis axis, TempList<ChildInfo> contentList) {

            for (int i = 0; i < contentList.size; i++) {

                ref ChildInfo info = ref contentList.Get(i);

                if (info.stretch == 0 && info.baseSize < info.minSize) info.baseSize = info.minSize;
                if (info.stretch == 0 && info.baseSize > info.maxSize) info.baseSize = info.maxSize;

                int totalStretch = info.marginStartStretch + info.marginEndStretch + info.stretch;

                StretchInfo eleGrowInfo = new StretchInfo();
                StretchInfo mStartGrowInfo = new StretchInfo();
                StretchInfo mEndGrowInfo = new StretchInfo();

                mStartGrowInfo.currentSize = info.marginStartStretch != 0 ? 0 : info.marginStartValue;
                mStartGrowInfo.stretchParts = info.marginStartStretch;
                mStartGrowInfo.minSize = info.marginStartValue;

                mEndGrowInfo.currentSize = info.marginEndStretch != 0 ? 0 : info.marginEndValue;
                mEndGrowInfo.stretchParts = info.marginEndStretch;
                mEndGrowInfo.minSize = info.marginEndValue;

                eleGrowInfo.currentSize = info.stretch != 0 ? 0 : info.baseSize;
                eleGrowInfo.stretchParts = info.stretch;
                eleGrowInfo.maxSize = info.maxSize;
                eleGrowInfo.minSize = info.minSize;

                float remaining = info.spannedCellSize - (mStartGrowInfo.currentSize + mEndGrowInfo.currentSize + eleGrowInfo.currentSize);
                SolvedSize size = axis.solvedPrefSizes[info.boxIndex];

                switch (size.unit) {

                    case SolvedSizeUnit.FillRemaining:
                        float fill = size.value * remaining;
                        remaining -= fill;
                        eleGrowInfo.currentSize = fill;
                        eleGrowInfo.stretchParts = 0;
                        break;

                    case SolvedSizeUnit.StretchContent:
                        eleGrowInfo.currentSize = 0;
                        eleGrowInfo.minSize = math.max(axis.sizes[info.boxIndex].baseSize, info.minSize);
                        break;

                    case SolvedSizeUnit.FitContent:
                        float partSize = remaining / totalStretch; // if fit content stretch val is always at least 1 so no div by 0 error
                        float contentSize = axis.sizes[info.boxIndex].baseSize;
                        eleGrowInfo.maxSize = contentSize > partSize * totalStretch
                            ? math.min(partSize * totalStretch, float.MaxValue)
                            : contentSize;
                        eleGrowInfo.currentSize = 0;
                        break;
                }

                if (totalStretch != 0 && remaining > 0) {

                    float partSize;

                    while (true) {

                        bool checkAgain = false;
                        partSize = remaining / totalStretch;

                        if (mStartGrowInfo.stretchParts > 0 && mStartGrowInfo.minSize > mStartGrowInfo.stretchParts * partSize) {
                            mStartGrowInfo.currentSize = mStartGrowInfo.minSize;
                            totalStretch -= mStartGrowInfo.stretchParts;
                            remaining -= mStartGrowInfo.minSize;
                            mStartGrowInfo.stretchParts = 0;
                            checkAgain = totalStretch > 0;
                        }

                        if (mEndGrowInfo.stretchParts > 0 && mEndGrowInfo.minSize > mEndGrowInfo.stretchParts * partSize) {
                            mEndGrowInfo.currentSize = mEndGrowInfo.minSize;
                            totalStretch -= mEndGrowInfo.stretchParts;
                            remaining -= mEndGrowInfo.minSize;
                            mEndGrowInfo.stretchParts = 0;
                            checkAgain = totalStretch > 0;
                        }

                        if (eleGrowInfo.stretchParts > 0 && eleGrowInfo.minSize > eleGrowInfo.stretchParts * partSize) {
                            eleGrowInfo.currentSize = eleGrowInfo.minSize;
                            totalStretch -= eleGrowInfo.stretchParts;
                            remaining -= eleGrowInfo.minSize;
                            eleGrowInfo.stretchParts = 0;
                            checkAgain = totalStretch > 0;
                        }

                        if (!checkAgain) break;
                    }

                    partSize = totalStretch == 0 ? 0 : remaining / totalStretch;

                    if (eleGrowInfo.stretchParts > 0 && eleGrowInfo.maxSize < eleGrowInfo.stretchParts * partSize) {
                        eleGrowInfo.currentSize = eleGrowInfo.maxSize;
                        totalStretch -= eleGrowInfo.stretchParts;
                        remaining -= eleGrowInfo.maxSize;
                        eleGrowInfo.stretchParts = 0;
                    }

                    partSize = totalStretch == 0 ? 0 : remaining / totalStretch;

                    mStartGrowInfo.currentSize += partSize * mStartGrowInfo.stretchParts;
                    mEndGrowInfo.currentSize += partSize * mEndGrowInfo.stretchParts;
                    eleGrowInfo.currentSize += partSize * eleGrowInfo.stretchParts;
                }

                axis.outputSizes[info.boxIndex] = eleGrowInfo.currentSize;
                axis.outputPositions[info.boxIndex] = info.fillFromEnd
                    ? info.spannedCellSize - (mEndGrowInfo.currentSize + eleGrowInfo.currentSize)
                    : info.cellOrigin + mStartGrowInfo.currentSize;
            }
        }

        private static TempList<GridStretchInfo> StretchGridAxis(TempList<LayoutBoxInfo> layoutInfos, ref LayoutAxis axis, ref GridLayoutData gridLayoutData, TempList<GridLayoutPassInfo> passList) {

            for (int i = 0; i < layoutInfos.size; i++) {
                int boxIndex = layoutInfos[i].boxIndex;
                passList[i] = new GridLayoutPassInfo() {
                    totalSize = axis.outputSizes[boxIndex],
                    borderStart = axis.borderStart[boxIndex],
                    borderEnd = axis.borderEnd[boxIndex],
                };
            }

            int stretchItemCount = 0;

            for (int i = 0; i < layoutInfos.size; i++) {
                LayoutBoxInfo layoutInfo = layoutInfos[i];
                passList.array[i].gridInfo = gridLayoutData.GetGridInfo(layoutInfo.boxIndex);
                stretchItemCount += (passList.array[i].gridInfo.cellCount * 2) + 1; // 1x for cells, (1x - 1) for space between, 2 for padding;
            }

            TempList<GridStretchInfo> stretchItemList = TypedUnsafe.MallocSizedTempList<GridStretchInfo>(stretchItemCount, Allocator.Temp);

            CheckedArray<GridCell> globalCellList = gridLayoutData.GetCellList();

            int writeIndex = 0;

            for (int passIndex = 0; passIndex < layoutInfos.size; passIndex++) {

                LayoutBoxInfo layoutInfo = layoutInfos[passIndex];
                int boxIndex = layoutInfo.boxIndex;

                ref GridLayoutPassInfo passInfo = ref passList.Get(passIndex);

                CheckedArray<GridCell> cellList = globalCellList.Slice(passInfo.gridInfo.cellOffset, passInfo.gridInfo.cellCount);

                ResolvedSpacerSize paddingStartSize = axis.paddingStart[boxIndex];
                ResolvedSpacerSize paddingEndSize = axis.paddingEnd[boxIndex];
                ResolvedSpacerSize spacerSize = axis.spaceBetween[boxIndex];

                int start = writeIndex;

                float contentAreaSize = passInfo.totalSize - (passInfo.borderStart + passInfo.borderEnd);
                float remaining = contentAreaSize;
                int cellCount = passInfo.gridInfo.cellCount;

                int totalStretch = 0;

                // will be copied into the stretch list cellCount - 1 times
                GridStretchInfo spacerStretch = new GridStretchInfo() {
                    minSize = spacerSize.value,
                    currentSize = spacerSize.stretch > 0 ? 0 : spacerSize.value,
                    stretchParts = spacerSize.stretch
                };

                ref GridStretchInfo paddingStart = ref stretchItemList.Get(writeIndex++);
                paddingStart.minSize = paddingStartSize.value;
                paddingStart.currentSize = paddingStartSize.stretch > 0 ? 0 : paddingStartSize.value;
                paddingStart.stretchParts = paddingStartSize.stretch;

                totalStretch += paddingStart.stretchParts;
                remaining -= paddingStart.currentSize;

                totalStretch += spacerStretch.stretchParts * (cellCount - 1);
                remaining -= spacerStretch.currentSize * (cellCount - 1);

                // todo -- we could support FillRemaining for grid cells here 
                
                for (int i = 0; i < cellList.size - 1; i++) {
                    ref GridCell cell = ref cellList.Get(i);
                    ref GridStretchInfo stretch = ref stretchItemList.Get(writeIndex++);

                    stretch.minSize = cell.resolvedBaseSize;
                    stretch.currentSize = cell.sizeDef.stretch > 0 ? 0 : cell.resolvedBaseSize;
                    stretch.stretchParts = cell.sizeDef.stretch;

                    totalStretch += stretch.stretchParts;
                    remaining -= stretch.currentSize;

                    stretchItemList[writeIndex++] = spacerStretch;
                }

                ref GridCell lastCell = ref cellList.Get(cellList.size - 1);
                ref GridStretchInfo lastCellStretch = ref stretchItemList.Get(writeIndex++);
                lastCellStretch.minSize = lastCell.resolvedBaseSize;
                lastCellStretch.currentSize = lastCell.sizeDef.stretch > 0 ? 0 : lastCell.resolvedBaseSize;
                lastCellStretch.stretchParts = lastCell.sizeDef.stretch;

                totalStretch += lastCellStretch.stretchParts;
                remaining -= lastCellStretch.currentSize;

                ref GridStretchInfo paddingEnd = ref stretchItemList.Get(writeIndex++);
                paddingEnd.minSize = paddingEndSize.value;
                paddingEnd.currentSize = paddingEndSize.stretch > 0 ? 0 : paddingEndSize.value;
                paddingEnd.stretchParts = paddingEndSize.stretch;

                totalStretch += paddingEnd.stretchParts;
                remaining -= paddingEnd.currentSize;

                passInfo.remaining = remaining;
                passInfo.passRange = new RangeInt(start, writeIndex - start);
                passInfo.totalStretch = totalStretch;

            }

            return stretchItemList;

        }

        private static void RunStretchPasses(TempList<GridLayoutPassInfo> passList, TempList<GridStretchInfo> stretchList) {

            for (int passIdx = 0; passIdx < passList.size; passIdx++) {

                ref GridLayoutPassInfo pass = ref passList.array[passIdx];

                float pieceSize;
                int start = pass.passRange.start;
                int end = pass.passRange.end;

                if (pass.totalStretch > 0) {

                    while (true) {

                        pieceSize = math.max(pass.remaining / pass.totalStretch, 0);

                        bool checkAgain = false;

                        for (int i = start; i < end; i++) {

                            ref GridStretchInfo item = ref stretchList.array[i];

                            if (item.stretchParts > 0 && item.minSize >= pieceSize * item.stretchParts) {
                                item.currentSize = item.minSize;
                                pass.totalStretch -= item.stretchParts;
                                pass.remaining -= item.minSize; // todo -- wrong since we accounted for current already? 
                                item.stretchParts = 0;
                                checkAgain = pass.totalStretch > 0;
                            }
                        }

                        if (!checkAgain) break;
                    }
                }

                if (pass.totalStretch == 0 || pass.remaining <= 0) {
                    pieceSize = 0;
                }
                else {
                    pieceSize = pass.remaining / pass.totalStretch;
                }

                ref GridStretchInfo firstItem = ref stretchList.array[start];
                firstItem.currentSize += firstItem.stretchParts * pieceSize;
                float offset = firstItem.currentSize;
                for (int i = start + 1; i < end; i++) {
                    ref GridStretchInfo item = ref stretchList.array[i];
                    item.currentSize += item.stretchParts * pieceSize;
                    item.position = offset;
                    offset += item.currentSize;
                }

            }

        }

        public static CheckedArray<LayoutBoxInfo> Layout(ref LayoutListBuffer listBuffer, ref LayoutAxis mainAxis, ref LayoutAxis crossAxis, ref GridLayoutData mainAxisData, ref GridLayoutData crossAxisData, CheckedArray<LayoutBoxInfo> boxes) {

            const bool CrossAxis = true;
            const bool MainAxis = false;

            listBuffer.controlledList.size = 0;

            LayoutUtil.UpdateAxisFlags(ref listBuffer, ref crossAxis, ref mainAxis, boxes, CrossAxis);

            ComputeContentSizes(ref crossAxis, listBuffer.contentSizeList, ref crossAxisData);

            LayoutUtil.SolveSizes(listBuffer.solveList, ref crossAxis);

            Layout(ref crossAxis, listBuffer.layoutList, ref crossAxisData);

            LayoutUtil.UpdateAxisFlags(ref listBuffer, ref mainAxis, ref crossAxis, boxes, MainAxis);

            ComputeContentSizes(ref mainAxis, listBuffer.contentSizeList, ref mainAxisData);

            LayoutUtil.SolveSizes(listBuffer.solveList, ref mainAxis);

            Layout(ref mainAxis, listBuffer.layoutList, ref mainAxisData);

            if (listBuffer.controlledList.size != 0) {

                LayoutUtil.UpdateAxisFlagsDeferred(ref listBuffer, ref mainAxis, ref crossAxis, listBuffer.controlledList.ToCheckedArray());

                // here we might still need to compute content size because of intrinsics, we shouldn't auto-unset the content resolved flag
                // (requires specialized version of LayoutUtil.UpdateAxisFlagsDeferred)
                ComputeContentSizes(ref crossAxis, listBuffer.layoutList, ref crossAxisData);

                Layout(ref crossAxis, listBuffer.layoutList, ref crossAxisData);

            }

            return LayoutUtil.FilterCompletedBoxes(boxes, mainAxis.readyFlags, crossAxis.readyFlags);

        }

        private static void Layout(ref LayoutAxis axis, TempList<LayoutBoxInfo> layoutList, ref GridLayoutData gridLayoutData) {

            TempList<GridLayoutPassInfo> passList = TypedUnsafe.MallocSizedTempList<GridLayoutPassInfo>(layoutList.size, Allocator.Temp);

            TempList<GridStretchInfo> stretchInfos = StretchGridAxis(layoutList, ref axis, ref gridLayoutData, passList);

            RunStretchPasses(passList, stretchInfos);

            // can collect grid info here for debugging or custom rendering

            TempList<ChildInfo> childInfos = TypedUnsafe.MallocSizedTempList<ChildInfo>(gridLayoutData.placements.size, Allocator.Temp);

            PlaceAndSizeChildren(layoutList, ref axis, passList, childInfos, stretchInfos, gridLayoutData.placements);

            childInfos.Dispose();
            stretchInfos.Dispose();
            passList.Dispose();
        }

        private static void ComputeContentSizes(ref LayoutAxis axis, TempList<LayoutBoxInfo> measureList, ref GridLayoutData gridLayoutData) {

            CheckedArray<GridCell> globalCellList = gridLayoutData.gridCells;
            CheckedArray<GridItemPlacement> placementList = gridLayoutData.placements;

            for (int measureIdx = 0; measureIdx < measureList.size; measureIdx++) {

                LayoutBoxInfo box = measureList[measureIdx];
                RangeInt childRange = box.childRange;

                GridAxisInfo gridAxisInfo = gridLayoutData.GetGridInfo(box.boxIndex);

                CheckedArray<GridCell> cellList = globalCellList.Slice(gridAxisInfo.cellOffset, gridAxisInfo.cellCount);

                if (gridAxisInfo.hasIntrinsicSizes) {
                    ResolveIntrinsicSizes(axis, placementList, cellList, gridAxisInfo, childRange.start);
                }

                float contentSize = axis.spaceBetween[box.boxIndex].value * (gridAxisInfo.cellCount - 1);
                for (int i = 0; i < cellList.size; i++) {
                    contentSize += cellList[i].resolvedBaseSize;
                }

                float maxChildSize = 0;
                float minChildSize = 0;

                if (childRange.length != 0) {

                    minChildSize = float.MaxValue;

                    for (int childIter = childRange.start; childIter < childRange.end; childIter++) {
                        float childSize = axis.sizes[childIter].baseSize;
                        childSize += axis.marginStart[childIter].value + axis.marginEnd[childIter].value;
                        if (childSize < minChildSize) minChildSize = childSize;
                        if (childSize > maxChildSize) maxChildSize = childSize;
                    }

                }

                axis.contentSizes[box.boxIndex] = new LayoutContentSizes() {
                    contentSize = contentSize,
                    minChildSize = minChildSize,
                    maxChildSize = maxChildSize
                };

                // note: % always points at parent size
                // fr (fill remaining) is a stretch based unit that points at remaining space which in the case of a grid is the cell

            }
        }

        private struct GridLayoutPassInfo {

            public float totalSize;
            public float borderStart;
            public float borderEnd;
            public GridAxisInfo gridInfo;
            public RangeInt passRange;
            public int totalStretch;
            public float remaining;

        }

        private struct ChildInfo {

            public int boxIndex;
            public float baseSize;
            public float minSize;
            public float maxSize;
            public float marginStartValue;
            public float marginEndValue;
            public ushort marginStartStretch;
            public ushort marginEndStretch;
            public ushort stretch;
            public float spannedCellSize;
            public float cellOrigin;
            public bool fillFromEnd;

        }

    }

}