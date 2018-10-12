using System;
using System.Collections.Generic;
using Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    [Flags]
    public enum GridTemplateUnit {

        Pixel,
        Container,
        ContainerContentArea,
        Em,
        ViewportWidth,
        ViewportHeight,

        Fraction,
        MinContent,
        MaxContent,

        Fixed = Pixel | Container | ContainerContentArea | Em | ViewportWidth | ViewportHeight

    }

    public struct GridTrackSize {

        public readonly float minValue;
        public readonly float maxValue;

        public readonly GridTemplateUnit minUnit;
        public readonly GridTemplateUnit maxUnit;

        public GridTrackSize(float value, GridTemplateUnit unit) {
            this.minUnit = unit;
            this.minValue = value;
            this.maxUnit = unit;
            this.maxValue = value;
        }

    }

    public class GridLayoutBox : LayoutBox {

        private readonly List<GridTrack> m_RowTracks;
        private readonly List<GridTrack> m_ColTracks;
        private readonly List<GridPlacement> m_Placements;

        private readonly HashSet<int> m_Occupied;

        private static readonly Dictionary<int, List<GridTrackSize>> s_ColTrackTemplates;
        private static readonly Dictionary<int, List<GridTrackSize>> s_RowTrackTemplates;

        private static readonly List<GridTrackSize> s_DefaultTrackSize = new List<GridTrackSize>();

        private readonly List<float> m_Widths;
        private readonly List<float> m_Heights;

        public GridLayoutBox(LayoutSystem2 layoutSystem, UIElement element)
            : base(layoutSystem, element) {
            this.m_Widths = new List<float>(4);
            this.m_Heights = new List<float>(4);
            this.m_RowTracks = new List<GridTrack>();
            this.m_ColTracks = new List<GridTrack>();
            this.m_Occupied = new HashSet<int>();
            this.m_Placements = new List<GridPlacement>();
        }

        internal static IReadOnlyList<GridTrackSize> GetTrackColTemplate(int id) {
            List<GridTrackSize> retn;
            if (id > 0 && s_ColTrackTemplates.TryGetValue(id, out retn)) {
                return retn;
            }

            return s_DefaultTrackSize;
        }

        internal static List<GridTrackSize> GetTrackRowTemplate(int id) {
            List<GridTrackSize> retn;
            if (id > 0 && s_RowTrackTemplates.TryGetValue(id, out retn)) {
                return retn;
            }

            return s_DefaultTrackSize;
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
                float baseWidth = m_Widths[childIndex];

                for (int j = colItem.trackStart; j < colItem.trackStart + colItem.trackSpan; j++) {
                    GridTrack spanned = m_ColTracks[j];
                    if ((spanned.size.minUnit & GridTemplateUnit.Fixed) != 0) {
                        baseWidth -= spanned.outputSize;
                        if (baseWidth <= 0) {
                            break;
                        }
                    }
                    else if (spanned.size.minUnit != GridTemplateUnit.Fraction) {
                        pieces++;
                    }
                }

                if (baseWidth > 0) {
                    minSize = Mathf.Min(minSize, value * (baseWidth / pieces));
                }
            }

            return minSize;
        }

        private float ResolveContentMaxWidth(GridTrack track, SizeType sizeType) {
            float maxSize = 0;

            GridTemplateUnit unit = track.size.minUnit;
            float value = track.size.minValue;

            if (sizeType == SizeType.Max) {
                value = track.size.maxValue;
            }

            for (int i = 0; i < track.spanningItems.Count; i++) {
                int childIndex = track.spanningItems[i];
                GridItem colItem = m_Placements[childIndex].colItem;
                int pieces = 0; // never 0 because we only call this function for intrinsic sized tracks
                float baseWidth = m_Widths[childIndex];

                for (int j = colItem.trackStart; j < colItem.trackStart + colItem.trackSpan; j++) {
                    GridTrack spanned = m_ColTracks[j];
                    if ((spanned.size.minUnit & GridTemplateUnit.Fixed) != 0) {
                        baseWidth -= spanned.outputSize;
                        if (baseWidth <= 0) {
                            break;
                        }
                    }
                    else if (spanned.size.minUnit != GridTemplateUnit.Fraction) {
                        pieces++;
                    }
                }

                if (baseWidth > 0) {
                    maxSize = Mathf.Max(maxSize, value * (baseWidth / pieces));
                }
            }

            return maxSize;
        }

        private float RunContentWidthLayout() {
            
            // todo -- precompute this & update only if needed
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                float minWidth = child.MinWidth;
                float maxWidth = child.MaxWidth;
                float prfWidth = child.PreferredWidth;
                m_Widths[i] = Mathf.Min(minWidth, Mathf.Min(prfWidth, maxWidth));
            }

            List<ValueTuple<int, GridTrack>> intrinsics = ListPool<ValueTuple<int, GridTrack>>.Get();
            List<ValueTuple<int, GridTrack>> flexes = ListPool<ValueTuple<int, GridTrack>>.Get();

            for (int i = 0; i < m_ColTracks.Count; i++) {
                GridTrack track = m_ColTracks[i];

                if ((track.size.minUnit & GridTemplateUnit.Fixed) != 0) {
                    track.outputSize = ResolveFixedWidthMeasurement(track.size, SizeType.Min);
                }
                else if (track.size.minUnit == GridTemplateUnit.Fraction) {
                    flexes.Add(ValueTuple.Create(i, track));
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
                    track.outputSize = ResolveContentMinWidth(track, SizeType.Min);
                }

                m_ColTracks[intrinsics[i].Item1] = track;
            }

            ListPool<ValueTuple<int, GridTrack>>.Release(intrinsics);
            ListPool<ValueTuple<int, GridTrack>>.Release(flexes);

            return m_ColTracks[m_ColTracks.Count - 1].End;
        }

        public override void RunWidthLayout() {
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                float minWidth = child.MinWidth;
                float maxWidth = child.MaxWidth;
                float prfWidth = child.PreferredWidth;
                m_Widths[i] = Mathf.Min(minWidth, Mathf.Min(prfWidth, maxWidth));
            }

            List<ValueTuple<int, GridTrack>> intrinsics = ListPool<ValueTuple<int, GridTrack>>.Get();
            List<ValueTuple<int, GridTrack>> flexes = ListPool<ValueTuple<int, GridTrack>>.Get();

            int flexPieces = 0;
            float remaining = allocatedWidth - PaddingHorizontal - BorderHorizontal;

            for (int i = 0; i < m_ColTracks.Count; i++) {
                GridTrack track = m_ColTracks[i];

                if ((track.size.minUnit & GridTemplateUnit.Fixed) != 0) {
                    track.outputSize = ResolveFixedWidthMeasurement(track.size, SizeType.Min);
                    remaining -= track.outputSize;
                }
                else if (track.size.minUnit == GridTemplateUnit.Fraction) {
                    flexes.Add(ValueTuple.Create(i, track));
                    flexPieces += (int) track.size.minValue; // move flex to max only?
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
                    track.outputSize = ResolveContentMinWidth(track, SizeType.Min);
                }

                remaining -= track.outputSize;
                m_ColTracks[intrinsics[i].Item1] = track;
            }


            if ((int) remaining > 0 && flexes.Count > 0) {
                float pieceSize = remaining / flexPieces;
                for (int i = 0; i < flexes.Count; i++) {
                    GridTrack track = flexes[i].Item2;
                    track.outputSize = track.size.minValue * pieceSize;
                    m_ColTracks[flexes[i].Item1] = track;
                }
            }

            for (int i = 1; i < m_ColTracks.Count; i++) {
                GridTrack track = m_ColTracks[i];
                track.position = m_ColTracks[i - 1].End;
                m_ColTracks[i] = track;
            }

            actualWidth = m_ColTracks[m_ColTracks.Count - 1].position;
            ListPool<ValueTuple<int, GridTrack>>.Release(intrinsics);
            ListPool<ValueTuple<int, GridTrack>>.Release(flexes);
        }

        public override void RunHeightLayout() {
            
        }


        private float ResolveFixedWidthMeasurement(GridTrackSize size, SizeType sizeType) {
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
                    return value * style.EmSize;
                case GridTemplateUnit.ViewportWidth:
                    return value * layoutSystem.ViewportRect.width;
                case GridTemplateUnit.ViewportHeight:
                    return value * layoutSystem.ViewportRect.height;
                case GridTemplateUnit.Container:
                    return parent.allocatedWidth * value;
                case GridTemplateUnit.ContainerContentArea:
                    return (parent.allocatedWidth - parent.PaddingHorizontal - parent.BorderHorizontal) * value;

                case GridTemplateUnit.Fraction:
                case GridTemplateUnit.MinContent:
                case GridTemplateUnit.MaxContent:
                    return 0f;

                default:
                    throw new UIForia.InvalidArgumentException();
            }
        }

        private float ResolveFixedHeightMeasurement(GridTrackSize size, SizeType sizeType) {
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
                    return value * style.EmSize;
                case GridTemplateUnit.ViewportWidth:
                    return value * layoutSystem.ViewportRect.width;
                case GridTemplateUnit.ViewportHeight:
                    return value * layoutSystem.ViewportRect.height;
                case GridTemplateUnit.Container:
                    return parent.allocatedHeight * value;
                case GridTemplateUnit.ContainerContentArea:
                    return (parent.allocatedHeight - parent.PaddingVertical - parent.BorderVertical) * value;

                case GridTemplateUnit.Fraction:
                case GridTemplateUnit.MinContent:
                case GridTemplateUnit.MaxContent:
                    return 0f;
                default:
                    throw new UIForia.InvalidArgumentException();
            }
        }

        private void Place() {
            GridTrackSize autoColSize = style.GridLayoutColAutoSize;
            GridTrackSize autoRowSize = style.GridLayoutRowAutoSize;
            LayoutDirection direction = style.GridLayoutDirection;

            bool autoFlowRow = direction == LayoutDirection.Row;

            List<GridPlacement> bothAxesLocked = ListPool<GridPlacement>.Get();
            List<GridPlacement> singleAxisLocked = ListPool<GridPlacement>.Get();
            List<GridPlacement> noAxisLocked = ListPool<GridPlacement>.Get();

            for (int i = 0; i < m_Placements.Count; i++) {
                GridPlacement placement = m_Placements[i];
                GridItem colItem = placement.colItem;
                GridItem rowItem = placement.rowItem;

                if (IntUtil.IsDefined(colItem.trackStart)) {
                    int colEnd = colItem.trackStart + colItem.trackSpan;

                    if (m_ColTracks.Count < colEnd) {
                        CreateTracks(m_ColTracks, colEnd, autoColSize);
                    }
                }

                if (IntUtil.IsDefined(rowItem.trackStart)) {
                    int rowEnd = rowItem.trackStart + rowItem.trackSpan;

                    if (m_RowTracks.Count < rowEnd) {
                        CreateTracks(m_RowTracks, rowEnd, autoRowSize);
                    }
                }

                if (colItem.IsAxisLocked && rowItem.IsAxisLocked) {
                    bothAxesLocked.Add(placement);
                }
                else if (autoFlowRow && rowItem.IsAxisLocked) {
                    singleAxisLocked.Add(placement);
                }
                else if (!autoFlowRow && colItem.IsAxisLocked) {
                    singleAxisLocked.Add(placement);
                }
                else {
                    noAxisLocked.Add(placement);
                }
            }

            PlaceBothAxesLocked(bothAxesLocked);
            PlaceSingleLockedItems(singleAxisLocked);
            PlaceUnlockedItems(noAxisLocked);
        }

        private void PlaceBothAxesLocked(List<GridPlacement> bothAxesLocked) {
            for (int i = 0; i < bothAxesLocked.Count; i++) {
                OccupyGridArea(bothAxesLocked[i]);
            }
        }

        private static void CreateTracks(List<GridTrack> tracks, int count, GridTrackSize size) {
            int total = count - tracks.Count;
            for (int i = 0; i < total; i++) {
                tracks.Add(new GridTrack(size));
            }
        }

        protected override Size RunContentSizeLayout() {
            // if pending placements -> place them
            return default(Size);
        }

        public override void OnChildAdded(LayoutBox child) {
            if ((child.style.LayoutBehavior & LayoutBehavior.Ignored) == 0) {
                children.Add(child);
                RequestLayout();
            }
        }

        public override void OnChildRemoved(LayoutBox child) { }

        public override void OnStylePropertyChanged(StyleProperty property) {
            switch (property.propertyId) {
                case StylePropertyId.GridLayoutDensity:
                case StylePropertyId.GridLayoutColTemplate:
                case StylePropertyId.GridLayoutRowTemplate:
                case StylePropertyId.GridLayoutDirection:
                case StylePropertyId.GridLayoutRowGap:
                case StylePropertyId.GridLayoutColGap:
                case StylePropertyId.GridLayoutColAutoSize:
                case StylePropertyId.GridLayoutRowAutoSize:
                    break;
            }
        }

        public override void OnChildStylePropertyChanged(LayoutBox child, StyleProperty property) {
            switch (property.propertyId) {
                case StylePropertyId.LayoutBehavior:
                    break;
                case StylePropertyId.GridItemColSpan:
                case StylePropertyId.GridItemRowSpan:
                case StylePropertyId.GridItemColStart:
                case StylePropertyId.GridItemRowStart:
                    break;
            }
        }

        private GridPlacement GetPlacementForId(int id) {
            for (int i = 0; i < m_Placements.Count; i++) {
                if (m_Placements[i].id == id) {
                    return m_Placements[i];
                }
            }

            throw new UIForia.InvalidArgumentException();
        }

        private void OccupyGridArea(GridPlacement placement) {
            int colStart = placement.colItem.trackStart;
            int colSpan = placement.colItem.trackSpan;
            int rowStart = placement.rowItem.trackStart;
            int rowSpan = placement.rowItem.trackSpan;

            for (int i = colStart; i < colStart + colSpan; i++) {
                for (int j = rowStart; j < rowStart + rowSpan; j++) {
                    m_Occupied.Add(BitUtil.SetHighLowBits(i, j));
                }
            }
        }

        private void PlaceSingleLockedItems(List<GridPlacement> placements) {
            // if sparse, track colPtr and always use a higher one, else dense will 'pack' the column
            GridLayoutDensity density = style.GridLayoutDensity;
            bool autoFlowRow = style.GridLayoutDirection != LayoutDirection.Column;

            if (autoFlowRow) {
                int colPtr = 0;
                GridTrackSize autoSize = style.GridLayoutColAutoSize;

                for (int i = 0; i < placements.Count; i++) {
                    GridPlacement placement = placements[i];
                    GridItem colItem = placement.colItem;
                    GridItem rowItem = placement.rowItem;
                    int rowStart = rowItem.trackStart;
                    int rowSpan = rowItem.trackSpan;
                    int colSpan = colItem.trackSpan;

                    if (density == GridLayoutDensity.Dense) {
                        colPtr = 0;
                    }

                    while (!IsGridAreaAvailable(colPtr, colSpan, rowStart, rowSpan)) {
                        colPtr++;
                    }

                    CreateTracks(m_ColTracks, colPtr + colSpan, autoSize);

                    colItem.trackStart = colPtr;

                    placements[i] = new GridPlacement(placement.id, placement.index, colItem, rowItem);

                    OccupyGridArea(m_Placements[placement.index]);
                }
            }
            else {
                int rowPtr = 0;
                GridTrackSize autoSize = style.GridLayoutRowAutoSize;
                for (int i = 0; i < placements.Count; i++) {
                    GridPlacement placement = placements[i];
                    GridItem rowItem = placements[i].rowItem;
                    GridItem colItem = placements[i].colItem;
                    int colStart = colItem.trackStart;
                    int colSpan = colItem.trackSpan;
                    int rowSpan = rowItem.trackSpan;

                    if (density == GridLayoutDensity.Dense) {
                        rowPtr = 0;
                    }

                    while (!IsGridAreaAvailable(colStart, colSpan, rowPtr, rowSpan)) {
                        rowPtr++;
                    }

                    CreateTracks(m_RowTracks, rowPtr + rowSpan, autoSize);

                    rowItem.trackStart = rowPtr;
                    m_Placements[placement.index] = new GridPlacement(placement.id, placement.index, colItem, rowItem);

                    OccupyGridArea(m_Placements[placement.index]);
                }
            }
        }

        private void PlaceUnlockedItems(List<GridPlacement> placements) {
            int cursorX = 0;
            int cursorY = 0;

            bool autoFlowRow = style.GridLayoutDirection == LayoutDirection.Row;
            bool dense = style.GridLayoutDensity == GridLayoutDensity.Dense;

            GridTrackSize autoColSize = style.GridLayoutColAutoSize;
            GridTrackSize autoRowSize = style.GridLayoutRowAutoSize;

            if (autoFlowRow) {
                int colCount = m_ColTracks.Count;

                for (int i = 0; i < placements.Count; i++) {
                    GridPlacement placement = placements[i];
                    GridItem colItem = placement.colItem;
                    GridItem rowItem = placement.rowItem;

                    int colSpan = colItem.trackSpan;
                    int rowSpan = rowItem.trackSpan;

                    if (dense) {
                        cursorX = 0;
                    }

                    while (!IsGridAreaAvailable(cursorX, colSpan, cursorY, rowSpan)) {
                        cursorX++;
                        if (cursorX + colSpan > colCount) {
                            colCount = cursorX + colSpan;
                            cursorX = 0;
                            cursorY++;
                        }
                    }

                    colItem.trackStart = cursorX;

                    m_Placements[placement.index] = new GridPlacement(placement.id, placement.index, colItem, rowItem);

                    CreateTracks(m_ColTracks, colCount, autoColSize);
                    CreateTracks(m_RowTracks, cursorY + rowSpan, autoRowSize);

                    OccupyGridArea(m_Placements[placement.index]);
                }
            }
            else {
                int rowCount = m_RowTracks.Count;

                for (int i = 0; i < placements.Count; i++) {
                    GridPlacement placement = placements[i];
                    GridItem colItem = placement.colItem;
                    GridItem rowItem = placement.rowItem;
                    int colSpan = colItem.trackSpan;
                    int rowSpan = rowItem.trackSpan;

                    if (dense) {
                        cursorY = 0;
                    }

                    while (!IsGridAreaAvailable(cursorX, colSpan, cursorY, rowSpan)) {
                        cursorY++;
                        if (cursorY + rowSpan > rowCount) {
                            rowCount = cursorY + rowSpan;
                            cursorY = 0;
                            cursorX++;
                        }
                    }

                    rowItem.trackStart = cursorY;

                    m_Placements[placement.index] = new GridPlacement(placement.id, placement.index, colItem, rowItem);

                    CreateTracks(m_ColTracks, cursorX + colSpan, autoColSize);
                    CreateTracks(m_RowTracks, cursorY + rowSpan, autoRowSize);

                    OccupyGridArea(m_Placements[placement.index]);
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

        private enum SizeType {

            Min,
            Max

        }

    }

}