using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Exceptions;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Layout.LayoutTypes {

    public class GridLayoutBox : LayoutBox {

        private struct GridItemSizes {

            public float minSize;
            public float maxSize;
            public float outputSize;
            public float marginStart;
            public float marginEnd;
            public float TotalMargin => marginStart + marginEnd;

        }

        private readonly StructList<GridTrack> m_RowTracks;
        private readonly StructList<GridTrack> m_ColTracks;
        private readonly List<GridPlacement> m_Placements;

        private readonly HashSet<int> m_Occupied;

        private readonly LightList<GridItemSizes> m_Widths;
        private readonly LightList<GridItemSizes> m_Heights;

        private bool m_IsPlacementDirty;

        public GridLayoutBox() {
            this.m_IsPlacementDirty = true;
            this.m_Widths = new LightList<GridItemSizes>(4);
            this.m_Heights = new LightList<GridItemSizes>(4);
            this.m_RowTracks = new StructList<GridTrack>();
            this.m_ColTracks = new StructList<GridTrack>();
            this.m_Occupied = new HashSet<int>();
            this.m_Placements = new List<GridPlacement>();
            this.m_IsPlacementDirty = true;
        }

        public override void OnSpawn(UIElement element) {
            base.OnSpawn(element);
            this.m_IsPlacementDirty = true;
        }

        public override void OnRelease() {
            base.OnRelease();
            m_Widths.QuickClear();
            m_Heights.QuickClear();
            m_RowTracks.QuickClear();
            m_ColTracks.QuickClear();
            m_Occupied.Clear();
            m_Placements.Clear();
            m_IsPlacementDirty = true;
        }

        internal StructList<GridTrack> GetRowTracks() {
            return m_RowTracks;
        }

        internal StructList<GridTrack> GetColTracks() {
            return m_ColTracks;
        }

        protected override float ComputeContentWidth() {
            if (children.Count == 0) {
                return 0f;
            }

            Place();


            for (int i = 0; i < children.Count; i++) {
                LayoutBox layoutBox = children[i];
                float marginStart = layoutBox.GetMarginLeft();
                float marginEnd = layoutBox.GetMarginRight();
                LayoutBoxSize layoutBoxSize = layoutBox.GetWidths();
                m_Widths[i] = new GridItemSizes() {
                    minSize = layoutBoxSize.minSize,
                    maxSize = layoutBoxSize.maxSize,
                    marginStart = marginStart,
                    marginEnd = marginEnd,
                    outputSize = layoutBoxSize.clampedSize + marginStart + marginEnd
                };
            }

            ResolveColumnTrackWidths(0);
            PositionColumnTracks();
            return Mathf.Max(ApplyColumnCrossAxisAlignment(true), m_ColTracks[m_ColTracks.Count - 1].End);
        }

        protected override float ComputeContentHeight(float width) {
            if (children.Count == 0) {
                return 0f;
            }

            // todo -- this section could be optimized out in the case where we ran ComputeContentWidth, otherwise we need to run it to setup widths

            // start section to be optimized
            Place();

            for (int i = 0; i < children.Count; i++) {
                LayoutBox layoutBox = children[i];
                LayoutBoxSize layoutBoxSize = layoutBox.GetWidths();
                float marginStart = layoutBox.GetMarginLeft();
                float marginEnd = layoutBox.GetMarginRight();
                m_Widths[i] = new GridItemSizes() {
                    minSize = layoutBoxSize.minSize,
                    maxSize = layoutBoxSize.maxSize,
                    marginStart = marginStart,
                    marginEnd = marginEnd,
                    outputSize = layoutBoxSize.clampedSize + marginStart + marginEnd
                };
            }

            ResolveColumnTrackWidths(width);
            StretchWidthsIfNeeded();
            // end section to be optimized

            for (int i = 0; i < children.Count; i++) {
                LayoutBox layoutBox = children[i];
                LayoutBoxSize layoutBoxSize = layoutBox.GetHeights(m_Widths[i].outputSize);
                float marginStart = layoutBox.GetMarginTop(m_Widths[i].outputSize);
                float marginEnd = layoutBox.GetMarginBottom(m_Widths[i].outputSize);

                m_Heights[i] = new GridItemSizes() {
                    minSize = layoutBoxSize.minSize,
                    maxSize = layoutBoxSize.maxSize,
                    marginStart = marginStart,
                    marginEnd = marginEnd,
                    outputSize = layoutBoxSize.clampedSize + marginStart + marginEnd
                };
            }

            ResolveRowTrackHeights(0); // resolve with 0 since we have no size yet
            PositionRowTracks();
            return Mathf.Max(m_RowTracks[m_RowTracks.Count - 1].End, ApplyRowCrossAxisAlignment(false));
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

        private float ResolveContentMinHeight(GridTrack track, SizeType sizeType) {
            float minSize = 0;

            float value = track.size.minValue;

            if (sizeType == SizeType.Max) {
                value = track.size.maxValue;
            }

            for (int i = 0; i < track.spanningItems.Count; i++) {
                int childIndex = track.spanningItems[i];
                GridItem rowItem = m_Placements[childIndex].rowItem;
                int pieces = 0; // never 0 because we only call this function for intrinsic sized tracks
                float baseHeight = m_Heights[childIndex].outputSize;

                for (int j = rowItem.trackStart; j < rowItem.trackStart + rowItem.trackSpan; j++) {
                    GridTrack spanned = m_RowTracks[j];
                    if ((spanned.size.minUnit & GridTemplateUnit.Fixed) != 0) {
                        baseHeight -= spanned.outputSize;
                        if (baseHeight <= 0) {
                            break;
                        }
                    }
                    else if (spanned.size.minUnit != GridTemplateUnit.FractionalRemaining) {
                        pieces++;
                    }
                }

                if (minSize == 0) {
                    minSize = baseHeight;
                }

                if (baseHeight > 0) {
                    minSize = Mathf.Min(minSize, value * (baseHeight / pieces));
                }
            }

            return minSize;
        }

        private float ResolveContentMaxWidth(GridTrack track, SizeType sizeType) {
            float maxSize = 0;

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

                    if ((spanned.size.minUnit & GridTemplateUnit.Fixed) != 0) {
                        baseWidth -= spanned.outputSize;
                        if (baseWidth <= 0) {
                            break;
                        }
                    }
                    else if (spanned.size.minUnit != GridTemplateUnit.FractionalRemaining) {
                        pieces++;
                    }
                }

                if (baseWidth > 0) {
                    maxSize = Mathf.Max(maxSize, value * (baseWidth / pieces));
                }
            }

            return maxSize;
        }

        private float ResolveContentMaxHeight(GridTrack track, SizeType sizeType) {
            float maxSize = 0;

            float value = track.size.minValue;

            if (sizeType == SizeType.Max) {
                value = track.size.maxValue;
            }

            for (int i = 0; i < track.spanningItems.Count; i++) {
                int childIndex = track.spanningItems[i];
                GridItem rowItem = m_Placements[childIndex].rowItem;
                int pieces = 0; // never 0 because we only call this function for intrinsic sized tracks
                float baseHeight = m_Heights[childIndex].outputSize;

                for (int j = rowItem.trackStart; j < rowItem.trackStart + rowItem.trackSpan; j++) {
                    GridTrack spanned = m_RowTracks[j];
                    if ((spanned.size.minUnit & GridTemplateUnit.Fixed) != 0) {
                        baseHeight -= spanned.outputSize;
                        if (baseHeight <= 0) {
                            break;
                        }
                    }
                    else if (spanned.size.minUnit != GridTemplateUnit.FractionalRemaining) {
                        pieces++;
                    }
                }

                if (baseHeight > 0) {
                    maxSize = Mathf.Max(maxSize, value * (baseHeight / pieces));
                }
            }

            return maxSize;
        }

        private void ResolveColumnTrackWidths(float remaining) {
            int flexPieces = 0;

            List<ValueTuple<int, GridTrack>> intrinsics = ListPool<ValueTuple<int, GridTrack>>.Get();
            List<ValueTuple<int, GridTrack>> flexes = ListPool<ValueTuple<int, GridTrack>>.Get();
            for (int i = 0; i < m_ColTracks.Count; i++) {
                GridTrack track = m_ColTracks[i];

                if ((track.size.minUnit & GridTemplateUnit.Fixed) != 0) {
                    track.outputSize = ResolveFixedWidthMeasurement(track.size, SizeType.Min);
                    remaining -= track.outputSize;
                    m_ColTracks[i] = track;
                }
                else if (track.size.minUnit == GridTemplateUnit.FractionalRemaining) {
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

        private void ResolveRowTrackHeights(float height) {
            List<ValueTuple<int, GridTrack>> intrinsics = ListPool<ValueTuple<int, GridTrack>>.Get();
            List<ValueTuple<int, GridTrack>> flexes = ListPool<ValueTuple<int, GridTrack>>.Get();

            int flexPieces = 0;
            float remaining = height - PaddingVertical - BorderVertical;

            for (int i = 0; i < m_RowTracks.Count; i++) {
                GridTrack track = m_RowTracks[i];

                if ((track.size.minUnit & GridTemplateUnit.Fixed) != 0) {
                    track.outputSize = ResolveFixedHeightMeasurement(track.size, SizeType.Min);
                    remaining -= track.outputSize;
                    m_RowTracks[i] = track;
                }
                else if (track.size.minUnit == GridTemplateUnit.FractionalRemaining) {
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
                    track.outputSize = ResolveContentMinHeight(track, SizeType.Min);
                }
                else if (track.size.minUnit == GridTemplateUnit.MaxContent) {
                    track.outputSize = ResolveContentMaxHeight(track, SizeType.Min);
                }

                remaining -= track.outputSize;
                m_RowTracks[intrinsics[i].Item1] = track;
            }

            if ((int) remaining > 0 && flexes.Count > 0) {
                float pieceSize = remaining / flexPieces;
                bool isContentBased = style.PreferredHeight.IsContentBased;
                for (int i = 0; i < flexes.Count; i++) {
                    GridTrack track = flexes[i].Item2;
                    track.outputSize = isContentBased ? 0 : track.size.minValue * pieceSize;
                    m_RowTracks[flexes[i].Item1] = track;
                }
            }

            ListPool<ValueTuple<int, GridTrack>>.Release(ref intrinsics);
            ListPool<ValueTuple<int, GridTrack>>.Release(ref flexes);
        }
        
        private void StretchWidthsIfNeeded() {
            float colGap = style.GridLayoutColGap;
            GridAxisAlignment colAlignment = style.GridLayoutColAlignment;
            for (int i = 0; i < m_Placements.Count; i++) {
                GridPlacement placement = m_Placements[i];

                LayoutBox child = children[i];
                GridItem colItem = placement.colItem;

                GridAxisAlignment alignment = child.style.GridItemColSelfAlignment;
                if (alignment == GridAxisAlignment.Unset) {
                    alignment = colAlignment;
                }

                if (alignment == GridAxisAlignment.Grow || alignment == GridAxisAlignment.Fit) {
                    float spannedTracksWidth = colGap * (colItem.trackSpan - 1);

                    for (int j = colItem.trackStart; j < colItem.trackStart + colItem.trackSpan; j++) {
                        spannedTracksWidth += m_ColTracks[j].outputSize;
                    }

                    m_Widths[i] = new GridItemSizes() {
                        maxSize = m_Widths[i].maxSize,
                        minSize = m_Widths[i].minSize,
                        outputSize = spannedTracksWidth,
                        marginEnd = m_Widths[i].marginEnd,
                        marginStart = m_Widths[i].marginStart
                    };
                }
            }
        }

        private float ApplyColumnCrossAxisAlignment(bool applySize) {
            GridAxisAlignment colAlignment = style.GridLayoutColAlignment;
            float paddingBorderLeft = resolvedPaddingLeft + resolvedBorderLeft;

            float maxXPlusWidth = 0;

            for (int i = 0; i < m_Placements.Count; i++) {
                GridPlacement placement = m_Placements[i];
                int idx = placement.index;
                LayoutBox child = children[idx];
                GridItem colItem = placement.colItem;

                GridAxisAlignment alignment = child.style.GridItemColSelfAlignment;
                if (alignment == GridAxisAlignment.Unset) {
                    alignment = colAlignment;
                }

                float x = m_ColTracks[colItem.trackStart].position;
                float spannedTracksWidth = m_ColTracks[colItem.trackStart + colItem.trackSpan - 1].End - x;

                float finalX;
                float finalWidth = m_Widths[idx].outputSize;

                switch (alignment) {
                    case GridAxisAlignment.Center:
                        // centering ignores margin, otherwise a 'center' operation doesn't really make sense
                        finalWidth -= m_Widths[idx].TotalMargin;
                        finalX = x + (spannedTracksWidth * 0.5f) - (finalWidth * 0.5f);
                        break;

                    case GridAxisAlignment.End:
                        finalX = x + spannedTracksWidth - finalWidth - m_Widths[placement.index].marginEnd;
                        finalWidth -= m_Widths[idx].TotalMargin;
                        break;

                    case GridAxisAlignment.Start:
                        finalX = x + m_Widths[idx].marginStart;
                        finalWidth -= m_Widths[idx].TotalMargin;
                        break;

                    case GridAxisAlignment.Grow:
                        finalX = x + m_Widths[idx].marginStart;
                        if (finalWidth <= spannedTracksWidth) {
                            finalWidth = spannedTracksWidth - m_Widths[idx].TotalMargin;
                        }

                        break;
                    case GridAxisAlignment.Shrink:
                        finalX = x + m_Widths[idx].marginStart;
                        if (finalWidth > spannedTracksWidth) {
                            finalWidth = spannedTracksWidth - m_Widths[idx].TotalMargin;
                        }

                        break;
                    case GridAxisAlignment.Fit:
                        finalX = x + m_Widths[idx].marginStart;
                        finalWidth = spannedTracksWidth - m_Widths[idx].TotalMargin;
                        break;
                    default:
                        finalX = x;
                        break;
                }


                if (applySize) {
                    child.SetAllocatedXAndWidth(finalX + paddingBorderLeft, finalWidth);
                }

                maxXPlusWidth = maxXPlusWidth < finalX + finalWidth ? finalX + finalWidth : maxXPlusWidth;
            }

            return maxXPlusWidth;
        }

        private float ApplyRowCrossAxisAlignment(bool applySizes) {
            GridAxisAlignment rowAlignment = style.GridLayoutRowAlignment;

            float maxYPlusHeight = 0;

            for (int i = 0; i < m_Placements.Count; i++) {
                GridPlacement placement = m_Placements[i];

                LayoutBox child = children[i];
                GridItem rowItem = placement.rowItem;

                GridAxisAlignment alignment = default; //child.style.GridItemRowSelfAlignment;
                if (alignment == GridAxisAlignment.Unset) {
                    alignment = rowAlignment;
                }

                float y = m_RowTracks[rowItem.trackStart].position;
                float spannedTracksHeight = m_RowTracks[rowItem.trackStart + rowItem.trackSpan - 1].End - y;

                float finalY;
                float marginStart = m_Heights[i].marginStart;
                float marginEnd = m_Heights[i].marginEnd;
                float finalHeight = m_Heights[i].outputSize - marginStart - marginEnd;

                switch (alignment) {
                    case GridAxisAlignment.Center:
                        finalY = y + (spannedTracksHeight * 0.5f) - (m_Heights[i].outputSize * 0.5f);
                        break;

                    case GridAxisAlignment.End:
                        finalY = y + spannedTracksHeight - m_Heights[i].outputSize - m_Heights[i].marginEnd;
                        break;

                    case GridAxisAlignment.Start:
                        finalY = y + m_Heights[placement.index].marginStart;
                        break;

                    case GridAxisAlignment.Grow:
                        finalY = y + m_Heights[placement.index].marginStart;
                        if (spannedTracksHeight > finalHeight) {
                            finalHeight = spannedTracksHeight - marginEnd - marginStart;
                        }

                        break;

                    case GridAxisAlignment.Fit:
                        finalY = y + m_Heights[placement.index].marginStart;
                        finalHeight = spannedTracksHeight;
                        break;

                    case GridAxisAlignment.Shrink:
                        finalY = y + m_Heights[placement.index].marginStart;
                        if (spannedTracksHeight < finalHeight) {
                            finalHeight = spannedTracksHeight;
                        }

                        break;

                    default:
                        finalY = y;
                        break;
                }

                float paddingBorderTop = resolvedPaddingTop + resolvedBorderTop;

                if (applySizes) {
                    child.SetAllocatedYAndHeight(finalY + paddingBorderTop, finalHeight);
                }

                if (finalY + finalHeight + marginEnd > maxYPlusHeight) {
                    maxYPlusHeight = finalY + finalHeight + marginEnd;
                }
            }

            return maxYPlusHeight;
        }

        private void PositionColumnTracks() {
            float colGap = style.GridLayoutColGap;

            for (int i = 1; i < m_ColTracks.Count; i++) {
                GridTrack track = m_ColTracks[i];
                track.position = colGap + m_ColTracks[i - 1].End;
                m_ColTracks[i] = track;
            }
        }

        private void PositionRowTracks() {
            float rowGap = style.GridLayoutRowGap;
            for (int i = 1; i < m_RowTracks.Count; i++) {
                GridTrack track = m_RowTracks[i];
                track.position = rowGap + m_RowTracks[i - 1].End;
                m_RowTracks[i] = track;
            }
        }

        public override void RunLayout() {
            if (children.Count == 0) {
                return;
            }

            Place();

            // todo -- might be able to avoid a lot of work here if children didn't change between last time we updated this stuff and now
            for (int i = 0; i < children.Count; i++) {
                LayoutBox layoutBox = children[i];
                LayoutBoxSize layoutBoxSize = layoutBox.GetWidths();
                float marginStart = layoutBox.GetMarginLeft();
                float marginEnd = layoutBox.GetMarginRight();
                m_Widths[i] = new GridItemSizes() {
                    minSize = layoutBoxSize.minSize,
                    maxSize = layoutBoxSize.maxSize,
                    marginStart = marginStart,
                    marginEnd = marginEnd,
                    outputSize = layoutBoxSize.clampedSize + marginStart + marginEnd
                };
            }

            ResolveColumnTrackWidths(allocatedWidth - PaddingHorizontal - BorderHorizontal);
            PositionColumnTracks();
            float maxWidth = ApplyColumnCrossAxisAlignment(true);

            for (int i = 0; i < children.Count; i++) {
                LayoutBox layoutBox = children[i];
                LayoutBoxSize layoutBoxSize = layoutBox.GetHeights(m_Widths[i].outputSize);
                float marginStart = layoutBox.GetMarginTop(m_Widths[i].outputSize);
                float marginEnd = layoutBox.GetMarginBottom(m_Widths[i].outputSize);
                m_Heights[i] = new GridItemSizes() {
                    minSize = layoutBoxSize.minSize,
                    maxSize = layoutBoxSize.maxSize,
                    marginStart = marginStart,
                    marginEnd = marginEnd,
                    outputSize = layoutBoxSize.clampedSize + marginStart + marginEnd
                };
            }

            ResolveRowTrackHeights(allocatedHeight);
            PositionRowTracks();

            float maxHeight = ApplyRowCrossAxisAlignment(true);
            actualWidth = Mathf.Max(maxWidth, m_ColTracks[m_ColTracks.Count - 1].End);
            actualHeight = Mathf.Max(maxHeight, m_RowTracks[m_RowTracks.Count - 1].End);
            if (allocatedWidth > actualWidth) {
                actualWidth = allocatedWidth;
            }
            else {
                actualWidth = actualWidth + PaddingHorizontal + BorderHorizontal;
            }

            if (allocatedHeight > actualHeight) {
                actualHeight = allocatedHeight;
            }
            else {
                actualHeight = actualHeight + PaddingVertical + BorderVertical;
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
                    return value * style.GetResolvedFontSize();
                case GridTemplateUnit.ViewportWidth:
                    return value * view.Viewport.width;
                case GridTemplateUnit.ViewportHeight:
                    return value * view.Viewport.height;
                case GridTemplateUnit.ParentSize:
                    return parent.allocatedWidth * value;
                case GridTemplateUnit.ParentContentArea:
                    return (parent.allocatedWidth - parent.PaddingHorizontal - parent.BorderHorizontal) * value;

                case GridTemplateUnit.FractionalRemaining:
                case GridTemplateUnit.MinContent:
                case GridTemplateUnit.MaxContent:
                    return 0f;

                default:
                    throw new InvalidArgumentException();
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
                    return value * style.GetResolvedFontSize();

                case GridTemplateUnit.ViewportWidth:
                    return value * view.Viewport.width;

                case GridTemplateUnit.ViewportHeight:
                    return value * view.Viewport.height;

                case GridTemplateUnit.ParentSize:
                    return parent.allocatedHeight * value;

                case GridTemplateUnit.ParentContentArea:
                    return (parent.allocatedHeight - parent.PaddingVertical - parent.BorderVertical) * value;

                case GridTemplateUnit.FractionalRemaining:
                case GridTemplateUnit.MinContent:
                case GridTemplateUnit.MaxContent:
                    return 0f;
                default:
                    throw new InvalidArgumentException();
            }
        }

        private static void ClearGridTracks(StructList<GridTrack> tracks) {
            GridTrack[] array = tracks.Array;
            for (int i = 0; i < tracks.Count; i++) {
                ListPool<int>.Release(ref array[i].spanningItems);
            }

            tracks.Clear();
        }

        private void Place() {
            if (!m_IsPlacementDirty) {
                return;
            }

            LayoutDirection direction = style.GridLayoutDirection;
            GridTrackSize autoColSize;
            GridTrackSize autoRowSize;

            if (direction == LayoutDirection.Horizontal) {
                autoColSize = style.GridLayoutMainAxisAutoSize;
                autoRowSize = style.GridLayoutCrossAxisAutoSize;
            }
            else {
                autoColSize = style.GridLayoutCrossAxisAutoSize;
                autoRowSize = style.GridLayoutMainAxisAutoSize;
            }

            List<GridPlacement> bothAxesLocked = ListPool<GridPlacement>.Get();
            List<GridPlacement> singleAxisLockedRow = ListPool<GridPlacement>.Get();
            List<GridPlacement> singleAxisLockedCol = ListPool<GridPlacement>.Get();
            List<GridPlacement> noAxisLocked = ListPool<GridPlacement>.Get();
            List<GridPlacement> remainingItems = ListPool<GridPlacement>.Get();

            m_Occupied.Clear();
            ClearGridTracks(m_ColTracks);
            ClearGridTracks(m_RowTracks);
            ResetPlacements();

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
                else {
                    remainingItems.Add(placement);
                }
            }

            PlaceBothAxesLocked(bothAxesLocked);
            PlaceRemainingItems(remainingItems);
            m_IsPlacementDirty = false;

            ListPool<GridPlacement>.Release(ref singleAxisLockedCol);
            ListPool<GridPlacement>.Release(ref singleAxisLockedRow);
            ListPool<GridPlacement>.Release(ref noAxisLocked);
            ListPool<GridPlacement>.Release(ref remainingItems);
            ListPool<GridPlacement>.Release(ref bothAxesLocked);
        }

        private void PlaceBothAxesLocked(List<GridPlacement> bothAxesLocked) {
            for (int i = 0; i < bothAxesLocked.Count; i++) {
                OccupyGridArea(bothAxesLocked[i]);
            }
        }

        private void PreAllocateMaxTrackSizes(List<GridPlacement> placements) {
            int maxColStartAndSpan = 0;
            int maxRowStartAndSpan = 0;

            LayoutDirection direction = style.GridLayoutDirection;
            GridTrackSize autoColSize;
            GridTrackSize autoRowSize;

            if (direction == LayoutDirection.Horizontal) {
                autoColSize = style.GridLayoutMainAxisAutoSize;
                autoRowSize = style.GridLayoutCrossAxisAutoSize;
            }
            else {
                autoColSize = style.GridLayoutCrossAxisAutoSize;
                autoRowSize = style.GridLayoutMainAxisAutoSize;
            }

            for (int i = 0; i < placements.Count; i++) {
                GridPlacement placement = placements[i];
                GridItem colItem = placement.colItem;
                GridItem rowItem = placement.rowItem;
                int rowStart = rowItem.trackStart;
                int rowSpan = rowItem.trackSpan;
                int colStart = colItem.trackStart;
                int colSpan = colItem.trackSpan;


                if (!IntUtil.IsDefined(colStart)) {
                    colStart = 0;
                }

                if (!IntUtil.IsDefined(rowStart)) {
                    rowStart = 0;
                }

                maxColStartAndSpan = maxColStartAndSpan > colStart + colSpan ? maxColStartAndSpan : colStart + colSpan;
                maxRowStartAndSpan = maxRowStartAndSpan > rowStart + rowSpan ? maxRowStartAndSpan : rowStart + rowSpan;
            }

            CreateTracks(m_ColTracks, maxColStartAndSpan, autoColSize);
            CreateTracks(m_RowTracks, maxRowStartAndSpan, autoRowSize);
        }

        private void PlaceRemainingItems(List<GridPlacement> placements) {
            bool flowHorizontal = style.GridLayoutDirection == LayoutDirection.Horizontal;
            bool dense = style.GridLayoutDensity == GridLayoutDensity.Dense;

            LayoutDirection direction = style.GridLayoutDirection;
            GridTrackSize autoColSize;
            GridTrackSize autoRowSize;

            if (direction == LayoutDirection.Horizontal) {
                autoColSize = style.GridLayoutMainAxisAutoSize;
                autoRowSize = style.GridLayoutCrossAxisAutoSize;
            }
            else {
                autoColSize = style.GridLayoutCrossAxisAutoSize;
                autoRowSize = style.GridLayoutMainAxisAutoSize;
            }

            int sparseStartX = 0;
            int sparseStartY = 0;

            // uncomment to pre-create overflow rows / cols
            PreAllocateMaxTrackSizes(placements);

            for (int i = 0; i < placements.Count; i++) {
                GridPlacement placement = placements[i];
                GridItem colItem = placement.colItem;
                GridItem rowItem = placement.rowItem;
                int rowStart = rowItem.trackStart;
                int rowSpan = rowItem.trackSpan;
                int colStart = colItem.trackStart;
                int colSpan = colItem.trackSpan;

                int colCount = m_ColTracks.Count;
                int rowCount = m_RowTracks.Count;

                int cursorX = 0;
                int cursorY = 0;

                if (rowItem.IsAxisLocked) {
                    cursorX = dense ? 0 : m_RowTracks[rowStart].autoPlacementCursor;

                    while (!IsGridAreaAvailable(cursorX, colSpan, rowStart, rowSpan)) {
                        cursorX++;
                    }

                    CreateTracks(m_ColTracks, cursorX + colSpan, autoColSize);

                    m_RowTracks.Array[rowStart].autoPlacementCursor = cursorX;

                    colItem.trackStart = cursorX;
                }
                else if (colItem.IsAxisLocked) {
                    cursorY = dense ? 0 : m_ColTracks[colStart].autoPlacementCursor;

                    while (!IsGridAreaAvailable(colStart, colSpan, cursorY, rowSpan)) {
                        cursorY++;
                    }

                    CreateTracks(m_RowTracks, cursorY + rowSpan, autoRowSize);

                    m_ColTracks.Array[colStart].autoPlacementCursor = cursorY;

                    rowItem.trackStart = cursorY;
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
                            CreateTracks(m_RowTracks, cursorY + rowSpan, autoRowSize);
                            cursorX = !dense ? m_RowTracks[cursorY].autoPlacementCursor : 0;
                            continue;
                        }

                        if (IsGridAreaAvailable(cursorX, colSpan, cursorY, rowSpan)) {
                            break;
                        }

                        cursorX++;
                    }

                    sparseStartX = cursorX + colSpan;
                    sparseStartY = cursorY;
                    colItem.trackStart = cursorX;
                    rowItem.trackStart = cursorY;
                    CreateTracks(m_ColTracks, cursorX + colSpan, autoColSize);
                    m_ColTracks.Array[cursorX].autoPlacementCursor = cursorY;
                    m_RowTracks.Array[cursorY].autoPlacementCursor = cursorX;
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
                            CreateTracks(m_ColTracks, cursorX + colSpan, autoColSize);
                            cursorY = !dense ? m_ColTracks[cursorX].autoPlacementCursor : 0;
                            continue;
                        }

                        if (IsGridAreaAvailable(cursorX, colSpan, cursorY, rowSpan)) {
                            break;
                        }

                        cursorY++;
                    }

                    sparseStartX = cursorX;
                    sparseStartY = cursorY + rowSpan;
                    colItem.trackStart = cursorX;
                    rowItem.trackStart = cursorY;
                    m_ColTracks.Array[cursorX].autoPlacementCursor = cursorY;
                    m_RowTracks.Array[cursorY].autoPlacementCursor = cursorX;
                    CreateTracks(m_ColTracks, cursorX + colSpan, autoColSize);
                    CreateTracks(m_RowTracks, cursorY + rowSpan, autoRowSize);
                }

                m_Placements[placement.index] = new GridPlacement(placement.id, placement.index, colItem, rowItem);
                OccupyGridArea(m_Placements[placement.index]);
            }
        }

        private static int CreateTracks(StructList<GridTrack> tracks, int count, GridTrackSize size) {
            int total = count - tracks.Count;
            for (int i = 0; i < total; i++) {
                tracks.Add(new GridTrack(size));
            }

            return total;
        }

        protected override void OnChildrenChanged() {
            m_Placements.Clear();
            m_Widths.Clear();
            m_Heights.Clear();
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                int colStart = child.style.GridItemColStart;
                int colSpan = child.style.GridItemColSpan;
                int rowStart = child.style.GridItemRowStart;
                int rowSpan = child.style.GridItemRowSpan;
                m_Widths.Add(new GridItemSizes());
                m_Heights.Add(new GridItemSizes());
                m_Placements.Add(new GridPlacement(
                    child.element.id,
                    m_Placements.Count,
                    new GridItem(colStart, colSpan),
                    new GridItem(rowStart, rowSpan))
                );
            }

            m_IsPlacementDirty = true;
        }

        public override void OnStylePropertyChanged(StructList<StyleProperty> properties) {
            for (int i = 0; i < properties.Count; i++) {
                StyleProperty property = properties[i];
                switch (property.propertyId) {
                    case StylePropertyId.GridLayoutDensity:
                    case StylePropertyId.GridLayoutColTemplate:
                    case StylePropertyId.GridLayoutRowTemplate:
                    case StylePropertyId.GridLayoutDirection:
                        m_IsPlacementDirty = true;
                        markedForLayout = true;
                        break;
                    case StylePropertyId.GridLayoutRowGap:
                    case StylePropertyId.GridLayoutColGap:
                    case StylePropertyId.GridLayoutMainAxisAutoSize:
                    case StylePropertyId.GridLayoutCrossAxisAutoSize:
                        markedForLayout = true;
                        break;
                }
            }
        }

        private void ResetPlacements() {
            m_Placements.Clear();
            for (int i = 0; i < children.Count; i++) {
                LayoutBox child = children[i];
                int colStart = child.style.GridItemColStart;
                int colSpan = child.style.GridItemColSpan;
                int rowStart = child.style.GridItemRowStart;
                int rowSpan = child.style.GridItemRowSpan;
                m_Placements.Add(new GridPlacement(child.element.id, i, new GridItem(colStart, colSpan), new GridItem(rowStart, rowSpan)));
            }
        }

        public override void OnChildStylePropertyChanged(LayoutBox child, StructList<StyleProperty> properties) {
            // int idx = GetPlacementIndexForId(child.element.id); // what was this intended for?
            for (int i = 0; i < properties.Count; i++) {
                StyleProperty property = properties[i];
                switch (property.propertyId) {
                    case StylePropertyId.LayoutBehavior:
                        // markedForLayout = true; already set by the layout system but could be set here explicitly again for clarity maybe?
                        m_IsPlacementDirty = true;
                        break;
                    case StylePropertyId.GridItemColSpan:
                    case StylePropertyId.GridItemRowSpan:
                    case StylePropertyId.GridItemColStart:
                    case StylePropertyId.GridItemRowStart:
                        m_IsPlacementDirty = true;
                        markedForLayout = true;
                        break;
                    case StylePropertyId.GridItemColSelfAlignment:
                    case StylePropertyId.GridItemRowSelfAlignment:
                        markedForLayout = true;
                        break;
                }
            }
        }

        private int GetPlacementIndexForId(int id) {
            for (int i = 0; i < m_Placements.Count; i++) {
                if (m_Placements[i].id == id) {
                    return i;
                }
            }

            throw new InvalidArgumentException();
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