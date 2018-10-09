using JetBrains.Annotations;
using Src;
using UnityEngine;

namespace Rendering {

    public partial class UIStyleSet {

        [PublicAPI]
        public UIFixedLength GetTransformPositionX(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.TransformPositionX, state);
            return property.IsDefined
                ? UIFixedLength.Decode(property.valuePart0, property.valuePart1)
                : UIFixedLength.Unset;
        }

        [PublicAPI]
        public void SetTransformPositionX(UIFixedLength x, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.TransformPositionX = x;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TransformPositionX)) {
                computedStyle.TransformPositionX = x;
            }
        }

        [PublicAPI]
        public UIFixedLength GetTransformPositionY(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.TransformPositionY, state);
            return property.IsDefined
                ? UIFixedLength.Decode(property.valuePart0, property.valuePart1)
                : UIFixedLength.Unset;
        }

        [PublicAPI]
        public void SetTransformPositionY(UIFixedLength y, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.TransformPositionY = y;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TransformPositionY)) {
                computedStyle.TransformPositionY = y;
            }
        }

        public void SetTransformPosition(FixedLengthVector position, StyleState state) {
            SetTransformPositionX(position.x, state);
            SetTransformPositionY(position.y, state);
        }

        [PublicAPI]
        public float GetTransformRotation(StyleState state) {
            StyleProperty property = GetPropertyValueInState(StylePropertyId.TransformRotation, state);
            return property.IsDefined
                ? FloatUtil.DecodeToFloat(property.valuePart0)
                : FloatUtil.UnsetValue;
        }

        [PublicAPI]
        public void SetTransformRotation(float rotationDeg, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.TransformRotation = rotationDeg;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TransformRotation)) {
                computedStyle.TransformRotation = rotationDeg;
            }
        }

        [PublicAPI]
        public float GetTransformScaleX(StyleState state) {
            return GetFloatValue(StylePropertyId.TransformScaleX, state);
        }

        [PublicAPI]
        public void SetTransformScaleX(float x, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.TransformScaleX = x;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TransformScaleX)) {
                computedStyle.TransformScaleX = x;
            }
        }

        [PublicAPI]
        public float GetTransformScaleY(StyleState state) {
            return GetFloatValue(StylePropertyId.TransformScaleY, state);
        }

        [PublicAPI]
        public void SetTransformScaleY(float y, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.TransformScaleY = y;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TransformScaleY)) {
                computedStyle.TransformScaleY = y;
            }
        }

        [PublicAPI]
        public UIMeasurement GetTransformPivotX(StyleState state) {
            return GetUIMeasurementValue(StylePropertyId.TransformPivotX, state);
        }

        [PublicAPI]
        public void SetTransformPivotX(float x, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.TransformPivotX = x;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TransformPivotX)) {
                computedStyle.TransformPivotX = x;
            }
        }

        [PublicAPI]
        public UIMeasurement GetTransformPivotY(StyleState state) {
            return GetUIMeasurementValue(StylePropertyId.TransformPivotY, state);
        }

        [PublicAPI]
        public void SetTransformPivotY(float y, StyleState state) {
            UIStyle style = GetOrCreateInstanceStyle(state);
            style.TransformPivotY = y;
            if ((state & currentState) != 0 && style == GetActiveStyleForProperty(StylePropertyId.TransformPivotY)) {
                computedStyle.TransformPivotY = y;
            }
        }

        public void SetTransformPosition(Vector2 position, StyleState state) {
            SetTransformPositionX(position.x, state);
            SetTransformPositionY(position.y, state);
        }

    }

}