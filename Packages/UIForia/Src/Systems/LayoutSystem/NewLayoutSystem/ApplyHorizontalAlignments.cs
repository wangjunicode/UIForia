using UIForia.Systems;
using UIForia.Util;
using UIForia.Util.Unsafe;
using Unity.Burst;
using Unity.Jobs;

namespace UIForia.Layout {

    [BurstCompile]
    public struct ApplyHorizontalAlignments : IJob, IVertigoParallel {

        public DataList<ElementId>.Shared elementList;
        public ElementTable<LayoutBoxInfo> layoutBoxInfoTable;
        public ElementTable<AlignmentInfo> alignmentTable;
        public ViewParameters viewParameters;
        public ElementId viewRootId;

        public float mousePosition;

        public ParallelParams parallel { get; set; }

        public void Execute() {
            Run(0, elementList.size);
        }

        public void Execute(int startIndex, int count) {
            Run(startIndex, startIndex + count);
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

                float originBase = MeasurementUtil.ResolveOriginBaseX(layoutBoxInfoTable, elementLayoutInfo, viewParameters.viewX, alignmentInfo.target, alignmentInfo.direction, mousePosition);

                float originSize = MeasurementUtil.ResolveOffsetOriginSizeX(layoutBoxInfoTable, elementLayoutInfo, viewParameters, alignmentInfo.target);

                float originOffset = MeasurementUtil.ResolveTransformMeasurement(elementLayoutInfo, parentLayoutInfo, viewParameters, alignmentInfo.origin, originSize);

                float offset = MeasurementUtil.ResolveTransformMeasurement(elementLayoutInfo, parentLayoutInfo, viewParameters, alignmentInfo.offset, elementLayoutInfo.actualWidth);

                if (alignmentInfo.direction == AlignmentDirection.End) {
                    elementLayoutInfo.alignedPosition.x = (originBase + originSize) - (originOffset + offset) - elementLayoutInfo.actualWidth;
                }
                else {
                    elementLayoutInfo.alignedPosition.x = originBase + originOffset + offset;
                }

                if (alignmentInfo.boundary != AlignmentBoundary.Unset) {
                    switch (alignmentInfo.boundary) {
                        case AlignmentBoundary.View: {
                            float viewPos = MeasurementUtil.GetXDistanceToView(layoutBoxInfoTable, viewRootId, elementLayoutInfo);
                            if (elementLayoutInfo.alignedPosition.x < viewPos) {
                                elementLayoutInfo.alignedPosition.x = viewPos;
                            }

                            if (elementLayoutInfo.alignedPosition.x + elementLayoutInfo.actualWidth > viewParameters.viewWidth + viewPos) {
                                elementLayoutInfo.alignedPosition.x = viewParameters.viewWidth + viewPos - elementLayoutInfo.actualWidth;
                            }

                            break;
                        }

                        case AlignmentBoundary.Clipper: {
                            float clipperPos = MeasurementUtil.GetXDistanceToClipper(layoutBoxInfoTable, elementLayoutInfo, elementLayoutInfo.clipperId, viewParameters.applicationWidth, out float clipperWidth);

                            if (elementLayoutInfo.alignedPosition.x < clipperPos) {
                                elementLayoutInfo.alignedPosition.x = clipperPos;
                            }

                            if (elementLayoutInfo.alignedPosition.x + elementLayoutInfo.actualWidth > clipperWidth + clipperPos) {
                                elementLayoutInfo.alignedPosition.x = clipperWidth + clipperPos - elementLayoutInfo.actualWidth;
                            }

                            break;
                        }

                        case AlignmentBoundary.Screen:

                            float screenPos = MeasurementUtil.GetXDistanceToScreen(layoutBoxInfoTable, elementLayoutInfo);
                            if (elementLayoutInfo.alignedPosition.x < screenPos) {
                                elementLayoutInfo.alignedPosition.x = screenPos;
                            }

                            if (elementLayoutInfo.alignedPosition.x + elementLayoutInfo.actualWidth > viewParameters.applicationWidth + screenPos) {
                                elementLayoutInfo.alignedPosition.x = viewParameters.applicationWidth + screenPos - elementLayoutInfo.actualWidth;
                            }

                            break;

                        case AlignmentBoundary.Parent: {
                            if (elementLayoutInfo.alignedPosition.x < 0) {
                                elementLayoutInfo.alignedPosition.x = 0;
                            }

                            if (elementLayoutInfo.alignedPosition.x + elementLayoutInfo.actualWidth > parentLayoutInfo.actualWidth) {
                                elementLayoutInfo.alignedPosition.x -= (elementLayoutInfo.alignedPosition.x + elementLayoutInfo.actualWidth) - parentLayoutInfo.actualWidth;
                            }

                            break;
                        }

                        case AlignmentBoundary.ParentContentArea: {
                            if (elementLayoutInfo.alignedPosition.x < parentLayoutInfo.paddingBorderStartHorizontal) {
                                elementLayoutInfo.alignedPosition.x = parentLayoutInfo.paddingBorderStartHorizontal;
                            }

                            float width = parentLayoutInfo.actualWidth - parentLayoutInfo.paddingBorderEndHorizontal;
                            if (elementLayoutInfo.alignedPosition.x + elementLayoutInfo.actualWidth > width) {
                                elementLayoutInfo.alignedPosition.x -= (elementLayoutInfo.alignedPosition.x + elementLayoutInfo.actualWidth) - width;
                            }

                            break;
                        }
                    }
                }

            }
        }

    }

}