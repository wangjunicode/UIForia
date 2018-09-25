using System.Collections.Generic;
using Rendering;
using Src.Util;
using UnityEngine;

namespace Src.Layout {

    // max-content size == preferred size!
    // smallest size in a given axis when given infinite space
    // ie min size of box that fits around content and avoids overflow

    // auto -> when max == max-content
    // auto -> when min == largest min size of track

    public class GridLayout : UILayout {

        private LayoutNode m_LayoutNode;
        private int m_MaxColSpan;
        private int m_MaxRowSpan;

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
            this.m_Placements = new List<GridPlacement>();
        }

        public override List<Rect> Run(Rect viewport, LayoutNode currentNode) {
            m_LayoutNode = currentNode;
            List<LayoutNode> children = m_LayoutNode.children;

            GridDefinition gridDefinition = m_LayoutNode.element.style.gridDefinition;

            float contentAreaWidth = m_LayoutNode.element.layoutResult.width;
            float contentAreaHeight = m_LayoutNode.element.layoutResult.height;

            List<GridPlacement> bothAxesLocked = ListPool<GridPlacement>.Get();
            List<GridPlacement> oneAxisLocked = ListPool<GridPlacement>.Get();
            List<GridPlacement> noAxisLocked = ListPool<GridPlacement>.Get();

            // todo -- try to make GridTrack a struct or at least pool

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

            bool autoFlowRow = gridDefinition.autoFlow != GridAutoFlow.Column;

            for (int i = 0; i < children.Count; i++) {
                LayoutNode child = children[i];

                GridItem colItem = CreateColItem(child, widthUnit, contentAreaWidth, viewport.width);
                GridItem rowItem = CreateRowItem(child, heightUnit, colItem.outputSize, contentAreaHeight, viewport.height);

                GridPlacementParameters gridPlacementParameters = child.element.style.gridItem;
                int colStart = gridPlacementParameters.colStart;
                int colSpan = gridPlacementParameters.colSpan;

                int rowStart = gridPlacementParameters.rowStart;
                int rowSpan = gridPlacementParameters.rowSpan;

                if (colSpan > m_MaxColSpan) m_MaxColSpan = colSpan;
                if (rowSpan > m_MaxRowSpan) m_MaxRowSpan = rowSpan;

                if (IntUtil.IsDefined(colStart)) {
                    colItem.trackStart = colStart;
                    colItem.trackSpan = IntUtil.IsDefined(colSpan) ? Mathf.Min(1, colSpan) : 1;
                    int colEnd = colStart + colSpan;

                    if (m_ColTracks.Count < colEnd) {
                        CreateImplicitTracks(m_ColTracks, colEnd, gridDefinition.autoColSize);
                    }

                    for (int j = colStart; j < colEnd; j++) {
                        if (m_ColTracks[j].sizingFn.minSizingFunction.type == GridTrackSizeType.Fractional) {
                            colItem.spansFlexible = true;
                        }
                    }
                }

                if (IntUtil.IsDefined(rowStart)) {
                    rowItem.trackStart = rowStart;
                    rowItem.trackSpan = IntUtil.IsDefined(rowSpan) ? Mathf.Min(1, rowSpan) : 1;
                    int rowEnd = rowStart + rowSpan;

                    if (m_RowTracks.Count < rowEnd) {
                        CreateImplicitTracks(m_RowTracks, rowEnd, gridDefinition.autoRowSize);
                    }

                    for (int j = rowStart; j < rowEnd; j++) {
                        if (m_RowTracks[j].sizingFn.minSizingFunction.type == GridTrackSizeType.Fractional) {
                            rowItem.spansFlexible = true;
                        }
                    }
                }

                if (colItem.IsAxisLocked && rowItem.IsAxisLocked) {
                    bothAxesLocked.Add(new GridPlacement(rowItem, colItem));
                }
                else if (autoFlowRow && rowItem.IsAxisLocked) {
                    oneAxisLocked.Add(new GridPlacement(rowItem, colItem));
                }
                else if (!autoFlowRow && colItem.IsAxisLocked) {
                    oneAxisLocked.Add(new GridPlacement(rowItem, colItem));
                }
                else {
                    noAxisLocked.Add(new GridPlacement(rowItem, colItem));
                }

                m_Placements.Add(new GridPlacement(rowItem, colItem));
            }

            PlaceBothAxesLocked(bothAxesLocked);
            PlaceSingleLockedItems(oneAxisLocked, gridDefinition);
            PlaceUnlockedItems(noAxisLocked, gridDefinition);

            ResolveColTrackSizes(viewport.width);
            ResolveRowTrackSizes(viewport.height);

            ListPool<GridPlacement>.Release(oneAxisLocked);
            ListPool<GridPlacement>.Release(noAxisLocked);
            ListPool<GridPlacement>.Release(bothAxesLocked);

            List<Rect> retn = SetRectPositions(children, gridDefinition.colGap, gridDefinition.rowGap);
            m_Occupied.Clear();
            m_Placements.Clear();
            m_RowTracks.Clear();
            m_ColTracks.Clear();
            m_LayoutNode = null;
            m_MaxColSpan = 0;
            m_MaxRowSpan = 0;
            return retn;
        }

        private void PlaceBothAxesLocked(List<GridPlacement> bothAxesLocked) {
            for (int i = 0; i < bothAxesLocked.Count; i++) {
                GridPlacement placement = bothAxesLocked[i];
                GridItem colItem = placement.colItem;
                GridItem rowItem = placement.rowItem;

                OccupyGridArea(placement.colItem, placement.rowItem, colItem.trackStart, colItem.trackSpan, rowItem.trackStart, rowItem.trackSpan);
            }
        }

        private void PlaceSingleLockedItems(List<GridPlacement> items, GridDefinition gridDefinition) {
            // if sparse, track colPtr and always use a higher one, else dense will 'pack' the column
            GridAutoPlaceDensity density = gridDefinition.gridFillDensity;
            bool autoFlowRow = gridDefinition.autoFlow != GridAutoFlow.Column;

            if (autoFlowRow) {
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

                    CreateImplicitTracks(m_ColTracks, colPtr + colSpan, gridDefinition.autoColSize);

                    while (!IsGridAreaAvailable(colPtr, colSpan, rowStart, rowSpan)) {
                        colPtr++;
                        CreateImplicitTracks(m_ColTracks, colPtr + colSpan, gridDefinition.autoColSize);
                    }

                    OccupyGridArea(colItem, rowItem, colPtr, colSpan, rowStart, rowSpan);
                }
            }
            else {
                int rowPtr = 0;
                for (int i = 0; i < items.Count; i++) {
                    GridItem rowItem = items[i].rowItem;
                    GridItem colItem = items[i].colItem;
                    int colStart = colItem.trackStart;
                    int colSpan = colItem.trackSpan;
                    int rowSpan = rowItem.trackSpan;

                    if (density == GridAutoPlaceDensity.Dense) {
                        rowPtr = 0;
                    }

                    CreateImplicitTracks(m_RowTracks, rowPtr + rowSpan, gridDefinition.autoRowSize);

                    while (!IsGridAreaAvailable(colStart, colSpan, rowPtr, rowSpan)) {
                        rowPtr++;
                        if (rowPtr + rowSpan > m_RowTracks.Count) {
                            CreateImplicitTracks(m_RowTracks, rowPtr + rowSpan, gridDefinition.autoRowSize);
                        }
                    }

                    OccupyGridArea(colItem, rowItem, colStart, colSpan, rowPtr, rowSpan);
                }
            }
        }

        // todo -- can probably optimize cursor position here
        private void PlaceUnlockedItems(List<GridPlacement> items, GridDefinition gridDefinition) {
            bool autoFlowRow = gridDefinition.autoFlow != GridAutoFlow.Column;

            int cursorX = 0;
            int cursorY = 0;

            bool dense = gridDefinition.gridFillDensity == GridAutoPlaceDensity.Dense;

            if (autoFlowRow) {
                // create all the columns to hold the grid items
                for (int i = 0; i < m_Placements.Count; i++) {
                    GridItem colItem = m_Placements[i].colItem;
                    if (colItem.IsAxisLocked) {
                        CreateImplicitTracks(m_ColTracks, colItem.trackStart + colItem.trackSpan, gridDefinition.autoColSize);
                    }
                    else if (colItem.trackSpan > m_ColTracks.Count) {
                        CreateImplicitTracks(m_ColTracks, colItem.trackSpan, gridDefinition.autoColSize);
                    }
                }

                for (int i = 0; i < items.Count; i++) {
                    GridPlacement placement = items[i];
                    GridItem colItem = placement.colItem;
                    GridItem rowItem = placement.rowItem;
                    int colSpan = colItem.trackSpan;
                    int rowSpan = rowItem.trackSpan;

                    if (dense) {
                        cursorX = 0;
                    }

                    if (placement.colItem.IsAxisLocked) {
                        int oldX = cursorX;
                        cursorX = placement.colItem.trackStart;

                        if (cursorX < oldX) {
                            cursorY++;
                        }

                        CreateImplicitTracks(m_RowTracks, cursorY + rowSpan, gridDefinition.autoRowSize);

                        while (!IsGridAreaAvailable(cursorX, colSpan, cursorY, rowSpan)) {
                            cursorY++;
                            CreateImplicitTracks(m_RowTracks, cursorY + rowSpan, gridDefinition.autoRowSize);
                        }
                    }
                    else {
                        CreateImplicitTracks(m_RowTracks, cursorY + rowSpan, gridDefinition.autoRowSize);
                        while (!IsGridAreaAvailable(cursorX, colSpan, cursorY, rowSpan)) {
                            cursorX++;
                            if (cursorX + colSpan > m_ColTracks.Count) {
                                cursorX = 0;
                                cursorY++;
                                CreateImplicitTracks(m_RowTracks, cursorY + rowSpan, gridDefinition.autoRowSize);
                            }
                        }
                    }

                    OccupyGridArea(colItem, rowItem, cursorX, colSpan, cursorY, rowSpan);
                }
            }
            else {
                for (int i = 0; i < m_Placements.Count; i++) {
                    GridItem rowItem = m_Placements[i].rowItem;
                    if (rowItem.IsAxisLocked) {
                        CreateImplicitTracks(m_RowTracks, rowItem.trackStart + rowItem.trackSpan, gridDefinition.autoRowSize);
                    }
                    else if (rowItem.trackSpan > m_RowTracks.Count) {
                        CreateImplicitTracks(m_RowTracks, rowItem.trackSpan, gridDefinition.autoRowSize);
                    }
                }

                for (int i = 0; i < items.Count; i++) {
                    GridPlacement placement = items[i];
                    GridItem colItem = placement.colItem;
                    GridItem rowItem = placement.rowItem;
                    int colSpan = colItem.trackSpan;
                    int rowSpan = rowItem.trackSpan;

                    if (dense) {
                        cursorX = 0;
                    }

                    if (placement.rowItem.IsAxisLocked) {
                        int oldY = cursorY;
                        cursorY = placement.rowItem.trackStart;
                        if (cursorY < oldY) {
                            cursorX++;
                        }

                        CreateImplicitTracks(m_ColTracks, cursorX + colSpan, gridDefinition.autoColSize);

                        while (!IsGridAreaAvailable(cursorX, colSpan, cursorY, rowSpan)) {
                            cursorX++;
                            CreateImplicitTracks(m_ColTracks, cursorX + colSpan, gridDefinition.autoColSize);
                        }
                    }
                    else {
                        CreateImplicitTracks(m_ColTracks, cursorX + colSpan, gridDefinition.autoColSize);
                        while (!IsGridAreaAvailable(cursorX, colSpan, cursorY, rowSpan)) {
                            cursorY++;
                            if (cursorY + rowSpan > m_RowTracks.Count) {
                                cursorY = 0;
                                cursorX++;
                                CreateImplicitTracks(m_ColTracks, cursorX + colSpan, gridDefinition.autoColSize);
                            }
                        }
                    }

                    OccupyGridArea(colItem, rowItem, cursorX, colSpan, cursorY, rowSpan);
                }
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

            for (int j = colItem.trackStart; j < colItem.trackStart + colItem.trackSpan; j++) {
                if (m_ColTracks[j].sizingFn.minSizingFunction.type == GridTrackSizeType.Fractional) {
                    colItem.spansFlexible = true;
                }
            }

            for (int j = rowItem.trackStart; j < rowItem.trackStart + rowItem.trackSpan; j++) {
                if (m_RowTracks[j].sizingFn.minSizingFunction.type == GridTrackSizeType.Fractional) {
                    rowItem.spansFlexible = true;
                }
            }
        }

        private bool IsGridAreaAvailable(int colStart, int colSpan, int rowStart, int rowSpan) {
            for (int i = colStart; i < colStart + colSpan; i++) {
                for (int j = rowStart; j < rowStart + rowSpan; j++) {
                    if (m_Occupied.Contains(BitUtil.SetHighLowBits(i, j))) return false;
                }
            }

            return true;
        }

        private static GridItem CreateColItem(LayoutNode node, UIUnit widthUnit, float contentAreaWidth, float viewportWidth) {
            GridPlacementParameters gridPlacement = node.element.style.gridItem;
            int colStart = gridPlacement.colStart;
            int colSpan = IntUtil.IsDefined(gridPlacement.colSpan) ? Mathf.Min(1, gridPlacement.colSpan) : 1;

            float preferredSize = node.GetPreferredWidth(widthUnit, contentAreaWidth, viewportWidth);
            float maxSize = node.GetMaxWidth(widthUnit, contentAreaWidth, viewportWidth);
            float minSize = node.GetMinWidth(widthUnit, contentAreaWidth, viewportWidth);
            preferredSize = FloatUtil.IsDefined(minSize) && preferredSize < minSize ? minSize : preferredSize;
            preferredSize = FloatUtil.IsDefined(maxSize) && preferredSize > maxSize ? maxSize : preferredSize;

            GridItem item = s_GridItemPool.Get();
            item.trackStart = colStart;
            item.trackSpan = colSpan;
            item.minSize = minSize;
            item.maxSize = maxSize;
            item.outputSize = preferredSize;

            return item;
        }

        private static GridItem CreateRowItem(LayoutNode node, UIUnit heightUnit, float width, float contentAreaHeight, float viewportHeight) {
            GridPlacementParameters gridItemParameters = node.element.style.gridItem;
            int rowStart = gridItemParameters.rowStart;
            int rowSpan = IntUtil.IsDefined(gridItemParameters.rowSpan) ? Mathf.Min(1, gridItemParameters.rowSpan) : 1;

            float preferredSize = node.GetPreferredHeight(heightUnit, width, contentAreaHeight, viewportHeight);
            float maxSize = node.GetMaxHeight(heightUnit, contentAreaHeight, viewportHeight);
            float minSize = node.GetMinHeight(heightUnit, contentAreaHeight, viewportHeight);
            preferredSize = FloatUtil.IsDefined(minSize) && preferredSize < minSize ? minSize : preferredSize;
            preferredSize = FloatUtil.IsDefined(maxSize) && preferredSize > maxSize ? maxSize : preferredSize;

            GridItem item = s_GridItemPool.Get();
            item.trackStart = rowStart;
            item.trackSpan = rowSpan;
            item.minSize = minSize;
            item.maxSize = maxSize;
            item.outputSize = preferredSize;

            return item;
        }

        private void ResolveColTrackSizes(float viewportWidth) {
            List<GridTrack> intrinsics = ListPool<GridTrack>.Get();

            for (int i = 0; i < m_ColTracks.Count; i++) {
                GridTrack track = m_ColTracks[i];

                GridTrackSizeType maxSizeType = track.sizingFn.maxSizingFunction.type;
                GridTrackSizeType minSizeType = track.sizingFn.minSizingFunction.type;

                track.baseSize = ResolveFixedWidthMeasurement(track.sizingFn.minSizingFunction, m_LayoutNode, viewportWidth);

                if ((minSizeType & GridTrackSizeType.Intrinsic) != 0) {
                    intrinsics.Add(track);
                }

                if ((maxSizeType & GridTrackSizeType.Intrinsic) != 0) {
                    track.growthLimit = float.MaxValue;
                }
                else if (maxSizeType == GridTrackSizeType.Fractional) {
                    track.growthLimit = track.baseSize;
                }
                else {
                    track.growthLimit = ResolveFixedWidthMeasurement(track.sizingFn.maxSizingFunction, m_LayoutNode, viewportWidth);
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
                    case GridTrackSizeType.Auto:
                    case GridTrackSizeType.MinContent:
                        track.baseSize = FindMaxMinSize(items).outputSize;
                        break;
                    case GridTrackSizeType.MaxContent:
                        track.baseSize = FindMaxMaxSize(items).outputSize;
                        break;
                }

                switch (track.sizingFn.maxSizingFunction.type) {
                    case GridTrackSizeType.MinContent:
                        track.growthLimit = FindMaxMinSize(items).outputSize;
                        break;
                    case GridTrackSizeType.MaxContent:
                        track.growthLimit = FindMaxMaxSize(items).outputSize;
                        // todo -- for fit-content need to clamp this
                        break;
                }

                if (track.growthLimit < track.baseSize) {
                    track.growthLimit = track.baseSize;
                }

                ListPool<GridItem>.Release(items);
            }

            ListPool<GridTrack>.Release(withSingleSpan);

            int spanSize = 2;
            while (spanSize > m_MaxColSpan) {
                List<GridTrack> nonFlexSpanned = FindTracksWithMembers(intrinsics, spanSize);

                for (int i = 0; i < nonFlexSpanned.Count; i++) {
                    GridTrack track = nonFlexSpanned[i];
                    List<GridItem> items = track.GetItemsWithSpan(spanSize);

                    GridTrackSizeType minType = track.sizingFn.minSizingFunction.type;

                    if (minType == GridTrackSizeType.MinContent) {
                        DistributeSpace(m_ColTracks, FindMaxMinSize(items));
                    }
                    else if (minType == GridTrackSizeType.MaxContent) {
                        DistributeSpace(m_ColTracks, FindMaxMaxSize(items));
                    }
                    spanSize++;
                }

                //minmax(min-content, max-content)
                ListPool<GridTrack>.Release(nonFlexSpanned);
            }
         
            // min content: as wide as narrowest contained element that does not overflow
            // how does grid compute content width / height? -- probably needs to run layout 
            // todo -- repeat keyword with auto-fill and maybe auto-fit
            
        }

        private void ResolveRowTrackSizes(float viewportSize) {
            List<GridTrack> intrinsics = ListPool<GridTrack>.Get();

            for (int i = 0; i < m_RowTracks.Count; i++) {
                GridTrack track = m_RowTracks[i];

                GridTrackSizeType maxSizeType = track.sizingFn.maxSizingFunction.type;
                GridTrackSizeType minSizeType = track.sizingFn.minSizingFunction.type;

                track.baseSize = ResolveFixedHeightMeasurement(track.sizingFn.minSizingFunction, m_LayoutNode, viewportSize);

                if ((minSizeType & GridTrackSizeType.Intrinsic) != 0) {
                    intrinsics.Add(track);
                }

                if ((maxSizeType & GridTrackSizeType.Intrinsic) != 0) {
                    track.growthLimit = float.MaxValue;
                }
                else if (maxSizeType == GridTrackSizeType.Fractional) {
                    track.growthLimit = track.baseSize;
                }
                else {
                    track.growthLimit = ResolveFixedHeightMeasurement(track.sizingFn.maxSizingFunction, m_LayoutNode, viewportSize);
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
                    case GridTrackSizeType.Auto:
                    case GridTrackSizeType.MinContent:
                        track.baseSize = FindMaxMinSize(items).outputSize;
                        break;
                    case GridTrackSizeType.MaxContent:
                        track.baseSize = FindMaxMaxSize(items).outputSize;
                        break;
                }

                switch (track.sizingFn.maxSizingFunction.type) {
                    case GridTrackSizeType.MinContent:
                        track.growthLimit = FindMaxMinSize(items).outputSize;
                        break;
                    case GridTrackSizeType.MaxContent:
                        track.growthLimit = FindMaxMaxSize(items).outputSize;
                        // todo -- for fit-content need to clamp this
                        break;
                }

                if (track.growthLimit < track.baseSize) {
                    track.growthLimit = track.baseSize;
                }

                ListPool<GridItem>.Release(items);
            }

            ListPool<GridTrack>.Release(withSingleSpan);
        }

        private List<Rect> SetRectPositions(List<LayoutNode> children, float colGap, float rowGap) {
            List<Rect> retn = ListPool<Rect>.Get();
            for (int i = 1; i < m_ColTracks.Count; i++) {
                m_ColTracks[i].offset = colGap + m_ColTracks[i - 1].offset + m_ColTracks[i - 1].baseSize;
            }

            for (int i = 1; i < m_RowTracks.Count; i++) {
                m_RowTracks[i].offset = rowGap + m_RowTracks[i - 1].offset + m_RowTracks[i - 1].baseSize;
            }

            for (int i = 0; i < children.Count; i++) {
                GridPlacement placement = m_Placements[i];

                GridItem rowItem = placement.rowItem;
                GridItem colItem = placement.colItem;

                int colEnd = colItem.trackStart + colItem.trackSpan;
                int rowEnd = rowItem.trackStart + rowItem.trackSpan;

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

        private static void DistributeSpace(List<GridTrack> tracks, GridItem item) {
            float spannedTrackSize = 0;
            int trackEnd = item.trackStart + item.trackSpan;
            for (int i = item.trackStart; i < trackEnd; i++) {
                spannedTrackSize += tracks[i].baseSize; // Mathf.Max(tracks[i].baseSize, tracks[i].growthLimit);?
            }

            float remainingSpace = item.outputSize - spannedTrackSize;
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
                if (items[i].outputSize < min) {
                    min = items[i].outputSize;
                    minIdx = i;
                }
            }

            return items[minIdx];
        }

        private static GridItem FindMaxMaxSize(List<GridItem> items) {
            float max = float.MinValue;
            int maxIdx = 0;
            for (int i = 0; i < items.Count; i++) {
                if (items[i].outputSize > max) {
                    max = items[i].outputSize;
                    maxIdx = i;
                }
            }

            return items[maxIdx];
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

        private static float ResolveFixedWidthMeasurement(GridTrackSizeFn measurement, LayoutNode current, float viewportSize) {
            switch (measurement.type) {
                case GridTrackSizeType.Pixel:
                    return measurement.value;
                case GridTrackSizeType.Em:
                    return current.element.style.fontSize * measurement.value;
                case GridTrackSizeType.Viewport:
                    return viewportSize * measurement.value;
                case GridTrackSizeType.Parent:
                    return measurement.value * current.element.layoutResult.width;
                default:
                    return 0;
            }
        }

        private static float ResolveFixedHeightMeasurement(GridTrackSizeFn measurement, LayoutNode current, float viewportSize) {
            switch (measurement.type) {
                case GridTrackSizeType.Pixel:
                    return measurement.value;
                case GridTrackSizeType.Em:
                    return current.element.style.fontSize * measurement.value;
                case GridTrackSizeType.Viewport:
                    return viewportSize * measurement.value;
                case GridTrackSizeType.Parent:
                    return measurement.value * current.element.layoutResult.height;
                default:
                    return 0;
            }
        }

        private static void CreateImplicitTracks(List<GridTrack> tracks, int count, GridTrackSizer autoCellSizeFn) {
            int start = tracks.Count;
            int total = count - start;
            for (int i = 0; i < total; i++) {
                tracks.Add(new GridTrack(autoCellSizeFn));
            }
        }

    }

}