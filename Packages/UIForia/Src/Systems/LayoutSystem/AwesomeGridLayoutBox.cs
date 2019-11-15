using System;
using System.Collections.Generic;
using UIForia.Layout;
using UIForia.Layout.LayoutTypes;
using UIForia.Rendering;
using UIForia.Util;

namespace UIForia.Systems {

    /* ideas:
        when used as a base size, fr resolves to 0
        when used as a non base size (if base size is not also intrinsic) mx and mn resolve to 0
        mx and mn can only really be resolved as base sizes
        alternative would be treat grow(100px, 1mx) as non fixed, always
        reason being: we need a consistent value for mx and mn across grow/shrink/clamp
        grow(baseSize, growLimit)
        shrink(baseSize, shrinkLimit)
        flex(baseSize, fr)
        clamp(baseSize, min, max)
                        
        1fr 100px ==  clamp(0, 0, 1fr) clamp(100px, 100px, 100px)
          
        A grid is a 2d layout box that works by taking a template definition on the row and column axis
        Items can be placed explicitly in numbered grid cells or placed implicitly using one of two placement
        algorithms(explained later)
        
        
    */
    public class AwesomeGridLayoutBox : AwesomeLayoutBox {

        private bool placementDirty;
        private readonly StructList<GridTrack> colTrackList;
        private readonly StructList<GridTrack> rowTrackList;
        private readonly StructList<GridPlacement> placementList;
        private static readonly StructList<GridRegion> s_OccupiedAreas = new StructList<GridRegion>(32);

        public int RowCount => rowTrackList.size;
        public int ColCount => colTrackList.size;

        public AwesomeGridLayoutBox() {
            this.placementList = new StructList<GridPlacement>();
            this.colTrackList = new StructList<GridTrack>(4);
            this.rowTrackList = new StructList<GridTrack>(4);
        }

        protected override float ComputeContentWidth() {
            if (firstChild == null) {
                return 0;
            }

            Place();

            ComputeItemWidths();

            // first pass, get fixed sizes
            ComputeHorizontalFixedTrackSizes();

            // now compute the intrinsic sizes
            ComputeContentWidthContributionSizes();

            ResolveTrackSizesHorizontal();

            float retn = 0;
            // this ignores fr sizes on purpose, fr size is always 0 for content size
            for (int i = 0; i < colTrackList.size; i++) {
                retn += colTrackList.array[i].resolvedMinSize;
            }

            retn += element.style.GridLayoutColGap * (colTrackList.size - 1);
            // fr values are always 0 when computing content width

            return retn;
        }

        private void ComputeHorizontalFixedTrackSizes() {
            for (int i = 0; i < colTrackList.size; i++) {
                ref GridTrack track = ref colTrackList.array[i];
                switch (track.minUnit) {
                    case GridTemplateUnit.Unset:
                    case GridTemplateUnit.Pixel:
                        track.fixedSizeMin = track.minValue;
                        break;
                    case GridTemplateUnit.ParentSize:
                        track.fixedSizeMin = ComputeBlockWidth(track.minValue);
                        break;
                    case GridTemplateUnit.ParentContentArea:
                        track.fixedSizeMin = ComputeBlockContentWidth(track.minValue);
                        break;
                    case GridTemplateUnit.Em:
                        track.fixedSizeMin = element.style.GetResolvedFontSize() * track.minValue;
                        break;
                    case GridTemplateUnit.ViewportWidth:
                        track.fixedSizeMin = element.View.Viewport.width * track.minValue;
                        break;
                    case GridTemplateUnit.ViewportHeight:
                        track.fixedSizeMin = element.View.Viewport.height * track.minValue;
                        break;
                    case GridTemplateUnit.Percent: {
                        if ((flags & AwesomeLayoutBoxFlags.WidthBlockProvider) != 0) {
                            float w = element.layoutResult.actualSize.width - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);
                            if (w < 0) w = 0;
                            track.fixedSizeMin = w * track.minValue;
                        }
                        else {
                            track.fixedSizeMin = ComputeBlockContentWidth(track.minValue);
                        }

                        break;
                    }
                    case GridTemplateUnit.FractionalRemaining:
                    case GridTemplateUnit.MinContent:
                    case GridTemplateUnit.MaxContent:
                        track.fixedSizeMin = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (track.maxUnit) {
                    case GridTemplateUnit.Unset:
                    case GridTemplateUnit.Pixel:
                        track.fixedSizeMax = track.maxValue;
                        break;
                    case GridTemplateUnit.ParentSize:
                        track.fixedSizeMax = ComputeBlockWidth(track.maxValue);
                        break;
                    case GridTemplateUnit.ParentContentArea:
                        track.fixedSizeMax = ComputeBlockContentWidth(track.maxValue);
                        break;
                    case GridTemplateUnit.Em:
                        track.fixedSizeMax = element.style.GetResolvedFontSize() * track.maxValue;
                        break;
                    case GridTemplateUnit.ViewportWidth:
                        track.fixedSizeMax = element.View.Viewport.width * track.maxValue;
                        break;
                    case GridTemplateUnit.ViewportHeight:
                        track.fixedSizeMax = element.View.Viewport.height * track.maxValue;
                        break;
                    case GridTemplateUnit.Percent: {
                        if ((flags & AwesomeLayoutBoxFlags.WidthBlockProvider) != 0) {
                            float w = element.layoutResult.actualSize.width - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);
                            if (w < 0) w = 0;
                            track.fixedSizeMax = w * track.maxValue;
                        }
                        else {
                            track.fixedSizeMax = ComputeBlockContentWidth(track.maxValue);
                        }

                        break;
                    }
                    case GridTemplateUnit.FractionalRemaining:
                    case GridTemplateUnit.MinContent:
                    case GridTemplateUnit.MaxContent:
                        track.fixedSizeMax = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Compute the fixed sizes of all grid tracks on the vertical axis. This will resolve all units except fr, mn, and mx into pixel values
        /// it will also set the fixedSizeMin and fixedSizeMax values on each track. If the unit is fr, mn, or mx, the fixed size is always 0
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ComputeVerticalFixedTrackSizes() {
            for (int i = 0; i < rowTrackList.size; i++) {
                ref GridTrack track = ref rowTrackList.array[i];
                switch (track.minUnit) {
                    case GridTemplateUnit.Unset:
                    case GridTemplateUnit.Pixel:
                        track.fixedSizeMin = track.minValue;
                        break;
                    case GridTemplateUnit.ParentSize:
                        track.fixedSizeMin = ComputeBlockHeight(track.minValue);
                        break;
                    case GridTemplateUnit.ParentContentArea:
                        track.fixedSizeMin = ComputeBlockContentHeight(track.minValue);
                        break;
                    case GridTemplateUnit.Em:
                        track.fixedSizeMin = element.style.GetResolvedFontSize() * track.minValue;
                        break;
                    case GridTemplateUnit.ViewportWidth:
                        track.fixedSizeMin = element.View.Viewport.width * track.minValue;
                        break;
                    case GridTemplateUnit.ViewportHeight:
                        track.fixedSizeMin = element.View.Viewport.height * track.minValue;
                        break;
                    case GridTemplateUnit.Percent: {
                        if ((flags & AwesomeLayoutBoxFlags.HeightBlockProvider) != 0) {
                            float w = element.layoutResult.actualSize.height - (paddingBorderVerticalStart + paddingBorderVerticalEnd);
                            if (w < 0) w = 0;
                            track.fixedSizeMin = w * track.minValue;
                        }
                        else {
                            track.fixedSizeMin = ComputeBlockContentHeight(track.minValue);
                        }

                        break;
                    }
                    case GridTemplateUnit.FractionalRemaining:
                    case GridTemplateUnit.MinContent:
                    case GridTemplateUnit.MaxContent:
                        track.fixedSizeMin = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                switch (track.maxUnit) {
                    case GridTemplateUnit.Unset:
                    case GridTemplateUnit.Pixel:
                        track.fixedSizeMax = track.maxValue;
                        break;
                    case GridTemplateUnit.ParentSize:
                        track.fixedSizeMax = ComputeBlockHeight(track.maxValue);
                        break;
                    case GridTemplateUnit.ParentContentArea:
                        track.fixedSizeMax = ComputeBlockContentHeight(track.maxValue);
                        break;
                    case GridTemplateUnit.Em:
                        track.fixedSizeMax = element.style.GetResolvedFontSize() * track.maxValue;
                        break;
                    case GridTemplateUnit.ViewportWidth:
                        track.fixedSizeMax = element.View.Viewport.width * track.maxValue;
                        break;
                    case GridTemplateUnit.ViewportHeight:
                        track.fixedSizeMax = element.View.Viewport.height * track.maxValue;
                        break;
                    case GridTemplateUnit.Percent: {
                        // if we have a fixed height and the unit is a percentage of our height, then we can safely return a fixed pixel value 
                        // as a percentage of this element's resolved height. If we have an unresolvable height we follow the BlockContentHeight
                        // algorithm as normal in all layout boxes. (ie check the ancestors until a resolvable height is found or the view height
                        // if no resolvable heights are found)
                        if ((flags & AwesomeLayoutBoxFlags.HeightBlockProvider) != 0) {
                            float w = element.layoutResult.actualSize.height - (paddingBorderVerticalStart + paddingBorderVerticalEnd);
                            if (w < 0) w = 0;
                            track.fixedSizeMax = w * track.maxValue;
                        }
                        else {
                            track.fixedSizeMax = ComputeBlockContentHeight(track.maxValue);
                        }

                        break;
                    }
                    case GridTemplateUnit.FractionalRemaining:
                    case GridTemplateUnit.MinContent:
                    case GridTemplateUnit.MaxContent:
                        track.fixedSizeMax = 0;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Some sizes in the grid track definition are not immediately resolvable to pixels sizes. For these cases (MaxContent, MinContent)
        /// we need to look at every item in the children and figure out what the min and max content contributions are. This is not a 1-1 with elements
        /// since an element might span multiple tracks. The resolution algorithm is as follows:
        /// First we find the sum of all the fixed sized tracks that this element spans (horizontal axis in this case)
        /// While doing this, keep track of the number of intrinsically sized tracks the element spans.
        /// if we have any intrinsically sized spanned tracks, we compute the content contribution min size as
        /// (element width + horizontal margins - spanned fixed track sizes) / number of spanned intrinsic tracks
        /// this value will be used later to resolve the sizes of mx and mn tracks 
        /// </summary>
        private void ComputeContentWidthContributionSizes() {
            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];
                float spannedFixedSizeMin = 0;
                float spannedFixedSizeMax = 0;
                int spannedIntrinsicsMin = 0;
                int spannedIntrinsicsMax = 0;

                for (int k = placement.x; k < placement.x + placement.width; k++) {
                    ref GridTrack track = ref colTrackList.array[k];
                    spannedFixedSizeMin += track.fixedSizeMin;
                    spannedFixedSizeMax += track.fixedSizeMax;

                    if (track.minUnit == GridTemplateUnit.MaxContent || track.minUnit == GridTemplateUnit.MinContent) {
                        spannedIntrinsicsMin++;
                    }

                    if (track.maxUnit == GridTemplateUnit.MaxContent || track.maxUnit == GridTemplateUnit.MinContent) {
                        spannedIntrinsicsMax++;
                    }
                }

                float contributionSizeMin = 0;
                float contributionSizeMax = 0;

                if (spannedIntrinsicsMin > 0) {
                    contributionSizeMin = placement.outputWidth - spannedFixedSizeMin;
                    if (contributionSizeMin > 0) {
                        contributionSizeMin /= spannedIntrinsicsMin;
                    }
                    else {
                        contributionSizeMin = 0;
                    }
                }

                placement.spannedHorizontalIntrinsics_min = spannedIntrinsicsMin;
                placement.widthContributionSizeMin = contributionSizeMin;

                if (spannedIntrinsicsMax > 0) {
                    contributionSizeMax = placement.outputWidth - spannedFixedSizeMax;
                    if (contributionSizeMax > 0) {
                        contributionSizeMax /= spannedIntrinsicsMax;
                    }
                    else {
                        contributionSizeMax = 0;
                    }
                }

                placement.spannedHorizontalIntrinsics_max = spannedIntrinsicsMax;
                placement.widthContributionSizeMax = contributionSizeMax;
            }
        }

        /// See ComputeContentWidthContributionSizes, use height instead of width 
        private void ComputeContentHeightContributionSizes() {
            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];
                float spannedFixedSizeMin = 0;
                float spannedFixedSizeMax = 0;
                int spannedIntrinsicsMin = 0;
                int spannedIntrinsicsMax = 0;

                for (int k = placement.y; k < placement.y + placement.height; k++) {
                    ref GridTrack track = ref rowTrackList.array[k];
                    spannedFixedSizeMin += track.fixedSizeMin;
                    spannedFixedSizeMax += track.fixedSizeMax;

                    if (track.minUnit == GridTemplateUnit.MaxContent || track.minUnit == GridTemplateUnit.MinContent) {
                        spannedIntrinsicsMin++;
                    }

                    if (track.maxUnit == GridTemplateUnit.MaxContent || track.maxUnit == GridTemplateUnit.MinContent) {
                        spannedIntrinsicsMax++;
                    }
                }

                float contributionSizeMin = 0;
                float contributionSizeMax = 0;

                if (spannedIntrinsicsMin > 0) {
                    contributionSizeMin = placement.outputHeight - spannedFixedSizeMin;
                    if (contributionSizeMin > 0) {
                        contributionSizeMin /= spannedIntrinsicsMin;
                    }
                    else {
                        contributionSizeMin = 0;
                    }
                }

                placement.spannedVerticalIntrinsics_min = spannedIntrinsicsMin;
                placement.heightContributionSizeMin = contributionSizeMin;

                if (spannedIntrinsicsMax > 0) {
                    contributionSizeMax = placement.outputHeight - spannedFixedSizeMax;
                    if (contributionSizeMax > 0) {
                        contributionSizeMax /= spannedIntrinsicsMax;
                    }
                    else {
                        contributionSizeMax = 0;
                    }
                }

                placement.spannedVerticalIntrinsics_max = spannedIntrinsicsMax;
                placement.heightContributionSizeMax = contributionSizeMax;
            }
        }

        /// <summary>
        /// Find a pixel size for `track`
        /// </summary>
        /// <param name="track"></param>
        /// <param name="unit"></param>
        /// <param name="value"></param>
        /// <param name="index"></param>
        /// <param name="isMin"></param>
        /// <returns></returns>
        private float ResolveHorizontalSize(int index, bool isMin) {
            ref GridTrack track = ref colTrackList.array[index];
            GridTemplateUnit unit = track.minUnit;
            float value = track.minValue;

            if (!isMin) {
                unit = track.maxUnit;
                value = track.maxValue;
            }

            if (unit == GridTemplateUnit.MaxContent) {
                float maxContribution = 0;
                for (int j = 0; j < track.spanningItems.size; j++) {
                    ref GridPlacement placement = ref placementList.array[track.spanningItems.array[j]];

                    float contribution = isMin
                        ? placement.widthContributionSizeMin
                        : placement.widthContributionSizeMax;

                    // for each previous track spanned by this placement
                    // subtract that track's resolved size from the contribution value used
                    for (int idx = placement.x; idx < placement.x + placement.width; idx++) {
                        // stop when we reach the current track
                        if (idx == index) {
                            break;
                        }

                        ref GridTrack spanned = ref colTrackList.array[idx];
                        contribution -= spanned.resolvedMinSize;
                    }

                    if (contribution > maxContribution) {
                        maxContribution = contribution;
                    }
                }

                return maxContribution * value;
            }
            else if (unit == GridTemplateUnit.MinContent) {
                float minContribution = float.MaxValue;
                bool valid = false;
                for (int j = 0; j < track.spanningItems.size; j++) {
                    ref GridPlacement placement = ref placementList.array[track.spanningItems.array[j]];

                    float contribution = isMin
                        ? placement.widthContributionSizeMin
                        : placement.widthContributionSizeMax;

                    int spannedIntrinsics = isMin
                        ? placement.spannedHorizontalIntrinsics_min
                        : placement.spannedHorizontalIntrinsics_max;

                    if (spannedIntrinsics > 0) {
                        // for each previous track spanned by this placement
                        // subtract that track's resolved size from the contribution value used
                        for (int idx = placement.x; idx < placement.x + placement.width; idx++) {
                            // stop when we reach the current track
                            if (idx == index) {
                                break;
                            }

                            ref GridTrack spanned = ref colTrackList.array[idx];
                            contribution -= spanned.resolvedMinSize;
                            if (contribution < 0) contribution = 0;
                        }

                        if (contribution != 0 && contribution < minContribution) {
                            minContribution = contribution;
                            valid = true;
                        }
                    }
                }

                if (!valid) return 0;
                return minContribution * value;
            }
            else if (unit != GridTemplateUnit.FractionalRemaining) {
                return isMin ? track.fixedSizeMin : track.fixedSizeMax;
            }

            return 0;
        }

        /// <summary>
        /// Find a pixels size for `track`
        /// </summary>
        /// <param name="index"></param>
        /// <param name="isMin"></param>
        /// <returns></returns>
        private float ResolveVerticalSize(int index, bool isMin) {
            ref GridTrack track = ref rowTrackList.array[index];
            GridTemplateUnit unit = track.minUnit;
            float value = track.minValue;

            if (!isMin) {
                unit = track.maxUnit;
                value = track.maxValue;
            }

            if (unit == GridTemplateUnit.MaxContent) {
                float maxContribution = 0;
                for (int j = 0; j < track.spanningItems.size; j++) {
                    ref GridPlacement placement = ref placementList.array[track.spanningItems.array[j]];

                    float contribution = isMin
                        ? placement.heightContributionSizeMin
                        : placement.heightContributionSizeMax;

                    // for each previous track spanned by this placement
                    // subtract that track's resolved size from the contribution value used
                    for (int idx = placement.y; idx < placement.y + placement.height; idx++) {
                        // stop when we reach the current track
                        if (idx == index) {
                            break;
                        }

                        ref GridTrack spanned = ref rowTrackList.array[idx];
                        contribution -= spanned.resolvedMinSize;
                    }

                    if (contribution > maxContribution) {
                        maxContribution = contribution;
                    }
                }

                return maxContribution * value;
            }
            else if (unit == GridTemplateUnit.MinContent) {
                float minContribution = float.MaxValue;
                bool valid = false;
                for (int j = 0; j < track.spanningItems.size; j++) {
                    ref GridPlacement placement = ref placementList.array[track.spanningItems.array[j]];

                    float contribution = isMin
                        ? placement.heightContributionSizeMin
                        : placement.heightContributionSizeMax;

                    int spannedIntrinsics = isMin
                        ? placement.spannedVerticalIntrinsics_min
                        : placement.spannedVerticalIntrinsics_max;

                    if (spannedIntrinsics > 0) {
                        // for each previous track spanned by this placement
                        // subtract that track's resolved size from the contribution value used
                        for (int idx = placement.y; idx < placement.y + placement.height; idx++) {
                            // stop when we reach the current track
                            if (idx == index) {
                                break;
                            }

                            ref GridTrack spanned = ref rowTrackList.array[idx];
                            contribution -= spanned.resolvedMinSize;
                            if (contribution < 0) contribution = 0;
                        }

                        if (contribution < minContribution) {
                            minContribution = contribution;
                            valid = true;
                        }
                    }
                }

                if (!valid) return 0;
                return minContribution * value;
            }
            else if (unit != GridTemplateUnit.FractionalRemaining) {
                return isMin ? track.fixedSizeMin : track.fixedSizeMax;
            }

            return 0;
        }


        private void ResolveTrackSizesHorizontal() {
            for (int i = 0; i < colTrackList.size; i++) {
                ref GridTrack track = ref colTrackList.array[i];
                track.resolvedMinSize = ResolveHorizontalSize(i, true);
                track.resolvedMaxSize = ResolveHorizontalSize(i, false);
            }
        }

        private void ResolveTrackSizesVertical() {
            for (int i = 0; i < rowTrackList.size; i++) {
                ref GridTrack track = ref rowTrackList.array[i];
                track.resolvedMinSize = ResolveVerticalSize(i, true);
                track.resolvedMaxSize = ResolveVerticalSize(i, false);
            }
        }

        private void ComputeItemWidths() {
            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetWidths(ref placement.widthData);
                placement.outputWidth = placement.widthData.Clamped + placement.widthData.marginStart + placement.widthData.marginEnd;
            }
        }

        private void ComputeItemHeights() {
            GridPlacement[] placements = placementList.array;
            int placementCount = placementList.size;

            for (int i = 0; i < placementCount; i++) {
                ref GridPlacement placement = ref placements[i];
                placement.layoutBox.GetHeights(ref placement.heightData);
                placement.outputHeight = placement.heightData.Clamped + placement.heightData.marginStart + placement.heightData.marginEnd;
            }
        }

        protected override float ComputeContentHeight() {
            if (firstChild == null) {
                return 0;
            }

            Place();

            // get a height for all children
            ComputeItemHeights();

            // first pass, get fixed sizes
            ComputeVerticalFixedTrackSizes();

            // now compute the intrinsic sizes for all tracks so we can resolve mn and mx
            ComputeContentHeightContributionSizes();

            // now we have all the size data, figure out actual pixel sizes for all vertical tracks
            ResolveTrackSizesVertical();

            // at this point we are done because we never grow and we never allocate to fr because by definition content height
            // is only the base size of the content without any growth applied
            // this ignores fr sizes on purpose, fr size is always 0 for content size

            // last step is to sum the base heights and add the row gap size
            float retn = 0;

            for (int i = 0; i < rowTrackList.size; i++) {
                retn += rowTrackList.array[i].resolvedMinSize;
            }

            retn += element.style.GridLayoutRowGap * (rowTrackList.size - 1);

            return retn;
        }

        public override void OnChildrenChanged(LightList<AwesomeLayoutBox> childList) {
            placementDirty = true;
            // todo -- history entry?
        }

        public override void OnStyleChanged(StructList<StyleProperty> propertyList) {
            StyleProperty[] array = propertyList.array;
            int size = propertyList.size;
            for (int i = 0; i < size; i++) {
                ref StyleProperty property = ref array[i];
                switch (property.propertyId) {
                    case StylePropertyId.GridLayoutColAlignment:
                    case StylePropertyId.GridLayoutRowAlignment:
                        // layout? not sure what to do here since we just need to adjust alignment, not re calc sizes
                        break;
                    case StylePropertyId.GridLayoutDensity:
                    case StylePropertyId.GridLayoutDirection:
                        placementDirty = true;
                        flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal | AwesomeLayoutBoxFlags.RequireLayoutVertical;
                        break;

                    case StylePropertyId.GridLayoutColAutoSize:
                    case StylePropertyId.GridLayoutColTemplate:
                        placementDirty = true;
                        // todo -- history entry?
                        flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                        break;
                    case StylePropertyId.GridLayoutRowAutoSize:
                    case StylePropertyId.GridLayoutRowTemplate:
                        flags |= AwesomeLayoutBoxFlags.RequireLayoutVertical;
                        placementDirty = true;
                        break;

                    case StylePropertyId.GridLayoutColGap:
                        flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                        break;

                    case StylePropertyId.GridLayoutRowGap:
                        flags |= AwesomeLayoutBoxFlags.RequireLayoutVertical;
                        // todo -- don't need to compute sizes again, just need to reposition tracks and assign sizes to children
                        break;
                    case StylePropertyId.FitItemsHorizontal:
                    case StylePropertyId.AlignItemsHorizontal:
                        flags |= AwesomeLayoutBoxFlags.RequireLayoutHorizontal;
                        break;
                    case StylePropertyId.FitItemsVertical:
                    case StylePropertyId.AlignItemsVertical:
                        flags |= AwesomeLayoutBoxFlags.RequireLayoutVertical;
                        // todo -- don't need to layout but do need to apply sizes again so these values update on children
                        break;
                }
            }
        }

        public override void OnChildStyleChanged(AwesomeLayoutBox child, StructList<StyleProperty> propertyList) {
            StyleProperty[] array = propertyList.array;
            int size = propertyList.size;
            for (int i = 0; i < size; i++) {
                ref StyleProperty property = ref array[i];
                switch (property.propertyId) {
                    case StylePropertyId.GridItemX:
                    case StylePropertyId.GridItemY:
                    case StylePropertyId.GridItemWidth:
                    case StylePropertyId.GridItemHeight:
                        placementDirty = true;
                        // todo -- history entry?
                        flags |= (AwesomeLayoutBoxFlags.RequireLayoutHorizontal | AwesomeLayoutBoxFlags.RequireLayoutVertical);
                        break;
                }
            }
        }

        public override void RunLayoutHorizontal(int frameId) {
            if (firstChild == null) {
                return;
            }

            Place();

            // todo -- some of these might not be dirty if we did a content layout pass, can used cached values already

            ComputeItemWidths();

            // first pass, get fixed sizes
            ComputeHorizontalFixedTrackSizes();

            // now compute the intrinsic sizes
            ComputeContentWidthContributionSizes();

            ResolveTrackSizesHorizontal();

            float contentWidth = element.layoutResult.actualSize.width - (paddingBorderHorizontalStart + paddingBorderHorizontalEnd);

            float retn = 0;

            // this ignores fr sizes on purpose, fr size is always 0 for content size
            for (int i = 0; i < colTrackList.size; i++) {
                ref GridTrack track = ref colTrackList.array[i];
                track.size = track.resolvedMinSize;
                retn += track.size;
            }

            retn += element.style.GridLayoutColGap * (colTrackList.size - 1);

            float remaining = contentWidth - retn;

            if (remaining > 0) {
                remaining = GrowToMaxSize(colTrackList, remaining);
                DistributeFractionals(colTrackList, remaining);
            }
            else if (remaining < 0) {
                // todo -- support shrink(baseSize, shrinkLimit)
            }

            PositionTracks(colTrackList, element.style.GridLayoutColGap);

            float alignment = element.style.AlignItemsHorizontal;
            LayoutFit fit = element.style.FitItemsHorizontal;

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];
                ref GridTrack startTrack = ref colTrackList.array[placement.x];
                ref GridTrack endTrack = ref colTrackList.array[placement.x + placement.width - 1];
                float elementWidth = placement.outputWidth - placement.widthData.marginStart - placement.widthData.marginEnd;
                float x = startTrack.position;
                float layoutBoxWidth = (endTrack.position + endTrack.size) - x;
                //+ (placement.widthData.marginStart - placement.widthData.marginEnd);
                float originBase = x + placement.widthData.marginStart;
                float originOffset = layoutBoxWidth * alignment;
                float offset = elementWidth * -alignment;
                float alignedPosition = originBase + originOffset + offset;
                placement.layoutBox.ApplyLayoutHorizontal(x, alignedPosition, elementWidth, layoutBoxWidth, fit, frameId);
            }
        }

        private static void PositionTracks(StructList<GridTrack> trackList, float gap) {
            float offset = 0;
            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList.array[i];
                track.position = offset;
                offset += gap + track.size;
            }
        }

        private static void DistributeFractionals(StructList<GridTrack> trackList, float remaining) {
            if (remaining <= 0) return;
            int pieces = 0;
            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList.array[i];

                if (track.maxUnit == GridTemplateUnit.FractionalRemaining) {
                    pieces += (int) track.maxValue;
                }
            }

            if (pieces == 0) {
                return;
            }

            float pieceSize = remaining / pieces;

            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList.array[i];

                if (track.maxUnit == GridTemplateUnit.FractionalRemaining) {
                    track.size += pieceSize * track.maxValue; // todo -- not sure if this should be an add, overwrite, or clamp
                }
            }
        }

        public override void RunLayoutVertical(int frameId) {
            Place();

            // todo -- some of these might not be dirty if we did a content layout pass, can used cached values already

            ComputeItemHeights();

            // first pass, get fixed sizes
            ComputeVerticalFixedTrackSizes();

            // now compute the intrinsic sizes
            ComputeContentHeightContributionSizes();

            ResolveTrackSizesVertical();

            float contentHeight = element.layoutResult.actualSize.height - (paddingBorderVerticalStart + paddingBorderVerticalEnd);

            float retn = 0;

            // this ignores fr sizes on purpose, fr size is always 0 for content size
            for (int i = 0; i < rowTrackList.size; i++) {
                ref GridTrack track = ref rowTrackList.array[i];
                track.size = track.resolvedMinSize;
                retn += track.size;
            }

            retn += element.style.GridLayoutRowGap * (rowTrackList.size - 1);

            float remaining = contentHeight - retn;

            if (remaining > 0) {
                remaining = GrowToMaxSize(rowTrackList, remaining);
                DistributeFractionals(rowTrackList, remaining);
            }
            else if (remaining < 0) {
                // todo -- support shrink(baseSize, shrinkLimit)
            }

            PositionTracks(rowTrackList, element.style.GridLayoutRowGap);

            float alignment = element.style.AlignItemsVertical;
            LayoutFit fit = element.style.FitItemsVertical;

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];
                ref GridTrack startTrack = ref rowTrackList.array[placement.y];
                ref GridTrack endTrack = ref rowTrackList.array[placement.y + placement.height - 1];
                float elementHeight = placement.outputHeight - placement.heightData.marginStart - placement.heightData.marginEnd;
                float y = startTrack.position;
                float layoutBoxHeight = (endTrack.position + endTrack.size) - y;
                //+ (placement.heightData.marginStart + placement.heightData.marginEnd);
                float originBase = y + placement.heightData.marginStart;
                float originOffset = layoutBoxHeight * alignment;
                float offset = elementHeight * -alignment;
                float alignedPosition = originBase + originOffset + offset;
                placement.layoutBox.ApplyLayoutVertical(y, alignedPosition, elementHeight, layoutBoxHeight, fit, frameId);
            }
        }

        private static float GrowToMaxSizeStep(StructList<GridTrack> trackList, float remaining) {
            // get grow limits for each track
            // distribute space in 'virtual' fr units across things that still want to grow
            float desiredSpace = 0;
            int pieces = 0;

            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList.array[i];

                if (track.maxUnit == GridTemplateUnit.FractionalRemaining) {
                    continue;
                }

                if (track.size < track.resolvedMaxSize) {
                    desiredSpace += track.resolvedMaxSize - track.size;
                    pieces++;
                }
            }


            if (pieces == 0) {
                return 0;
            }

            float allocatedSpace = 0;

            if (desiredSpace > remaining) {
                desiredSpace = remaining;
            }

            float pieceSize = desiredSpace / pieces;

            for (int i = 0; i < trackList.size; i++) {
                ref GridTrack track = ref trackList.array[i];

                if (track.maxUnit == GridTemplateUnit.FractionalRemaining) {
                    continue;
                }

                if (track.size < track.resolvedMaxSize) {
                    track.size += pieceSize;
                    allocatedSpace += pieceSize;
                    if (track.size > track.resolvedMaxSize) {
                        allocatedSpace -= (track.size - track.resolvedMaxSize);
                        track.size = track.resolvedMaxSize;
                        return allocatedSpace;
                    }
                }
            }

            return allocatedSpace;
        }


        private static float GrowToMaxSize(StructList<GridTrack> trackList, float remaining) {
            float allocated = 0;
            do {
                allocated = GrowToMaxSizeStep(trackList, remaining);
                remaining -= allocated;
            } while (allocated != 0);

            return remaining;
        }

        private void Place() {
            if (!placementDirty) {
                return;
            }

            placementDirty = false;

            placementList.size = 0;
            AwesomeLayoutBox child = firstChild;

            while (child != null) {
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

            GenerateExplicitTracks();

            s_OccupiedAreas.size = 0;

            IReadOnlyList<GridTrackSize> autoColSizePattern = element.style.GridLayoutColAutoSize;
            IReadOnlyList<GridTrackSize> autoRowSizePattern = element.style.GridLayoutRowAutoSize;

            int rowSizeAutoPtr = 0;
            int colSizeAutoPtr = 0;

            PreAllocateRowAndColumns(ref colSizeAutoPtr, ref rowSizeAutoPtr, autoColSizePattern, autoRowSizePattern);

            PlaceBothAxisLocked();

            PlaceSingleAxisLocked(ref colSizeAutoPtr, autoColSizePattern, ref rowSizeAutoPtr, autoRowSizePattern);

            PlaceRemainingItems(ref colSizeAutoPtr, autoColSizePattern, ref rowSizeAutoPtr, autoRowSizePattern);

            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];
                for (int j = placement.x; j < placement.x + placement.width; j++) {
                    colTrackList.array[j].spanningItems.Add(i);
                }

                for (int j = placement.y; j < placement.y + placement.height; j++) {
                    rowTrackList.array[j].spanningItems.Add(i);
                }
            }
        }

        private static void GenerateExplicitTracksForAxis(IReadOnlyList<GridTrackSize> templateList, float contentSize, StructList<GridTrack> trackList, float gutter) {
            int idx = 0;
            int fillRepeatIndex = -1;

            trackList.size = 0;

            trackList.EnsureCapacity(templateList.Count);

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
                    //  totalMin += ResolveMin(blockSize, trackList.array[i].minValue, trackList.array[i].minUnit);
                    totalMin += gutter;
                }

                float remaining = contentSize - totalMin;

                StructList<GridTrack> repeats = StructList<GridTrack>.Get();

                do {
                    repeats.EnsureAdditionalCapacity(repeatFill.pattern.Length);
                    for (int i = 0; i < repeatFill.pattern.Length; i++) {
                        repeats.array[idx] = new GridTrack(repeatFill.pattern[i]);
                        //  float val = ResolveMin(blockSize, trackList.array[i].minValue, trackList.array[i].minUnit);
                        float val = 0;
                        if (val <= 0) val = 1;
                        remaining -= val; // this is a pretty crap way to handle this
                        remaining -= gutter;
                    }
                } while (remaining > 0);

                trackList.InsertRange(fillRepeatIndex, repeats);

                repeats.Release();
            }
        }

        private void GenerateExplicitTracks() {
            for (int i = 0; i < colTrackList.size; i++) {
                StructList<int>.Release(ref colTrackList.array[i].spanningItems);
            }

            for (int i = 0; i < rowTrackList.size; i++) {
                StructList<int>.Release(ref rowTrackList.array[i].spanningItems);
            }

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

            GenerateExplicitTracksForAxis(colTemplate, contentAreaWidth, colTrackList, horizontalGutter);
            GenerateExplicitTracksForAxis(rowTemplate, contentAreaHeight, rowTrackList, verticalGutter);
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
            for (int i = 0; i < placementList.size; i++) {
                ref GridPlacement placement = ref placementList.array[i];

                if (placement.y >= 0 && placement.x >= 0) {
                    GridRegion region = new GridRegion();
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

                    s_OccupiedAreas.Add(new GridRegion() {
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

                    s_OccupiedAreas.Add(new GridRegion() {
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

            int sparseStartX = 0; // purposefully not reading autoCursor value because that results in weird behavior for sparse grids (this is not the same as css!)
            int sparseStartY = 0; // purposefully not reading autoCursor value because that results in weird behavior for sparse grids (this is not the same as css!)

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
//                    rowTrackList.array[cursorY].autoPlacementCursor = cursorX + width;
                        colTrackList.array[cursorX].autoPlacementCursor = cursorY;
                    //for (int j = cursorX; j < cursorX + width; j++) {
                    //}
                    for (int j = cursorY; j < cursorY + height; j++) {
                        rowTrackList.array[j].autoPlacementCursor = cursorX + width;
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
//                    colTrackList.array[cursorX].autoPlacementCursor = cursorY;
                    rowTrackList.array[cursorY].autoPlacementCursor = cursorX;
                    for (int j = cursorX; j < cursorX + width; j++) {
                        colTrackList.array[j].autoPlacementCursor = cursorY + height;
                    }
                    EnsureImplicitTrackCapacity(colTrackList, cursorX + width, ref colSizeAutoPtr, autoColSizePattern);
                    EnsureImplicitTrackCapacity(rowTrackList, cursorY + height, ref rowSizeAutoPtr, autoRowSizePattern);
                }

                s_OccupiedAreas.Add(new GridRegion() {
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

            GridRegion[] array = s_OccupiedAreas.array;
            int count = s_OccupiedAreas.size;

            for (int i = 0; i < count; i++) {
                ref GridRegion check = ref array[i];
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
                    tracksList.array[idx++] = new GridTrack(autoSizes[autoSize]);
                    autoSize = (autoSize + 1) % autoSizes.Count;
                }

                tracksList.size = idx;
            }
        }

        internal struct GridPlacement {

            public int x;
            public int y;
            public int width;
            public int height;
            public AwesomeLayoutBox layoutBox;
            public LayoutSize widthData;
            public LayoutSize heightData;
            public float outputWidth;
            public float outputHeight;
            public int spannedHorizontalIntrinsics_min;
            public int spannedHorizontalIntrinsics_max;
            public float widthContributionSizeMin;
            public float widthContributionSizeMax;
            public int spannedVerticalIntrinsics_min;
            public int spannedVerticalIntrinsics_max;
            public float heightContributionSizeMin;
            public float heightContributionSizeMax;

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
            public StructList<int> spanningItems;
            public float fixedSizeMax;
            public float fixedSizeMin;
            public float resolvedMinSize;
            public float resolvedMaxSize;


            public GridTrack(in GridTrackSize template) {
                this.minValue = template.minValue;
                this.minUnit = template.minUnit;
                this.maxValue = template.maxValue;
                this.maxUnit = template.maxUnit;
                this.position = 0;
                this.size = 0;
                this.autoPlacementCursor = 0;
                this.maxSize = 0;
                this.spanningItems = StructList<int>.Get();
                this.fixedSizeMin = 0;
                this.fixedSizeMax = 0;
                this.resolvedMinSize = 0;
                this.resolvedMaxSize = 0;
            }

        }

    }

}