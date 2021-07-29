using UIForia.Util.Unsafe;
using Unity.Mathematics;

namespace UIForia.Layout {

    internal static unsafe class LayoutUtil {

        public const ReadyFlags k_ReadyForLayout = ReadyFlags.ContentReady | ReadyFlags.FinalSizeResolved;
        public const ReadyFlags k_BaseSizeResolvable = ReadyFlags.AspectReady | ReadyFlags.ContentSizeResolved | ReadyFlags.ParentReady;
        public const ReadyFlags k_FullyResolvable = ReadyFlags.AspectReady | ReadyFlags.ContentSizeResolved | ReadyFlags.ParentReady | ReadyFlags.StretchReady;
        public const ReadyFlags k_Pristine = ReadyFlags.AspectReady | ReadyFlags.ParentReady | ReadyFlags.StretchReady | ReadyFlags.ContentSizeResolved;

        public static void SolveSizes(TempList<LayoutBoxInfo> solveList, ref LayoutAxis axis) {

            for (int i = 0; i < solveList.size; i++) {
                LayoutBoxInfo box = solveList.array[i];

                int boxIndex = box.boxIndex;
                int parentIndex = box.parentIndex;

                float parentSize = 0;
                float parentPaddingBorder = 0;

                LayoutContentSizes contentSizes = axis.contentSizes[box.boxIndex];

                if (parentIndex >= 0) {
                    // todo -- find a way to avoid this i guess
                    parentSize = axis.outputSizes[parentIndex];
                    parentPaddingBorder = axis.paddingBorderStart[parentIndex] + axis.paddingBorderEnd[parentIndex];
                }

                SolvedSize prefSize = axis.solvedPrefSizes[boxIndex];
                SolvedConstraint minSize = axis.solvedMinSizes[boxIndex];
                SolvedConstraint maxSize = axis.solvedMaxSizes[boxIndex];

                BaseSize baseSize = SolveBaseSize(axis.animatedSizes, prefSize, contentSizes, parentSize, parentPaddingBorder);
                float minClamp = SolveConstraintSize(minSize, contentSizes, parentSize, parentPaddingBorder);
                float maxClamp = SolveConstraintSize(maxSize, contentSizes, parentSize, parentPaddingBorder);

                if (baseSize.value > maxClamp) baseSize.value = maxClamp;
                if (baseSize.value < minClamp) baseSize.value = minClamp;

                ref LayoutSizes layoutSize = ref axis.sizes.array[boxIndex];

                layoutSize.min = minClamp;
                layoutSize.max = maxClamp;
                layoutSize.baseSize = baseSize.value;
                layoutSize.stretch = baseSize.stretch;

                // this will be the case for fully fixed elements w/o stretch
                if ((axis.readyFlags[boxIndex] & ReadyFlags.FinalSizeResolved) != 0) {
                    axis.outputSizes[boxIndex] = layoutSize.baseSize;
                }

            }

        }

        public static CheckedArray<LayoutBoxInfo> FilterCompletedBoxes(CheckedArray<LayoutBoxInfo> boxes, CheckedArray<ReadyFlags> flags0, CheckedArray<ReadyFlags> flags1) {
            int size = boxes.size;
            LayoutBoxInfo* array = boxes.array;

            for (int i = 0; i < size; i++) {
                LayoutBoxInfo box = array[i];
                ReadyFlags flag = flags0[box.boxIndex] & flags1[box.boxIndex];
                if ((flag & ReadyFlags.LayoutComplete) == ReadyFlags.LayoutComplete) {
                    array[i--] = array[--size];
                }
            }

            return new CheckedArray<LayoutBoxInfo>(array, size);
        }

        public static BaseSize SolveBaseSize(DataList<InterpolatedStyleValue> animatedValues,  SolvedSize pref, in LayoutContentSizes sizes, float parentSize, float parentPaddingBorder) {
            switch (pref.unit) {

                default:
                case SolvedSizeUnit.Pixel:
                    return new BaseSize(pref.value, 0);

                case SolvedSizeUnit.Stretch:
                    return new BaseSize(0, (uint) pref.value);

                case SolvedSizeUnit.FillRemaining:
                    return new BaseSize(0, 0); // looked up from solved size directly 
                
                case SolvedSizeUnit.StretchContent:
                case SolvedSizeUnit.FitContent:
                    return new BaseSize(sizes.contentSize, (uint) pref.value);

                case SolvedSizeUnit.Content:
                    return new BaseSize((sizes.contentSize * pref.value), 0);

                case SolvedSizeUnit.MaxChild:
                    return new BaseSize((sizes.maxChildSize * pref.value), 0);

                case SolvedSizeUnit.MinChild:
                    return new BaseSize((sizes.minChildSize * pref.value), 0);

                case SolvedSizeUnit.Controlled:
                    return new BaseSize(sizes.contentSize / pref.value, 0); // controlled read from content size  

                case SolvedSizeUnit.ParentSize:
                    return new BaseSize(parentSize * (pref.value * 0.01f), 0);

                case SolvedSizeUnit.Percent:
                    return new BaseSize(((parentSize - parentPaddingBorder) * (pref.value * 0.01f)), 0);

                case SolvedSizeUnit.Animation: {
                    int index = (int) pref.value;
                    ref InterpolatedMeasurement styleVal = ref animatedValues[index].measurement;
                    
                    SolvedSize nextSize = new SolvedSize(styleVal.nextValue, styleVal.nextUnit_solved);
                    SolvedSize prevSize = new SolvedSize(styleVal.prevValue, styleVal.prevUnit_solved);

                    BaseSize next = SolveBaseSize(default, nextSize, sizes, parentSize, parentPaddingBorder);
                    BaseSize prev = SolveBaseSize(default, prevSize, sizes, parentSize, parentPaddingBorder);
                    return new BaseSize(math.lerp(prev.value, next.value, styleVal.t), (uint)math.lerp(prev.stretch, next.stretch, styleVal.t));
                }
            }
        }

        public static float SolveConstraintSize(SolvedConstraint val, in LayoutContentSizes sizes, float parentSize, float parentPaddingBorder) {
            switch (val.unit) {

                case SolvedConstraintUnit.Animation:
                    return 0;

                default:
                case SolvedConstraintUnit.Pixel:
                    return val.value;

                case SolvedConstraintUnit.Content:
                    return (val.value * sizes.contentSize);

                case SolvedConstraintUnit.MaxChild:
                    return (val.value * sizes.maxChildSize);

                case SolvedConstraintUnit.MinChild:
                    return (val.value * sizes.minChildSize);

                case SolvedConstraintUnit.Percent:
                    return (parentSize - parentPaddingBorder) * (val.value * 0.01f);

                case SolvedConstraintUnit.ParentSize:
                    return parentSize * (val.value * 0.01f);

            }
        }

        public static void UpdateAxisFlags(ref LayoutListBuffer listBuffer, ref LayoutAxis axis, ref LayoutAxis opposite, CheckedArray<LayoutBoxInfo> boxes, bool isCrossAxis) {

            ref TempList<LayoutBoxInfo> solveList = ref listBuffer.solveList;
            ref TempList<LayoutBoxInfo> layoutList = ref listBuffer.layoutList;
            ref TempList<LayoutBoxInfo> contentSizeList = ref listBuffer.contentSizeList;

            solveList.size = 0;
            layoutList.size = 0;
            contentSizeList.size = 0;

            // todo -- maybe we can wrap this in a 'level has aspect' or some other meta data?

            UpdateAxisFlagsControlled(ref listBuffer, ref axis, opposite, boxes, isCrossAxis);

            for (int i = 0; i < boxes.size; i++) {

                LayoutBoxInfo box = boxes.array[i];
                ReadyFlags readyFlags = axis.readyFlags[box.boxIndex];

                if ((readyFlags & ReadyFlags.LayoutComplete) != 0) {
                    continue;
                }

                if ((readyFlags & ReadyFlags.ContentReady) == 0) {
                    // see if its ready now

                    bool hasFinalSize = (readyFlags & ReadyFlags.FinalSizeResolved) != 0;

                    if (hasFinalSize) {

                        bool childrenAllHaveBaseSize = true;

                        for (int childIter = box.childRange.start; childIter < box.childRange.end; childIter++) {

                            ReadyFlags c = axis.readyFlags[childIter];

                            if ((c & ReadyFlags.BaseSizeResolved) != 0) {
                                continue;
                            }

                            axis.readyFlags[childIter] |= ReadyFlags.ParentReady;

                            // content & aspect are good to go, just missing parent size (which we know we have)
                            if ((c & (ReadyFlags.AspectReady | ReadyFlags.ContentReady)) == (ReadyFlags.AspectReady | ReadyFlags.ContentReady)) {
                                LayoutBoxInfo child = new LayoutBoxInfo(childIter, box.boxIndex);
                                SolveSizes(new TempList<LayoutBoxInfo>(&child, 1), ref axis);
                            }
                            else {
                                childrenAllHaveBaseSize = false;
                            }
                        }

                        if (childrenAllHaveBaseSize) {

                            if ((readyFlags & ReadyFlags.ContentSizeResolved) == 0) {
                                contentSizeList.array[contentSizeList.size++] = box;
                            }

                            readyFlags |= ReadyFlags.ContentReady | ReadyFlags.ContentSizeResolved;
                        }

                    }
                    else {

                        ReadyFlags childFlags = ReadyFlags.BaseSizeResolved;

                        for (int childIter = box.childRange.start; childIter < box.childRange.end; childIter++) {
                            childFlags &= axis.readyFlags[childIter];
                        }

                        childFlags &= ReadyFlags.BaseSizeResolved;

                        // if children are ready except for parent size -> can we just put them in the solve list and assume they take parent size?

                        if (childFlags == ReadyFlags.BaseSizeResolved) {
                            // all children are good to go, measure content if we need it

                            if ((readyFlags & ReadyFlags.ContentSizeResolved) == 0) {
                                contentSizeList.array[contentSizeList.size++] = box;
                            }

                            readyFlags |= ReadyFlags.ContentReady | ReadyFlags.ContentSizeResolved;

                        }

                    }

                }

                // this element is ready but all content hasn't been ensured to be resolved yet
                
                if (((readyFlags & ReadyFlags.BaseSizeResolved) == 0) && (readyFlags & k_BaseSizeResolvable) == k_BaseSizeResolvable) {

                    readyFlags |= ReadyFlags.BaseSizeResolved;
                    solveList.array[solveList.size++] = box;

                    // if not waiting for parent stretch or parent size -> we're done

                    if ((readyFlags & ReadyFlags.StretchReady) != 0) {
                        readyFlags |= ReadyFlags.FinalSizeResolved;
                    }

                }

                if ((readyFlags & k_ReadyForLayout) == k_ReadyForLayout) {
                    if (box.childRange.length == 0) {
                        readyFlags |= ReadyFlags.LayoutComplete;
                    }
                    else {
                        layoutList.array[layoutList.size++] = box;
                    }
                }

                axis.readyFlags[box.boxIndex] = readyFlags;

            }

        }

        private static void UpdateAxisFlagsControlled(ref LayoutListBuffer listBuffer, ref LayoutAxis axis, LayoutAxis opposite, CheckedArray<LayoutBoxInfo> boxes, bool isCrossAxis) {
            if (isCrossAxis) {

                for (int i = 0; i < boxes.size; i++) {
                    LayoutBoxInfo box = boxes.array[i];
                    ReadyFlags readyFlags = axis.readyFlags[box.boxIndex];
                    
                    if ((readyFlags & ReadyFlags.AspectReady) != 0) {
                        continue;
                    }
                    
                    if ((opposite.readyFlags[box.boxIndex] & ReadyFlags.FinalSizeResolved) == 0) {
                        listBuffer.controlledList.Add(box);
                    }
                    else {
                        // solve immediately

                        float oppositeSize = opposite.outputSizes[box.boxIndex];
                        float ratio = axis.solvedPrefSizes[box.boxIndex].value;

                        float finalSize = axis.isVertical
                            ? oppositeSize / ratio
                            : oppositeSize * ratio;

                        axis.readyFlags[box.boxIndex] |= ReadyFlags.AspectReady | ReadyFlags.FinalSizeResolved | ReadyFlags.BaseSizeResolved;

                        axis.sizes[box.boxIndex] = new LayoutSizes() {
                            max = float.MaxValue,
                            min = finalSize,
                            baseSize = finalSize,
                            stretch = 0
                        };
                    }
                }

            }
            else {

                for (int i = 0; i < boxes.size; i++) {

                    LayoutBoxInfo box = boxes.array[i];
                    ReadyFlags readyFlags = axis.readyFlags[box.boxIndex];

                    if ((readyFlags & ReadyFlags.AspectReady) != 0 || (opposite.readyFlags[box.boxIndex] & ReadyFlags.FinalSizeResolved) == 0) {
                        continue;
                    }

                    // solve immediately

                    float oppositeSize = opposite.outputSizes[box.boxIndex];
                    float ratio = axis.solvedPrefSizes[box.boxIndex].value;

                    float finalSize = axis.isVertical
                        ? oppositeSize / ratio
                        : oppositeSize * ratio;

                    axis.readyFlags[box.boxIndex] |= ReadyFlags.AspectReady | ReadyFlags.FinalSizeResolved | ReadyFlags.BaseSizeResolved;

                    axis.sizes[box.boxIndex] = new LayoutSizes() {
                        max = float.MaxValue,
                        min = finalSize,
                        baseSize = finalSize,
                        stretch = 0
                    };

                }
            }
        }

        public static void UpdateAxisFlagsDeferred(ref LayoutListBuffer listBuffer, ref LayoutAxis mainAxis, ref LayoutAxis crossAxis, CheckedArray<LayoutBoxInfo> boxes) {

            ref TempList<LayoutBoxInfo> layoutList = ref listBuffer.layoutList;

            layoutList.size = 0;

            for (int i = 0; i < boxes.size; i++) {

                LayoutBoxInfo box = boxes.array[i];
                ReadyFlags mainAxisFlags = mainAxis.readyFlags[box.boxIndex];
                ReadyFlags readyFlags = crossAxis.readyFlags[box.boxIndex];

                if ((mainAxisFlags & ReadyFlags.FinalSizeResolved) == 0) {
                    continue;
                }

                readyFlags |= ReadyFlags.AspectReady | ReadyFlags.BaseSizeResolved | ReadyFlags.FinalSizeResolved;

                // solving in this case is just applying aspect
                float finalSize = mainAxis.outputSizes[box.boxIndex] * crossAxis.solvedPrefSizes[box.boxIndex].value;

                crossAxis.outputSizes[box.boxIndex] = finalSize;

                crossAxis.sizes[box.boxIndex] = new LayoutSizes() {
                    baseSize = finalSize,
                    max = float.MaxValue,
                    min = finalSize,
                    stretch = 0,
                };

                if ((readyFlags & ReadyFlags.ContentReady) == 0) {
                    bool childrenAllHaveBaseSize = true;

                    for (int childIter = box.childRange.start; childIter < box.childRange.end; childIter++) {

                        ReadyFlags c = crossAxis.readyFlags[childIter];

                        if ((c & ReadyFlags.BaseSizeResolved) != 0) {
                            continue;
                        }

                        crossAxis.readyFlags[childIter] |= ReadyFlags.ParentReady;

                        // content & aspect are good to go, just missing parent size (which we know we have)
                        if ((c & (ReadyFlags.AspectReady | ReadyFlags.ContentReady)) != 0) {
                            LayoutBoxInfo child = new LayoutBoxInfo(childIter, box.boxIndex);
                            SolveSizes(new TempList<LayoutBoxInfo>(&child, 1), ref crossAxis);
                        }
                        else {
                            childrenAllHaveBaseSize = false;
                        }
                    }

                    if (childrenAllHaveBaseSize) {
                        readyFlags |= ReadyFlags.ContentReady | ReadyFlags.ContentSizeResolved;
                    }

                }

                // at this point if our content is ready, we can layout 
                if ((readyFlags & ReadyFlags.ContentReady) != 0) {
                    layoutList.array[layoutList.size++] = box;
                }

                crossAxis.readyFlags[box.boxIndex] = readyFlags;

            }
        }

    }

}