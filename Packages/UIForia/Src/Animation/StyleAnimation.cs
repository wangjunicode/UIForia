using System;
using UIForia.Rendering;
using UnityEngine;

namespace UIForia.Animation {

    public abstract class StyleAnimation {

        public enum AnimationStatus {

            Pending,
            Running,
            Completed

        }
        
        public AnimationOptions m_Options;

        public abstract AnimationStatus Update(UIStyleSet styleSet, Rect viewport, float deltaTime);

        public virtual void OnStart(UIStyleSet styleSet, Rect viewport) { }

        public virtual void OnEnd(UIStyleSet styleSet) { }

        public virtual void OnPause(UIStyleSet styleSet) { }

        public virtual void OnResume(UIStyleSet styleSet) { }

        protected float ResolveTransformOffset(UIElement element, Rect viewport, TransformOffset transformOffset) {
            switch (transformOffset.unit) {
                
                case TransformUnit.Unset:
                    return 0;
                
                case TransformUnit.Pixel:
                    return transformOffset.value;
                
                case TransformUnit.ActualWidth:
                    return element.layoutResult.actualSize.width * transformOffset.value;
                
                case TransformUnit.ActualHeight:
                    return element.layoutResult.actualSize.height * transformOffset.value;
                
                case TransformUnit.AllocatedWidth:
                    return element.layoutResult.allocatedSize.width * transformOffset.value;

                case TransformUnit.AllocatedHeight:
                    return element.layoutResult.allocatedSize.height * transformOffset.value;
                
                case TransformUnit.ContentWidth:
                    return element.layoutResult.contentRect.width * transformOffset.value;
                
                case TransformUnit.ContentHeight:
                    return element.layoutResult.contentRect.height * transformOffset.value;

                case TransformUnit.ViewportWidth:
                    return viewport.width * transformOffset.value;
                
                case TransformUnit.ViewportHeight:
                    return viewport.height * transformOffset.value;

                case TransformUnit.Em:
                case TransformUnit.ContentAreaWidth:
                case TransformUnit.ContentAreaHeight:
                case TransformUnit.AnchorWidth:
                case TransformUnit.AnchorHeight:
                    throw new NotImplementedException();
                
                case TransformUnit.ParentWidth:
                    if (element.parent == null) {
                        return viewport.width * transformOffset.value;
                    }

                    return element.parent.layoutResult.actualSize.width * transformOffset.value;
                
                case TransformUnit.ParentHeight:
                    if (element.parent == null) {
                        return viewport.height * transformOffset.value;
                    }

                    return element.parent.layoutResult.actualSize.height * transformOffset.value;
                
                case TransformUnit.ParentContentAreaWidth:
                    if (element.parent == null) {
                        return viewport.width * transformOffset.value;
                    }

                    return element.parent.layoutResult.contentRect.width * transformOffset.value;
                
                case TransformUnit.ParentContentAreaHeight:
                    if (element.parent == null) {
                        return viewport.height * transformOffset.value;
                    }

                    return element.parent.layoutResult.contentRect.height * transformOffset.value;
                
                case TransformUnit.ScreenWidth:
                    return Screen.width * transformOffset.value;
                
                case TransformUnit.ScreenHeight:
                    return Screen.height * transformOffset.value;
                
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        protected static float ResolveFixedWidth(UIElement element, Rect viewport, UIFixedLength width) {
            switch (width.unit) {
                case UIFixedUnit.Pixel:
                    return width.value;

                case UIFixedUnit.Percent:
                    return element.layoutResult.AllocatedWidth * width.value;

                case UIFixedUnit.ViewportHeight:
                    return viewport.height * width.value;

                case UIFixedUnit.ViewportWidth:
                    return viewport.width * width.value;

                case UIFixedUnit.Em:
                    return element.style.TextFontAsset.fontInfo.PointSize * width.value;

                default:
                    return 0;
            }
        }

        protected static float ResolveFixedHeight(UIElement element, Rect viewport, UIFixedLength height) {
            switch (height.unit) {
                case UIFixedUnit.Pixel:
                    return height.value;

                case UIFixedUnit.Percent:
                    return element.layoutResult.AllocatedHeight * height.value;

                case UIFixedUnit.ViewportHeight:
                    return viewport.height * height.value;

                case UIFixedUnit.ViewportWidth:
                    return viewport.width * height.value;

                case UIFixedUnit.Em:
                    return element.style.TextFontAsset.fontInfo.PointSize * height.value;

                default:
                    return 0;
            }
        }


        public static float ResolveWidthMeasurement(UIElement element, Rect viewport, UIMeasurement measurement) {
            switch (measurement.unit) {
                case UIMeasurementUnit.Unset:
                    return 0;

                case UIMeasurementUnit.Pixel:
                    return measurement.value;

                case UIMeasurementUnit.Content:
                    return element.layoutResult.actualSize.width * measurement.value;

                case UIMeasurementUnit.ParentSize:
                    if (element.parent.style.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, element.parent.layoutResult.AllocatedWidth * measurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, viewport.width * measurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, viewport.height * measurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    UIStyleSet parentStyle = element.parent.style;
                    if (parentStyle.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, (element.parent.layoutResult.AllocatedWidth
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingLeft)
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingRight)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderRight)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderLeft)));
                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, element.style.TextFontAsset.fontInfo.PointSize * measurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public static float ResolveHeightMeasurement(UIElement element, Rect viewport, UIMeasurement measurement) {
            switch (measurement.unit) {
                case UIMeasurementUnit.Unset:
                    return 0;

                case UIMeasurementUnit.Pixel:
                    return measurement.value;

                case UIMeasurementUnit.Content:
                    return element.layoutResult.actualSize.height * measurement.value;

                case UIMeasurementUnit.ParentSize:
                    if (element.parent.style.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, element.parent.layoutResult.AllocatedHeight * measurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, viewport.width * measurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, viewport.height * measurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    UIStyleSet parentStyle = element.parent.style;
                    if (parentStyle.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, (element.parent.layoutResult.AllocatedHeight
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingTop)
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingBottom)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderBottom)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderTop)));
                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, element.style.TextFontAsset.fontInfo.PointSize * measurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static UIElement ResolveLayoutParent(UIElement element) {
            return  element.parent;
        }

        private static float ResolveVerticalAnchorBaseHeight(UIElement element, Rect viewport) {
            switch (element.style.AnchorTarget) {
                case AnchorTarget.Parent:
                    return ResolveLayoutParent(element).layoutResult.AllocatedHeight;

                case AnchorTarget.ParentContentArea:
                    UIElement layoutParent = ResolveLayoutParent(element);
                    UIStyleSet parentStyle = layoutParent.style;
                    return Mathf.Max(0, (element.parent.layoutResult.AllocatedHeight
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingTop)
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingBottom)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderBottom)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderTop)));

                case AnchorTarget.Viewport:
                    return viewport.height;

                case AnchorTarget.Screen:
                    return Screen.height;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private static float ResolveHorizontalAnchorBaseWidth(UIElement element, Rect viewport) {
            switch (element.style.AnchorTarget) {
                case AnchorTarget.Parent:
                    return ResolveLayoutParent(element).layoutResult.AllocatedWidth;

                case AnchorTarget.ParentContentArea:
                    UIElement layoutParent = ResolveLayoutParent(element);
                    UIStyleSet parentStyle = layoutParent.style;
                    return Mathf.Max(0, (element.parent.layoutResult.AllocatedWidth
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingRight)
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingLeft)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderLeft)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderRight)));

                case AnchorTarget.Viewport:
                    return viewport.width;

                case AnchorTarget.Screen:
                    return Screen.width;

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float ResolveAnchorTop(UIElement element, Rect viewport, UIFixedLength anchor) {
            switch (element.style.AnchorTarget) {
                case AnchorTarget.Parent:
                    return ResolveVerticalAnchor(element, viewport, anchor);

                case AnchorTarget.Viewport:
                    return viewport.y + ResolveVerticalAnchor(element, viewport, anchor);

                case AnchorTarget.Screen:
                    return ResolveLayoutParent(element).layoutResult.screenPosition.y - ResolveVerticalAnchor(element, viewport, anchor);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float ResolveAnchorBottom(UIElement element, Rect viewport, UIFixedLength anchor) {
            UIElement layoutParent = ResolveLayoutParent(element);
            switch (element.style.AnchorTarget) {
                case AnchorTarget.Parent:
                    return ResolveVerticalAnchor(element, viewport, anchor);

                case AnchorTarget.Viewport:
                    return (layoutParent.layoutResult.screenPosition.y + layoutParent.layoutResult.AllocatedHeight) + viewport.y + viewport.height +
                           ResolveVerticalAnchor(element, viewport, anchor);

                case AnchorTarget.Screen:
                    return (layoutParent.layoutResult.screenPosition.y + layoutParent.layoutResult.AllocatedHeight) -
                           ResolveVerticalAnchor(element, viewport, anchor);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float ResolveAnchorLeft(UIElement element, Rect viewport, UIFixedLength anchor) {
            switch (element.style.AnchorTarget) {
                case AnchorTarget.Parent:
                    return ResolveHorizontalAnchor(element, viewport, anchor);

                case AnchorTarget.Viewport:
                    return ResolveLayoutParent(element).layoutResult.screenPosition.x - viewport.x + ResolveHorizontalAnchor(element, viewport, anchor);

                case AnchorTarget.Screen:
                    return ResolveHorizontalAnchor(element, viewport, anchor);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float ResolveAnchorRight(UIElement element, Rect viewport, UIFixedLength anchor) {
            return ResolveHorizontalAnchor(element, viewport, anchor);
//            UIElement layoutParent = ResolveLayoutParent(element);
//            switch (element.ComputedStyle.AnchorTarget) {
//                case AnchorTarget.Parent:
//                    return ResolveHorizontalAnchor(element, viewport, anchor);
//
//                case AnchorTarget.Viewport:
//                    return (layoutParent.layoutResult.screenPosition.x + layoutParent.layoutResult.allocatedWidth) + viewport.x + viewport.width +
//                           ResolveHorizontalAnchor(element, viewport, anchor);
//
//                case AnchorTarget.Screen:
//                    float resolved = ResolveHorizontalAnchor(element, viewport, anchor);
//
//                    return resolved;//(layoutParent.layoutResult.screenPosition.x + layoutParent.layoutResult.allocatedWidth) -
////                           resolved;
//
//                default:
//                    throw new ArgumentOutOfRangeException();
//            }
        }

        public float ResolveVerticalAnchor(UIElement element, Rect viewport, UIFixedLength anchor) {
            switch (anchor.unit) {
                case UIFixedUnit.Pixel:
                    return anchor.value;

                case UIFixedUnit.Percent:
                    return ResolveVerticalAnchorBaseHeight(element, viewport) * anchor.value;

                case UIFixedUnit.ViewportHeight:
                    return viewport.height * anchor.value;

                case UIFixedUnit.ViewportWidth:
                    return viewport.width * anchor.value;

                case UIFixedUnit.Em:
                    return element.style.EmSize;

                default:
                    return 0;
            }
        }

        public float ResolveHorizontalAnchor(UIElement element, Rect viewport, UIFixedLength anchor) {
            switch (anchor.unit) {
                case UIFixedUnit.Pixel:
                    return anchor.value;

                case UIFixedUnit.Percent:
                    return ResolveHorizontalAnchorBaseWidth(element, viewport) * anchor.value;

                case UIFixedUnit.ViewportHeight:
                    return viewport.height * anchor.value;

                case UIFixedUnit.ViewportWidth:
                    return viewport.width * anchor.value;

                case UIFixedUnit.Em:
                    return element.style.EmSize;

                default:
                    return 0;
            }
        }

        public virtual void OnComplete() {}

    }

}