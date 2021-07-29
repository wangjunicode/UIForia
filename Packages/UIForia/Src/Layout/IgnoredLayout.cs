using UnityEngine;

namespace UIForia.Layout {

    internal unsafe static class IgnoredLayout {

        const ReadyFlags k_ReadyForPositioning = ReadyFlags.AspectReady | ReadyFlags.ContentReady | ReadyFlags.ContentSizeResolved | ReadyFlags.BaseSizeResolved;

        public static CheckedArray<LayoutBoxInfo> Layout(ref LayoutListBuffer listBuffer, ref LayoutAxis horizontalAxis, ref LayoutAxis verticalAxis, CheckedArray<LayoutBoxInfo> boxes) {

            // behaves like normal element parent
            // assigns a size & position & sets upwards ready flags for child

            for (int i = 0; i < boxes.size; i++) {

                ref LayoutBoxInfo boxInfo = ref boxes.array[i];

                // real parent must be final size resolved
                // ignored child must be (aspect ready | content ready | content size | base size resolved);

                if (!boxInfo.ignoredPositionHSet) {
                    boxInfo.ignoredPositionHSet = ApplyPositions(ref horizontalAxis, boxInfo);
                }

                if (!boxInfo.ignoredPositionVSet) {
                    boxInfo.ignoredPositionVSet = ApplyPositions(ref verticalAxis, boxInfo);
                }

            }

            int size = boxes.size;
            LayoutBoxInfo* array = boxes.array;

            for (int i = 0; i < size; i++) {
                LayoutBoxInfo box = array[i];
                if (box.ignoredPositionHSet && box.ignoredPositionVSet) {
                    array[i--] = array[--size];
                }
            }

            return new CheckedArray<LayoutBoxInfo>(array, size);

        }

        private static bool ApplyPositions(ref LayoutAxis axis, in LayoutBoxInfo boxInfo) {

            ReadyFlags parentFlags = axis.readyFlags.array[boxInfo.parentIndex];
            ref ReadyFlags ignoredElementFlags = ref axis.readyFlags.array[boxInfo.boxIndex];

            if ((parentFlags & ReadyFlags.FinalSizeResolved) != 0) {
                ignoredElementFlags |= ReadyFlags.ParentReady | ReadyFlags.StretchReady;
            }

            if ((parentFlags & ReadyFlags.FinalSizeResolved) == 0 || (ignoredElementFlags & k_ReadyForPositioning) != k_ReadyForPositioning) {
                return false;
            }

            // have all sizes -> do positioning

            LayoutSizes sizes = axis.sizes[boxInfo.boxIndex];

            CrossAxisInfo crossInfo = new CrossAxisInfo() {
                boxIndex = boxInfo.boxIndex,
                stretch = (int) sizes.stretch,
                baseSize = sizes.baseSize,
                maxSize = sizes.max,
                minSize = sizes.min,
                parentSize = axis.outputSizes[boxInfo.parentIndex],
                marginEndStretch = (ushort) axis.marginEnd[boxInfo.boxIndex].stretch,
                marginEndValue = axis.marginEnd[boxInfo.boxIndex].value + axis.borderEnd[boxInfo.parentIndex],
                marginStartStretch = (ushort) axis.marginStart[boxInfo.boxIndex].stretch,
                marginStartValue = axis.marginStart[boxInfo.boxIndex].value + axis.borderStart[boxInfo.parentIndex]
            };

            CheckedArray<CrossAxisInfo> info = new CheckedArray<CrossAxisInfo>(&crossInfo, 1);

            FlexLayout.StretchCrossAxis(ref axis, info, true);

            return true;
        }

        public static void CollapseIgnoredMargins(LayoutTree* layoutTree, MarginCollapseInfo marginCollapseInfo) {
            for (int d = 0; d < layoutTree->depthLevels.size; d++) {
                RangeInt range = layoutTree->depthLevels[d].ignoredRange;

                for (int boxIndex = range.start; boxIndex < range.end; boxIndex++) {

                    int parentIndex = layoutTree->nodeList[boxIndex].parentIndex;

                    SpaceCollapse collapse = marginCollapseInfo.spaceCollapse[parentIndex];
                    ResolvedSpacerSize parentPaddingStart = marginCollapseInfo.paddingStart[parentIndex];
                    ResolvedSpacerSize parentPaddingEnd = marginCollapseInfo.paddingEnd[parentIndex];

                    ref ResolvedSpacerSize childMarginStart = ref marginCollapseInfo.marginStart.Get(boxIndex);
                    ref ResolvedSpacerSize childMarginEnd = ref marginCollapseInfo.marginEnd.Get(boxIndex);

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

    }

}