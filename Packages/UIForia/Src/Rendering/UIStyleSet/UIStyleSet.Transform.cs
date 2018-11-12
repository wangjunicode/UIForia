using JetBrains.Annotations;
using UIForia;
using UnityEngine;

namespace UIForia.Rendering {

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
            SetFixedLengthProperty(StylePropertyId.TransformPositionX, x, state);
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
            SetFixedLengthProperty(StylePropertyId.TransformPositionY, y, state);
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
           SetFloatProperty(StylePropertyId.TransformRotation, rotationDeg, state);
        }

        [PublicAPI]
        public float GetTransformScaleX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformScaleX, state).AsFloat;
        }

        [PublicAPI]
        public void SetTransformScaleX(float x, StyleState state) {
         SetFloatProperty(StylePropertyId.TransformScaleX, x, state);
        }

        [PublicAPI]
        public float GetTransformScaleY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformScaleY, state).AsFloat;
        }

        [PublicAPI]
        public void SetTransformScaleY(float y, StyleState state) {
            SetFloatProperty(StylePropertyId.TransformScaleY, y, state);

        }

        [PublicAPI]
        public float GetTransformPivotX(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPivotX, state).AsFloat;
        }

        [PublicAPI]
        public void SetTransformPivotX(float x, StyleState state) {
            SetFloatProperty(StylePropertyId.TransformPivotX, x, state);
        }

        [PublicAPI]
        public float GetTransformPivotY(StyleState state) {
            return GetPropertyValueInState(StylePropertyId.TransformPivotX, state).AsFloat;
        }

        [PublicAPI]
        public void SetTransformPivotY(float y, StyleState state) {
            SetFloatProperty(StylePropertyId.TransformPivotY, y, state);
        }

        public void SetTransformPosition(Vector2 position, StyleState state) {
            SetTransformPositionX(position.x, state);
            SetTransformPositionY(position.y, state);
        }

        public void SetTransformBehavior(TransformBehavior behavior, StyleState state) {
            SetEnumProperty(StylePropertyId.TransformBehaviorX, (int) behavior, state);
            SetEnumProperty(StylePropertyId.TransformBehaviorY, (int) behavior, state);
        }

        public void SetTransformBehaviorX(TransformBehavior behavior, StyleState state) {
            SetEnumProperty(StylePropertyId.TransformBehaviorX, (int) behavior, state);
        }

        public void SetTransformBehaviorY(TransformBehavior behavior, StyleState state) {
            SetEnumProperty(StylePropertyId.TransformBehaviorY, (int) behavior, state);
        }

        public TransformBehavior GetTransformBehaviorX(StyleState state) {
            return (TransformBehavior) GetPropertyValueInState(StylePropertyId.TransformBehaviorX, state).AsInt;
        }

        public TransformBehavior GetTransformBehaviorY(StyleState state) {
            return (TransformBehavior) GetPropertyValueInState(StylePropertyId.TransformBehaviorX, state).AsInt;
        }

    }

}