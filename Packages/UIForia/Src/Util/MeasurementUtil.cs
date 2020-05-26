using System;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Util {

    public static class MeasurementUtil {

        public static float ResolveOriginBaseX(LayoutResult[] layoutTable, in LayoutResult result, in ViewParameters viewParameters, AlignmentTarget target, AlignmentDirection direction, InputSystem inputSystem) {
            switch (target) {
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return result.allocatedPosition.x;

                case AlignmentTarget.Parent:
                    return 0;

                case AlignmentTarget.ParentContentArea:
                    if (result.layoutParent == default) return 0;

                    ref LayoutResult parentResult = ref layoutTable[result.elementId.index];

                    if (direction == AlignmentDirection.Start) {
                        return parentResult.padding.left + parentResult.border.left;
                    }
                    else {
                        return parentResult.padding.right + parentResult.border.right;
                    }

                case AlignmentTarget.Template:
                    // todo handle transclusion
                    return 0;

                case AlignmentTarget.TemplateContentArea:
                    // todo handle transclusion
                    return 0;

                case AlignmentTarget.View: {

                    float output = viewParameters.viewX;

                    ElementId ptr = result.layoutParent;

                    while (ptr != default) {

                        ref LayoutResult layoutResult = ref layoutTable[ptr.index];

                        output -= layoutResult.alignedPosition.x;

                        ptr = layoutResult.layoutParent;

                    }

                    return output;
                }

                case AlignmentTarget.Screen: {
                    float output = 0;

                    ElementId ptr = result.layoutParent;

                    while (ptr != default) {

                        ref LayoutResult layoutResult = ref layoutTable[ptr.index];

                        output -= layoutResult.alignedPosition.x;

                        ptr = layoutResult.layoutParent;

                    }

                    return output;
                }

                case AlignmentTarget.Mouse: {
                    float dist = GetXDistanceToScreen(layoutTable, result);
                    return inputSystem.MousePosition.x + dist;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public static float ResolveOriginBaseY(LayoutResult[] layoutTable, in LayoutResult result, float viewportY, AlignmentTarget target, AlignmentDirection direction, InputSystem inputSystem) {
            switch (target) {
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return result.allocatedPosition.y;

                case AlignmentTarget.Parent:
                    return 0;

                case AlignmentTarget.ParentContentArea:
                    if (result.layoutParent == default) return 0;

                    ref LayoutResult parentResult = ref layoutTable[result.elementId.index];

                    if (direction == AlignmentDirection.Start) {
                        return parentResult.padding.top + parentResult.border.top;
                    }
                    else {
                        return parentResult.padding.bottom + parentResult.border.bottom;
                    }

                case AlignmentTarget.Template:
                    throw new NotImplementedException();

                case AlignmentTarget.TemplateContentArea:
                    throw new NotImplementedException();

                case AlignmentTarget.View: {

                    float output = viewportY;

                    ElementId ptr = result.layoutParent;

                    while (ptr != default) {

                        ref LayoutResult layoutResult = ref layoutTable[ptr.index];

                        output -= layoutResult.alignedPosition.y;

                        ptr = layoutResult.layoutParent;

                    }

                    return output;
                }

                case AlignmentTarget.Screen: {

                    float output = 0;

                    ElementId ptr = result.layoutParent;

                    while (ptr != default) {

                        ref LayoutResult layoutResult = ref layoutTable[ptr.index];

                        output -= layoutResult.alignedPosition.y;

                        ptr = layoutResult.layoutParent;

                    }

                    return output;

                }

                case AlignmentTarget.Mouse:
                    float dist = GetYDistanceToScreen(layoutTable, result);
                    return inputSystem.MousePosition.y + dist;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public static float GetXDistanceToClipper(LayoutResult[] layoutTable, LayoutResult result, ElementId clipper, float applicationWidth, out float width) {

            float output = 0;

            if (clipper == default) {
                width = applicationWidth;
                return GetXDistanceToScreen(layoutTable, result);
            }

            ElementId ptr = result.layoutParent;

            width = layoutTable[clipper.index].actualSize.width;

            while (ptr != clipper) {

                ref LayoutResult layoutResult = ref layoutTable[ptr.index];

                output -= layoutResult.alignedPosition.x;

                ptr = layoutResult.layoutParent;

            }

            return output;
        }

        public static float GetYDistanceToClipper(LayoutResult[] layoutTable, LayoutResult result, ElementId clipper, float applicationHeight, out float height) {
            float output = 0;

            if (clipper == default) {
                height = applicationHeight;
                return GetXDistanceToScreen(layoutTable, result);
            }

            ElementId ptr = result.layoutParent;

            height = layoutTable[clipper.index].actualSize.height;

            while (ptr != clipper) {

                ref LayoutResult layoutResult = ref layoutTable[ptr.index];

                output -= layoutResult.alignedPosition.y;

                ptr = layoutResult.layoutParent;

            }

            return output;
        }

        public static float GetXDistanceToView(LayoutResult[] layoutTable, ElementId viewRoot, LayoutResult result) {

            ElementId ptr = result.layoutParent;

            float output = 0;

            while (ptr != viewRoot && ptr != default) {
                ref LayoutResult layoutResult = ref layoutTable[ptr.index];
                output -= layoutResult.alignedPosition.x;
                ptr = layoutResult.layoutParent;
                ;
            }

            return output;
        }

        public static float GetYDistanceToView(LayoutResult[] layoutTable, ElementId viewRoot, LayoutResult result) {
            ElementId ptr = result.layoutParent;

            float output = 0;

            // should never be default, should always hit viewRoot
            while (ptr != viewRoot && ptr != default) {
                ref LayoutResult layoutResult = ref layoutTable[ptr.index];
                output -= layoutResult.alignedPosition.y;
                ptr = layoutResult.layoutParent;
            }

            return output;
        }

        public static float GetXDistanceToScreen(LayoutResult[] layoutTable, LayoutResult result) {
            ElementId ptr = result.layoutParent;

            float output = 0;

            while (ptr != default) {
                ref LayoutResult layoutResult = ref layoutTable[ptr.index];
                output -= layoutResult.alignedPosition.x;
                ptr = layoutResult.layoutParent;
            }

            return output;
        }

        public static float GetYDistanceToScreen(LayoutResult[] layoutTable, LayoutResult result) {
            ElementId ptr = result.layoutParent;

            float output = 0;

            while (ptr != default) {
                ref LayoutResult layoutResult = ref layoutTable[ptr.index];
                output -= layoutResult.alignedPosition.y;
                ptr = layoutResult.layoutParent;
            }

            return output;
        }

        public static float ResolveOffsetOriginSizeX(LayoutResult[] layoutTable, in LayoutResult layoutResult, in ViewParameters viewParameters, AlignmentTarget target) {
            switch (target) {
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return layoutResult.allocatedSize.width;

                case AlignmentTarget.Parent:

                    if (layoutResult.layoutParent == default) {
                        return viewParameters.viewWidth;
                    }

                    return layoutTable[layoutResult.layoutParent.index].actualSize.width;

                case AlignmentTarget.ParentContentArea:
                    if (layoutResult.layoutParent == default) {
                        return viewParameters.viewWidth;
                    }

                    return Mathf.Max(0, layoutTable[layoutResult.layoutParent.index].ContentAreaWidth);

                case AlignmentTarget.Template:
                    // todo handle transclusion
                    return 0;

                case AlignmentTarget.TemplateContentArea:
                    // todo handle transclusion
                    return 0;

                case AlignmentTarget.View:
                    return viewParameters.viewWidth;

                case AlignmentTarget.Screen:
                    return viewParameters.applicationWidth;

                case AlignmentTarget.Mouse: {
                    return 0;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public static float ResolveOffsetOriginSizeY(LayoutResult[] layoutTable, in LayoutResult layoutResult, in ViewParameters viewParameters, AlignmentTarget target) {
            switch (target) {
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return layoutResult.allocatedSize.height;

                case AlignmentTarget.Parent:

                    if (layoutResult.layoutParent == default) {
                        return viewParameters.viewHeight;
                    }

                    return layoutTable[layoutResult.layoutParent.index].actualSize.height;

                case AlignmentTarget.ParentContentArea:
                    if (layoutResult.layoutParent == default) {
                        return viewParameters.viewHeight;
                    }

                    return Mathf.Max(0, layoutTable[layoutResult.layoutParent.index].ContentAreaHeight);

                case AlignmentTarget.Template:
                    // todo handle transclusion
                    return 0;

                case AlignmentTarget.TemplateContentArea:
                    // todo handle transclusion
                    return 0;

                case AlignmentTarget.View:
                    return viewParameters.viewHeight;

                case AlignmentTarget.Screen:
                    return viewParameters.applicationHeight;

                case AlignmentTarget.Mouse: {
                    return 0;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        [DebuggerStepThrough]
        public static float ResolveFixedSize(float baseSize, float viewWidth, float viewHeight, float emSize, UIFixedLength fixedSize) {
            switch (fixedSize.unit) {
                case UIFixedUnit.Pixel:
                    return fixedSize.value;

                case UIFixedUnit.Percent:
                    return baseSize * fixedSize.value;

                case UIFixedUnit.ViewportHeight:
                    return viewHeight * fixedSize.value;

                case UIFixedUnit.ViewportWidth:
                    return viewWidth * fixedSize.value;

                case UIFixedUnit.Em:
                    return emSize * fixedSize.value;

                default:
                    return 0;
            }
        }

        public static float ResolveOffsetMeasurement(LayoutResult[] layoutTable, UIElement element, in ViewParameters viewParameters, in OffsetMeasurement measurement, float percentageRelativeVal) {
            switch (measurement.unit) {
                case OffsetMeasurementUnit.Unset:
                    return 0;

                case OffsetMeasurementUnit.Pixel:
                    return measurement.value;

                case OffsetMeasurementUnit.Em:
                    return element.style.GetResolvedFontSize() * measurement.value;

                case OffsetMeasurementUnit.ActualWidth:
                    return measurement.value * layoutTable[element.id.index].actualSize.width;

                case OffsetMeasurementUnit.ActualHeight:
                    return measurement.value * layoutTable[element.id.index].actualSize.height;

                case OffsetMeasurementUnit.AllocatedWidth:
                    return measurement.value * layoutTable[element.id.index].allocatedSize.width;

                case OffsetMeasurementUnit.AllocatedHeight:
                    return measurement.value * layoutTable[element.id.index].allocatedSize.height;

                case OffsetMeasurementUnit.ContentWidth:
                    throw new NotImplementedException();
//                    return ResolveContentWidth(box) * measurement.value;

                case OffsetMeasurementUnit.ContentHeight:
                    throw new NotImplementedException();
//                    return ResolveContentHeight(box) * measurement.value;

                case OffsetMeasurementUnit.ContentAreaWidth:
                    throw new NotImplementedException();
//                    return box.contentSize.width * measurement.value;

                case OffsetMeasurementUnit.ContentAreaHeight:
                    throw new NotImplementedException();
//                    return box.contentSize.height * measurement.value;

                case OffsetMeasurementUnit.ViewportWidth:
                    return viewParameters.viewWidth * measurement.value;

                case OffsetMeasurementUnit.ViewportHeight:
                    return viewParameters.viewHeight * measurement.value;

                case OffsetMeasurementUnit.ParentWidth:
                    throw new NotImplementedException();
                // if box.parent is null the box is the root, otherwise call ResolveLayoutParent to handle transclusion
//                    if (box.parent == null) return 0;
//                    return box.ResolveLayoutParent().size.width * measurement.value;

                case OffsetMeasurementUnit.ParentHeight:
                    throw new NotImplementedException();

                // if box.parent is null the box is the root, otherwise call ResolveLayoutParent to handle transclusion
//                    if (box.parent == null) return 0;
//                    return box.ResolveLayoutParent().size.height * measurement.value;

                case OffsetMeasurementUnit.ParentContentAreaWidth:
                    throw new NotImplementedException();

                // if box.parent is null the box is the root, otherwise call ResolveLayoutParent to handle transclusion
//                    if (box.parent == null) return 0;
//                    return box.ResolveLayoutParent().contentSize.width * measurement.value;

                case OffsetMeasurementUnit.ParentContentAreaHeight:
                    throw new NotImplementedException();

//                    if (box.parent == null) return 0;
//                    return box.ResolveLayoutParent().contentSize.height * measurement.value;

                case OffsetMeasurementUnit.ScreenWidth:
                    return viewParameters.applicationWidth * measurement.value;

                case OffsetMeasurementUnit.ScreenHeight:
                    return viewParameters.applicationHeight * measurement.value;

                case OffsetMeasurementUnit.Percent:
                    return percentageRelativeVal * measurement.value;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}