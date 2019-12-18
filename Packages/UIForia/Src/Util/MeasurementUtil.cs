using System;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Systems;
using Unity.Mathematics;
using UnityEngine;

namespace UIForia.Util {

    public static class MeasurementUtil {

        public static float ResolveOriginBaseX(LayoutResult result, float viewportX, AlignmentTarget target, AlignmentDirection direction) {
            switch (target) {
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return result.allocatedPosition.x;

                case AlignmentTarget.Parent:
                    return 0;

                case AlignmentTarget.ParentContentArea:
                    LayoutResult parentResult = result.layoutParent;

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
                    LayoutResult ptr = result.element.parent.layoutResult;
                    float output = viewportX;
                    while (ptr != null) {
                        output -= ptr.alignedPosition.x;
                        if (ptr.element.parent == null) {
                            return output;
                        }

                        ptr = ptr.element.parent.layoutResult;
                    }

                    return output;
                }

                case AlignmentTarget.Screen: {
                    LayoutResult ptr = result.element.parent.layoutResult;
                    float output = 0;
                    while (ptr != null) {
                        output -= ptr.alignedPosition.x;
                        if (ptr.element.parent == null) {
                            return output;
                        }

                        ptr = ptr.element.parent.layoutResult;
                    }

                    return output;
                }


                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public static float ResolveOriginBaseY(LayoutResult result, float viewportY, AlignmentTarget target, AlignmentDirection direction) {
            switch (target) {
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return result.allocatedPosition.y;

                case AlignmentTarget.Parent:
                    return 0;

                case AlignmentTarget.ParentContentArea:
                    LayoutResult parentResult = result.layoutParent;

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
                    LayoutResult ptr = result.element.parent.layoutResult;
                    float output = viewportY;
                    while (ptr != null) {
                        output -= ptr.alignedPosition.y;
                        if (ptr.element.parent == null) {
                            return output;
                        }

                        ptr = ptr.element.parent.layoutResult;
                    }

                    return output;
                }

                case AlignmentTarget.Screen: {
                    LayoutResult ptr = result.element.parent.layoutResult;
                    float output = 0;
                    while (ptr != null) {
                        output -= ptr.alignedPosition.y;
                        if (ptr.element.parent == null) {
                            return output;
                        }

                        ptr = ptr.element.parent.layoutResult;
                    }

                    return output;
                }

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }


        public static float ResolveOffsetOriginSizeX(LayoutResult layoutResult, float viewportWidth, AlignmentTarget target) {
            switch (target) {
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return layoutResult.allocatedSize.width;

                case AlignmentTarget.Parent:
                    if (layoutResult.layoutParent == null) {
                        return viewportWidth;
                    }

                    return layoutResult.layoutParent.actualSize.width;

                case AlignmentTarget.ParentContentArea:
                    if (layoutResult.layoutParent == null) {
                        return viewportWidth;
                    }

                    return Mathf.Max(0, layoutResult.layoutParent.ContentWidth);

                case AlignmentTarget.Template:
                    // todo handle transclusion
                    return 0;

                case AlignmentTarget.TemplateContentArea:
                    // todo handle transclusion
                    return 0;

                case AlignmentTarget.View:
                    return viewportWidth;

                case AlignmentTarget.Screen:
                    return layoutResult.element.Application.Width;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public static float ResolveOffsetOriginSizeY(LayoutResult result, float viewportHeight, AlignmentTarget target) {
            switch (target) {
                case AlignmentTarget.Unset:
                case AlignmentTarget.LayoutBox:
                    return result.allocatedSize.height;

                case AlignmentTarget.Parent:
                    if (result.layoutParent == null) {
                        return viewportHeight;
                    }

                    return result.layoutParent.actualSize.height;

                case AlignmentTarget.ParentContentArea:
                    if (result.layoutParent == null) {
                        return viewportHeight;
                    }

                    return Mathf.Max(0, result.layoutParent.ContentHeight);

                case AlignmentTarget.Template:
                    // todo handle transclusion
                    return 0;

                case AlignmentTarget.TemplateContentArea:
                    // todo handle transclusion
                    return 0;

                case AlignmentTarget.View:
                    return viewportHeight;

                case AlignmentTarget.Screen:
                    return result.element.Application.Height;

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

        public static float ResolveOffsetMeasurement(UIElement element, float viewportWidth, float viewportHeight, in OffsetMeasurement measurement, float percentageRelativeVal) {
            switch (measurement.unit) {
                case OffsetMeasurementUnit.Unset:
                    return 0;

                case OffsetMeasurementUnit.Pixel:
                    return measurement.value;

                case OffsetMeasurementUnit.Em:
                    return element.style.GetResolvedFontSize() * measurement.value;

                case OffsetMeasurementUnit.ActualWidth:
                    return measurement.value * element.layoutResult.actualSize.width;

                case OffsetMeasurementUnit.ActualHeight:
                    return measurement.value * element.layoutResult.actualSize.height;

                case OffsetMeasurementUnit.AllocatedWidth:
                    return measurement.value * element.layoutResult.allocatedSize.width;

                case OffsetMeasurementUnit.AllocatedHeight:
                    return measurement.value * element.layoutResult.allocatedSize.height;

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
                    return viewportWidth * measurement.value;

                case OffsetMeasurementUnit.ViewportHeight:
                    return viewportHeight * measurement.value;

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
                    return Screen.width * measurement.value;

                case OffsetMeasurementUnit.ScreenHeight:
                    return Screen.height * measurement.value;

                case OffsetMeasurementUnit.Percent:
                    return percentageRelativeVal * measurement.value;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

    }

}