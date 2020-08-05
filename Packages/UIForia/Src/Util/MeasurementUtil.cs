using System.Diagnostics;
using UIForia.Layout;
using UIForia.Systems;
using Unity.Mathematics;

namespace UIForia.Util {

    public static class MeasurementUtil {

        public static float ResolveOriginBaseX(ElementTable<LayoutBoxInfo> layoutTable, in LayoutBoxInfo result, float viewportX, AlignmentTarget target, AlignmentDirection direction, float mouseX) {
            switch (target) {
                default:
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return result.allocatedPosition.x;

                case AlignmentTarget.Parent:
                    return 0;

                case AlignmentTarget.ParentContentArea:

                    ref LayoutBoxInfo parentResult = ref layoutTable[result.layoutParentId];

                    return direction == AlignmentDirection.Start
                        ? parentResult.paddingBorderStartHorizontal
                        : parentResult.paddingBorderEndHorizontal;

                case AlignmentTarget.View: {

                    float output = viewportX;

                    ElementId ptr = result.layoutParentId;

                    while (ptr != default) {

                        ref LayoutBoxInfo layoutResult = ref layoutTable[ptr];

                        output -= layoutResult.alignedPosition.x;

                        ptr = layoutResult.layoutParentId;

                    }

                    return output;
                }

                case AlignmentTarget.Screen: {
                    float output = 0;

                    ElementId ptr = result.layoutParentId;

                    while (ptr != default) {

                        ref LayoutBoxInfo layoutResult = ref layoutTable[ptr];

                        output -= layoutResult.alignedPosition.x;

                        ptr = layoutResult.layoutParentId;

                    }

                    return output;
                }

                case AlignmentTarget.Mouse: {
                    return mouseX + GetXDistanceToScreen(layoutTable, result);
                }

            }
        }

        public static float ResolveOriginBaseY(ElementTable<LayoutBoxInfo> layoutTable, in LayoutBoxInfo result, float viewportY, AlignmentTarget target, AlignmentDirection direction, float mouseY) {
            switch (target) {
                default:
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return result.allocatedPosition.y;

                case AlignmentTarget.Parent:
                    return 0;

                case AlignmentTarget.ParentContentArea:

                    ref LayoutBoxInfo parentResult = ref layoutTable[result.layoutParentId];

                    return direction == AlignmentDirection.Start
                        ? parentResult.paddingBorderStartVertical
                        : parentResult.paddingBorderEndVertical;

                case AlignmentTarget.View: {

                    float output = viewportY;

                    ElementId ptr = result.layoutParentId;

                    while (ptr != default) {

                        ref LayoutBoxInfo layoutResult = ref layoutTable[ptr];

                        output -= layoutResult.alignedPosition.y;

                        ptr = layoutResult.layoutParentId;

                    }

                    return output;
                }

                case AlignmentTarget.Screen: {

                    float output = 0;

                    ElementId ptr = result.layoutParentId;

                    while (ptr != default) {

                        ref LayoutBoxInfo layoutResult = ref layoutTable[ptr];

                        output -= layoutResult.alignedPosition.y;

                        ptr = layoutResult.layoutParentId;

                    }

                    return output;

                }

                case AlignmentTarget.Mouse:
                    return mouseY + GetYDistanceToScreen(layoutTable, result);
            }
        }

        public static float GetXDistanceToClipper(ElementTable<LayoutBoxInfo> layoutTable, in LayoutBoxInfo result, ElementId clipperId, float applicationWidth, out float width) {

            float output = 0;

            if (clipperId == default) {
                width = applicationWidth;
                return GetXDistanceToScreen(layoutTable, result);
            }

            ElementId ptr = result.layoutParentId;

            width = layoutTable[clipperId].actualWidth;

            while (ptr != clipperId) {

                ref LayoutBoxInfo layoutResult = ref layoutTable[ptr];

                output -= layoutResult.alignedPosition.x;

                ptr = layoutResult.layoutParentId;

            }

            return output;
        }

        public static float GetYDistanceToClipper(ElementTable<LayoutBoxInfo> layoutTable, in LayoutBoxInfo result, ElementId clipperId, float applicationHeight, out float height) {
            float output = 0;

            if (clipperId == default) {
                height = applicationHeight;
                return GetXDistanceToScreen(layoutTable, result);
            }

            ElementId ptr = result.layoutParentId;

            height = layoutTable[clipperId].actualHeight;

            while (ptr != clipperId) {

                ref LayoutBoxInfo layoutResult = ref layoutTable[ptr];

                output -= layoutResult.alignedPosition.y;

                ptr = layoutResult.layoutParentId;

            }

            return output;
        }

        public static float GetXDistanceToView(ElementTable<LayoutBoxInfo> layoutTable, ElementId viewRoot, in LayoutBoxInfo result) {

            ElementId ptr = result.layoutParentId;

            float output = 0;

            while (ptr != viewRoot && ptr != default) {
                ref LayoutBoxInfo layoutResult = ref layoutTable[ptr];
                output -= layoutResult.alignedPosition.x;
                ptr = layoutResult.layoutParentId;
            }

            return output;
        }

        public static float GetYDistanceToView(ElementTable<LayoutBoxInfo> layoutTable, ElementId viewRoot, in LayoutBoxInfo result) {
            ElementId ptr = result.layoutParentId;

            float output = 0;

            // should never be default, should always hit viewRoot
            while (ptr != viewRoot && ptr != default) {
                ref LayoutBoxInfo layoutResult = ref layoutTable[ptr];
                output -= layoutResult.alignedPosition.y;
                ptr = layoutResult.layoutParentId;
            }

            return output;
        }

        public static float GetXDistanceToScreen(ElementTable<LayoutBoxInfo> layoutTable, in LayoutBoxInfo elementInfo) {
            ElementId ptr = elementInfo.layoutParentId;

            float output = 0;

            while (ptr != default) {
                ref LayoutBoxInfo layoutResult = ref layoutTable[ptr];
                output -= layoutResult.alignedPosition.x;
                ptr = layoutResult.layoutParentId;
            }

            return output;
        }

        public static float GetYDistanceToScreen(ElementTable<LayoutBoxInfo> layoutTable, in LayoutBoxInfo elementInfo) {
            ElementId ptr = elementInfo.layoutParentId;

            float output = 0;

            while (ptr != default) {
                ref LayoutBoxInfo layoutResult = ref layoutTable[ptr];
                output -= layoutResult.alignedPosition.y;
                ptr = layoutResult.layoutParentId;
            }

            return output;
        }

        public static float ResolveOffsetOriginSizeX(ElementTable<LayoutBoxInfo> layoutResultTable, in LayoutBoxInfo layoutResult, in ViewParameters viewParameters, AlignmentTarget target) {
            switch (target) {
                default:
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return layoutResult.allocatedWidth;

                case AlignmentTarget.Parent:
                    return layoutResultTable[layoutResult.layoutParentId].actualSize.x;

                case AlignmentTarget.ParentContentArea:
                    return math.max(0, layoutResultTable[layoutResult.layoutParentId].ContentAreaWidth);

                case AlignmentTarget.View:
                    return viewParameters.viewWidth;

                case AlignmentTarget.Screen:
                    return viewParameters.applicationWidth;

                case AlignmentTarget.Mouse: {
                    return 0;
                }
            }
        }

        public static float ResolveOffsetOriginSizeY(ElementTable<LayoutBoxInfo> layoutTable, in LayoutBoxInfo layoutResult, in ViewParameters viewParameters, AlignmentTarget target) {
            switch (target) {
                default:
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return layoutResult.allocatedHeight;

                case AlignmentTarget.Parent:
                    return layoutTable[layoutResult.layoutParentId].actualHeight;

                case AlignmentTarget.ParentContentArea:
                    return math.max(0, layoutTable[layoutResult.layoutParentId].ContentAreaHeight);

                case AlignmentTarget.View:
                    return viewParameters.viewHeight;

                case AlignmentTarget.Screen:
                    return viewParameters.applicationHeight;

                case AlignmentTarget.Mouse: {
                    return 0;
                }
            }
        }

        public static float ResolveTransformPivot(float baseSize, in ViewParameters viewParameters, float emSize, UIFixedLength fixedSize) {
            switch (fixedSize.unit) {

                default:
                case UIFixedUnit.Unset:
                case UIFixedUnit.Pixel:
                    return fixedSize.value;

                case UIFixedUnit.Percent:
                    return baseSize * fixedSize.value;
                
                case UIFixedUnit.Em:
                    return emSize;

                case UIFixedUnit.ViewportWidth:
                    return viewParameters.viewWidth;

                case UIFixedUnit.ViewportHeight:
                    return viewParameters.viewHeight;
            }
        }

        public static float ResolveFixedLayoutSize(in ViewParameters viewParameters, float emSize, UIFixedLength fixedSize) {
            switch (fixedSize.unit) {

                default:
                case UIFixedUnit.Unset:
                case UIFixedUnit.Percent: // percent not supported in layouts
                case UIFixedUnit.Pixel:
                    return fixedSize.value;

                case UIFixedUnit.Em:
                    return emSize;

                case UIFixedUnit.ViewportWidth:
                    return viewParameters.viewWidth;

                case UIFixedUnit.ViewportHeight:
                    return viewParameters.viewHeight;
            }
        }

        [DebuggerStepThrough]
        public static float ResolveFixedSize(float baseSize, in ViewParameters viewParameters, float emSize, UIFixedLength fixedSize) {
            switch (fixedSize.unit) {
                case UIFixedUnit.Pixel:
                    return fixedSize.value;

                case UIFixedUnit.Percent:
                    return baseSize * fixedSize.value;

                case UIFixedUnit.ViewportHeight:
                    return viewParameters.viewHeight * fixedSize.value;

                case UIFixedUnit.ViewportWidth:
                    return viewParameters.viewWidth * fixedSize.value;

                case UIFixedUnit.Em:
                    return emSize * fixedSize.value;

                default:
                    return 0;
            }
        }

        public static float ResolveTransformMeasurement(in LayoutBoxInfo layoutResult, in LayoutBoxInfo parentResult, in ViewParameters viewParameters, in OffsetMeasurement measurement, float percentageRelativeVal) {
            switch (measurement.unit) {
                default:
                case OffsetMeasurementUnit.Unset:
                case OffsetMeasurementUnit.Pixel:
                    return measurement.value;

                case OffsetMeasurementUnit.Em:
                    return layoutResult.emSize * measurement.value;

                case OffsetMeasurementUnit.ActualWidth:
                    return layoutResult.actualSize.x * measurement.value;

                case OffsetMeasurementUnit.ActualHeight:
                    return layoutResult.actualSize.y * measurement.value;

                case OffsetMeasurementUnit.AllocatedWidth:
                    return layoutResult.allocatedSize.x * measurement.value;

                case OffsetMeasurementUnit.AllocatedHeight:
                    return layoutResult.allocatedSize.y * measurement.value;

                case OffsetMeasurementUnit.ContentWidth:
                    return math.max(0, layoutResult.ContentAreaWidth * measurement.value);

                case OffsetMeasurementUnit.ContentHeight:
                    return math.max(0, layoutResult.ContentAreaHeight * measurement.value);

                case OffsetMeasurementUnit.ViewportWidth:
                    return viewParameters.viewWidth * measurement.value;

                case OffsetMeasurementUnit.ViewportHeight:
                    return viewParameters.viewHeight * measurement.value;

                case OffsetMeasurementUnit.ParentWidth:
                    return parentResult.actualSize.x * measurement.value;

                case OffsetMeasurementUnit.ParentHeight:
                    return parentResult.actualSize.y * measurement.value;

                case OffsetMeasurementUnit.ParentContentAreaWidth:
                    return math.max(0, parentResult.ContentAreaWidth * measurement.value);

                case OffsetMeasurementUnit.ParentContentAreaHeight:
                    return math.max(0, parentResult.ContentAreaHeight * measurement.value);

                case OffsetMeasurementUnit.ScreenWidth:
                    return viewParameters.applicationWidth * measurement.value;

                case OffsetMeasurementUnit.ScreenHeight:
                    return viewParameters.applicationWidth * measurement.value;

                case OffsetMeasurementUnit.Percent:
                    return percentageRelativeVal * measurement.value;

            }
        }
        
    }

}