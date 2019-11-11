using System;
using System.Diagnostics;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Systems;
using UnityEngine;

namespace UIForia.Util {

    public static class MeasurementUtil {

        private static float ResolveContentWidth(FastLayoutBox box) {
            float minX = float.MaxValue;
            float maxX = float.MinValue;

            if (box.firstChild == null) {
                return 0;
            }

            FastLayoutBox ptr = box.firstChild;
            // using allocated box sizes and ignoring alignment & transforms

            while (ptr != null) {
                if (ptr.allocatedPosition.x < minX) minX = ptr.allocatedPosition.x;
                if (ptr.allocatedPosition.x + ptr.size.width > maxX) maxX = ptr.allocatedPosition.x + ptr.size.width;
                ptr = ptr.nextSibling;
            }

            return maxX - minX;
        }

        private static float ResolveContentHeight(FastLayoutBox box) {
            float minY = float.MaxValue;
            float maxY = float.MinValue;

            if (box.firstChild == null) {
                return 0;
            }

            FastLayoutBox ptr = box.firstChild;
            // using allocated box sizes and ignoring alignment & transforms

            while (ptr != null) {
                if (ptr.allocatedPosition.y < minY) minY = ptr.allocatedPosition.y;
                if (ptr.allocatedPosition.y + ptr.size.height > maxY) maxY = ptr.allocatedPosition.y + ptr.size.height;
                ptr = ptr.nextSibling;
            }

            return maxY - minY;
        }

        public static float ResolveOriginBaseX(LayoutResult result, float viewportX, AlignmentBehavior target, AlignmentDirection direction) {
            switch (target) {
                case AlignmentBehavior.Unset:
                case AlignmentBehavior.Default:
                case AlignmentBehavior.LayoutBox:
                    return result.allocatedPosition.x;

                case AlignmentBehavior.Parent:
                    return 0;

                case AlignmentBehavior.ParentContentArea:
                    throw new NotImplementedException();

//                    FastLayoutBox parentBox = el.ResolveLayoutParent();
//                    if (direction == AlignmentDirection.Start) {
//                        return parentBox.paddingBox.left + parentBox.borderBox.left;
//                    }
//                    else {
//                        return parentBox.paddingBox.right + parentBox.borderBox.right;
//                    }

                case AlignmentBehavior.Template:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.TemplateContentArea:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.View:
                    return viewportX;

                case AlignmentBehavior.Screen:
                    throw new NotImplementedException();
//                    FastLayoutBox ptr = box.ResolveLayoutParent();
                    float output = 0;
//                    while (ptr != null) {
//                        output -= ptr.alignedPosition.x;
//                        output -= ptr.transformPositionX.value;
//                        ptr = ptr.ResolveLayoutParent();
//                    }
                    
                    return output;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public static float ResolveOriginBaseY(LayoutResult result, float viewportY, AlignmentBehavior target, AlignmentDirection direction) {
            switch (target) {
                case AlignmentBehavior.Unset:
                case AlignmentBehavior.Default:
                case AlignmentBehavior.LayoutBox:
                    return result.allocatedPosition.y;

                case AlignmentBehavior.Parent:
                    return 0;

                case AlignmentBehavior.ParentContentArea:
                    LayoutResult parentResult = result.layoutParent;

                    if (direction == AlignmentDirection.Start) {
                        return parentResult.padding.top + parentResult.border.top;
                    }
                    else {
                        return parentResult.padding.bottom + parentResult.border.bottom;
                    }

                case AlignmentBehavior.Template:
                    throw new NotImplementedException();
                
                case AlignmentBehavior.TemplateContentArea:
                    throw new NotImplementedException();

                case AlignmentBehavior.View:
                    return viewportY;

                case AlignmentBehavior.Screen:
                    throw new NotImplementedException();

//                    FastLayoutBox ptr = box.ResolveLayoutParent();
//                    float output = 0;
//                    while (ptr != null) {
//                        output -= ptr.alignedPosition.y;
//                        output -= ptr.transformPositionY.value;
//                        ptr = ptr.ResolveLayoutParent();
//                    }
                    
//                    return output;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }


        public static float ResolveOffsetOriginSizeX(LayoutResult layoutResult, float viewportWidth, AlignmentBehavior target) {
            switch (target) {
                case AlignmentBehavior.Unset:
                case AlignmentBehavior.Default:
                case AlignmentBehavior.LayoutBox:
                    return layoutResult.allocatedSize.width;

                case AlignmentBehavior.Parent:
                    return layoutResult.layoutParent.actualSize.width;

                case AlignmentBehavior.ParentContentArea:
                    return layoutResult.layoutParent.actualSize.width;

//                    return box.ResolveLayoutParent().contentSize.width;

                case AlignmentBehavior.Template:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.TemplateContentArea:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.View:
                    return viewportWidth;

                case AlignmentBehavior.Screen:
                    return layoutResult.element.Application.Width;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public static float ResolveOffsetOriginSizeY(LayoutResult result, float viewportHeight, AlignmentBehavior target) {
            switch (target) {
                case AlignmentBehavior.Unset:
                case AlignmentBehavior.Default:
                case AlignmentBehavior.LayoutBox:
                    return result.allocatedSize.height;

                case AlignmentBehavior.Parent:
                    return result.layoutParent.actualSize.height;

                case AlignmentBehavior.ParentContentArea:
                    return result.layoutParent.ContentHeight;

                case AlignmentBehavior.Template:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.TemplateContentArea:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.View:
                    return viewportHeight;

                case AlignmentBehavior.Screen:
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
        
        public static float ResolveOffsetMeasurement(FastLayoutBox box, float viewportWidth, float viewportHeight, in OffsetMeasurement measurement, float percentageRelativeVal) {
            switch (measurement.unit) {
                case OffsetMeasurementUnit.Unset:
                    return 0;

                case OffsetMeasurementUnit.Pixel:
                    return measurement.value;

                case OffsetMeasurementUnit.Em:
                    return box.element.style.GetResolvedFontSize() * measurement.value;

                case OffsetMeasurementUnit.ActualWidth:
                    return measurement.value * box.size.width;

                case OffsetMeasurementUnit.ActualHeight:
                    return measurement.value * box.size.height;

                case OffsetMeasurementUnit.AllocatedWidth:
                    return measurement.value * box.allocatedSize.width;

                case OffsetMeasurementUnit.AllocatedHeight:
                    return measurement.value * box.allocatedSize.height;

                case OffsetMeasurementUnit.ContentWidth:
                    return ResolveContentWidth(box) * measurement.value;

                case OffsetMeasurementUnit.ContentHeight:
                    return ResolveContentHeight(box) * measurement.value;

                case OffsetMeasurementUnit.ContentAreaWidth:
                    return box.contentSize.width * measurement.value;

                case OffsetMeasurementUnit.ContentAreaHeight:
                    return box.contentSize.height * measurement.value;

                case OffsetMeasurementUnit.ViewportWidth:
                    return viewportWidth * measurement.value;

                case OffsetMeasurementUnit.ViewportHeight:
                    return viewportHeight * measurement.value;

                case OffsetMeasurementUnit.ParentWidth:
                    // if box.parent is null the box is the root, otherwise call ResolveLayoutParent to handle transclusion
                    if (box.parent == null) return 0;
                    return box.ResolveLayoutParent().size.width * measurement.value;

                case OffsetMeasurementUnit.ParentHeight:
                    // if box.parent is null the box is the root, otherwise call ResolveLayoutParent to handle transclusion
                    if (box.parent == null) return 0;
                    return box.ResolveLayoutParent().size.height * measurement.value;

                case OffsetMeasurementUnit.ParentContentAreaWidth:
                    // if box.parent is null the box is the root, otherwise call ResolveLayoutParent to handle transclusion
                    if (box.parent == null) return 0;
                    return box.ResolveLayoutParent().contentSize.width * measurement.value;

                case OffsetMeasurementUnit.ParentContentAreaHeight:
                    if (box.parent == null) return 0;
                    return box.ResolveLayoutParent().contentSize.height * measurement.value;

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