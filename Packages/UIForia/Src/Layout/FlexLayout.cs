using UIForia.Util.Unsafe;
using Unity.Collections;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Layout {

    internal static unsafe class FlexLayout {

        public const bool CrossAxis = true;
        public const bool MainAxis = false;

        public static CheckedArray<LayoutBoxInfo> LayoutTopDown(ref LayoutListBuffer listBuffer, ref LayoutAxis mainAxis, ref LayoutAxis crossAxis, CheckedArray<LayoutBoxInfo> boxes) {
            return LayoutBottomUp(ref listBuffer, ref mainAxis, ref crossAxis, boxes);
        }

        public static CheckedArray<LayoutBoxInfo> LayoutBottomUp(ref LayoutListBuffer listBuffer, ref LayoutAxis mainAxis, ref LayoutAxis crossAxis, CheckedArray<LayoutBoxInfo> boxes) {

            listBuffer.controlledList.size = 0;

            LayoutUtil.UpdateAxisFlags(ref listBuffer, ref crossAxis, ref mainAxis, boxes, CrossAxis);

            ComputeContentSizes(ref crossAxis, listBuffer.contentSizeList, CrossAxis);

            LayoutUtil.SolveSizes(listBuffer.solveList, ref crossAxis);

            LayoutCrossAxis(ref crossAxis, listBuffer.layoutList);

            LayoutUtil.UpdateAxisFlags(ref listBuffer, ref mainAxis, ref crossAxis, boxes, MainAxis);

            ComputeContentSizes(ref mainAxis, listBuffer.contentSizeList, MainAxis);

            LayoutUtil.SolveSizes(listBuffer.solveList, ref mainAxis);

            LayoutMainAxis(ref mainAxis, listBuffer.layoutList);

            if (listBuffer.controlledList.size != 0) {

                LayoutUtil.UpdateAxisFlagsDeferred(ref listBuffer, ref mainAxis, ref crossAxis, listBuffer.controlledList.ToCheckedArray());

                LayoutCrossAxis(ref crossAxis, listBuffer.layoutList);

            }

            return LayoutUtil.FilterCompletedBoxes(boxes, crossAxis.readyFlags, mainAxis.readyFlags);

        }

        public static void ComputeContentSizes(ref LayoutAxis axis, TempList<LayoutBoxInfo> contentSizeList, bool isCrossAxis) {

            for (int i = 0; i < contentSizeList.size; i++) {

                LayoutBoxInfo solveInfo = contentSizeList[i];
                RangeInt childRange = solveInfo.childRange;

                // we can report a content size for these boxes. The total output size of the element may change due to stretch or other factors, but we don't care.

                float maxChildSize = 0;
                float minChildSize = 0;
                float contentSize = 0;

                if (childRange.length != 0) {

                    minChildSize = float.MaxValue;

                    for (int childIter = childRange.start; childIter < childRange.end; childIter++) {
                        float childSize = axis.sizes[childIter].baseSize;
                        
                        childSize += axis.marginStart[childIter].value + axis.marginEnd[childIter].value;
                        
                        if (childSize < minChildSize) minChildSize = childSize;
                        if (childSize > maxChildSize) maxChildSize = childSize;
                        
                        contentSize += childSize;
                    }

                }

                // todo -- explore folding fixed size borders into first/last margin like we do for padding
                float borderStart = axis.borderStart[solveInfo.boxIndex];
                float borderEnd = axis.borderEnd[solveInfo.boxIndex];
                
                contentSize += borderStart + borderEnd;
                maxChildSize += borderStart + borderEnd;
                minChildSize += borderStart + borderEnd;
                
                axis.contentSizes[solveInfo.boxIndex] = new LayoutContentSizes() {
                    contentSize = isCrossAxis ? maxChildSize : contentSize,
                    maxChildSize = maxChildSize,
                    minChildSize = minChildSize
                };

            }

        }

        private static void LayoutMainAxis(ref LayoutAxis mainAxis, TempList<LayoutBoxInfo> layoutList) {

            if (layoutList.size == 0) return;

            int totalChildCount = 0;

            for (int i = 0; i < layoutList.size; i++) {
                totalChildCount += layoutList.array[i].childRange.length;
            }

            // extra space for padding blocks which will be treated as layout segments 
            TempList<StretchInfo> explodedList = TypedUnsafe.MallocUnsizedTempList<StretchInfo>((totalChildCount * 3) + (2 * layoutList.size), Allocator.Temp);
            TempList<MainAxisPassInfo> passList = TypedUnsafe.MallocUnsizedTempList<MainAxisPassInfo>(layoutList.size, Allocator.Temp);

            PopulateExplodedList(layoutList, mainAxis, ref explodedList, ref passList);

            HandleMinStretchSizes(passList, explodedList);

            HandleMaxStretchSizes(passList, explodedList);

            DistributeStretchAmounts(passList, explodedList);

            ConstructOutput(ref mainAxis, passList, explodedList);

            for (int i = 0; i < layoutList.size; i++) {
                mainAxis.readyFlags[layoutList[i].boxIndex] |= ReadyFlags.LayoutComplete;
            }

        }

        private static void ConstructOutput(ref LayoutAxis main, TempList<MainAxisPassInfo> passList, TempList<StretchInfo> explodedList) {
            for (int passIdx = 0; passIdx < passList.size; passIdx++) {
                ref MainAxisPassInfo pass = ref passList.array[passIdx];

                int boxIdx = pass.childRange.start;

                float offset = pass.borderInset;

                if (pass.fillFromEnd) {
                    float totalChildSize = 0;
                    for (int i = pass.explodeRange.start; i < pass.explodeRange.end; i++) {
                        totalChildSize += explodedList.array[i].currentSize;
                    }

                    offset = pass.totalSize - pass.borderInset - totalChildSize;
                }

                for (int i = pass.explodeRange.start; i < pass.explodeRange.end; i += 3) {

                    ref StretchInfo mStart = ref explodedList.array[i + 0];
                    ref StretchInfo ele = ref explodedList.array[i + 1];
                    ref StretchInfo mEnd = ref explodedList.array[i + 2];

                    main.outputSizes[boxIdx] = ele.currentSize;
                    main.outputPositions[boxIdx] = offset + mStart.currentSize;

                    main.readyFlags[boxIdx] |= ReadyFlags.ParentReady | ReadyFlags.StretchReady | ReadyFlags.FinalSizeResolved;

                    offset += mStart.currentSize + ele.currentSize + mEnd.currentSize;

                    boxIdx++;

                }

            }

        }

        private static void DistributeStretchAmounts(TempList<MainAxisPassInfo> passList, TempList<StretchInfo> explodedList) {
            for (int passIdx = 0; passIdx < passList.size; passIdx++) {

                ref MainAxisPassInfo pass = ref passList.array[passIdx];

                if (pass.totalStretch == 0 || pass.remaining <= 0) {
                    continue;
                }

                float pieceSize = pass.remaining / pass.totalStretch;

                for (int i = pass.explodeRange.start; i < pass.explodeRange.end; i++) {
                    ref StretchInfo item = ref explodedList.array[i];
                    item.currentSize += explodedList.array[i].stretchParts * pieceSize;
                }

            }
        }

        private static void HandleMinStretchSizes(TempList<MainAxisPassInfo> passList, TempList<StretchInfo> explodedList) {
            for (int passIdx = 0; passIdx < passList.size; passIdx++) {

                ref MainAxisPassInfo pass = ref passList.array[passIdx];

                if (pass.totalStretch == 0) {
                    continue;
                }

                while (true) {

                    float pieceSize = math.max(pass.remaining / pass.totalStretch, 0);

                    bool checkAgain = false;

                    for (int i = pass.explodeRange.start; i < pass.explodeRange.end; i++) {

                        ref StretchInfo item = ref explodedList.array[i];

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
        }

        private static void HandleMaxStretchSizes(TempList<MainAxisPassInfo> passList, TempList<StretchInfo> explodedList) {
            for (int passIdx = 0; passIdx < passList.size; passIdx++) {

                ref MainAxisPassInfo pass = ref passList.array[passIdx];

                if (pass.totalStretch == 0 || pass.remaining <= 0) {
                    continue;
                }

                float pieceSize = pass.remaining / pass.totalStretch;

                for (int i = pass.explodeRange.start; i < pass.explodeRange.end; i++) {

                    ref StretchInfo item = ref explodedList.array[i];

                    if (item.stretchParts > 0 && item.maxSize < pieceSize * item.stretchParts) {
                        item.currentSize = item.maxSize;
                        pass.totalStretch -= item.stretchParts;
                        pass.remaining -= item.maxSize; // todo -- wrong since we accounted for current already? 
                        item.stretchParts = 0;
                    }
                }

            }
        }

        private static TempList<CrossAxisInfo> GatherCrossAxisData(TempList<LayoutBoxInfo> layoutList, in LayoutAxis cross, int totalElementCount) {

            TempList<CrossAxisInfo> crossInfo = TypedUnsafe.MallocUnsizedTempList<CrossAxisInfo>(totalElementCount, Allocator.Temp);

            for (int layoutIter = 0; layoutIter < layoutList.size; layoutIter++) {

                LayoutBoxInfo layoutInfo = layoutList[layoutIter];

                RangeInt childRange = layoutInfo.childRange;
                int parentIndex = layoutInfo.boxIndex;

                float borderStart = cross.borderStart[parentIndex];
                float borderEnd = cross.borderEnd[parentIndex];
                float parentSize = cross.outputSizes[parentIndex];

                for (int i = childRange.start; i < childRange.end; i++) {

                    ref CrossAxisInfo crossAxisInfo = ref crossInfo.array[crossInfo.size++];
                    LayoutSizes sizes = cross.sizes[i];
                    crossAxisInfo.boxIndex = i;
                    crossAxisInfo.parentSize = parentSize;

                    crossAxisInfo.stretch = (int) sizes.stretch;
                    crossAxisInfo.baseSize = sizes.baseSize;
                    crossAxisInfo.minSize = sizes.min;
                    crossAxisInfo.maxSize = sizes.max;

                    // parent padding already accounted for in collapse step 

                    crossAxisInfo.marginStartStretch = (ushort) cross.marginStart[i].stretch;
                    crossAxisInfo.marginStartValue = cross.marginStart[i].value + borderStart;

                    crossAxisInfo.marginEndStretch = (ushort) cross.marginEnd[i].stretch;
                    crossAxisInfo.marginEndValue = cross.marginEnd[i].value + borderEnd;

                    // todo -- not sure if we still do this or not 
                    // if (cross.finalPassSize[i].sizeType != FinalPassSizeType.Controlled) {
                    //     continue;
                    // }
                    //
                    // childInfo.stretch = 0;
                    // childInfo.minSize = 0;
                    // childInfo.maxSize = float.MaxValue;
                    // childInfo.baseSize = 0;
                    // aspectLocked.Add(childInfo);
                    // crossInfo.size--; // remove from the list 

                }

            }

            return crossInfo;
        }

        public static void StretchCrossAxis(ref LayoutAxis cross, CheckedArray<CrossAxisInfo> crossInfo, bool setChildFlags) {

            for (int i = 0; i < crossInfo.size; i++) {

                ref CrossAxisInfo info = ref crossInfo.Get(i);

                // if stretch & has min/max size then curr size needs to be 0'd out 

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

                float remaining = info.parentSize - (mStartGrowInfo.currentSize + mEndGrowInfo.currentSize + eleGrowInfo.currentSize);

                SolvedSize prefSize = cross.solvedPrefSizes[info.boxIndex];
                switch (prefSize.unit) {

                    // I think we've already handled min clamping here and no longer need this 
                    // stretch content means we init to min of (reported) content size and this elements min value
                    // then we'll stretch from that point if we're able to 
                    // case SolvedSizeUnit.StretchContent:
                    //     eleGrowInfo.currentSize = 0;
                    //     eleGrowInfo.minSize = math.max(cross.sizes[i].resolved, info.minSize); 
                    //     break;

                    // fit content means we set our max size depending on how much room there is to stretch 
                    // if contentSize > partSize * totalStretch, we set max to min of that value and our element's max size
                    // otherwise we set max to content size. We basically just clamp the max value

                    case SolvedSizeUnit.FillRemaining: {
                        float fill = prefSize.value * remaining;
                        remaining -= fill;
                        eleGrowInfo.currentSize = fill;
                        eleGrowInfo.stretchParts = 0;
                        break;
                    }

                    // todo -- verify this still works 
                    case SolvedSizeUnit.FitContent:
                        float partSize = remaining / totalStretch; // if fit content stretch val is always at least 1 so no div by 0 error
                        float contentSize = eleGrowInfo.currentSize; // cross.finalPassSize[i].value;
                        eleGrowInfo.maxSize = contentSize > partSize * totalStretch
                            ? math.min(partSize * totalStretch, info.maxSize)
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

                cross.outputSizes.array[info.boxIndex] = eleGrowInfo.currentSize;
                bool fillFromEnd = false; // todo -- find this a home 
                if (fillFromEnd) {
                    cross.outputPositions[info.boxIndex] = info.parentSize - (mEndGrowInfo.currentSize + eleGrowInfo.currentSize);
                }
                else {
                    cross.outputPositions[info.boxIndex] = mStartGrowInfo.currentSize;
                }

            }

            if (setChildFlags) {
                for (int i = 0; i < crossInfo.size; i++) {
                    cross.readyFlags[crossInfo[i].boxIndex] |= ReadyFlags.FinalSizeResolved | ReadyFlags.ParentReady | ReadyFlags.StretchReady;
                }
            }

        }

        private static void PopulateExplodedList(TempList<LayoutBoxInfo> layoutInfos, in LayoutAxis main, ref TempList<StretchInfo> explodedList, ref TempList<MainAxisPassInfo> passList) {

            for (int layoutIdx = 0; layoutIdx < layoutInfos.size; layoutIdx++) {

                RangeInt childRange = layoutInfos[layoutIdx].childRange;

                // do nothing if no children
                if (childRange.length == 0) {
                    continue;
                }

                int parentIndex = layoutInfos[layoutIdx].boxIndex;

                float parentSize = main.outputSizes[parentIndex];
                float borderStart = main.borderStart[parentIndex];
                float borderEnd = main.borderEnd[parentIndex];

                // break children into 3 parts, marginTop, content, marginBottom 

                int start = explodedList.size;

                bool hasFitContent = false;
                bool hasFillRemaining = false;

                int totalStretch = 0;
                float remaining = parentSize - (borderStart + borderEnd);

                for (int i = childRange.start; i < childRange.end; i++) {

                    // there are no height controlled aspects at this point, last step resolved them

                    ref StretchInfo mStartGrowInfo = ref explodedList.array[explodedList.size++];
                    ref StretchInfo elementGrowInfo = ref explodedList.array[explodedList.size++];
                    ref StretchInfo mEndGrowInfo = ref explodedList.array[explodedList.size++];

                    ResolvedSpacerSize marginStart = main.marginStart[i];
                    ResolvedSpacerSize marginEnd = main.marginEnd[i];
                    SolvedSize solvedSize = main.solvedPrefSizes[i];

                    mStartGrowInfo.maxSize = float.MaxValue;
                    mStartGrowInfo.currentSize = marginStart.stretch != 0 ? 0 : marginStart.value;
                    mStartGrowInfo.stretchParts = marginStart.stretch;
                    mStartGrowInfo.minSize = marginStart.value;

                    LayoutSizes sizes = main.sizes[i];

                    switch (solvedSize.unit) {
                        case SolvedSizeUnit.StretchContent:
                            elementGrowInfo.currentSize = 0;
                            elementGrowInfo.minSize = sizes.baseSize; // be at least content size
                            elementGrowInfo.maxSize = sizes.max;
                            elementGrowInfo.stretchParts = (int) sizes.stretch;
                            break;

                        case SolvedSizeUnit.FillRemaining:
                            hasFillRemaining = true; // need to defer this because we need to know stretch total size
                            elementGrowInfo.currentSize = 0; // set later 
                            elementGrowInfo.minSize = sizes.min;
                            elementGrowInfo.maxSize = sizes.max;
                            elementGrowInfo.stretchParts = 0;
                            break;

                        case SolvedSizeUnit.FitContent:
                            hasFitContent = true; // need to defer this because we need to know stretch total size
                            elementGrowInfo.currentSize = 0;
                            elementGrowInfo.maxSize = sizes.max;
                            elementGrowInfo.stretchParts = (int) sizes.stretch;
                            break;

                        // case SolvedSizeUnit.Controlled: {
                        //
                        //     if (main.isVertical) {
                        //         // todo -- might not be output size at this point
                        //         elementGrowInfo.currentSize = cross.outputSizes[i] / sizes.baseSize; // resolved will hold the aspect ratio 
                        //     }
                        //     else {
                        //         elementGrowInfo.currentSize = cross.outputSizes[i] * sizes.baseSize;
                        //     }
                        //
                        //     elementGrowInfo.maxSize = float.MaxValue;
                        //     elementGrowInfo.minSize = 0;
                        //     elementGrowInfo.stretchParts = 0;
                        //     break;
                        // }

                        default:
                            elementGrowInfo.currentSize = sizes.stretch == 0 ? sizes.baseSize : 0;
                            elementGrowInfo.minSize = sizes.min;
                            elementGrowInfo.maxSize = sizes.max;
                            elementGrowInfo.stretchParts = (int) sizes.stretch;
                            break;
                    }

                    mEndGrowInfo.currentSize = marginEnd.stretch != 0 ? 0 : marginEnd.value;
                    mEndGrowInfo.minSize = marginEnd.value;
                    mEndGrowInfo.maxSize = float.MaxValue;
                    mEndGrowInfo.stretchParts = marginEnd.stretch;

                    remaining -= mStartGrowInfo.currentSize;
                    remaining -= elementGrowInfo.currentSize;
                    remaining -= mEndGrowInfo.currentSize;

                    totalStretch += mStartGrowInfo.stretchParts;
                    totalStretch += elementGrowInfo.stretchParts;
                    totalStretch += mEndGrowInfo.stretchParts;
                }

                if (hasFillRemaining) {

                    float baseRemaining = remaining;

                    for (int loopCounter = 0, childIndex = childRange.start; childIndex < childRange.end; childIndex++, loopCounter++) {

                        SolvedSize solvedSize = main.solvedPrefSizes[childIndex];
                        if (solvedSize.unit == SolvedSizeUnit.FillRemaining) {
                            float fillAmount = baseRemaining * solvedSize.value;
                            remaining -= fillAmount;
                            ref StretchInfo elementGrowInfo = ref explodedList.array[start + (loopCounter * 3) + 1]; // +1 is because start points at first margin start, we want the element
                            elementGrowInfo.currentSize = fillAmount;
                        }
                    }

                    if (remaining < 0) remaining = 0;

                }

                if (hasFitContent) {

                    float partSize = remaining / totalStretch;

                    for (int loopCounter = 0, childIndex = childRange.start; childIndex < childRange.end; childIndex++, loopCounter++) {

                        if (main.solvedPrefSizes[childIndex].unit != SolvedSizeUnit.FitContent) {
                            continue;
                        }

                        ref StretchInfo elementGrowInfo = ref explodedList.array[start + (loopCounter * 3) + 1]; // +1 is because start points at first margin start, we want the element

                        float contentSize = main.sizes[childIndex].baseSize;

                        elementGrowInfo.maxSize = contentSize > partSize * totalStretch
                            ? math.min(partSize * totalStretch, float.MaxValue)
                            : contentSize;

                    }

                }

                // todo -- could put border into horizontal/vertical space solvers and fold into child edge margin fixed sizes 

                bool fillFromEnd = false; // todo -- somehow need this size
                passList.array[passList.size++] = new MainAxisPassInfo() {
                    explodeRange = new RangeInt(start, explodedList.size - start),
                    childRange = childRange,
                    remaining = remaining,
                    totalStretch = totalStretch,
                    totalSize = parentSize,
                    borderInset = fillFromEnd ? borderEnd : borderStart,
                    fillFromEnd = fillFromEnd
                };

            }

        }

        public static void HandleMainAxisMarginCollapse(ref MarginCollapseInfo collapseInfo, CheckedArray<LayoutInfo2> layoutList) {

            for (int i = 0; i < layoutList.size; i++) {

                RangeInt childRange = layoutList[i].childRange;
                if(childRange.length == 0) continue;
                
                int boxIndex = layoutList[i].boxIndex;

                SpaceCollapse collapse = collapseInfo.spaceCollapse[boxIndex];

                ResolvedSpacerSize parentPaddingStart = collapseInfo.paddingStart[boxIndex];
                ResolvedSpacerSize parentPaddingEnd = collapseInfo.paddingEnd[boxIndex];

                // parentPaddingStart.value += collapseInfo.borderStart[boxIndex];
                // parentPaddingEnd.value += collapseInfo.borderEnd[boxIndex];
                
                ref ResolvedSpacerSize firstChildMarginStart = ref collapseInfo.marginStart.Get(childRange.start);
                ref ResolvedSpacerSize lastChildMarginEnd = ref collapseInfo.marginEnd.Get(childRange.end - 1);

                if ((collapse & SpaceCollapse.RemoveOuter) != 0) {
                    firstChildMarginStart = default;
                    lastChildMarginEnd = default;
                }
                else if ((collapse & SpaceCollapse.CollapseOuter) != 0) {

                    float maxStart = firstChildMarginStart.value > parentPaddingStart.value ? firstChildMarginStart.value : parentPaddingStart.value;
                    float maxEnd = lastChildMarginEnd.value > parentPaddingEnd.value ? lastChildMarginEnd.value : parentPaddingEnd.value;

                    ushort maxStretchStart = (ushort) (firstChildMarginStart.stretch > parentPaddingStart.stretch ? firstChildMarginStart.stretch : parentPaddingStart.stretch);
                    ushort maxStretchEnd = (ushort) (lastChildMarginEnd.stretch > parentPaddingEnd.stretch ? lastChildMarginEnd.stretch : parentPaddingEnd.stretch);

                    firstChildMarginStart.value = maxStart;
                    firstChildMarginStart.stretch = maxStretchStart;

                    lastChildMarginEnd.value = maxEnd;
                    lastChildMarginEnd.stretch = maxStretchEnd;
                }
                else {
                    firstChildMarginStart.value += parentPaddingStart.value;
                    firstChildMarginStart.stretch += parentPaddingStart.stretch;
                    lastChildMarginEnd.value += parentPaddingEnd.value;
                    lastChildMarginEnd.stretch += parentPaddingEnd.stretch;
                }

            }

            for (int i = 0; i < layoutList.size; i++) {

                RangeInt childRange = layoutList[i].childRange;

                int boxIndex = layoutList[i].boxIndex;

                SpaceCollapse collapse = collapseInfo.spaceCollapse[boxIndex];
                ResolvedSpacerSize spacer = collapseInfo.betweenSpacer[boxIndex];

                if (childRange.length == 1) spacer = default;

                if ((spacer.value != 0 || spacer.stretch != 0) || (collapse & SpaceCollapse.RemoveInner) != 0) {

                    collapseInfo.marginEnd[childRange.start] = spacer;
                    collapseInfo.marginStart[childRange.end - 1] = default;

                    for (int cIdx = childRange.start + 1; cIdx < childRange.end - 1; cIdx++) {
                        collapseInfo.marginStart[cIdx] = default;
                        collapseInfo.marginEnd[cIdx] = spacer;
                    }

                }
                else if (((collapse & SpaceCollapse.CollapseInner) != 0)) {

                    for (int cIdx = childRange.start + 1; cIdx < childRange.end; cIdx++) {
                        ref ResolvedSpacerSize start = ref collapseInfo.marginStart.Get(cIdx);
                        ref ResolvedSpacerSize end = ref collapseInfo.marginEnd.Get(cIdx - 1);

                        float maxValue = start.value > end.value ? start.value : end.value;
                        int maxStretch = start.stretch > end.stretch ? start.stretch : end.stretch;

                        start.value = 0;
                        start.stretch = 0;
                        end.value = maxValue;
                        end.stretch = maxStretch;

                    }
                }

            }
        }

        public static void HandleCrossAxisMarginCollapse(ref MarginCollapseInfo marginCollapseInfo, CheckedArray<LayoutInfo2> layoutList) {

            for (int i = 0; i < layoutList.size; i++) {

                RangeInt range = layoutList[i].childRange;

                int idx = layoutList[i].boxIndex;

                SpaceCollapse collapse = marginCollapseInfo.spaceCollapse[idx];

                ResolvedSpacerSize parentPaddingStart = marginCollapseInfo.paddingStart[idx];
                ResolvedSpacerSize parentPaddingEnd = marginCollapseInfo.paddingEnd[idx];

                for (int cIdx = range.start; cIdx < range.end; cIdx++) {

                    ref ResolvedSpacerSize childMarginStart = ref marginCollapseInfo.marginStart.Get(cIdx);
                    ref ResolvedSpacerSize childMarginEnd = ref marginCollapseInfo.marginEnd.Get(cIdx);

                    if (((collapse & SpaceCollapse.RemoveOuter) != 0)) {
                        childMarginStart = parentPaddingStart;
                        childMarginEnd = parentPaddingEnd;
                    }
                    else if ((collapse & SpaceCollapse.CollapseOuter) != 0) {

                        float maxStart = childMarginStart.value > parentPaddingStart.value ? childMarginStart.value : parentPaddingStart.value;
                        float maxEnd = childMarginEnd.value > parentPaddingEnd.value ? childMarginEnd.value : parentPaddingEnd.value;

                        ushort maxStretchStart = (ushort) (childMarginStart.stretch > parentPaddingStart.stretch ? childMarginStart.stretch : parentPaddingStart.stretch);
                        ushort maxStretchEnd = (ushort) (childMarginEnd.stretch > parentPaddingEnd.stretch ? childMarginEnd.stretch : parentPaddingEnd.stretch);

                        childMarginStart.value = maxStart;
                        childMarginStart.stretch = maxStretchStart;

                        childMarginEnd.value = maxEnd;
                        childMarginEnd.stretch = maxStretchEnd;

                    }
                    else {
                        childMarginStart.value += parentPaddingStart.value;
                        childMarginStart.stretch += parentPaddingStart.stretch;
                        childMarginEnd.value += parentPaddingEnd.value;
                        childMarginEnd.stretch += parentPaddingEnd.stretch;
                    }

                }

            }

        }

        private struct MainAxisPassInfo {

            public RangeInt explodeRange;
            public RangeInt childRange;
            public int totalStretch;
            public float remaining;
            public float totalSize;
            public bool fillFromEnd;
            public float borderInset;

        }

      

        public static void LayoutCrossAxis(ref LayoutAxis axis, TempList<LayoutBoxInfo> layoutList, bool setChildFlags = true) {

            if (layoutList.size == 0) return;

            int totalChildCount = 0;

            for (int i = 0; i < layoutList.size; i++) {
                totalChildCount += layoutList.array[i].childRange.length;
            }

            using TempList<CrossAxisInfo> crossInfo = GatherCrossAxisData(layoutList, axis, totalChildCount); // todo -- I have a bad feeling about this allocator, I think I want a temp buffer to bump allocate it 

            StretchCrossAxis(ref axis, crossInfo.ToCheckedArray(), setChildFlags);

            for (int i = 0; i < layoutList.size; i++) {
                axis.readyFlags[layoutList[i].boxIndex] |= ReadyFlags.LayoutComplete;
            }

        }

    }
   
    internal struct LayoutListBuffer {

        public TempList<LayoutBoxInfo> solveList;
        public TempList<LayoutBoxInfo> layoutList;
        public DataList<LayoutBoxInfo> controlledList;
        public TempList<LayoutBoxInfo> contentSizeList;

    }

    internal struct LayoutContentSizes {

        public float contentSize;
        public float minChildSize;
        public float maxChildSize;

    }

    internal struct BaseSize {

        public float value;
        public uint stretch;

        public BaseSize(float value, uint stretch) {
            this.value = value;
            this.stretch = stretch;
        }

    }
    internal struct CrossAxisInfo {

        public int stretch;
        public int boxIndex;
        public float baseSize;
        public float minSize;
        public float maxSize;
        public float marginStartValue;
        public float marginEndValue;
        public ushort marginStartStretch;
        public ushort marginEndStretch;
        public float parentSize;

    }

}