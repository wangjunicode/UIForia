using UIForia.Systems;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia.Layout {

    [BurstCompile]
    internal struct ApplyVerticalAlignments : IJob, IVertigoParallel {

        public ElementId viewRootId;
        public ViewParameters viewParameters;
        public DataList<ElementId>.Shared elementList;
        public ElementTable<AlignmentInfo> alignmentTable;
        public ElementTable<LayoutBoxInfo> layoutBoxInfoTable;

        public float mousePosition;
        public ParallelParams parallel { get; set; }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
        }

        public void Execute() {
            Run(0, elementList.size);
        }

        private void Run(int start, int end) {

            for (int i = start; i < end; i++) {

                ElementId elementId = elementList[i];

                ref LayoutBoxInfo elementLayoutInfo = ref layoutBoxInfoTable[elementId];
                ref LayoutBoxInfo parentLayoutInfo = ref layoutBoxInfoTable[elementLayoutInfo.layoutParentId];

                AlignmentInfo alignmentInfo = alignmentTable[elementList[i]];

                if (alignmentInfo.target == AlignmentTarget.Unset) {
                    continue;
                }

                float originBase = MeasurementUtil.ResolveOriginBaseY(layoutBoxInfoTable, elementLayoutInfo, viewParameters.viewY, alignmentInfo.target, alignmentInfo.direction, mousePosition);

                float originSize = MeasurementUtil.ResolveOffsetOriginSizeY(layoutBoxInfoTable, elementLayoutInfo, viewParameters, alignmentInfo.target);

                float originOffset = MeasurementUtil.ResolveTransformMeasurement(elementLayoutInfo, parentLayoutInfo, viewParameters, alignmentInfo.origin, originSize);

                float offset = MeasurementUtil.ResolveTransformMeasurement(elementLayoutInfo, parentLayoutInfo, viewParameters, alignmentInfo.offset, elementLayoutInfo.actualHeight);

                if (alignmentInfo.direction == AlignmentDirection.End) {
                    elementLayoutInfo.alignedPosition.y = (originBase + originSize) - (originOffset + offset) - elementLayoutInfo.actualHeight;
                }
                else {
                    elementLayoutInfo.alignedPosition.y = originBase + originOffset + offset;
                }

                if (alignmentInfo.boundary != AlignmentBoundary.Unset) {
                    switch (alignmentInfo.boundary) {
                        case AlignmentBoundary.View: {
                            float viewPos = MeasurementUtil.GetYDistanceToView(layoutBoxInfoTable, viewRootId, elementLayoutInfo);
                            if (elementLayoutInfo.alignedPosition.y < viewPos) {
                                elementLayoutInfo.alignedPosition.y = viewPos;
                            }

                            if (elementLayoutInfo.alignedPosition.y + elementLayoutInfo.actualHeight > viewParameters.viewHeight + viewPos) {
                                elementLayoutInfo.alignedPosition.y = viewParameters.viewHeight + viewPos - elementLayoutInfo.actualHeight;
                            }

                            break;
                        }

                        case AlignmentBoundary.Clipper: {
                            float clipperPos = MeasurementUtil.GetYDistanceToClipper(layoutBoxInfoTable, elementLayoutInfo, elementLayoutInfo.clipperId, viewParameters.applicationHeight, out float clipperHeight);

                            if (elementLayoutInfo.alignedPosition.y < clipperPos) {
                                elementLayoutInfo.alignedPosition.y = clipperPos;
                            }

                            if (elementLayoutInfo.alignedPosition.y + elementLayoutInfo.actualHeight > clipperHeight + clipperPos) {
                                elementLayoutInfo.alignedPosition.y = clipperHeight + clipperPos - elementLayoutInfo.actualHeight;
                            }

                            break;
                        }

                        case AlignmentBoundary.Screen:

                            float screenPos = MeasurementUtil.GetYDistanceToScreen(layoutBoxInfoTable, elementLayoutInfo);
                            if (elementLayoutInfo.alignedPosition.y < screenPos) {
                                elementLayoutInfo.alignedPosition.y = screenPos;
                            }

                            if (elementLayoutInfo.alignedPosition.y + elementLayoutInfo.actualHeight > viewParameters.applicationHeight + screenPos) {
                                elementLayoutInfo.alignedPosition.y = viewParameters.applicationHeight + screenPos - elementLayoutInfo.actualHeight;
                            }

                            break;

                        case AlignmentBoundary.Parent: {
                            if (elementLayoutInfo.alignedPosition.y < 0) {
                                elementLayoutInfo.alignedPosition.y = 0;
                            }

                            if (elementLayoutInfo.alignedPosition.y + elementLayoutInfo.actualHeight > parentLayoutInfo.actualHeight) {
                                elementLayoutInfo.alignedPosition.y -= (elementLayoutInfo.alignedPosition.y + elementLayoutInfo.actualHeight) - parentLayoutInfo.actualHeight;
                            }

                            break;
                        }

                        case AlignmentBoundary.ParentContentArea: {
                            if (elementLayoutInfo.alignedPosition.y < parentLayoutInfo.paddingBorderStartVertical) {
                                elementLayoutInfo.alignedPosition.y = parentLayoutInfo.paddingBorderStartVertical;
                            }

                            float height = parentLayoutInfo.actualHeight - parentLayoutInfo.paddingBorderEndVertical;
                            if (elementLayoutInfo.alignedPosition.y + elementLayoutInfo.actualHeight > height) {
                                elementLayoutInfo.alignedPosition.y -= (elementLayoutInfo.alignedPosition.y + elementLayoutInfo.actualHeight) - height;
                            }

                            break;
                        }
                    }
                }

            }
        }

    }

}