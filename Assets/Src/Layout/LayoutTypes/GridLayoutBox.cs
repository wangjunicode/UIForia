using System;
using System.Collections.Generic;
using Rendering;
using Src.Systems;
using Src.Util;
using UnityEngine;

namespace Src.Layout.LayoutTypes {

    [Flags]
    public enum GridTemplateUnit {

        Unset = 0,
        Pixel = 1 << 0,
        Container = 1 << 1,
        ContainerContentArea = 1 << 2,
        Em = 1 << 3,
        ViewportWidth = 1 << 4,
        ViewportHeight = 1 << 5,

        Fraction = 1 << 6,
        MinContent = 1 << 7,
        MaxContent = 1 << 8,

        Fixed = Pixel | Container | ContainerContentArea | Em | ViewportWidth | ViewportHeight

    }

    public struct GridTrackSize {

        public readonly float minValue;
        public readonly float maxValue;

        public readonly GridTemplateUnit minUnit;
        public readonly GridTemplateUnit maxUnit;

        public GridTrackSize(float value, GridTemplateUnit unit = GridTemplateUnit.Pixel) {
            this.minUnit = unit;
            this.minValue = value;
            this.maxUnit = unit;
            this.maxValue = value;
        }

        public static GridTrackSize Unset => new GridTrackSize(0, GridTemplateUnit.Unset);
        public static GridTrackSize MaxContent => new GridTrackSize(1f, GridTemplateUnit.MaxContent);
        public static GridTrackSize MinContent => new GridTrackSize(1f, GridTemplateUnit.MinContent);

        public static bool operator ==(GridTrackSize a, GridTrackSize b) {
            return a.minValue == b.minValue
                   && a.minUnit == b.minUnit
                   && a.maxValue == b.maxValue
                   && a.maxUnit == b.maxUnit;
        }

        public static bool operator !=(GridTrackSize a, GridTrackSize b) {
            return !(a == b);
        }

    }

    public class GridLayoutBox : LayoutBox {

        private readonly List<GridTrack> m_RowTracks;
        private readonly List<GridTrack> m_ColTracks;
        private readonly List<GridPlacement> m_Placements;

        private readonly HashSet<int> m_Occupied;

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

                if (minSize == 0) {
                    minSize = baseWidth;
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

            ListPool<ValueTuple<int, GridTrack>>.Release(ref intrinsics);
            ListPool<ValueTuple<int, GridTrack>>.Release(ref flexes);

            return m_ColTracks[m_ColTracks.Count - 1].End;
        }

        public override void RunWidthLayout() {
            Place();
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                float minWidth = child.MinWidth;
                float maxWidth = child.MaxWidth;
                float prfWidth = child.PreferredWidth;
                m_Widths[i] = Mathf.Max(minWidth, Mathf.Min(prfWidth, maxWidth));
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
                    m_ColTracks[i] = track;

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
                    track.outputSize = ResolveContentMaxWidth(track, SizeType.Min);
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

            actualWidth = m_ColTracks[m_ColTracks.Count - 1].End;
            ListPool<ValueTuple<int, GridTrack>>.Release(ref intrinsics);
            ListPool<ValueTuple<int, GridTrack>>.Release(ref flexes);

            CrossAxisAlignment colAlignment = style.GridLayoutColAlignment;

            for (int i = 0; i < m_Placements.Count; i++) {

                GridPlacement placement = m_Placements[i];

                LayoutBox child = children[i];
                GridItem colItem = placement.colItem;

                CrossAxisAlignment alignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
                    ? child.style.FlexItemSelfAlignment // todo move to grid item property
                    : colAlignment;

                float x = m_ColTracks[colItem.trackStart].position;
                float spannedTracksWidth = m_ColTracks[colItem.trackStart + colItem.trackSpan - 1].End - x;

                float finalX = x;
                float finalWidth = m_Widths[i];

                if (finalWidth < spannedTracksWidth) {
                    switch (alignment) {
                        case CrossAxisAlignment.Center:
                            finalX = (spannedTracksWidth * 0.5f) - (m_Widths[i] * 0.5f);
                            break;

                        case CrossAxisAlignment.End:
                            finalX = spannedTracksWidth - m_Widths[i];
                            break;

                        case CrossAxisAlignment.Start:
                            finalX = x;
                            break;

                        case CrossAxisAlignment.Stretch:
                            finalX = x;
                            finalWidth = spannedTracksWidth;
                            break;
                        default:
                            finalX = x;
                            break;
                    }
                }
                child.SetAllocatedXAndWidth(finalX, finalWidth);

            }
        }

        public override void RunHeightLayout() {
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                float minHeight = child.MinHeight;
                float maxHeight = child.MaxHeight;
                float prfWidth = child.GetPreferredHeightForWidth(m_Widths[i]);
                m_Heights[i] = Mathf.Max(minHeight, Mathf.Min(prfWidth, maxHeight));
            }

            List<ValueTuple<int, GridTrack>> intrinsics = ListPool<ValueTuple<int, GridTrack>>.Get();
            List<ValueTuple<int, GridTrack>> flexes = ListPool<ValueTuple<int, GridTrack>>.Get();

            int flexPieces = 0;
            float remaining = allocatedHeight - PaddingVertical - BorderVertical;

            for (int i = 0; i < m_RowTracks.Count; i++) {
                GridTrack track = m_RowTracks[i];

                if ((track.size.minUnit & GridTemplateUnit.Fixed) != 0) {
                    track.outputSize = ResolveFixedHeightMeasurement(track.size, SizeType.Min);
                    remaining -= track.outputSize;
                    m_RowTracks[i] = track;

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
                    track.outputSize = ResolveContentMaxWidth(track, SizeType.Min);
                }

                remaining -= track.outputSize;
                m_RowTracks[intrinsics[i].Item1] = track;
            }

            if ((int) remaining > 0 && flexes.Count > 0) {
                float pieceSize = remaining / flexPieces;
                for (int i = 0; i < flexes.Count; i++) {
                    GridTrack track = flexes[i].Item2;
                    track.outputSize = track.size.minValue * pieceSize;
                    m_RowTracks[flexes[i].Item1] = track;
                }
            }

            for (int i = 1; i < m_RowTracks.Count; i++) {
                GridTrack track = m_RowTracks[i];
                track.position = m_RowTracks[i - 1].End;
                m_RowTracks[i] = track;
            }

            actualHeight = m_RowTracks[m_RowTracks.Count - 1].End;
            ListPool<ValueTuple<int, GridTrack>>.Release(ref intrinsics);
            ListPool<ValueTuple<int, GridTrack>>.Release(ref flexes);

            CrossAxisAlignment rowAlignment = style.GridLayoutRowAlignment;

            for (int i = 0; i < m_Placements.Count; i++) {

                GridPlacement placement = m_Placements[i];

                LayoutBox child = children[i];
                GridItem rowItem = placement.rowItem;

                CrossAxisAlignment alignment = child.style.FlexItemSelfAlignment != CrossAxisAlignment.Unset
                    ? child.style.FlexItemSelfAlignment // todo move to grid item property
                    : rowAlignment;

                float y = m_RowTracks[rowItem.trackStart].position;
                float spannedTracksHeight = m_RowTracks[rowItem.trackStart + rowItem.trackSpan - 1].End - y;

                float finalY = y;
                float finalHeight = m_Heights[i];

                if (finalHeight < spannedTracksHeight) {
                    switch (alignment) {
                        case CrossAxisAlignment.Center:
                            finalY = (spannedTracksHeight * 0.5f) - (m_Heights[i] * 0.5f);
                            break;

                        case CrossAxisAlignment.End:
                            finalY = spannedTracksHeight - m_Heights[i];
                            break;

                        case CrossAxisAlignment.Start:
                            finalY = y;
                            break;

                        case CrossAxisAlignment.Stretch:
                            finalY = y;
                            finalHeight = spannedTracksHeight;
                            break;
                        default:
                            finalY = y;
                            break;
                    }
                }
                child.SetAllocatedYAndHeight(finalY, finalHeight);

            }
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

            // todo move this
            m_ColTracks.Clear();
            m_RowTracks.Clear();
            m_Occupied.Clear();

            IReadOnlyList<GridTrackSize> colTemplate = style.GridLayoutColTemplate;
            IReadOnlyList<GridTrackSize> rowTemplate = style.GridLayoutRowTemplate;

            for (int i = 0; i < colTemplate.Count; i++) {
                m_ColTracks.Add(new GridTrack(colTemplate[i]));
            }

            for (int i = 0; i < rowTemplate.Count; i++) {
                m_RowTracks.Add(new GridTrack(rowTemplate[i]));
            }

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
                int colStart = child.style.GridItemColStart;
                int colSpan = child.style.GridItemColSpan;
                int rowStart = child.style.GridItemRowStart;
                int rowSpan = child.style.GridItemRowSpan;
                m_Placements.Add(new GridPlacement(child.element.id, m_Placements.Count, new GridItem(colStart, colSpan), new GridItem(rowStart, rowSpan)));
                RequestLayout();
                m_Widths.Add(0);
                m_Heights.Add(0);
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
                    RequestLayout();
                    break;
            }
        }

        public override void OnChildStylePropertyChanged(LayoutBox child, StyleProperty property) {
            int idx = GetPlacementIndexForId(child.element.id);
            GridPlacement placement = m_Placements[idx];
            switch (property.propertyId) {
                case StylePropertyId.LayoutBehavior:
                    RequestLayout();
                    break;
                case StylePropertyId.GridItemColSpan:
                    m_Placements[idx] = new GridPlacement(child.element.id, idx, new GridItem(placement.colItem.trackStart, property.AsInt), placement.rowItem);
                    RequestLayout();
                    break;
                case StylePropertyId.GridItemRowSpan:
                    m_Placements[idx] = new GridPlacement(child.element.id, idx, placement.colItem, new GridItem(placement.rowItem.trackStart, property.AsInt));
                    RequestLayout();
                    break;
                case StylePropertyId.GridItemColStart:
                    m_Placements[idx] = new GridPlacement(child.element.id, idx, new GridItem(property.AsInt, placement.colItem.trackSpan), placement.rowItem);
                    RequestLayout();
                    break;
                case StylePropertyId.GridItemRowStart:
                    m_Placements[idx] = new GridPlacement(child.element.id, idx, placement.colItem, new GridItem(property.AsInt, placement.rowItem.trackSpan));
                    RequestLayout();
                    break;
            }
        }

        private int GetPlacementIndexForId(int id) {
            for (int i = 0; i < m_Placements.Count; i++) {
                if (m_Placements[i].id == id) {
                    return i;
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
                m_ColTracks[i].spanningItems.Add(placement.index);
                for (int j = rowStart; j < rowStart + rowSpan; j++) {
                    m_RowTracks[j].spanningItems.Add(placement.index);
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
                    rowItem.trackStart = cursorY;

                    m_Placements[placement.index] = new GridPlacement(placement.id, placement.index, colItem, rowItem);

                    CreateTracks(m_ColTracks, cursorX + colSpan, autoColSize);
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

                    colItem.trackStart = cursorX;
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