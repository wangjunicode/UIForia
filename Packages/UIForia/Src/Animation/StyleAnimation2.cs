using System;
using System.Collections.Generic;
using UIForia.Elements;
using UIForia.Layout;
using UIForia.Rendering;
using UIForia.Util;
using UnityEngine;

namespace UIForia.Animation {

    public abstract class StyleAnimation2 : AnimationTask {

        public UIElement target;
        public UIElement templateRoot;
        public UIElement templateParent;
        public UIElement animationRoot;
        public UIElement animationParent;

        public LightList<AnimationTrigger> triggers;
        public LightList<AnimationVariable> variables;
        public LightList<StyleAnimation2> childAnimations;
        
        public AnimationState2 status;
        
        public StyleAnimationData data;
        
        protected static readonly KeyFrameSorter KeyFrameSorter = new KeyFrameSorter();
        
        public StyleAnimation2(UIElement target, StyleAnimationData data) : base(AnimationTaskType.Style) {
            this.target = target;
            this.templateRoot = target.templateContext.rootObject as UIElement;
            this.templateParent = target.parent;
            this.animationRoot = target;
            this.animationParent = null;
            this.data = data;
        }

        public void SetVariable(string variableName, Type type, object value) {
                
        }

        public void SetVariable<T>(string variableName, T value) {
            
        }
        
        public override void OnInitialized() {
            
        }

        public override void OnCompleted() {
            base.OnCompleted();
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

    }

}