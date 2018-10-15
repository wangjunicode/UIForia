using Rendering;
using Src.Rendering;
using UnityEngine;

namespace Src.Animation {

    public class PropertyAnimation : StyleAnimation {

        public StyleProperty endValue;
        public StylePropertyId propertyId;
        public float floatVal;
        public Color colorValue;

        public PropertyAnimation(StyleProperty targetProperty, AnimationOptions options = default(AnimationOptions)) {
                
        }
        
        public PropertyAnimation(StylePropertyId propertyId, StyleProperty startValue, AnimationOptions options) { }

        // updates always work with absolute float values
        // on final output -> set style value to endValue

        public override void Update(UIElement element, Rect viewport, float deltaTime) {
            float t = Easing.Interpolate(options.duration, options.timingFunction) / 0.1f;
            float v;
            switch (propertyId) {
                case StylePropertyId.TransformPivotX:
                case StylePropertyId.TransformPositionX:
                case StylePropertyId.PaddingLeft:
                case StylePropertyId.PaddingRight:
                case StylePropertyId.BorderLeft:
                case StylePropertyId.BorderRight:
                case StylePropertyId.MarginLeft:
                case StylePropertyId.MarginRight:
                    v = Mathf.Lerp(floatVal, ResolveFixedWidth(element, viewport, endValue.AsFixedLength), t);
                    element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIFixedLength(v)));
                    break;
                case StylePropertyId.TransformPivotY:
                case StylePropertyId.TransformPositionY:
                case StylePropertyId.PaddingTop:
                case StylePropertyId.PaddingBottom:
                case StylePropertyId.BorderTop:
                case StylePropertyId.BorderBottom:
                case StylePropertyId.MarginTop:
                case StylePropertyId.MarginBottom:
                    v = Mathf.Lerp(floatVal, ResolveFixedHeight(element, viewport, endValue.AsFixedLength), t);
                    element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIFixedLength(v)));
                    break;
                case StylePropertyId.PreferredWidth:
                    v = Mathf.Lerp(floatVal, ResolveWidthMeasurement(element, viewport, endValue.AsMeasurement), t);
                    element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIMeasurement(v)));
                    break;
                case StylePropertyId.PreferredHeight:
                    v = Mathf.Lerp(floatVal, ResolveHeightMeasurement(element, viewport, endValue.AsMeasurement), t);
                    element.style.SetAnimatedProperty(new StyleProperty(propertyId, new UIMeasurement(v)));
                    break;
                case StylePropertyId.Opacity:
                case StylePropertyId.TransformScaleX:
                case StylePropertyId.TransformScaleY:
                case StylePropertyId.TransformRotation:
                case StylePropertyId.GridLayoutColGap:
                case StylePropertyId.GridLayoutRowGap:
                    v = Mathf.Lerp(floatVal, endValue.AsFloat, t);
                    element.style.SetAnimatedProperty(new StyleProperty(propertyId, v));
                    break;
                case StylePropertyId.BorderColor:
                case StylePropertyId.BackgroundColor:
                case StylePropertyId.TextColor:
                    Color c = Color.Lerp(colorValue, endValue.AsColor, t);
                    element.style.SetAnimatedProperty(new StyleProperty(propertyId, c));
                    break;

                default:
                    throw new UIForia.InvalidArgumentException(propertyId + " is not a supported animation property");
            }
        }

    }

}