using System.Diagnostics;
using System.Runtime.InteropServices;
using UIForia.Style;
using UIForia.Text;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Jobs;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace UIForia.Layout {

    [StructLayout(LayoutKind.Explicit)]
    internal struct LayoutBoxInfo {

        [FieldOffset(0)] public int boxIndex;
        [FieldOffset(4)] public int parentIndex;
        [FieldOffset(8)] public RangeInt childRange;
        [FieldOffset(8)] public bool ignoredPositionHSet;
        [FieldOffset(9)] public bool ignoredPositionVSet;

        public LayoutBoxInfo(int boxIndex, int parentIndex, RangeInt childRange = default) {
            this.ignoredPositionHSet = false;
            this.ignoredPositionVSet = false;
            this.boxIndex = boxIndex;
            this.parentIndex = parentIndex;
            this.childRange = childRange;
        }

    }

    [BurstCompile(DisableSafetyChecks = UIApplication.DisableJobSafety)]
    internal unsafe struct PerformQuadPassLayout : IJob {

        [NativeDisableUnsafePtrRestriction] public LayoutTree* layoutTree;
        [NativeDisableUnsafePtrRestriction] public TextDataTable* textDataTable;
        [NativeDisableUnsafePtrRestriction] public PerFrameLayoutData* perFrameLayoutData;
        [NativeDisableUnsafePtrRestriction] public PerFrameLayoutInfo* perFrameLayoutBoxes;
        [NativeDisableUnsafePtrRestriction] public PerFrameLayoutOutput* perFrameLayoutOutput;

        public CheckedArray<Rect> viewRects;

        public DataList<DataList<InterpolatedStyleValue>> animationValueBuffer;

        private struct LayoutTypeInfoList {

            public LayoutBoxInfo* array;
            public int size;

            [DebuggerStepThrough]
            public CheckedArray<LayoutBoxInfo> ToCheckedArray() {
                return new CheckedArray<LayoutBoxInfo>(array, size);
            }

        }

        public void Execute() {

            int elementCount = layoutTree->elementCount;
            textDataTable->SetupLayout();

            LayoutLevelBoxes* boxBuffer = stackalloc LayoutLevelBoxes[layoutTree->depthLevels.size];
            LayoutTypeInfoList* layoutTypeListBuffer = stackalloc LayoutTypeInfoList[(int) LayoutBoxType.__COUNT__];

            CheckedArray<LayoutTypeInfoList> layoutTypeLists = new CheckedArray<LayoutTypeInfoList>(layoutTypeListBuffer, (int) LayoutBoxType.__COUNT__);
            CheckedArray<LayoutLevelBoxes> levelBoxes = new CheckedArray<LayoutLevelBoxes>(boxBuffer, layoutTree->depthLevels.size);

            int ignoredCount = 0;
            for (int i = 0; i < layoutTree->depthLevels.size; i++) {
                ignoredCount += layoutTree->depthLevels[i].ignoredRange.length;
            }

            using TempList<LayoutBoxInfo> layoutBuffer = TypedUnsafe.MallocSizedTempList<LayoutBoxInfo>(elementCount + ignoredCount, Allocator.TempJob);
            using TempList<float> floatScopeList = TypedUnsafe.MallocClearedTempList<float>(10 * elementCount, Allocator.TempJob);
            using TempList<ReadyFlags> readyFlagBuffer = TypedUnsafe.MallocClearedTempList<ReadyFlags>(elementCount * 2, Allocator.TempJob);
            using TempList<LayoutSizes> layoutSizeBuffer = TypedUnsafe.MallocClearedTempList<LayoutSizes>(elementCount * 2, Allocator.TempJob);
            using TempList<LayoutContentSizes> contentSizesBuffer = TypedUnsafe.MallocClearedTempList<LayoutContentSizes>(elementCount * 2, Allocator.TempJob);

            CheckedArray<ResolvedSpacerSize> marginTop = new CheckedArray<ResolvedSpacerSize>(perFrameLayoutData->marginTops, elementCount);
            CheckedArray<ResolvedSpacerSize> marginRight = new CheckedArray<ResolvedSpacerSize>(perFrameLayoutData->marginRights, elementCount);
            CheckedArray<ResolvedSpacerSize> marginBottom = new CheckedArray<ResolvedSpacerSize>(perFrameLayoutData->marginBottoms, elementCount);
            CheckedArray<ResolvedSpacerSize> marginLeft = new CheckedArray<ResolvedSpacerSize>(perFrameLayoutData->marginLefts, elementCount);
            CheckedArray<ResolvedSpacerSize> paddingTop = new CheckedArray<ResolvedSpacerSize>(perFrameLayoutData->paddingTops, elementCount);
            CheckedArray<ResolvedSpacerSize> paddingRight = new CheckedArray<ResolvedSpacerSize>(perFrameLayoutData->paddingRights, elementCount);
            CheckedArray<ResolvedSpacerSize> paddingBottom = new CheckedArray<ResolvedSpacerSize>(perFrameLayoutData->paddingBottoms, elementCount);
            CheckedArray<ResolvedSpacerSize> paddingLeft = new CheckedArray<ResolvedSpacerSize>(perFrameLayoutData->paddingLefts, elementCount);
            CheckedArray<ResolvedSpacerSize> spaceBetweenHorizontal = new CheckedArray<ResolvedSpacerSize>(perFrameLayoutData->spaceBetweenHorizontal, elementCount);
            CheckedArray<ResolvedSpacerSize> spaceBetweenVertical = new CheckedArray<ResolvedSpacerSize>(perFrameLayoutData->spaceBetweenVertical, elementCount);

            CheckedArray<SpaceCollapse> spaceCollapseHorizontal = new CheckedArray<SpaceCollapse>(perFrameLayoutData->spaceCollapseHorizontal, elementCount);
            CheckedArray<SpaceCollapse> spaceCollapseVertical = new CheckedArray<SpaceCollapse>(perFrameLayoutData->spaceCollapseVertical, elementCount);

            CheckedArray<float> borderTop = new CheckedArray<float>(perFrameLayoutData->borderTops, elementCount);
            CheckedArray<float> borderRight = new CheckedArray<float>(perFrameLayoutData->borderRights, elementCount);
            CheckedArray<float> borderBottom = new CheckedArray<float>(perFrameLayoutData->borderBottoms, elementCount);
            CheckedArray<float> borderLeft = new CheckedArray<float>(perFrameLayoutData->borderLefts, elementCount);

            CheckedArray<float> marginHorizontals = floatScopeList.Slice(elementCount * 0, elementCount);
            CheckedArray<float> marginVerticals = floatScopeList.Slice(elementCount * 1, elementCount);

            CheckedArray<float> outputWidths = floatScopeList.Slice(elementCount * 2, elementCount);
            CheckedArray<float> outputHeights = floatScopeList.Slice(elementCount * 3, elementCount);
            CheckedArray<float> outputPositionX = floatScopeList.Slice(elementCount * 4, elementCount);
            CheckedArray<float> outputPositionY = floatScopeList.Slice(elementCount * 5, elementCount);

            CheckedArray<float> paddingBorderTop = floatScopeList.Slice(elementCount * 6, elementCount);
            CheckedArray<float> paddingBorderRight = floatScopeList.Slice(elementCount * 7, elementCount);
            CheckedArray<float> paddingBorderBottom = floatScopeList.Slice(elementCount * 8, elementCount);
            CheckedArray<float> paddingBorderLeft = floatScopeList.Slice(elementCount * 9, elementCount);

            CheckedArray<LayoutSizes> horizontalSizes = layoutSizeBuffer.Slice(0, elementCount);
            CheckedArray<LayoutSizes> verticalSizes = layoutSizeBuffer.Slice(elementCount, elementCount);

            MarginCollapseInfo marginCollapseHorizontal = new MarginCollapseInfo() {
                betweenSpacer = spaceBetweenHorizontal,
                marginStart = marginLeft,
                marginEnd = marginRight,
                paddingStart = paddingLeft,
                paddingEnd = paddingRight,
                spaceCollapse = spaceCollapseHorizontal
            };

            MarginCollapseInfo marginCollapseVertical = new MarginCollapseInfo() {
                betweenSpacer = spaceBetweenHorizontal,
                marginStart = marginTop,
                marginEnd = marginBottom,
                paddingStart = paddingTop,
                paddingEnd = paddingBottom,
                spaceCollapse = spaceCollapseVertical,
            };

            // Note -- updates margin values so we need to be sure there isnt any thread contention
            GridLayout.HandleMarginCollapse(ref marginCollapseHorizontal, ref perFrameLayoutBoxes->gridHorizontalMainAxis);
            GridLayout.HandleMarginCollapse(ref marginCollapseVertical, ref perFrameLayoutBoxes->gridHorizontalCrossAxis);
            GridLayout.HandleMarginCollapse(ref marginCollapseVertical, ref perFrameLayoutBoxes->gridVerticalMainAxis);
            GridLayout.HandleMarginCollapse(ref marginCollapseHorizontal, ref perFrameLayoutBoxes->gridVerticalCrossAxis);

            // gather sum up padding & borders so we have them by axis already prepared 
            for (int i = 0; i < elementCount; i++) {
                paddingBorderLeft[i] = paddingLeft[i].value + borderLeft[i];
                paddingBorderRight[i] = paddingRight[i].value + borderRight[i];
            }

            for (int i = 0; i < elementCount; i++) {
                paddingBorderTop[i] = paddingTop[i].value + borderTop[i];
                paddingBorderBottom[i] = paddingBottom[i].value + borderBottom[i];
            }

            for (int i = 0; i < elementCount; i++) {
                marginVerticals[i] = marginTop[i].value + marginBottom[i].value;
            }

            for (int i = 0; i < elementCount; i++) {
                marginHorizontals[i] = marginLeft[i].value + marginRight[i].value;
            }

            LayoutAxis horizontalAxis = new LayoutAxis {

                marginSize = marginHorizontals,
                paddingBorderStart = paddingBorderLeft,
                paddingBorderEnd = paddingBorderRight,
                spaceBetween = spaceBetweenHorizontal,
                marginStart = marginLeft,
                marginEnd = marginRight,

                solvedPrefSizes = new CheckedArray<SolvedSize>(perFrameLayoutData->solvedWidths, elementCount),
                solvedMinSizes = new CheckedArray<SolvedConstraint>(perFrameLayoutData->minWidths, elementCount),
                solvedMaxSizes = new CheckedArray<SolvedConstraint>(perFrameLayoutData->maxWidths, elementCount),

                contentSizes = new CheckedArray<LayoutContentSizes>(contentSizesBuffer.array, elementCount),
                readyFlags = new CheckedArray<ReadyFlags>(readyFlagBuffer.array, elementCount),

                sizes = horizontalSizes,
                borderStart = borderLeft,
                borderEnd = borderRight,
                isVertical = false,

                outputPositions = outputPositionX,
                outputSizes = outputWidths,
                paddingStart = paddingLeft,
                paddingEnd = paddingRight,

                animatedSizes = animationValueBuffer[(int) PropertyId.PreferredWidth]

            };

            LayoutAxis verticalAxis = new LayoutAxis() {

                marginSize = marginVerticals,

                paddingBorderStart = paddingBorderTop,
                paddingBorderEnd = paddingBorderBottom,

                spaceBetween = spaceBetweenVertical,
                marginStart = marginTop,
                marginEnd = marginBottom,

                solvedPrefSizes = new CheckedArray<SolvedSize>(perFrameLayoutData->solvedHeights, elementCount),
                solvedMinSizes = new CheckedArray<SolvedConstraint>(perFrameLayoutData->minHeights, elementCount),
                solvedMaxSizes = new CheckedArray<SolvedConstraint>(perFrameLayoutData->maxHeights, elementCount),

                contentSizes = new CheckedArray<LayoutContentSizes>(contentSizesBuffer.array + elementCount, elementCount),
                readyFlags = new CheckedArray<ReadyFlags>(readyFlagBuffer.array + elementCount, elementCount),

                sizes = verticalSizes,
                borderStart = borderTop,
                borderEnd = borderBottom,
                isVertical = true,

                outputPositions = outputPositionY,
                outputSizes = outputHeights,
                paddingStart = paddingTop,
                paddingEnd = paddingBottom,

                animatedSizes = animationValueBuffer[(int) PropertyId.PreferredHeight]

            };

            InitializeReadyFlags(ref horizontalAxis, ref verticalAxis);

            UnsetIntrinsicGridContentSizeFlags(perFrameLayoutBoxes->gridHorizontalMainAxis.gridAxisInfos, ref horizontalAxis);
            UnsetIntrinsicGridContentSizeFlags(perFrameLayoutBoxes->gridHorizontalCrossAxis.gridAxisInfos, ref horizontalAxis);
            UnsetIntrinsicGridContentSizeFlags(perFrameLayoutBoxes->gridVerticalMainAxis.gridAxisInfos, ref verticalAxis);
            UnsetIntrinsicGridContentSizeFlags(perFrameLayoutBoxes->gridVerticalCrossAxis.gridAxisInfos, ref verticalAxis);

            GatherBoxes(layoutTree, levelBoxes, layoutTypeLists, layoutBuffer.array);

            int maxGroupCount = 0;

            for (int i = 0; i < levelBoxes.size; i++) {
                int levelMax = levelBoxes[i].GetMaxGroupCount();
                if (levelMax > maxGroupCount) maxGroupCount = levelMax;
            }

            using TempList<LayoutBoxInfo> listBufferData = TypedUnsafe.MallocSizedTempList<LayoutBoxInfo>(maxGroupCount * 3, Allocator.TempJob);

            DataList<LayoutBoxInfo> controlledBuffer = new DataList<LayoutBoxInfo>(32, Allocator.Temp); // todo -- could count this too 

            LayoutListBuffer listBuffer = new LayoutListBuffer() {
                controlledList = controlledBuffer,
                solveList = new TempList<LayoutBoxInfo>(listBufferData.array + (maxGroupCount * 0)),
                layoutList = new TempList<LayoutBoxInfo>(listBufferData.array + (maxGroupCount * 1)),
                contentSizeList = new TempList<LayoutBoxInfo>(listBufferData.array + (maxGroupCount * 2)),
            };

            int iterationCount = 0;

            const int k_MaxIterations = 100;

            while (iterationCount < k_MaxIterations) {
                iterationCount++;

                for (int d = layoutTree->depthLevels.size - 1; d >= 0; d--) {

                    ref LayoutLevelBoxes boxesForLevel = ref levelBoxes.Get(d);

                    if (boxesForLevel.remainingBoxCount == 0) {
                        continue;
                    }

                    if (boxesForLevel.flexHorizontal.size > 0) {
                        boxesForLevel.flexHorizontal = FlexLayout.LayoutBottomUp(ref listBuffer, ref horizontalAxis, ref verticalAxis, boxesForLevel.flexHorizontal);
                    }

                    if (boxesForLevel.flexVertical.size > 0) {
                        boxesForLevel.flexVertical = FlexLayout.LayoutBottomUp(ref listBuffer, ref verticalAxis, ref horizontalAxis, boxesForLevel.flexVertical);
                    }

                    if (boxesForLevel.stack.size > 0) {
                        boxesForLevel.stack = StackLayout.LayoutBottomUp(ref listBuffer, ref horizontalAxis, ref verticalAxis, boxesForLevel.stack);
                    }

                    if (boxesForLevel.textHorizontal.size > 0) {
                        boxesForLevel.textHorizontal = TextHorizontalQuadPassLayout.LayoutBottomUp(ref listBuffer, ref horizontalAxis, ref verticalAxis, boxesForLevel.textHorizontal, ref textDataTable[0]);
                    }

                    if (boxesForLevel.gridHorizontal.size > 0) {
                        boxesForLevel.gridHorizontal = GridLayout.Layout(ref listBuffer, ref horizontalAxis, ref verticalAxis, ref perFrameLayoutBoxes->gridHorizontalMainAxis, ref perFrameLayoutBoxes->gridHorizontalCrossAxis, boxesForLevel.gridHorizontal);
                    }

                    if (boxesForLevel.gridVertical.size > 0) {
                        boxesForLevel.gridVertical = GridLayout.Layout(ref listBuffer, ref verticalAxis, ref horizontalAxis, ref perFrameLayoutBoxes->gridVerticalMainAxis, ref perFrameLayoutBoxes->gridVerticalCrossAxis, boxesForLevel.gridVertical);
                    }

                    if (boxesForLevel.ignored.size > 0) {
                        boxesForLevel.ignored = IgnoredLayout.Layout(ref listBuffer, ref horizontalAxis, ref verticalAxis, boxesForLevel.ignored);
                    }

                    boxesForLevel.UpdateRemaining();

                }

                int remainingCount = 0;

                for (int i = 0; i < levelBoxes.size; i++) {
                    remainingCount += levelBoxes[i].remainingBoxCount;
                }

                if (remainingCount == 0) {
                    break;
                }

                RangeInt rootRange = layoutTree->depthLevels[0].nodeRange;

                for (int i = rootRange.start; i < rootRange.end; i++) {

                    ReadyFlags horizontalFlag = horizontalAxis.readyFlags[i];
                    ReadyFlags verticalFlag = verticalAxis.readyFlags[i];

                    if ((horizontalFlag & ReadyFlags.BaseSizeResolved) != 0 && (horizontalFlag & ReadyFlags.FinalSizeResolved) == 0) {
                        horizontalAxis.readyFlags[i] |= ReadyFlags.FinalSizeResolved;
                        horizontalAxis.outputSizes[i] = horizontalAxis.sizes[i].baseSize;
                    }

                    if ((verticalFlag & ReadyFlags.BaseSizeResolved) != 0 && (verticalFlag & ReadyFlags.FinalSizeResolved) == 0) {
                        verticalAxis.readyFlags[i] |= ReadyFlags.FinalSizeResolved;
                        verticalAxis.outputSizes[i] = verticalAxis.sizes[i].baseSize;
                    }
                }

                for (int d = 0; d < layoutTree->depthLevels.size; d++) {

                    ref LayoutLevelBoxes boxesForLevel = ref levelBoxes.Get(d);

                    if (boxesForLevel.remainingBoxCount == 0) {
                        continue;
                    }

                    if (boxesForLevel.flexHorizontal.size > 0) {
                        boxesForLevel.flexHorizontal = FlexLayout.LayoutTopDown(ref listBuffer, ref horizontalAxis, ref verticalAxis, boxesForLevel.flexHorizontal);
                    }

                    if (boxesForLevel.flexVertical.size > 0) {
                        boxesForLevel.flexVertical = FlexLayout.LayoutTopDown(ref listBuffer, ref verticalAxis, ref horizontalAxis, boxesForLevel.flexVertical);
                    }

                    if (boxesForLevel.stack.size > 0) {
                        boxesForLevel.stack = StackLayout.LayoutTopDown(ref listBuffer, ref horizontalAxis, ref verticalAxis, boxesForLevel.stack);
                    }

                    if (boxesForLevel.textHorizontal.size > 0) {
                        boxesForLevel.textHorizontal = TextHorizontalQuadPassLayout.LayoutTopDown(ref listBuffer, ref horizontalAxis, ref verticalAxis, boxesForLevel.textHorizontal, ref textDataTable[0]);
                    }

                    if (boxesForLevel.gridHorizontal.size > 0) {
                        boxesForLevel.gridHorizontal = GridLayout.Layout(ref listBuffer, ref horizontalAxis, ref verticalAxis, ref perFrameLayoutBoxes->gridHorizontalMainAxis, ref perFrameLayoutBoxes->gridHorizontalCrossAxis, boxesForLevel.gridHorizontal);
                    }

                    if (boxesForLevel.gridVertical.size > 0) {
                        boxesForLevel.gridVertical = GridLayout.Layout(ref listBuffer, ref verticalAxis, ref horizontalAxis, ref perFrameLayoutBoxes->gridVerticalMainAxis, ref perFrameLayoutBoxes->gridVerticalCrossAxis, boxesForLevel.gridVertical);
                    }

                    if (boxesForLevel.ignored.size > 0) {
                        boxesForLevel.ignored = IgnoredLayout.Layout(ref listBuffer, ref horizontalAxis, ref verticalAxis, boxesForLevel.ignored);
                    }

                    boxesForLevel.UpdateRemaining();

                }

                remainingCount = 0;

                for (int i = 0; i < levelBoxes.size; i++) {
                    remainingCount += levelBoxes[i].remainingBoxCount;
                }

                if (remainingCount == 0) {
                    break;
                }

            }

            if (iterationCount >= k_MaxIterations) {
                Debug.Log("Infinite layout loop");
            }
            else {
                // Debug.Log(iterationCount + " iterations");
            }

            for (int i = 0; i < elementCount; i++) {
                perFrameLayoutOutput->borders.array[i] = new OffsetRect() {
                    top = borderTop.array[i],
                    bottom = borderBottom.array[i],
                    left = borderLeft.array[i],
                    right = borderRight.array[i]
                };
            }

            for (int i = 0; i < elementCount; i++) {
                perFrameLayoutOutput->paddings.array[i] = new OffsetRect() {
                    top = paddingTop.array[i].value,
                    bottom = paddingBottom.array[i].value,
                    left = paddingLeft.array[i].value,
                    right = paddingRight.array[i].value
                };
            }

            CopyToOutput(elementCount, horizontalAxis, verticalAxis);

            controlledBuffer.Dispose();
        }

        private void CopyToOutput(int elementCount, in LayoutAxis horizontal, in LayoutAxis vertical) {
            if (!BurstCompiler.IsEnabled) {

                ReadyFlags flags = ReadyFlags.LayoutComplete;

                for (int i = 0; i < elementCount; i++) {
                    flags &= horizontal.readyFlags[i] & vertical.readyFlags[i];
                }

                flags &= ReadyFlags.LayoutComplete;

                if (flags != ReadyFlags.LayoutComplete) {
                    Debug.Log("Layout failed");
                }

            }

            // todo -- I need elementId -> layoutIndex to report these sizes to the user

            for (int i = 0; i < elementCount; i++) {
                perFrameLayoutOutput->sizes.array[i].width = horizontal.outputSizes[i];
                perFrameLayoutOutput->sizes.array[i].height = vertical.outputSizes[i];
            }

            for (int i = 0; i < elementCount; i++) {
                perFrameLayoutOutput->localPositions.array[i].x = horizontal.outputPositions[i];
                perFrameLayoutOutput->localPositions.array[i].y = vertical.outputPositions[i];
            }

        }

        private void HandleRoots(ref LayoutAxis horizontal, ref LayoutAxis vertical) {
            LayoutDepthLevel rootLevel = layoutTree->depthLevels[0];
            RangeInt rootRange = rootLevel.nodeRange;

            const ReadyFlags done = (ReadyFlags.AspectReady | ReadyFlags.ParentReady | ReadyFlags.StretchReady | ReadyFlags.BaseSizeResolved | ReadyFlags.FinalSizeResolved);

            int viewIdx = 0;

            // todo -- I'm assuming viewIdx matches root index, verify this!

            for (int i = rootRange.start; i < rootRange.end; i++) {

                horizontal.readyFlags.array[i] = done;
                vertical.readyFlags.array[i] = done;
                horizontal.outputSizes[i] = viewRects[viewIdx].width;
                vertical.outputSizes[i] = viewRects[viewIdx].height;
                viewIdx++;

            }

        }

        private void InitializeReadyFlags(ref LayoutAxis horizontal, ref LayoutAxis vertical) {

            ClassifyPrefSizeReadyFlags(perFrameLayoutData->solvedWidths, horizontal.readyFlags, horizontal.animatedSizes);
            ClassifyPrefSizeReadyFlags(perFrameLayoutData->solvedHeights, vertical.readyFlags, vertical.animatedSizes);

            ClassifyConstraintReadySize(perFrameLayoutData->minWidths, horizontal.readyFlags);
            ClassifyConstraintReadySize(perFrameLayoutData->maxWidths, horizontal.readyFlags);
            ClassifyConstraintReadySize(perFrameLayoutData->minHeights, vertical.readyFlags);
            ClassifyConstraintReadySize(perFrameLayoutData->maxHeights, vertical.readyFlags);

            HandleRoots(ref horizontal, ref vertical);

            for (int d = 0; d < layoutTree->depthLevels.size; d++) {

                LayoutDepthLevel depthLevel = layoutTree->depthLevels[d];

                RangeInt range = depthLevel.nodeRange;
                range.length += depthLevel.ignoredRange.length; // no special cases that I can think of for ignored elements for flag setup, just extend the range to encompass ignored

                if (d != 0) {

                    for (int i = range.start; i < range.end; i++) {

                        int parentIndex = layoutTree->nodeList[i].parentIndex;

                        if ((horizontal.readyFlags[parentIndex] & ReadyFlags.FinalSizeResolved) != 0) {
                            horizontal.readyFlags[i] |= ReadyFlags.ParentReady;
                        }

                        if ((vertical.readyFlags[parentIndex] & ReadyFlags.FinalSizeResolved) != 0) {
                            vertical.readyFlags[i] |= ReadyFlags.ParentReady;
                        }

                    }

                }

                LayoutContentSizes contentSizes = default;

                for (int i = range.start; i < range.end; i++) {

                    ref ReadyFlags horizontalFlags = ref horizontal.readyFlags.array[i];

                    if ((horizontalFlags & LayoutUtil.k_FullyResolvable) == LayoutUtil.k_FullyResolvable) {

                        horizontalFlags |= ReadyFlags.FinalSizeResolved | ReadyFlags.BaseSizeResolved;

                        int parentIndex = layoutTree->nodeList[i].parentIndex;

                        float parentSize = 0;
                        float parentPaddingBorder = 0;

                        if (parentIndex >= 0) {
                            // todo -- find a way to avoid this i guess
                            parentSize = horizontal.outputSizes[parentIndex];
                            parentPaddingBorder = horizontal.paddingBorderStart[parentIndex] + horizontal.paddingBorderEnd[parentIndex];
                        }

                        SolvedSize prefSize = horizontal.solvedPrefSizes[i];
                        SolvedConstraint minSize = horizontal.solvedMinSizes[i];
                        SolvedConstraint maxSize = horizontal.solvedMaxSizes[i];

                        BaseSize baseSize = LayoutUtil.SolveBaseSize(horizontal.animatedSizes, prefSize, contentSizes, parentSize, parentPaddingBorder);
                        float minClamp = LayoutUtil.SolveConstraintSize(minSize, contentSizes, parentSize, parentPaddingBorder);
                        float maxClamp = LayoutUtil.SolveConstraintSize(maxSize, contentSizes, parentSize, parentPaddingBorder);

                        if (baseSize.value > maxClamp) baseSize.value = maxClamp;
                        if (baseSize.value < minClamp) baseSize.value = minClamp;

                        ref LayoutSizes layoutSize = ref horizontal.sizes.array[i];

                        layoutSize.min = minClamp;
                        layoutSize.max = maxClamp;
                        layoutSize.baseSize = baseSize.value;
                        layoutSize.stretch = 0;

                        horizontal.outputSizes[i] = baseSize.value;
                    }

                }

                for (int i = range.start; i < range.end; i++) {

                    ref ReadyFlags verticalFlags = ref vertical.readyFlags.array[i];

                    if ((verticalFlags & LayoutUtil.k_FullyResolvable) == LayoutUtil.k_FullyResolvable) {

                        verticalFlags |= ReadyFlags.FinalSizeResolved | ReadyFlags.BaseSizeResolved;

                        int parentIndex = layoutTree->nodeList[i].parentIndex;

                        float parentSize = 0;
                        float parentPaddingBorder = 0;

                        if (parentIndex >= 0) {
                            // todo -- find a way to avoid this i guess, overallocate by 1 slot and place the ptr at 0 but allocate from -1
                            parentSize = vertical.outputSizes[parentIndex];
                            parentPaddingBorder = vertical.paddingBorderStart[parentIndex] + vertical.paddingBorderEnd[parentIndex];
                        }

                        SolvedSize prefSize = vertical.solvedPrefSizes[i];
                        SolvedConstraint minSize = vertical.solvedMinSizes[i];
                        SolvedConstraint maxSize = vertical.solvedMaxSizes[i];

                        BaseSize baseSize = LayoutUtil.SolveBaseSize(vertical.animatedSizes, prefSize, contentSizes, parentSize, parentPaddingBorder);
                        float minClamp = LayoutUtil.SolveConstraintSize(minSize, contentSizes, parentSize, parentPaddingBorder);
                        float maxClamp = LayoutUtil.SolveConstraintSize(maxSize, contentSizes, parentSize, parentPaddingBorder);

                        if (baseSize.value > maxClamp) baseSize.value = maxClamp;
                        if (baseSize.value < minClamp) baseSize.value = minClamp;

                        ref LayoutSizes layoutSize = ref vertical.sizes.array[i];

                        layoutSize.min = minClamp;
                        layoutSize.max = maxClamp;
                        layoutSize.baseSize = baseSize.value;
                        layoutSize.stretch = 0;

                        vertical.outputSizes[i] = baseSize.value;

                    }

                }

            }

        }

        // since I know the set of data I need available for an element to report its size, I think I don't need to store a value, just a set of flags to solve 
        // once all flags are solved we can compute the size. not really even sure I need to store the data at all, can just compute on the fly once available  
        private void ClassifyPrefSizeReadyFlags(SolvedSize* solvedSizes, CheckedArray<ReadyFlags> flags, in DataList<InterpolatedStyleValue> animatedSizes) {

            for (int i = 0; i < layoutTree->elementCount; i++) {

                SolvedSize prefSize = solvedSizes[i];

                ReadyFlags flag = LayoutUtil.k_Pristine;

                switch (prefSize.unit) {
                    default:
                    case SolvedSizeUnit.Pixel:
                        break;

                    case SolvedSizeUnit.Controlled: // may be resolvable in this pass in some cases, depends on the other axis & stretching   
                        flag &= ~ReadyFlags.AspectReady;
                        break;

                    case SolvedSizeUnit.MaxChild:
                    case SolvedSizeUnit.MinChild:
                    case SolvedSizeUnit.Content:
                        flag &= ~ReadyFlags.ContentSizeResolved;
                        break;

                    case SolvedSizeUnit.Stretch:
                        flag &= ~ReadyFlags.StretchReady;
                        break;

                    case SolvedSizeUnit.StretchContent:
                        flag &= ~(ReadyFlags.StretchReady | ReadyFlags.ContentSizeResolved);
                        break;

                    case SolvedSizeUnit.FitContent:
                        flag &= ~(ReadyFlags.StretchReady | ReadyFlags.ContentSizeResolved);
                        break;

                    case SolvedSizeUnit.Percent:
                    case SolvedSizeUnit.ParentSize:
                        flag &= ~(ReadyFlags.ParentReady);
                        break;

                    case SolvedSizeUnit.Animation:

                        InterpolatedMeasurement size = animatedSizes[(int) prefSize.value].measurement;
                        ClassifyAnimated(ref flag, size.nextValue, size.nextUnit_solved);
                        ClassifyAnimated(ref flag, size.prevValue, size.prevUnit_solved);

                        break;
                }

                flags.array[i] = flag;

            }

            for (int i = 0; i < layoutTree->elementCount; i++) {

                if (layoutTree->nodeList[i].childRange.length == 0 && layoutTree->nodeList[i].layoutBoxType != LayoutBoxType.TextHorizontal) {
                    flags.array[i] |= ReadyFlags.ContentReady | ReadyFlags.ContentSizeResolved;
                }

            }

        }

        private static void ClassifyAnimated(ref ReadyFlags flag, float value, SolvedSizeUnit unit) {
            switch (unit) {
                default:
                case SolvedSizeUnit.Pixel:
                    break;

                case SolvedSizeUnit.Controlled: // may be resolvable in this pass in some cases, depends on the other axis & stretching   
                    flag &= ~ReadyFlags.AspectReady;
                    break;

                case SolvedSizeUnit.MaxChild:
                case SolvedSizeUnit.MinChild:
                case SolvedSizeUnit.Content:
                    flag &= ~ReadyFlags.ContentSizeResolved;
                    break;

                case SolvedSizeUnit.Stretch:
                    flag &= ~ReadyFlags.StretchReady;
                    break;

                case SolvedSizeUnit.StretchContent:
                    flag &= ~(ReadyFlags.StretchReady | ReadyFlags.ContentSizeResolved);
                    break;

                case SolvedSizeUnit.FitContent:
                    flag &= ~(ReadyFlags.StretchReady | ReadyFlags.ContentSizeResolved);
                    break;

                case SolvedSizeUnit.Percent:
                case SolvedSizeUnit.ParentSize:
                    flag &= ~(ReadyFlags.ParentReady);
                    break;
            }
        }

        private static void ClassifyConstraintReadySize(SolvedConstraint* solvedConstraints, CheckedArray<ReadyFlags> flags) {
            for (int i = 0; i < flags.size; i++) {

                // does it matter if we do cross before main?
                // we want to also report min/max clamped size if possible 

                SolvedConstraint size = solvedConstraints[i];

                ReadyFlags flag = flags[i];

                switch (size.unit) {

                    case SolvedConstraintUnit.MaxChild:
                    case SolvedConstraintUnit.MinChild:
                    case SolvedConstraintUnit.Content:
                        flag &= ~ReadyFlags.ContentSizeResolved;
                        break;

                    case SolvedConstraintUnit.Percent:
                    case SolvedConstraintUnit.ParentSize:
                        flag &= ~ReadyFlags.ParentReady;
                        break;
                }

                flags[i] = flag;
            }
        }

        private static void GatherBoxes(LayoutTree* layoutTree, CheckedArray<LayoutLevelBoxes> output, CheckedArray<LayoutTypeInfoList> layoutTypeLists, LayoutBoxInfo* layoutBuffer) {

            LayoutBoxInfo* allocPtr = layoutBuffer;

            for (int d = 0; d < layoutTree->depthLevels.size; d++) {

                LayoutDepthLevel level = layoutTree->depthLevels[d];

                int start = level.nodeRange.start;
                int end = level.nodeRange.end;
                end += level.ignoredRange.length;

                layoutTypeLists.MemClear();

                for (int i = start; i < end; i++) {
                    int idx = (int) layoutTree->nodeList[i].layoutBoxType;
                    ref LayoutTypeInfoList list = ref layoutTypeLists.Get(idx);
                    list.size++;
                }

                for (int i = 0; i < layoutTypeLists.size; i++) {
                    ref LayoutTypeInfoList list = ref layoutTypeLists.Get(i);
                    list.array = allocPtr;
                    allocPtr += list.size;
                    list.size = 0; // will get reset in next loop
                }

                for (int i = start; i < end; i++) {

                    ref LayoutTypeInfoList list = ref layoutTypeLists.Get((int) layoutTree->nodeList[i].layoutBoxType);

                    list.array[list.size++] = new LayoutBoxInfo() {
                        boxIndex = i,
                        parentIndex = layoutTree->nodeList[i].parentIndex,
                        childRange = layoutTree->nodeList[i].childRange,
                    };

                }

                if (d != 0) {
                    CheckedArray<LayoutBoxInfo> ignored = new CheckedArray<LayoutBoxInfo>(allocPtr, level.ignoredRange.length);
                    allocPtr += level.ignoredRange.length;
                    int idx = 0;
                    for (int i = level.ignoredRange.start; i < level.ignoredRange.end; i++) {
                        ignored.array[idx++] = new LayoutBoxInfo() {
                            boxIndex = i,
                            parentIndex = layoutTree->nodeList[i].parentIndex,
                        };
                    }

                    output.array[d - 1].ignored = ignored;
                }

                output[d] = new LayoutLevelBoxes() {
                    stack = layoutTypeLists[(int) LayoutBoxType.Stack].ToCheckedArray(),

                    textHorizontal = layoutTypeLists[(int) LayoutBoxType.TextHorizontal].ToCheckedArray(),

                    flexHorizontal = layoutTypeLists[(int) LayoutBoxType.FlexHorizontal].ToCheckedArray(),
                    flexVertical = layoutTypeLists[(int) LayoutBoxType.FlexVertical].ToCheckedArray(),

                    gridHorizontal = layoutTypeLists[(int) LayoutBoxType.GridHorizontal].ToCheckedArray(),
                    gridVertical = layoutTypeLists[(int) LayoutBoxType.GridVertical].ToCheckedArray(),

                    // ignored boxes get injected into the PARENT level, not the box itself. they are pseudo parents of the ignored elements

                };

                // must be .array since we're dealing with value types 

            }

            for (int d = 0; d < layoutTree->depthLevels.size; d++) {
                output.array[d].UpdateRemaining();
            }

        }

        private static void UnsetIntrinsicGridContentSizeFlags(CheckedArray<GridAxisInfo> gridInfos, ref LayoutAxis axis) {

            for (int i = 0; i < gridInfos.size; i++) {
                int boxId = gridInfos[i].boxId;
                if (gridInfos[i].hasIntrinsicSizes) {
                    axis.readyFlags[boxId] &= ~ReadyFlags.ContentSizeResolved;
                }
            }
        }

        private struct LayoutLevelBoxes {

            public CheckedArray<LayoutBoxInfo> stack;
            public CheckedArray<LayoutBoxInfo> flexHorizontal;
            public CheckedArray<LayoutBoxInfo> flexVertical;
            public CheckedArray<LayoutBoxInfo> textHorizontal;
            public CheckedArray<LayoutBoxInfo> gridHorizontal;
            public CheckedArray<LayoutBoxInfo> gridVertical;
            public CheckedArray<LayoutBoxInfo> ignored;

            // note -- when adding a layout type be sure to update UpdateRemaining() and GetMaxGroupCount()

            public int remainingBoxCount;

            public void UpdateRemaining() {
                remainingBoxCount = flexHorizontal.size +
                                    flexVertical.size +
                                    stack.size +
                                    textHorizontal.size +
                                    gridHorizontal.size +
                                    gridVertical.size +
                                    ignored.size;
            }

            public int GetMaxGroupCount() {
                int max = 0;

                if (stack.size > max) max = stack.size;
                if (flexHorizontal.size > max) max = flexHorizontal.size;
                if (flexVertical.size > max) max = flexVertical.size;
                if (textHorizontal.size > max) max = textHorizontal.size;
                if (gridHorizontal.size > max) max = gridHorizontal.size;
                if (gridVertical.size > max) max = gridVertical.size;
                if (ignored.size > max) max = ignored.size;

                return max;
            }

        }

    }

}