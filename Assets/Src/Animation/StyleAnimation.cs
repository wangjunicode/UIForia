using System;
using Rendering;
using UnityEngine;

namespace Src.Animation {

    public abstract class StyleAnimation {

        public AnimationOptions m_Options;

        public abstract bool Update(UIStyleSet styleSet, Rect viewport, float deltaTime);

        public virtual void OnStart(UIStyleSet styleSet, Rect viewport) { }

        public virtual void OnEnd(UIStyleSet styleSet) { }

        public virtual void OnPause(UIStyleSet styleSet) { }

        public virtual void OnResume(UIStyleSet styleSet) { }

        protected float ResolveFixedWidth(UIElement element, Rect viewport, UIFixedLength width) {
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
                    return element.style.computedStyle.FontAsset.fontInfo.PointSize * width.value;

                default:
                    return 0;
            }
        }

        protected float ResolveFixedHeight(UIElement element, Rect viewport, UIFixedLength height) {
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
                    return element.style.computedStyle.FontAsset.fontInfo.PointSize * height.value;

                default:
                    return 0;
            }
        }


        public float ResolveWidthMeasurement(UIElement element, Rect viewport, UIMeasurement measurement) {
            switch (measurement.unit) {
                case UIMeasurementUnit.Unset:
                    return 0;

                case UIMeasurementUnit.Pixel:
                    return measurement.value;

                case UIMeasurementUnit.Content:
                    return element.layoutResult.actualSize.width * measurement.value;

                case UIMeasurementUnit.ParentSize:
                    if (element.parent.style.computedStyle.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, element.parent.layoutResult.AllocatedWidth * measurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, viewport.width * measurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, viewport.height * measurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    ComputedStyle parentStyle = element.parent.style.computedStyle;
                    if (parentStyle.PreferredWidth.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, (element.parent.layoutResult.AllocatedWidth
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingLeft)
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingRight)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderRight)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderLeft)));
                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, element.style.computedStyle.FontAsset.fontInfo.PointSize * measurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public float ResolveHeightMeasurement(UIElement element, Rect viewport, UIMeasurement measurement) {
            switch (measurement.unit) {
                case UIMeasurementUnit.Unset:
                    return 0;

                case UIMeasurementUnit.Pixel:
                    return measurement.value;

                case UIMeasurementUnit.Content:
                    return element.layoutResult.actualSize.height * measurement.value;

                case UIMeasurementUnit.ParentSize:
                    if (element.parent.style.computedStyle.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, element.parent.layoutResult.AllocatedHeight * measurement.value);

                case UIMeasurementUnit.ViewportWidth:
                    return Mathf.Max(0, viewport.width * measurement.value);

                case UIMeasurementUnit.ViewportHeight:
                    return Mathf.Max(0, viewport.height * measurement.value);

                case UIMeasurementUnit.ParentContentArea:
                    ComputedStyle parentStyle = element.parent.style.computedStyle;
                    if (parentStyle.PreferredHeight.IsContentBased) {
                        return 0f;
                    }

                    return Mathf.Max(0, (element.parent.layoutResult.AllocatedHeight
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingTop)
                                         - ResolveFixedWidth(element, viewport, parentStyle.PaddingBottom)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderBottom)
                                         - ResolveFixedWidth(element, viewport, parentStyle.BorderTop)));
                case UIMeasurementUnit.Em:
                    return Mathf.Max(0, element.style.computedStyle.FontAsset.fontInfo.PointSize * measurement.value);

                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private UIElement ResolveLayoutParent(UIElement element) {
            UIElement ptr = element.parent;
            while ((ptr.flags & UIElementFlags.RequiresLayout) == 0) {
                ptr = ptr.parent;
            }

            return ptr;
        }

        private float ResolveVerticalAnchorBaseHeight(UIElement element, Rect viewport) {
            switch (element.ComputedStyle.AnchorTarget) {
                case AnchorTarget.Parent:
                    return ResolveLayoutParent(element).layoutResult.AllocatedHeight;

                case AnchorTarget.ParentContentArea:
                    UIElement layoutParent = ResolveLayoutParent(element);
                    ComputedStyle parentStyle = layoutParent.ComputedStyle;
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

        private float ResolveHorizontalAnchorBaseWidth(UIElement element, Rect viewport) {
            switch (element.ComputedStyle.AnchorTarget) {
                case AnchorTarget.Parent:
                    return ResolveLayoutParent(element).layoutResult.AllocatedWidth;

                case AnchorTarget.ParentContentArea:
                    UIElement layoutParent = ResolveLayoutParent(element);
                    ComputedStyle parentStyle = layoutParent.ComputedStyle;
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
            switch (element.ComputedStyle.AnchorTarget) {
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
            switch (element.ComputedStyle.AnchorTarget) {
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
            switch (element.ComputedStyle.AnchorTarget) {
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
                    return element.ComputedStyle.FontAsset.fontInfo.PointSize * anchor.value;

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
                    return element.ComputedStyle.FontAsset.fontInfo.PointSize * anchor.value;

                default:
                    return 0;
            }
        }

    }

}