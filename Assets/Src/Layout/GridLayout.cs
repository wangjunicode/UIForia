using System;
using System.Collections.Generic;
using Rendering;
using Src.Util;
using UnityEngine;

namespace Src.Layout {

    public struct GridPlacement {

        public readonly int rowStart;
        public readonly int rowSpan;
        public readonly int colStart;
        public readonly int colSpan;

        public GridPlacement(int rowStart, int rowSpan, int colStart, int colSpan) {
            this.rowStart = rowStart;
            this.rowSpan = rowSpan;
            this.colStart = colStart;
            this.colSpan = colSpan;
        }

        public int rowEnd => rowStart + rowSpan;

        public int colEnd => colStart + colSpan;

    }

    // todo pool this but keep as ref type probably
    public class GridItem {

        public LayoutNode node;
        public int trackStart;
        public int trackSpan;
        public float minSize;
        public float preferredSize;
        public float maxSize;
        public bool spansFlexible;

        public GridItem(LayoutNode node, int trackStart, int trackSpan) {
            this.node = node;
            this.trackStart = trackStart;
            this.trackSpan = trackSpan;
            this.preferredSize = 0;
            this.minSize = 0;
            this.maxSize = float.MaxValue;
            this.spansFlexible = false;
        }

    }

    [Flags]
    public enum GridCellMeasurementType {

        Fractional = 1 << 0,
        Pixel = 1 << 1,
        Em = 1 << 2,
        MaxContent = 1 << 3,
        MinContent = 1 << 4,
        FitContent = 1 << 5,
        MinMax = 1 << 6,
        Viewport = 1 << 7,
        Parent = 1 << 8,
        Content = 1 << 9,
        Auto = 1 << 10,

        Intrinsic = Auto | MinContent | MaxContent | FitContent

    }

    // max-content size == preferred size!
    // smallest size in a given axis when given infinite space
    // ie min size of box that fits around content and avoids overflow

    public struct GridTrackSizeFn {

        public GridCellMeasurementType type;
        public float value;

        public GridTrackSizeFn(GridCellMeasurementType type, float value) {
            this.type = type;
            this.value = value;
        }

    }

    public struct GridDefinition {

        public GridTrackSizer[] rowTemplate;
        public GridTrackSizer[] colTemplate;
        public GridTrackSizer autoRowSize;
        public GridTrackSizer autoColSize;

    }

    public struct GridTrackSizer {

        public GridTrackSizeFn minSizingFunction;
        public GridTrackSizeFn maxSizingFunction;

        public GridTrackSizer(GridTrackSizeFn minSizingFunction, GridTrackSizeFn maxSizingFunction = default(GridTrackSizeFn)) {
            this.minSizingFunction = minSizingFunction;
            this.maxSizingFunction = maxSizingFunction;
        }

    }

    public class GridTrack {

        public readonly GridTrackSizer sizingFn;
        public float offset;
        public float baseSize;
        public float growthLimit;
        public List<GridItem> originMembers;
        public float plannedGrowthSize;
        public bool isOccupied;

        public GridTrack(GridTrackSizer sizingFn) {
            this.sizingFn = sizingFn;
            this.originMembers = ListPool<GridItem>.Get();
            this.offset = 0;
            this.baseSize = 0;
            this.growthLimit = 0;
        }

        public List<GridItem> GetItemsWithSpan(int spanSize) {
            List<GridItem> retn = ListPool<GridItem>.Get();
            for (int i = 0; i < originMembers.Count; i++) {
                if (!originMembers[i].spansFlexible && originMembers[i].trackSpan == spanSize) {
                    retn.Add(originMembers[i]);
                }
            }

            return retn;
        }

    }

    // auto -> when max == max-content
    // auto -> when min == largest min size of track

    public class GridLayout : UILayout {

        private readonly List<GridTrack> m_RowTracks;
        private readonly List<GridTrack> m_ColTracks;
        private readonly LayoutNode currentNode;
        private readonly List<LayoutNode> m_SpanSortedRowItems;
        private readonly List<LayoutNode> m_SpanSortedColItems;

        public GridLayout(ITextSizeCalculator textSizeCalculator) : base(textSizeCalculator) {
            this.m_RowTracks = new List<GridTrack>();
            this.m_ColTracks = new List<GridTrack>();
            this.m_SpanSortedColItems = new List<LayoutNode>();
            this.m_SpanSortedRowItems = new List<LayoutNode>();
        }

        private void InitializeTrackSizes() {
            List<GridTrack> intrinsics = ListPool<GridTrack>.Get();

            for (int i = 0; i < m_ColTracks.Count; i++) {
                GridTrack track = m_ColTracks[i];
                track.baseSize = ResolveFixedWidthMeasurement(track.sizingFn.minSizingFunction, currentNode);
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
                    track.growthLimit = ResolveFixedWidthMeasurement(track.sizingFn.maxSizingFunction, currentNode);
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

            didAllocate = remainingSpace > 0;
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

        private float ResolveFixedWidthMeasurement(GridTrackSizeFn measurement, LayoutNode current) {
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

        public override List<Rect> Run(Rect viewport, LayoutNode m_LayoutNode) {
            List<Rect> retn = ListPool<Rect>.Get();
            List<LayoutNode> children = m_LayoutNode.children;

            GridDefinition gridDefinition = m_LayoutNode.element.style.gridDefinition;

            float contentAreaWidth = m_LayoutNode.element.layoutResult.width;
            float contentAreaHeight = m_LayoutNode.element.layoutResult.height;


            List<LayoutNode> rowLocked = ListPool<LayoutNode>.Get();
            List<LayoutNode> unlocked = ListPool<LayoutNode>.Get();

            int implicitGridRowStart = gridDefinition.rowTemplate.Length;
            int implicitGridColStart = gridDefinition.colTemplate.Length;

            Dictionary<int, GridPlacement> placements = new Dictionary<int, GridPlacement>();

            for (int i = 0; i < gridDefinition.rowTemplate.Length; i++) {
                m_RowTracks.Add(new GridTrack(gridDefinition.rowTemplate[i]));
            }

            for (int i = 0; i < gridDefinition.colTemplate.Length; i++) {
                m_ColTracks.Add(new GridTrack(gridDefinition.colTemplate[i]));
            }

            UIUnit widthUnit = m_LayoutNode.element.style.width.unit;
            UIUnit heightUnit = m_LayoutNode.element.style.height.unit;

            for (int i = 0; i < children.Count; i++) {
                LayoutNode child = children[i];
                GridPlacement gridPlacement = child.element.style.gridItem;

                int rowStart = gridPlacement.rowStart;
                int colStart = gridPlacement.colStart;
                int rowSpan = gridPlacement.rowSpan;
                int colSpan = gridPlacement.colSpan;
                int rowEnd = rowStart + rowSpan;
                int colEnd = colStart + colSpan;

                if (m_RowTracks.Count < rowEnd) {
                    CreateImplicitTracks(m_RowTracks, rowEnd - m_RowTracks.Count, gridDefinition.autoRowSize);
                }

                if (m_ColTracks.Count < colEnd) {
                    CreateImplicitTracks(m_ColTracks, colEnd - m_ColTracks.Count, gridDefinition.autoColSize);
                }

                float preferredWidth = child.GetPreferredWidth(widthUnit, contentAreaWidth, viewport.width);
                float maxSize = child.GetMaxWidth(widthUnit, contentAreaWidth, viewport.width);
                float minSize = child.GetMinWidth(widthUnit, contentAreaWidth, viewport.width);

                float preferredHeight = child.GetPreferredHeight(heightUnit, preferredWidth, contentAreaHeight, viewport.height);

                GridItem colItem = new GridItem(child, colStart, colSpan);
                GridItem rowItem = new GridItem(child, rowStart, rowSpan);

                colItem.preferredSize = preferredWidth;
                colItem.maxSize = maxSize;
                colItem.minSize = minSize;
                colItem.spansFlexible = false;

                for (int j = colStart; j < colEnd; j++) {
                    if (m_ColTracks[j].sizingFn.minSizingFunction.type == GridCellMeasurementType.Fractional) {
                        colItem.spansFlexible = true;
                    }

                    m_ColTracks[j].isOccupied = true;
                }

                m_ColTracks[colItem.trackStart].originMembers.Add(colItem);

                rowItem.preferredSize = preferredHeight;

                for (int j = rowStart; j < rowEnd; j++) {
                    if (m_RowTracks[j].sizingFn.minSizingFunction.type == GridCellMeasurementType.Fractional) {
                        rowItem.spansFlexible = true;
                    }

                    m_RowTracks[j].isOccupied = true;
                }

                m_RowTracks[rowItem.trackStart].originMembers.Add(rowItem);

                placements[child.UniqueId] = new GridPlacement(rowStart, rowSpan, colStart, colSpan);
            }

            InitializeTrackSizes();

            for (int i = 1; i < m_ColTracks.Count; i++) {
                m_ColTracks[i].offset = m_ColTracks[i - 1].offset + m_ColTracks[i - 1].baseSize;
            }

            for (int i = 1; i < m_RowTracks.Count; i++) {
                m_RowTracks[i].offset = m_RowTracks[i - 1].offset + m_RowTracks[i - 1].baseSize;
            }

//                LayoutNode child = children[i];
//                GridPlacement gridPlacement = child.element.style.gridItem;
//
//                //if (gridPlacement.IsDefined && gridPlacement.IsDefined) {
//                    int rowStart = gridPlacement.rowStart;
//                    int colStart = gridPlacement.colStart;
//                    int rowSpan = gridPlacement.rowSpan;
//                    int colSpan = gridPlacement.colSpan;
//                    int rowEnd = rowStart + rowSpan;
//                    int colEnd = colStart + colSpan;
//
//                    if (m_RowTracks.Count < rowEnd) {
//                        CreateImplicitTracks(m_RowTracks, rowEnd - m_RowTracks.Count, gridDefinition.autoRowSize);
//                    }
//
//                    if (m_ColTracks.Count < colEnd) {
//                        CreateImplicitTracks(m_ColTracks, colEnd - m_ColTracks.Count, gridDefinition.autoColSize);
//                    }
//
//                    OccupyCells(child, m_RowTracks, rowStart, rowEnd);
//                    OccupyCells(child, m_ColTracks, colStart, colEnd);
//
//                    placements[child.UniqueId] = new GridPlacement(rowStart, rowEnd, colStart, colEnd);
//              ///  }
//              //  else if (gridItem.IsRowDefined) {
//               //     rowLocked.Add(child);
//              //      placements[child.UniqueId] = new GridPlacement(0, 0, 0, 0);
//              //  }
//               // else {
//               //     unlocked.Add(child);
//               //     placements[child.UniqueId] = new GridPlacement(0, 0, 0, 0);
//               // }
//            }
//
//            float remainingSpace = contentAreaWidth;
//
//            for (int i = 0; i < m_ColTracks.Count; i++) {
//                GridTrack track = m_ColTracks[i];
//                List<LayoutNode> members = track.members;
//                UIUnit widthUnit = m_LayoutNode.element.style.width.unit;
//                float currentMin = float.MaxValue;
//                float currentMax = float.MinValue;
//                List<GridItemSize> itemSizes = ListPool<GridItemSize>.Get();
//
//                // if colStart == i -> include
//
//                for (int j = 0; j < members.Count; j++) {
//                    LayoutNode node = members[j];
//
//                    float preferredWidth = node.GetPreferredWidth(widthUnit, contentAreaWidth, viewport.width);
//                    float maxSize = node.GetMaxWidth(widthUnit, contentAreaWidth, viewport.width);
//                    float minSize = node.GetMinWidth(widthUnit, contentAreaWidth, viewport.width);
//
//                    float outputSize = preferredWidth;
//
//                    if (FloatUtil.IsDefined(maxSize)) {
//                        if (outputSize > maxSize) {
//                            outputSize = maxSize;
//                        }
//
//                        if (maxSize > currentMax) {
//                            currentMax = maxSize;
//                        }
//                    }
//
//                    if (FloatUtil.IsDefined(minSize)) {
//                        if (outputSize < minSize) {
//                            outputSize = minSize;
//                        }
//
//                        if (minSize > currentMin) {
//                            currentMin = minSize;
//                        }
//                    }
//
//                    itemSizes.Add(new GridItemSize(outputSize, minSize, maxSize));
//                    remainingSpace -= outputSize;
//                }
//
//                // 1 track has 1 measurement
//                float size = ResolveTrackSize(track, itemSizes, currentMin, currentMax);
//            }

            /*
            * for each item
            *     x = colTracks[item.colTrackIdx].cells[item.colCellIndex].offset
            *     y = rowCells[item.rowStart].offset
            *     w = colCells[item.colEnd].end - colCells[item.colStart].offset
            *     h = rowCells[item.rowEnd].end - rowCells[item.rowStart].offset 
            */
            for (int i = 0; i < children.Count; i++) {
                GridPlacement placement = placements[children[i].UniqueId];
                float x = m_ColTracks[placement.colStart].offset;
                float y = m_RowTracks[placement.rowStart].offset;
                float w = (m_ColTracks[placement.colEnd - 1].offset + m_ColTracks[placement.colEnd - 1].baseSize) - x; // todo -- account for size 
                float h = 100f;//m_RowTracks[placement.rowEnd - 1].offset - y;
                retn.Add(new Rect(x, y, w, h));
            }

            m_RowTracks.Clear();
            m_ColTracks.Clear();
            m_SpanSortedColItems.Clear();
            m_SpanSortedRowItems.Clear();
            m_LayoutNode = null;
            return retn;
        }

        public struct GridItemSize {

            public float outputSize;
            public float minSize;
            public float maxSize;

            public GridItemSize(float outputSize, float minSize, float maxSize) {
                this.outputSize = outputSize;
                this.minSize = minSize;
                this.maxSize = maxSize;
            }

        }


        private static bool IsFixedMeasurement(GridCellMeasurementType type) {
            return (type != GridCellMeasurementType.Auto && type != GridCellMeasurementType.Fractional);
        }

        private static float ResolveMeasurement(GridTrackSizeFn measurement) {
            switch (measurement.type) {
                case GridCellMeasurementType.Fractional:
                    break;
                case GridCellMeasurementType.Pixel:
                    break;
                case GridCellMeasurementType.Em:
                    break;
                case GridCellMeasurementType.MaxContent:
                    break;
                case GridCellMeasurementType.MinContent:
                    break;
                case GridCellMeasurementType.FitContent:
                    break;
                case GridCellMeasurementType.MinMax:
                    break;
                case GridCellMeasurementType.Viewport:
                    break;
                case GridCellMeasurementType.Parent:
                    break;
                case GridCellMeasurementType.Content:
                    break;
                case GridCellMeasurementType.Auto:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            return 0f;
        }

//        private static void OccupyCells(LayoutNode node, List<GridTrack> tracks, int start, int end) {
//            for (int i = start; i < end; i++) {
//                tracks[i].members.Add(node);
//            }
//        }
//      
        private static void CreateImplicitTracks(List<GridTrack> tracks, int count, GridTrackSizer autoCellSizeFn) {
            int total = tracks.Count + count;
            for (int i = tracks.Count; i < total; i++) {
                tracks.Add(new GridTrack(autoCellSizeFn));
            }
        }

    }

}