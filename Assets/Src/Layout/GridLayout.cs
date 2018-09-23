using System;
using System.Collections.Generic;
using System.Linq;
using Rendering;
using Src.Util;
using UnityEngine;

namespace Src.Layout {

    // todo pool this but keep as ref type probably

    // max-content size == preferred size!
    // smallest size in a given axis when given infinite space
    // ie min size of box that fits around content and avoids overflow

    // auto -> when max == max-content
    // auto -> when min == largest min size of track

    public class GridLayout : UILayout {

        private LayoutNode m_LayoutNode;
        private readonly List<GridTrack> m_RowTracks;
        private readonly List<GridTrack> m_ColTracks;
        private readonly List<GridPlacement> m_Placements;
        private readonly HashSet<int> m_Occupied;

        private static readonly ObjectPool<GridItem> s_GridItemPool = new ObjectPool<GridItem>(null, (item) => item.Reset());

        public GridLayout(ITextSizeCalculator textSizeCalculator) : base(textSizeCalculator) {
            this.m_RowTracks = new List<GridTrack>();
            this.m_ColTracks = new List<GridTrack>();
            this.m_Occupied = new HashSet<int>();
            this.m_Placements = new List<GridPlacement>();
        }

        public override List<Rect> Run(Rect viewport, LayoutNode currentNode) {
            m_LayoutNode = currentNode;
            List<LayoutNode> children = m_LayoutNode.children;

            GridDefinition gridDefinition = m_LayoutNode.element.style.gridDefinition;

            float contentAreaWidth = m_LayoutNode.element.layoutResult.width;
            float contentAreaHeight = m_LayoutNode.element.layoutResult.height;

            List<GridPlacement> axisLocked = ListPool<GridPlacement>.Get();
            List<GridPlacement> rowLocked = ListPool<GridPlacement>.Get();
            List<GridPlacement> unlocked = ListPool<GridPlacement>.Get();

            if (gridDefinition.rowTemplate != null) {
                for (int i = 0; i < gridDefinition.rowTemplate.Length; i++) {
                    m_RowTracks.Add(new GridTrack(gridDefinition.rowTemplate[i]));
                }
            }

            if (gridDefinition.colTemplate != null) {
                for (int i = 0; i < gridDefinition.colTemplate.Length; i++) {
                    m_ColTracks.Add(new GridTrack(gridDefinition.colTemplate[i]));
                }
            }

            UIUnit widthUnit = m_LayoutNode.element.style.width.unit;
            UIUnit heightUnit = m_LayoutNode.element.style.height.unit;

            for (int i = 0; i < children.Count; i++) {
                LayoutNode child = children[i];

                GridItem colItem = CreateColItem(child, widthUnit, contentAreaWidth, viewport.width, gridDefinition.autoColSize);
                GridItem rowItem = CreateRowItem(child, heightUnit, colItem.preferredSize, contentAreaHeight, viewport.height, gridDefinition.autoRowSize);

                GridPlacementParameters gridPlacementParameters = child.element.style.gridItem;
                int colStart = gridPlacementParameters.colStart;
                int colSpan = gridPlacementParameters.colSpan;
                int colEnd = colStart + colSpan;

                int rowStart = gridPlacementParameters.rowStart;
                int rowSpan = gridPlacementParameters.rowSpan;
                int rowEnd = rowStart + rowSpan;

                if (IntUtil.IsDefined(colStart)) {
                    colItem.trackStart = colStart;
                    colItem.trackSpan = IntUtil.IsDefined(colSpan) ? Mathf.Min(1, colSpan) : 1;

                    if (m_ColTracks.Count < colEnd) {
                        CreateImplicitTracks(m_ColTracks, colEnd - m_ColTracks.Count, gridDefinition.autoColSize);
                    }

                    for (int j = colStart; j < colEnd; j++) {
                        if (m_ColTracks[j].sizingFn.minSizingFunction.type == GridCellMeasurementType.Fractional) {
                            colItem.spansFlexible = true;
                        }
                    }
                }

                if (IntUtil.IsDefined(rowStart)) {
                    rowItem.trackStart = rowStart;
                    rowItem.trackSpan = IntUtil.IsDefined(rowSpan) ? Mathf.Min(1, rowSpan) : 1;

                    if (m_RowTracks.Count < rowEnd) {
                        CreateImplicitTracks(m_RowTracks, rowEnd - m_RowTracks.Count, gridDefinition.autoRowSize);
                    }

                    for (int j = rowStart; j < rowEnd; j++) {
                        if (m_RowTracks[j].sizingFn.minSizingFunction.type == GridCellMeasurementType.Fractional) {
                            rowItem.spansFlexible = true;
                        }
                    }
                }

                if (colItem.IsAxisLocked && rowItem.IsAxisLocked) {
                    axisLocked.Add(new GridPlacement(rowItem, colItem));
                }
                else if (rowItem.IsAxisLocked) { // todo for auto-flow column this needs to be reversed
                    rowLocked.Add(new GridPlacement(rowItem, colItem));
                }
                else {
                    unlocked.Add(new GridPlacement(rowItem, colItem));
                }

                m_Placements.Add(new GridPlacement(rowItem, colItem));
            }

            for (int i = 0; i < axisLocked.Count; i++) {
                GridPlacement placement = axisLocked[i];
                int colStart = placement.colItem.trackStart;
                int colSpan = placement.colItem.trackSpan;
                int rowStart = placement.rowItem.trackStart;
                int rowSpan = placement.rowItem.trackSpan;
                for (int j = colStart; j < colStart + colSpan; j++) {
                    for (int k = rowStart; k < rowStart + rowSpan; k++) {
                        m_Occupied.Add(BitUtil.SetHighLowBits(j, k));
                    }
                }

                m_ColTracks[colStart].originMembers.Add(placement.colItem);
                m_RowTracks[rowStart].originMembers.Add(placement.rowItem);
            }

            PlaceRowLockedItems(rowLocked, gridDefinition.autoColSize, gridDefinition.gridFillDensity);
            PlaceUnlockedItems(unlocked);

            InitializeTrackSizes();

            ListPool<GridPlacement>.Release(rowLocked);
            ListPool<GridPlacement>.Release(unlocked);
            ListPool<GridPlacement>.Release(axisLocked);

            List<Rect> retn = SetRectPositions(children);
            m_Occupied.Clear();
            m_Placements.Clear();
            m_RowTracks.Clear();
            m_ColTracks.Clear();
            m_LayoutNode = null;
            return retn;
        }

        private List<Rect> SetRectPositions(List<LayoutNode> children) {
            List<Rect> retn = ListPool<Rect>.Get();

            for (int i = 1; i < m_ColTracks.Count; i++) {
                m_ColTracks[i].offset = m_ColTracks[i - 1].offset + m_ColTracks[i - 1].baseSize;
            }

            for (int i = 1; i < m_RowTracks.Count; i++) {
                m_RowTracks[i].offset = m_RowTracks[i - 1].offset + m_RowTracks[i - 1].baseSize;
            }

            for (int i = 0; i < children.Count; i++) {
                GridPlacement placement = m_Placements[i];

                GridItem rowItem = placement.rowItem;
                GridItem colItem = placement.colItem;

                int colEnd = colItem.trackEnd;
                int rowEnd = rowItem.trackEnd;

                float x = m_ColTracks[colItem.trackStart].offset;
                float y = m_RowTracks[rowItem.trackStart].offset;
                float w = (m_ColTracks[colEnd - 1].offset + m_ColTracks[colEnd - 1].baseSize) - x;
                float h = (m_RowTracks[rowEnd - 1].offset + m_RowTracks[rowEnd - 1].baseSize) - y;

                retn.Add(new Rect(x, y, w, h));

                s_GridItemPool.Release(placement.colItem);
                s_GridItemPool.Release(placement.rowItem);
            }

            return retn;
        }


        private void PlaceRowLockedItems(List<GridPlacement> items, GridTrackSizer autoSizer, GridAutoPlaceDensity density) {
            // if sparse, track colPtr and always use a higher one, else dense will 'pack' the column
            int colPtr = 0;
            for (int i = 0; i < items.Count; i++) {
                GridItem rowItem = items[i].rowItem;
                GridItem colItem = items[i].colItem;
                int rowStart = rowItem.trackStart;
                int rowSpan = rowItem.trackSpan;
                int colSpan = colItem.trackSpan;
                if (density == GridAutoPlaceDensity.Dense) {
                    colPtr = 0;
                }
                if (colPtr + colSpan > m_ColTracks.Count) {
                    CreateImplicitTracks(m_ColTracks, m_ColTracks.Count - colPtr + colSpan, autoSizer);
                }

                while (!IsGridAreaAvailable(colPtr, colSpan, rowStart, rowSpan)) {
                    colPtr++;
                    if (colPtr + colSpan > m_ColTracks.Count) {
                        CreateImplicitTracks(m_ColTracks, m_ColTracks.Count - colPtr + colSpan, autoSizer);
                    }
                }

                OccupyGridArea(colItem, rowItem, colPtr, colSpan, rowStart, rowSpan);
                
            }
        }

        private void OccupyGridArea(GridItem colItem, GridItem rowItem, int colStart, int colSpan, int rowStart, int rowSpan) {
            for (int i = colStart; i < colStart + colSpan; i++) {
                for (int j = rowStart; j < rowStart + rowSpan; j++) {
                    m_Occupied.Add(BitUtil.SetHighLowBits(i, j));
                }
            }

            colItem.trackStart = colStart;
            rowItem.trackStart = rowStart;
            m_ColTracks[colStart].originMembers.Add(colItem);
            m_RowTracks[rowStart].originMembers.Add(rowItem);
        }

        private bool IsGridAreaAvailable(int colStart, int colSpan, int rowStart, int rowSpan) {
            for (int i = colStart; i < colStart + colSpan; i++) {
                for (int j = rowStart; j < rowStart + rowSpan; j++) {
                    if (m_Occupied.Contains(BitUtil.SetHighLowBits(i, j))) return false;
                }
            }

            return true;
        }

        private void PlaceUnlockedItems(List<GridPlacement> items) { }

        private GridItem CreateColItem(LayoutNode node, UIUnit widthUnit, float contentAreaWidth, float viewportWidth, GridTrackSizer autoSizer) {
            GridPlacementParameters gridPlacement = node.element.style.gridItem;
            int colStart = gridPlacement.colStart;
            int colSpan = IntUtil.IsDefined(gridPlacement.colSpan) ? Mathf.Min(1, gridPlacement.colSpan) : 1;
            bool spansFlexible = false;

            if (IntUtil.IsDefined(colStart)) {
                int colEnd = colStart + colSpan;
                if (m_ColTracks.Count < colEnd) {
                    CreateImplicitTracks(m_ColTracks, colEnd - m_ColTracks.Count, autoSizer);
                }

                for (int j = colStart; j < colEnd; j++) {
                    if (m_ColTracks[j].sizingFn.minSizingFunction.type == GridCellMeasurementType.Fractional) {
                        spansFlexible = true;
                    }

                    m_ColTracks[j].isOccupied = true;
                }
            }

            float preferredSize = node.GetPreferredWidth(widthUnit, contentAreaWidth, viewportWidth);
            float maxSize = node.GetMaxWidth(widthUnit, contentAreaWidth, viewportWidth);
            float minSize = node.GetMinWidth(widthUnit, contentAreaWidth, viewportWidth);
            preferredSize = FloatUtil.IsDefined(minSize) && preferredSize < minSize ? minSize : preferredSize;
            preferredSize = FloatUtil.IsDefined(maxSize) && preferredSize > maxSize ? maxSize : preferredSize;

            GridItem colItem = s_GridItemPool.Get();
            colItem.trackStart = colStart;
            colItem.trackSpan = colSpan;
            colItem.minSize = minSize;
            colItem.maxSize = maxSize;
            colItem.preferredSize = preferredSize;
            colItem.spansFlexible = spansFlexible;

            return colItem;
        }

        private GridItem CreateRowItem(LayoutNode node, UIUnit heightUnit, float width, float contentAreaHeight, float viewportHeight, GridTrackSizer autoSizer) {
            GridPlacementParameters gridItemParameters = node.element.style.gridItem;
            int rowStart = gridItemParameters.rowStart;
            int rowSpan = IntUtil.IsDefined(gridItemParameters.rowSpan) ? Mathf.Min(1, gridItemParameters.rowSpan) : 1;
            bool spansFlexible = false;

            if (IntUtil.IsDefined(rowStart)) {
                int rowEnd = rowStart + rowSpan;

                if (m_RowTracks.Count < rowEnd) {
                    CreateImplicitTracks(m_RowTracks, rowEnd - m_RowTracks.Count, autoSizer);
                }

                for (int j = rowStart; j < rowEnd; j++) {
                    if (m_RowTracks[j].sizingFn.minSizingFunction.type == GridCellMeasurementType.Fractional) {
                        spansFlexible = true;
                    }

                    m_RowTracks[j].isOccupied = true;
                }
            }

            float preferredSize = node.GetPreferredHeight(heightUnit, width, contentAreaHeight, viewportHeight);
            float maxSize = node.GetMaxHeight(heightUnit, contentAreaHeight, viewportHeight);
            float minSize = node.GetMinHeight(heightUnit, contentAreaHeight, viewportHeight);
            preferredSize = FloatUtil.IsDefined(minSize) && preferredSize < minSize ? minSize : preferredSize;
            preferredSize = FloatUtil.IsDefined(maxSize) && preferredSize > maxSize ? maxSize : preferredSize;
          
            GridItem rowItem = new GridItem(rowStart, rowSpan, minSize, maxSize, preferredSize, spansFlexible);

            return rowItem;
        }

        private void InitializeTrackSizes() {
            List<GridTrack> intrinsics = ListPool<GridTrack>.Get();

            for (int i = 0; i < m_ColTracks.Count; i++) {
                GridTrack track = m_ColTracks[i];
                track.baseSize = ResolveFixedWidthMeasurement(track.sizingFn.minSizingFunction, m_LayoutNode);
                if ((track.sizingFn.minSizingFunction.type & GridCellMeasurementType.Intrinsic) != 0) {
                    intrinsics.Add(track);
                }

                GridCellMeasurementType maxSizeType = track.sizingFn.maxSizingFunction.type;

                if ((maxSizeType & GridCellMeasurementType.Intrinsic) != 0) {
                    track.growthLimit = float.MaxValue;
                }
                else if (maxSizeType == GridCellMeasurementType.Fractional) {
                    track.growthLimit = track.baseSize;
                }
                else {
                    track.growthLimit = ResolveFixedWidthMeasurement(track.sizingFn.maxSizingFunction, m_LayoutNode);
                }

                if (track.growthLimit < track.baseSize) {
                    track.growthLimit = track.baseSize;
                }
            }

            List<GridTrack> withSingleSpan = FindTracksWithMembers(intrinsics, 1);

            for (int i = 0; i < withSingleSpan.Count; i++) {
                GridTrack track = withSingleSpan[i];
                List<GridItem> items = track.GetItemsWithSpan(1);

                switch (track.sizingFn.minSizingFunction.type) {
                    case GridCellMeasurementType.Auto:
                    case GridCellMeasurementType.MinContent:
                        track.baseSize = FindMaxMinSize(items).minSize;
                        break;
                    case GridCellMeasurementType.MaxContent:
                        track.baseSize = FindMaxMaxSize(items);
                        break;
                }

                switch (track.sizingFn.maxSizingFunction.type) {
                    case GridCellMeasurementType.MinContent:
                        track.growthLimit = FindMaxMinSize(items).minSize;
                        break;
                    case GridCellMeasurementType.MaxContent:
                        track.growthLimit = FindMaxMaxSize(items);
                        // todo -- for fit-content need to clamp this
                        break;
                }

                if (track.growthLimit < track.baseSize) {
                    track.growthLimit = track.baseSize;
                }

                ListPool<GridItem>.Release(items);
            }

            ListPool<GridTrack>.Release(withSingleSpan);
            List<GridTrack> nonFlexSpanned = FindTracksWithMembers(intrinsics, 2);

            // todo -- walk this up until reached max-span
            for (int i = 0; i < nonFlexSpanned.Count; i++) {
                GridTrack track = nonFlexSpanned[i];
                List<GridItem> items = track.GetItemsWithSpan(2);

                if ((track.sizingFn.minSizingFunction.type & GridCellMeasurementType.Intrinsic) != 0) {
                    if (track.sizingFn.minSizingFunction.type == GridCellMeasurementType.MinContent) {
                        float min = float.MaxValue;
                        bool shouldDistribute = false;
                        GridItem maxMinItem = FindMaxMinSize(items);
                        for (int j = 0; j < items.Count; j++) {
                            if (FloatUtil.IsDefined(maxMinItem.minSize) && maxMinItem.minSize < min) {
                                shouldDistribute = true;
                            }
                        }

                        if (shouldDistribute) {
                            DistributeSpace(m_RowTracks, items[i], items[i].minSize);
                        }
                    }

                    else if (track.sizingFn.minSizingFunction.type == GridCellMeasurementType.MaxContent) {
                        float max = float.MinValue;
                        bool shouldDistribute = false;
                        for (int j = 0; j < items.Count; j++) {
                            if (FloatUtil.IsDefined(items[i].maxSize) && items[i].maxSize < max) {
                                max = items[i].maxSize;
                                shouldDistribute = true;
                            }
                        }

                        if (shouldDistribute) {
                            DistributeSpace(m_RowTracks, items[i], items[i].minSize);
                        }
                    }
                }

                ListPool<GridItem>.Release(items);
            }

            // for each spanned track
            // space to distribute = subtrack spanned track basesize from item size
            // extra space = max(0, size-contribution - sum of track sizes)
            // find items with span of x that do not span a flexible track
            // if intrinsic min: distribute extra space to accomodate min size
        }

        private static void DistributeSpace(List<GridTrack> tracks, GridItem item, float itemSize) {
            float spannedTrackSize = 0;
            int trackEnd = item.trackStart + item.trackSpan;
            for (int i = item.trackStart; i < trackEnd; i++) {
                spannedTrackSize += tracks[i].baseSize; // Mathf.Max(tracks[i].baseSize, tracks[i].growthLimit);
            }

            float remainingSpace = itemSize - spannedTrackSize;
            float pieceSize = ComputePieceSize(remainingSpace, tracks, item);

            bool didAllocate = remainingSpace > 0;
            while (didAllocate) {
                didAllocate = false;
                for (int i = item.trackStart; i < trackEnd; i++) {
                    GridTrack track = tracks[i];
                    if (track.baseSize < track.growthLimit) {
                        didAllocate = true;
                        track.baseSize += pieceSize;
                        if (track.baseSize > track.growthLimit) {
                            float diff = track.growthLimit - track.baseSize;
                            track.baseSize = track.growthLimit;
                            remainingSpace -= diff;
                            pieceSize = ComputePieceSize(remainingSpace, tracks, item);
                        }
                        else {
                            remainingSpace -= pieceSize;
                        }

                        if (pieceSize <= 0) {
                            didAllocate = false;
                            break;
                        }
                    }
                }
            }

            // if we have more space here we grow past growth limits
            if ((int) remainingSpace > 0) {
                pieceSize = remainingSpace / item.trackSpan;
                for (int i = item.trackStart; i < trackEnd; i++) {
                    tracks[i].baseSize += pieceSize;
                }
            }
        }

        private static float ComputePieceSize(float remainingSpace, List<GridTrack> tracks, GridItem item) {
            int pieces = 0;
            int trackEnd = item.trackStart + item.trackSpan;
            for (int i = item.trackStart; i < trackEnd; i++) {
                if (tracks[i].baseSize < tracks[i].growthLimit) {
                    pieces++;
                }
            }

            if (pieces == 0) return 0;
            return remainingSpace / pieces;
        }

        private static GridItem FindMaxMinSize(List<GridItem> items) {
            float min = float.MaxValue;
            int minIdx = 0;
            for (int i = 0; i < items.Count; i++) {
                if (items[i].minSize < min) min = items[i].minSize;
            }

            return items[minIdx];
        }

        private static float FindMaxMaxSize(List<GridItem> items) {
            float max = float.MinValue;
            for (int i = 0; i < items.Count; i++) {
                if (items[i].maxSize < max) max = items[i].maxSize;
            }

            return max;
        }

        private static List<GridTrack> FindTracksWithMembers(List<GridTrack> tracks, int spanSize) {
            List<GridTrack> retn = ListPool<GridTrack>.Get();
            for (int i = 0; i < tracks.Count; i++) {
                for (int j = 0; j < tracks[i].originMembers.Count; j++) {
                    GridItem item = tracks[i].originMembers[j];
                    if (!item.spansFlexible && item.trackSpan == spanSize) {
                        retn.Add(tracks[i]);
                        break;
                    }
                }
            }

            return retn;
        }

        private static float ResolveFixedWidthMeasurement(GridTrackSizeFn measurement, LayoutNode current) {
            switch (measurement.type) {
                case GridCellMeasurementType.Pixel:
                    return measurement.value;
                case GridCellMeasurementType.Em:
                    return 0;
                case GridCellMeasurementType.Viewport:
                    return 0;
                case GridCellMeasurementType.Parent:
                    return measurement.value * current.element.layoutResult.width;
                case GridCellMeasurementType.Content:
                    return 0;
                default:
                    // todo wrong
                    return float.MaxValue;
            }
        }

        private static void CreateImplicitTracks(List<GridTrack> tracks, int count, GridTrackSizer autoCellSizeFn) {
            int start = tracks.Count;
            int total = start + count;
            for (int i = start; i < total; i++) {
                tracks.Add(new GridTrack(autoCellSizeFn));
            }
        }

    }

}