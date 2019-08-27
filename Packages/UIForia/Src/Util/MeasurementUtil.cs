using System;
using UIForia.Layout;
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

        public static float ResolveOriginBaseX(FastLayoutBox box, float viewportX, AlignmentBehavior target, AlignmentDirection direction) {
            switch (target) {
                case AlignmentBehavior.Unset:
                case AlignmentBehavior.Default:
                case AlignmentBehavior.LayoutBox:
                    return box.allocatedPosition.x;

                case AlignmentBehavior.Parent:
                    return box.parent.allocatedPosition.x;

                case AlignmentBehavior.ParentContentArea:
                    if (direction == AlignmentDirection.Start) {
                        return box.parent.allocatedPosition.x + box.parent.paddingBox.left + box.parent.borderBox.left;
                    }
                    else {
                        return box.parent.allocatedPosition.x + box.parent.paddingBox.right + box.parent.borderBox.right;
                    }

                case AlignmentBehavior.Template:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.TemplateContentArea:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.View:
                    return viewportX;

                case AlignmentBehavior.Screen:
                    return 0;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public static float ResolveOriginBaseY(FastLayoutBox box, float viewportY, AlignmentBehavior target, AlignmentDirection direction) {
            switch (target) {
                case AlignmentBehavior.Unset:
                case AlignmentBehavior.Default:
                case AlignmentBehavior.LayoutBox:
                    return box.allocatedPosition.y;

                case AlignmentBehavior.Parent:
                    return box.parent.allocatedPosition.y;

                case AlignmentBehavior.ParentContentArea:
                    if (direction == AlignmentDirection.Start) {
                        return box.parent.allocatedPosition.y + box.parent.paddingBox.top + box.parent.borderBox.top;
                    }
                    else {
                        return box.parent.allocatedPosition.y + box.parent.paddingBox.bottom + box.parent.borderBox.bottom;
                    }

                case AlignmentBehavior.Template:
                    throw new NotImplementedException();
                
                case AlignmentBehavior.TemplateContentArea:
                    throw new NotImplementedException();

                case AlignmentBehavior.View:
                    return viewportY;

                case AlignmentBehavior.Screen:
                    return 0;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }


        public static float ResolveOffsetOriginSizeX(FastLayoutBox box, float viewportWidth, AlignmentBehavior target) {
            switch (target) {
                case AlignmentBehavior.Unset:
                case AlignmentBehavior.Default:
                case AlignmentBehavior.LayoutBox:
                    return box.allocatedSize.width;

                case AlignmentBehavior.Parent:
                    return box.parent.size.width;

                case AlignmentBehavior.ParentContentArea:
                    return box.parent.contentSize.width;

                case AlignmentBehavior.Template:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.TemplateContentArea:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.View:
                    return viewportWidth;

                case AlignmentBehavior.Screen:
                    return box.element.Application.Width;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
            }
        }

        public static float ResolveOffsetOriginSizeY(FastLayoutBox box, float viewportHeight, AlignmentBehavior target) {
            switch (target) {
                case AlignmentBehavior.Unset:
                case AlignmentBehavior.Default:
                case AlignmentBehavior.LayoutBox:
                    return box.allocatedSize.height;

                case AlignmentBehavior.Parent:
                    return box.parent.size.height;

                case AlignmentBehavior.ParentContentArea:
                    return box.parent.contentSize.height;

                case AlignmentBehavior.Template:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.TemplateContentArea:
                    // todo handle transclusion
                    return 0;

                case AlignmentBehavior.View:
                    return viewportHeight;

                case AlignmentBehavior.Screen:
                    return box.element.Application.Height;

                default:
                    throw new ArgumentOutOfRangeException(nameof(target), target, null);
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
                    if (box.parent == null) return 0;
                    return box.parent.size.width * measurement.value;

                case OffsetMeasurementUnit.ParentHeight:
                    if (box.parent == null) return 0;
                    return box.parent.size.height * measurement.value;

                case OffsetMeasurementUnit.ParentContentAreaWidth:
                    if (box.parent == null) return 0;
                    return box.parent.contentSize.width * measurement.value;

                case OffsetMeasurementUnit.ParentContentAreaHeight:
                    if (box.parent == null) return 0;
                    return box.parent.contentSize.height * measurement.value;

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